using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Jory.IOC
{
    /// <summary>
    /// IOC 容器核心实现（线程安全）。
    /// 负责：维护注册表、解析服务、并自动完成“构造函数 / 属性 / 方法”三级依赖注入。
    /// 实现 IContainer（= IResolver + IRegistrator），并额外实现 IDisposable 以释放其创建的可释放实例。
    /// </summary>
    /// <remarks>
    /// 目标框架：.NET Framework 4.5。为保证在 C# 5 编译器下亦可编译，本文件刻意不使用
    /// nameof 与字符串插值（“$” 语法），统一改用字符串字面量与 string.Format。
    /// </remarks>
    public sealed class Container : IContainer, IDisposable
    {
        #region 字段

        /// <summary>
        /// 注册表：键 = “服务类型全名 + Key”，值 = 依赖描述符。
        /// 使用线程安全的 ConcurrentDictionary 存储。
        /// </summary>
        private readonly ConcurrentDictionary<string, DependencyDescriptor> m_registrations
            = new ConcurrentDictionary<string, DependencyDescriptor>();

        /// <summary>
        /// 反射信息缓存：避免每次解析都重复做昂贵的反射（选择构造函数、收集属性/方法）。
        /// 键为“实现类型”，值为该类型的注入信息。多个线程首次访问同一类型时由 GetOrAdd 保证只计算一次。
        /// </summary>
        private readonly ConcurrentDictionary<Type, InjectionInfo> m_injectionCache
            = new ConcurrentDictionary<Type, InjectionInfo>();

        /// <summary>
        /// 容器自身创建、且实现了 IDisposable 的实例集合，便于在容器释放时统一回收。
        /// 注意：用户通过 RegisterSingleton(instance) 显式传入的实例不在此列（由用户自行管理）。
        /// 用 ConcurrentDictionary 而非 ConcurrentBag，是因为后者在 4.5 没有 Clear() 方法，无法在 Dispose 时清空。
        /// </summary>
        private readonly ConcurrentDictionary<IDisposable, byte> m_disposables
            = new ConcurrentDictionary<IDisposable, byte>();

        /// <summary>
        /// 每个线程独立的“正在解析的类型栈”，用于检测循环依赖，避免无限递归。
        /// 使用 ThreadLocal 保证线程隔离，且每个 Container 实例拥有独立的 ThreadLocal（互不干扰）。
        /// </summary>
        private readonly ThreadLocal<Stack<Type>> m_resolveStack
            = new ThreadLocal<Stack<Type>>(() => new Stack<Type>());

        /// <summary>
        /// 是否跟踪瞬态实例（含 ResolveWithoutRoot 创建的实例）并在 Dispose 时统一释放。
        /// 默认 false：符合主流 DI 容器（Autofac/Unity/MS DI）的约定——瞬态由调用方自行管理，
        /// 避免长期运行的服务因瞬态无限累积而内存泄漏。
        /// 如需旧行为（容器自动跟踪并释放瞬态），请使用 new Container(true)。
        /// 单例实例始终由容器跟踪，不受此开关影响。
        /// </summary>
        private readonly bool m_trackTransients;

        /// <summary>释放标志：0 = 未释放，1 = 已释放。用 Interlocked 保证线程安全。</summary>
        private int m_disposed;

        #endregion

        #region 构造

        /// <summary>
        /// 创建容器（默认不跟踪瞬态），并把容器自身注册为 IResolver / IServiceProvider（单例），
        /// 从而允许把容器本身作为依赖注入（例如某服务需要 IResolver 来动态解析其它服务）。
        /// </summary>
        public Container() : this(false)
        {
        }

        /// <summary>
        /// 创建容器，并指定是否跟踪瞬态实例。
        /// </summary>
        /// <param name="trackTransients">
        /// true：瞬态与 ResolveWithoutRoot 创建的可释放实例也由容器跟踪并在 Dispose 时释放（旧行为，注意可能的内存累积）；
        /// false（默认）：仅跟踪单例，瞬态由调用方自行释放（推荐，可配合 Release(instance) 显式回收）。
        /// </param>
        public Container(bool trackTransients)
        {
            m_trackTransients = trackTransients;
            this.RegisterSingleton<IResolver>(this);
            this.RegisterSingleton<IServiceProvider>(this);
        }

        #endregion

        #region IContainer / IRegistrator 实现

        /// <inheritdoc/>
        public IResolver BuildResolver()
        {
            ThrowIfDisposed();
            return this;
        }

        /// <inheritdoc/>
        /// <summary>
        /// 返回所有已注册描述符的快照（ToArray 复制），避免外部枚举期间注册表变更引发异常。
        /// </summary>
        public IEnumerable<DependencyDescriptor> GetDescriptors()
        {
            return m_registrations.Values.ToArray();
        }

        /// <inheritdoc/>
        /// <summary>兼容 IServiceProvider：直接转发到 Resolve。</summary>
        public object GetService(Type serviceType)
        {
            ThrowIfDisposed();
            // 兼容 IServiceProvider 约定：未注册的类型返回 null（而非抛异常）；
            // 已注册但构造失败（如循环依赖）仍会冒泡异常，便于排查。
            if (serviceType == null)
            {
                return null;
            }

            if (IsRegistered(serviceType))
            {
                return Resolve(serviceType);
            }

            return null;
        }

        /// <inheritdoc/>
        public bool IsRegistered(Type fromType)
        {
            return IsRegistered(fromType, null);
        }

        /// <inheritdoc/>
        public bool IsRegistered(Type fromType, string key)
        {
            if (fromType == null)
            {
                throw new ArgumentNullException("fromType");
            }

            // 对闭合泛型，统一转换为开放泛型定义再匹配（与 Register / Resolve 保持一致），
            // 这样既能查到“注册开放泛型定义”，也能查到“注册具体闭合泛型”。
            if (fromType.IsGenericType && !fromType.IsGenericTypeDefinition)
            {
                fromType = fromType.GetGenericTypeDefinition();
            }

            string k = key == null ? fromType.FullName : string.Format("{0}{1}", fromType.FullName, key);
            return m_registrations.ContainsKey(k);
        }

        /// <inheritdoc/>
        public void Register(DependencyDescriptor descriptor)
        {
            Register(descriptor, null);
        }

        /// <inheritdoc/>
        public void Register(DependencyDescriptor descriptor, string key)
        {
            ThrowIfDisposed();
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }
            if (descriptor.FromType == null)
            {
                throw new ArgumentException("descriptor.FromType 不能为 null。", "descriptor");
            }

            string k = key == null
                ? descriptor.FromType.FullName
                : string.Format("{0}{1}", descriptor.FromType.FullName, key);

            // 重复注册后者覆盖（AddOrUpdate 的更新函数直接返回新值）。
            // 覆盖前清理旧描述符持有的单例实例登记，避免旧实例残留在 m_disposables 中造成双重释放或泄漏。
            m_registrations.AddOrUpdate(k, descriptor, (existingKey, existingValue) =>
            {
                if (existingValue != descriptor)
                {
                    UntrackDisposable(existingValue);
                }
                return descriptor;
            });
        }

        /// <inheritdoc/>
        public void Unregister(DependencyDescriptor descriptor)
        {
            Unregister(descriptor, null);
        }

        /// <inheritdoc/>
        public void Unregister(DependencyDescriptor descriptor, string key)
        {
            ThrowIfDisposed();
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }

            string k = key == null
                ? descriptor.FromType.FullName
                : string.Format("{0}{1}", descriptor.FromType.FullName, key);
            DependencyDescriptor old;
            if (m_registrations.TryRemove(k, out old))
            {
                // 移除注册时，把旧单例实例从待释放集合中摘除（由调用方决定是否自行 Dispose）。
                UntrackDisposable(old);
            }
        }

        #endregion

        #region IResolver 实现（公共解析入口）

        /// <inheritdoc/>
        public object Resolve(Type fromType)
        {
            return ResolveCore(fromType, null);
        }

        /// <inheritdoc/>
        public object Resolve(Type fromType, string key)
        {
            return ResolveCore(fromType, key);
        }

        /// <summary>
        /// 解析（创建）一个“未注册的根类型”实例，并按构造函数 / 属性 / 方法三级注入其成员依赖。
        /// 适用于“目标类型本身不注册到容器，但其成员依赖已注册”的场景。
        /// <para>
        /// 与扩展方法 <see cref="ContainerExtension.ResolveWithoutRoot(IResolver, Type)"/> 相比，本实例方法
        /// 复用容器内部的反射缓存、循环依赖检测，并完整支持属性与方法注入（扩展方法在非 Container 时走 fallback）。
        /// </para>
        /// </summary>
        /// <param name="fromType">要创建的根类型（无需注册）。</param>
        /// <returns>创建并注入完成后的实例。</returns>
        public object ResolveWithoutRoot(Type fromType)
        {
            ThrowIfDisposed();
            if (fromType == null)
            {
                throw new ArgumentNullException("fromType");
            }

            Stack<Type> stack = m_resolveStack.Value;
            if (stack.Contains(fromType))
            {
                string chain = string.Join(" -> ", stack.Reverse().Select(t => t.FullName))
                                + " -> " + fromType.FullName;
                throw new InvalidOperationException("检测到循环依赖：" + chain);
            }

            stack.Push(fromType);
            try
            {
                InjectionInfo info = m_injectionCache.GetOrAdd(fromType, ComputeInjectionInfo);
                object instance = InstantiateAndInject(info, fromType);
                // 行为与瞬态一致：是否由容器跟踪受 m_trackTransients 控制。
                TrackIfDisposable(instance, m_trackTransients);
                return instance;
            }
            finally
            {
                stack.Pop();
            }
        }

        /// <summary>
        /// 显式释放一个由容器创建（且被跟踪）的实例：从待释放集合移除并调用其 Dispose。
        /// 用于在使用完瞬态实例后主动回收，避免长时间持有造成内存压力。
        /// 若实例未被容器跟踪（如 trackTransients=false 创建的瞬态，或用户显式注册的单例实例），本方法不做任何事。
        /// </summary>
        /// <param name="instance">要释放的实例。</param>
        public void Release(object instance)
        {
            if (instance == null)
            {
                return;
            }

            IDisposable d = instance as IDisposable;
            if (d == null)
            {
                return;
            }

            if (m_disposables.TryRemove(d, out _))
            {
                try
                {
                    d.Dispose();
                }
                catch
                {
                    // 忽略单个释放异常，避免影响调用方。
                }
            }
        }

        #endregion

        #region 解析核心

        /// <summary>
        /// 解析核心逻辑：统一处理“带 Key / 不带 Key”、“泛型 / 非泛型”的查找与创建，
        /// 并在入口处做循环依赖检测（借助线程独立的解析栈）。
        /// </summary>
        /// <param name="fromType">请求的服务抽象类型。</param>
        /// <param name="key">命名 Key（可为 null）。</param>
        /// <returns>解析得到的实例。</returns>
        private object ResolveCore(Type fromType, string key)
        {
            ThrowIfDisposed();
            if (fromType == null)
            {
                throw new ArgumentNullException("fromType");
            }

            Stack<Type> stack = m_resolveStack.Value;

            // 循环依赖检测：当前类型已在本次解析链中，说明出现环。
            if (stack.Contains(fromType))
            {
                string chain = string.Join(" -> ", stack.Reverse().Select(t => t.FullName))
                                + " -> " + fromType.FullName;
                throw new InvalidOperationException("检测到循环依赖：" + chain);
            }

            stack.Push(fromType);
            try
            {
                return ResolveInternal(fromType, key);
            }
            finally
            {
                // 无论成功还是抛异常，都要出栈，否则会影响同线程后续解析。
                stack.Pop();
            }
        }

        /// <summary>
        /// 真正的查找 + 创建（不含循环检测，循环检测在 ResolveCore 中统一处理）。
        /// </summary>
        private object ResolveInternal(Type fromType, string key)
        {
            DependencyDescriptor descriptor;
            string k;

            // —— 泛型分支：把闭合泛型转为开放泛型定义去注册表匹配 ——
            if (fromType.IsGenericType)
            {
                Type openType = fromType.GetGenericTypeDefinition();
                k = key == null ? openType.FullName : string.Format("{0}{1}", openType.FullName, key);
                if (m_registrations.TryGetValue(k, out descriptor))
                {
                    return CreateOrGet(descriptor, fromType, key);
                }
            }

            // —— 非泛型 / 泛型未命中分支 ——
            k = key == null ? fromType.FullName : string.Format("{0}{1}", fromType.FullName, key);
            if (m_registrations.TryGetValue(k, out descriptor))
            {
                return CreateOrGet(descriptor, fromType, key);
            }

            // —— 未注册处理 ——
            // 基础类型 / 字符串：返回 null（与 TryResolve 语义一致，便于“可选依赖”场景）。
            if (fromType.IsPrimitive || fromType == typeof(string))
            {
                return null;
            }

            // 其它类型未注册：抛出带类型名的明确异常，便于排查（原实现仅抛无信息 Exception）。
            throw new InvalidOperationException(string.Format(
                "类型 {0} 尚未注册，无法解析。请先调用 Register / RegisterSingleton / RegisterTransient 注册，" +
                "或使用 ResolveWithoutRoot 解析未注册的根类型。", fromType.FullName));
        }

        /// <summary>
        /// 根据描述符的生命周期返回实例：
        /// 单例走缓存（线程安全双重检查锁，保证唯一实例，含工厂单例的缓存修复）；
        /// 瞬态每次新建；若设置了工厂委托则优先调用工厂（单例下工厂结果会被缓存）。
        /// </summary>
        private object CreateOrGet(DependencyDescriptor descriptor, Type requestedType, string key)
        {
            // —— 单例：双重检查锁保证唯一实例 ——
            if (descriptor.Lifetime == Lifetime.Singleton)
            {
                if (descriptor.ToInstance != null)
                {
                    return descriptor.ToInstance;
                }

                lock (descriptor)
                {
                    if (descriptor.ToInstance != null)
                    {
                        return descriptor.ToInstance;
                    }

                    object instance;
                    if (descriptor.ImplementationFactory != null)
                    {
                        // 工厂单例：原实现每次都调用工厂导致“单例不单”，这里缓存结果修复该问题。
                        instance = descriptor.ImplementationFactory.Invoke(this);
                    }
                    else
                    {
                        Type toType = descriptor.ToType;
                        // 开放泛型注册：用请求的具体类型参数还原实现类型。
                        if (toType.IsGenericTypeDefinition)
                        {
                            toType = toType.MakeGenericType(requestedType.GetGenericArguments());
                        }

                        instance = Create(descriptor, toType);
                    }

                    return descriptor.ToInstance = instance;
                }
            }

            // —— 瞬态：工厂优先，否则每次新建 ——
            if (descriptor.ImplementationFactory != null)
            {
                return descriptor.ImplementationFactory.Invoke(this);
            }

            Type transientToType = descriptor.ToType;
            if (transientToType.IsGenericTypeDefinition)
            {
                transientToType = transientToType.MakeGenericType(requestedType.GetGenericArguments());
            }

            return Create(descriptor, transientToType);
        }

        #endregion

        #region 实例创建与依赖注入

        /// <summary>
        /// 创建实现类型实例，并完成“构造 / 属性 / 方法”三级依赖注入；随后触发 OnResolved 回调并登记可释放实例。
        /// 注入信息（构造函数选择、需注入的属性与方法）优先从缓存读取，避免重复反射。
        /// </summary>
        /// <param name="descriptor">本条注册的描述符（用于触发 OnResolved 回调，可为 null 表示无回调）。</param>
        /// <param name="toType">实际要实例化的类型（可能已套用泛型参数）。</param>
        /// <returns>创建好的实例。</returns>
        private object Create(DependencyDescriptor descriptor, Type toType)
        {
            InjectionInfo info = m_injectionCache.GetOrAdd(toType, ComputeInjectionInfo);
            object instance = InstantiateAndInject(info, toType);

            // 解析完成回调（单例仅触发一次；瞬态每次触发）。
            if (descriptor != null && descriptor.OnResolved != null)
            {
                descriptor.OnResolved.Invoke(instance);
            }

            // 单例始终跟踪；瞬态根据 m_trackTransients 决定。
            bool track = descriptor == null
                ? m_trackTransients
                : (descriptor.Lifetime == Lifetime.Singleton || m_trackTransients);
            TrackIfDisposable(instance, track);

            return instance;
        }

        /// <summary>
        /// 按注入信息实例化类型并完成“构造 / 属性 / 方法”三级注入。
        /// 该方法被 Create（已注册类型）与 ResolveWithoutRoot（未注册根类型）共用，保证注入逻辑一致且共享反射缓存。
        /// </summary>
        /// <param name="info">类型的注入信息（来自缓存）。</param>
        /// <param name="toType">要实例化的类型。</param>
        /// <returns>实例化并注入完成后的实例。</returns>
        private object InstantiateAndInject(InjectionInfo info, Type toType)
        {
            // 根据类上的 [DependencyType] 决定启用哪些注入方式；无特性则三种都进行。
            DependencyTypeAttribute typeAttr = info.DependencyTypeAttribute;
            bool doCtor = typeAttr == null || typeAttr.Type.HasFlag(DependencyType.Constructor);
            bool doProp = typeAttr == null || typeAttr.Type.HasFlag(DependencyType.Property);
            bool doMethod = typeAttr == null || typeAttr.Type.HasFlag(DependencyType.Method);

            // —— 1) 构造函数注入 ——
            object[] ctorArgs = null;
            if (doCtor)
            {
                ParameterInfo[] parameters = info.Ctor.GetParameters();
                if (parameters.Length > 0)
                {
                    ctorArgs = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        ctorArgs[i] = ResolveParameter(parameters[i]);
                    }
                }
            }

            object instance = (ctorArgs == null || ctorArgs.Length == 0)
                ? Activator.CreateInstance(toType)
                : Activator.CreateInstance(toType, ctorArgs);

            // —— 2) 属性注入 ——
            if (doProp)
            {
                foreach (PropertyInfo prop in info.InjectProperties)
                {
                    // 缓存已保证属性带 [DependencyInject]，直接取特性并解析。
                    DependencyInjectAttribute attr = prop.GetCustomAttribute<DependencyInjectAttribute>();
                    object value = ResolveMember(prop.PropertyType, attr);
                    prop.SetValue(instance, value);
                }
            }

            // —— 3) 方法注入 ——
            if (doMethod)
            {
                foreach (MethodInfo method in info.InjectMethods)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    object[] args = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        args[i] = ResolveParameter(parameters[i]);
                    }

                    method.Invoke(instance, args);
                }
            }

            return instance;
        }

        /// <summary>
        /// 解析单个构造函数/方法参数的依赖：
        /// 基础类型 / 字符串给默认值（或参数默认值）；否则按 [DependencyInject] 指定的 Type/Key 或参数声明类型解析。
        /// </summary>
        private object ResolveParameter(ParameterInfo parameter)
        {
            Type pType = parameter.ParameterType;
            if (pType.IsPrimitive || pType == typeof(string))
            {
                return parameter.HasDefaultValue ? parameter.DefaultValue : null;
            }

            DependencyInjectAttribute attr = parameter.GetCustomAttribute<DependencyInjectAttribute>();
            Type resolveType = (attr != null && attr.Type != null) ? attr.Type : pType;
            string resolveKey = attr != null ? attr.Key : null;
            return ResolveCore(resolveType, resolveKey);
        }

        /// <summary>
        /// 解析属性/方法成员的依赖（与参数解析类似；成员本身已通过 [DependencyInject] 过滤）。
        /// </summary>
        private object ResolveMember(Type memberType, DependencyInjectAttribute attr)
        {
            if (memberType.IsPrimitive || memberType == typeof(string))
            {
                return null;
            }

            Type resolveType = (attr != null && attr.Type != null) ? attr.Type : memberType;
            string resolveKey = attr != null ? attr.Key : null;
            return ResolveCore(resolveType, resolveKey);
        }

        /// <summary>
        /// 计算并缓存某个实现类型的注入信息（仅做一次昂贵反射）。
        /// 包括：选中的构造函数、需注入的属性、需注入的方法、类上的 [DependencyType]。
        /// </summary>
        private InjectionInfo ComputeInjectionInfo(Type toType)
        {
            // 1) 选择构造函数：优先带 [DependencyInject] 的；否则取参数最多的公共构造函数。
            ConstructorInfo ctor = toType.GetConstructors()
                .FirstOrDefault(c => c.IsDefined(typeof(DependencyInjectAttribute), true));
            if (ctor == null)
            {
                ConstructorInfo[] ctors = toType.GetConstructors();
                if (ctors.Length == 0)
                {
                    throw new InvalidOperationException(string.Format(
                        "类型 {0} 没有可访问的公共构造函数，无法实例化。", toType.FullName));
                }

                ctor = ctors.OrderByDescending(c => c.GetParameters().Length).First();
            }

            // 2) 收集带 [DependencyInject] 的可写属性。
            PropertyInfo[] props = toType.GetProperties()
                .Where(p => p.IsDefined(typeof(DependencyInjectAttribute), true) && p.CanWrite)
                .ToArray();

            // 3) 收集带 [DependencyInject] 的方法（含公共/非公共、实例/静态）。
            MethodInfo[] methods = toType.GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.IsDefined(typeof(DependencyInjectAttribute), true))
                .ToArray();

            // 4) 读取类上的 [DependencyType]（限定注入范围）。
            DependencyTypeAttribute typeAttr = toType.IsDefined(typeof(DependencyTypeAttribute), true)
                ? toType.GetCustomAttribute<DependencyTypeAttribute>()
                : null;

            return new InjectionInfo
            {
                Ctor = ctor,
                InjectProperties = props,
                InjectMethods = methods,
                DependencyTypeAttribute = typeAttr
            };
        }

        #endregion

        #region 释放管理辅助

        /// <summary>若实例实现了 IDisposable 且满足跟踪条件，则登记到待释放集合。</summary>
        private void TrackIfDisposable(object instance, bool track)
        {
            if (track && instance is IDisposable d)
            {
                m_disposables.TryAdd(d, 0);
            }
        }

        /// <summary>把描述符持有的单例实例从待释放集合中摘除（不主动 Dispose，交由调用方/GC 处理）。</summary>
        private void UntrackDisposable(DependencyDescriptor descriptor)
        {
            if (descriptor == null)
            {
                return;
            }

            object inst = descriptor.ToInstance;
            if (inst is IDisposable d)
            {
                m_disposables.TryRemove(d, out _);
            }
        }

        /// <summary>容器已释放则抛 ObjectDisposedException，防止释放后误用。</summary>
        private void ThrowIfDisposed()
        {
            if (Interlocked.CompareExchange(ref m_disposed, 0, 0) != 0)
            {
                throw new ObjectDisposedException("Container", "容器已被释放，不能再进行注册或解析操作。");
            }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// 释放容器：释放所有由容器创建且实现 IDisposable 的实例（默认仅单例；若启用 trackTransients 则含瞬态）。
        /// 注意：用户通过 RegisterSingleton(instance) 显式传入的实例不会被释放（由用户管理其生命周期）。
        /// </summary>
        public void Dispose()
        {
            // 原子置位，保证只释放一次。
            if (Interlocked.Exchange(ref m_disposed, 1) != 0)
            {
                return;
            }

            foreach (IDisposable d in m_disposables.Keys)
            {
                try
                {
                    d.Dispose();
                }
                catch
                {
                    // 忽略单个释放异常，继续释放其余实例，避免一个失败阻断全部回收。
                }
            }

            m_disposables.Clear();

            // 释放线程本地存储（清理当前线程的解析栈）。
            if (m_resolveStack != null)
            {
                m_resolveStack.Dispose();
            }
        }

        #endregion

        #region 内部类型

        /// <summary>
        /// 某个实现类型的注入信息缓存（避免重复反射）。
        /// </summary>
        private class InjectionInfo
        {
            /// <summary>选中的构造函数（带 [DependencyInject] 或参数最多者）。</summary>
            public ConstructorInfo Ctor;

            /// <summary>需要注入的属性（带 [DependencyInject] 且可写）。</summary>
            public PropertyInfo[] InjectProperties;

            /// <summary>需要注入的方法（带 [DependencyInject]）。</summary>
            public MethodInfo[] InjectMethods;

            /// <summary>类上的 [DependencyType]（可能为 null，表示三种注入都进行）。</summary>
            public DependencyTypeAttribute DependencyTypeAttribute;
        }

        #endregion
    }
}
