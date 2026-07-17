using System;
using System.Linq;
using System.Reflection;

namespace Jory.IOC
{
    /// <summary>
    /// Container 的扩展方法集合。
    /// 把底层 IRegistrator / IResolver 的“描述符式”操作，包装成大量易用、
    /// 可读的泛型/类型友好方法，并支持链式调用（注册方法返回 IRegistrator）。
    /// </summary>
    /// <remarks>
    /// 目标框架：.NET Framework 4.5。为兼容 C# 5 编译器，本文件不使用 nameof 与字符串插值，
    /// 统一使用字符串字面量（如 "instance"）与 string.Format。
    /// </remarks>
    public static class ContainerExtension
    {
        #region RegisterSingleton（单例注册）

        /// <summary>注册单例：TFrom(接口) -> TTo(实现，由容器创建并缓存)。</summary>
        public static IRegistrator RegisterSingleton<TFrom, TTo>(this IRegistrator registrator, TTo instance)
              where TFrom : class
              where TTo : class, TFrom
        {
            RegisterSingleton(registrator, typeof(TFrom), instance);
            return registrator;
        }

        /// <summary>注册单例：以实例的实际类型作为服务类型（自映射式单例）。</summary>
        public static IRegistrator RegisterSingleton(this IRegistrator registrator, object instance)
        {
            if (instance == null)
            {
                // C# 5 兼容：使用字符串字面量代替 nameof(instance)。
                throw new ArgumentNullException("instance");
            }

            RegisterSingleton(registrator, instance.GetType(), instance);
            return registrator;
        }

        /// <summary>注册单例：类型自映射（TFrom -> TFrom，单例）。</summary>
        public static IRegistrator RegisterSingleton(this IRegistrator registrator, Type fromType)
        {
            if (fromType == null)
            {
                throw new ArgumentNullException("fromType");
            }

            RegisterSingleton(registrator, fromType, fromType);
            return registrator;
        }

        /// <summary>注册单例：TFrom -> TTo(实例)，带命名 Key。</summary>
        public static IRegistrator RegisterSingleton<TFrom, TTo>(this IRegistrator registrator, string key, TTo instance)
              where TFrom : class
              where TTo : class, TFrom
        {
            RegisterSingleton(registrator, typeof(TFrom), instance, key);
            return registrator;
        }

        /// <summary>注册单例：以 fromType 为服务类型，instance 为单例实例，带命名 Key。</summary>
        public static IRegistrator RegisterSingleton(this IRegistrator registrator, Type fromType, object instance, string key)
        {
            registrator.Register(new DependencyDescriptor(fromType, instance), key);
            return registrator;
        }

        /// <summary>注册单例：以 fromType 为服务类型，instance 为单例实例（默认 Key）。</summary>
        public static IRegistrator RegisterSingleton(this IRegistrator registrator, Type fromType, object instance)
        {
            registrator.Register(new DependencyDescriptor(fromType, instance));
            return registrator;
        }

        /// <summary>注册单例：TFrom -> 指定实例 instance，带命名 Key（按 TFrom 作为服务类型）。</summary>
        public static IRegistrator RegisterSingleton<TFrom>(this IRegistrator registrator, object instance, string key)
        {
            registrator.Register(new DependencyDescriptor(typeof(TFrom), instance), key);
            return registrator;
        }

        /// <summary>注册单例：TFrom -> 指定实例 instance（默认 Key）。</summary>
        public static IRegistrator RegisterSingleton<TFrom>(this IRegistrator registrator, object instance)
        {
            registrator.Register(new DependencyDescriptor(typeof(TFrom), instance));
            return registrator;
        }

        /// <summary>注册单例：TFrom -> TFrom，带命名 Key（自映射）。</summary>
        public static IRegistrator RegisterSingleton<TFrom>(this IRegistrator registrator, string key)
        {
            registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TFrom), Lifetime.Singleton), key);
            return registrator;
        }

        /// <summary>注册单例：TFrom -> TFrom（自映射，默认 Key）。</summary>
        public static IRegistrator RegisterSingleton<TFrom>(this IRegistrator registrator)
        {
            registrator.Register(new DependencyDescriptor(typeof(TFrom), typeof(TFrom), Lifetime.Singleton));
            return registrator;
        }

        /// <summary>注册单例：fromType -> toType，默认 Key（类型映射）。</summary>
        public static IRegistrator RegisterSingleton(this IRegistrator registrator, Type fromType, Type toType)
        {
            registrator.Register(new DependencyDescriptor(fromType, toType, Lifetime.Singleton));
            return registrator;
        }

        /// <summary>注册单例：fromType -> toType，带命名 Key（类型映射）。</summary>
        public static IRegistrator RegisterSingleton(this IRegistrator registrator, Type fromType, Type toType, string key)
        {
            registrator.Register(new DependencyDescriptor(fromType, toType, Lifetime.Singleton), key);
            return registrator;
        }

        /// <summary>注册单例（自定义工厂）：TFrom 由 func 创建并缓存为单例。</summary>
        public static IRegistrator RegisterSingleton<TFrom>(this IRegistrator registrator, Func<IResolver, object> func)
        {
            registrator.Register(new DependencyDescriptor(typeof(TFrom), Lifetime.Singleton)
            {
                ImplementationFactory = func
            });
            return registrator;
        }

        /// <summary>注册单例（自定义工厂，带 Key）：TFrom 由 func 创建并缓存为单例。</summary>
        public static IRegistrator RegisterSingleton<TFrom>(this IRegistrator registrator, Func<IResolver, object> func, string key)
        {
            registrator.Register(new DependencyDescriptor(typeof(TFrom), Lifetime.Singleton)
            {
                ImplementationFactory = func
            }, key);
            return registrator;
        }

        /// <summary>注册单例（自定义工厂）：fromType 由 func 创建并缓存为单例。</summary>
        public static IRegistrator RegisterSingleton(this IRegistrator registrator, Type fromType, Func<IResolver, object> func)
        {
            registrator.Register(new DependencyDescriptor(fromType, Lifetime.Singleton)
            {
                ImplementationFactory = func
            });
            return registrator;
        }

        /// <summary>注册单例（自定义工厂，带 Key）：fromType 由 func 创建并缓存为单例。</summary>
        public static IRegistrator RegisterSingleton(this IRegistrator registrator, Type fromType, Func<IResolver, object> func, string key)
        {
            registrator.Register(new DependencyDescriptor(fromType, Lifetime.Singleton)
            {
                ImplementationFactory = func
            }, key);
            return registrator;
        }

        /// <summary>注册单例：TFrom -> TTo（类型映射，默认 Key）。</summary>
        public static IRegistrator RegisterSingleton<TFrom, TTO>(this IRegistrator registrator)
             where TFrom : class
             where TTO : class, TFrom
        {
            RegisterSingleton(registrator, typeof(TFrom), typeof(TTO));
            return registrator;
        }

        /// <summary>注册单例：TFrom -> TTo（类型映射，带 Key）。</summary>
        public static IRegistrator RegisterSingleton<TFrom, TTO>(this IRegistrator registrator, string key)
             where TFrom : class
             where TTO : class, TFrom
        {
            RegisterSingleton(registrator, typeof(TFrom), typeof(TTO), key);
            return registrator;
        }

        #endregion RegisterSingleton

        #region RegisterTransient（瞬态注册）

        /// <summary>注册瞬态：TFrom -> TTO（类型映射，默认 Key），每次解析新建。</summary>
        public static IRegistrator RegisterTransient<TFrom, TTO>(this IRegistrator registrator)
             where TFrom : class
             where TTO : class, TFrom
        {
            RegisterTransient(registrator, typeof(TFrom), typeof(TTO));
            return registrator;
        }

        /// <summary>注册瞬态：TFrom -> TFrom（自映射，默认 Key）。</summary>
        public static IRegistrator RegisterTransient<TFrom>(this IRegistrator registrator)
             where TFrom : class
        {
            RegisterTransient(registrator, typeof(TFrom), typeof(TFrom));
            return registrator;
        }

        /// <summary>注册瞬态：TFrom -> TFrom，带命名 Key。</summary>
        public static IRegistrator RegisterTransient<TFrom>(this IRegistrator registrator, string key)
             where TFrom : class
        {
            RegisterTransient(registrator, typeof(TFrom), typeof(TFrom), key);
            return registrator;
        }

        /// <summary>注册瞬态：TFrom -> TTO，带命名 Key。</summary>
        public static IRegistrator RegisterTransient<TFrom, TTO>(this IRegistrator registrator, string key)
            where TFrom : class
            where TTO : class, TFrom
        {
            RegisterTransient(registrator, typeof(TFrom), typeof(TTO), key);
            return registrator;
        }

        /// <summary>注册瞬态：fromType -> fromType（自映射，默认 Key）。</summary>
        public static IRegistrator RegisterTransient(this IRegistrator registrator, Type fromType)
        {
            RegisterTransient(registrator, fromType, fromType);
            return registrator;
        }

        /// <summary>注册瞬态：fromType -> fromType，带命名 Key。</summary>
        public static IRegistrator RegisterTransient(this IRegistrator registrator, Type fromType, string key)
        {
            RegisterTransient(registrator, fromType, fromType, key);
            return registrator;
        }

        /// <summary>注册瞬态：fromType -> toType（类型映射，默认 Key）。</summary>
        public static IRegistrator RegisterTransient(this IRegistrator registrator, Type fromType, Type toType)
        {
            registrator.Register(new DependencyDescriptor(fromType, toType, Lifetime.Transient));
            return registrator;
        }

        /// <summary>注册瞬态：fromType -> toType，带命名 Key（类型映射）。</summary>
        public static IRegistrator RegisterTransient(this IRegistrator registrator, Type fromType, Type toType, string key)
        {
            registrator.Register(new DependencyDescriptor(fromType, toType, Lifetime.Transient), key);
            return registrator;
        }

        /// <summary>注册瞬态（自定义工厂）：TFrom 由 func 每次新建。</summary>
        public static IRegistrator RegisterTransient<TFrom>(this IRegistrator registrator, Func<IResolver, object> func)
        {
            registrator.Register(new DependencyDescriptor(typeof(TFrom), Lifetime.Transient)
            {
                ImplementationFactory = func
            });
            return registrator;
        }

        /// <summary>注册瞬态（自定义工厂，带 Key）：TFrom 由 func 每次新建。</summary>
        public static IRegistrator RegisterTransient<TFrom>(this IRegistrator registrator, Func<IResolver, object> func, string key)
        {
            registrator.Register(new DependencyDescriptor(typeof(TFrom), Lifetime.Transient)
            {
                ImplementationFactory = func
            }, key);
            return registrator;
        }

        /// <summary>注册瞬态（自定义工厂）：fromType 由 func 每次新建。</summary>
        public static IRegistrator RegisterTransient(this IRegistrator registrator, Type fromType, Func<IResolver, object> func)
        {
            registrator.Register(new DependencyDescriptor(fromType, Lifetime.Transient)
            {
                ImplementationFactory = func
            });
            return registrator;
        }

        /// <summary>注册瞬态（自定义工厂，带 Key）：fromType 由 func 每次新建。</summary>
        public static IRegistrator RegisterTransient(this IRegistrator registrator, Type fromType, Func<IResolver, object> func, string key)
        {
            registrator.Register(new DependencyDescriptor(fromType, Lifetime.Transient)
            {
                ImplementationFactory = func
            }, key);
            return registrator;
        }

        #endregion Transient

        #region Unregister（移除注册）

        /// <summary>移除注册：按 fromType（默认 Key）。</summary>
        public static IRegistrator Unregister(this IRegistrator registrator, Type fromType)
        {
            registrator.Unregister(new DependencyDescriptor(fromType));
            return registrator;
        }

        /// <summary>移除注册：按 fromType 与命名 Key。</summary>
        public static IRegistrator Unregister(this IRegistrator registrator, Type fromType, string key)
        {
            registrator.Unregister(new DependencyDescriptor(fromType), key);
            return registrator;
        }

        /// <summary>移除注册：按 TFrom（默认 Key）。</summary>
        public static IRegistrator Unregister<TFrom>(this IRegistrator registrator)
        {
            registrator.Unregister(new DependencyDescriptor(typeof(TFrom)));
            return registrator;
        }

        /// <summary>移除注册：按 TFrom 与命名 Key。</summary>
        public static IRegistrator Unregister<TFrom>(this IRegistrator registrator, string key)
        {
            registrator.Unregister(new DependencyDescriptor(typeof(TFrom)), key);
            return registrator;
        }

        #endregion Unregister

        #region Resolve（解析）

        /// <summary>泛型解析：按默认 key 创建 T 类型实例。</summary>
        public static T Resolve<T>(this IResolver resolver)
        {
            return (T)resolver.Resolve(typeof(T));
        }

        /// <summary>泛型解析：按命名 Key 创建 T 类型实例。</summary>
        public static T Resolve<T>(this IResolver resolver, string key)
        {
            return (T)resolver.Resolve(typeof(T), key);
        }

        /// <summary>
        /// 创建未注册的根类型实例：适用于“目标类型未注册，但其成员依赖已注册”的场景，
        /// 按构造函数 / 属性 / 方法注入其成员依赖。不进入容器注册表。
        /// <para>
        /// 若 resolver 为 <see cref="Container"/>，则优先调用其实例方法以复用反射缓存、
        /// 循环依赖检测，并完整支持三级注入；否则走 fallback（无缓存，但同样支持三级注入）。
        /// </para>
        /// </summary>
        public static object ResolveWithoutRoot(this IResolver resolver, Type fromType)
        {
            Container container = resolver as Container;
            if (container != null)
            {
                return container.ResolveWithoutRoot(fromType);
            }

            return ResolveWithoutRootFallback(resolver, fromType);
        }

        /// <summary>泛型版本：创建未注册的根类型 T 实例。</summary>
        public static T ResolveWithoutRoot<T>(this IResolver resolver)
        {
            return (T)ResolveWithoutRoot(resolver, typeof(T));
        }

        /// <summary>
        /// 非 Container 的 IResolver 所用的 fallback 实现：
        /// 完整支持构造函数 / 属性 / 方法三级注入（修复早期版本仅做构造注入的缺陷），但不带反射缓存。
        /// </summary>
        private static object ResolveWithoutRootFallback(IResolver resolver, Type fromType)
        {
            if (fromType == null)
            {
                throw new ArgumentNullException("fromType");
            }

            // 选择构造函数：优先带 [DependencyInject]；否则取参数最多的公共构造函数。
            ConstructorInfo ctor = fromType.GetConstructors()
                .FirstOrDefault(c => c.IsDefined(typeof(DependencyInjectAttribute), true));
            if (ctor == null)
            {
                ConstructorInfo[] ctors = fromType.GetConstructors();
                if (ctors.Length == 0)
                {
                    throw new InvalidOperationException(
                        string.Format("类型 {0} 没有可访问的公共构造函数。", fromType.FullName));
                }

                ctor = ctors.OrderByDescending(c => c.GetParameters().Length).First();
            }

            DependencyTypeAttribute typeAttr = fromType.IsDefined(typeof(DependencyTypeAttribute), true)
                ? fromType.GetCustomAttribute<DependencyTypeAttribute>()
                : null;
            bool doCtor = typeAttr == null || typeAttr.Type.HasFlag(DependencyType.Constructor);
            bool doProp = typeAttr == null || typeAttr.Type.HasFlag(DependencyType.Property);
            bool doMethod = typeAttr == null || typeAttr.Type.HasFlag(DependencyType.Method);

            object instance;
            if (doCtor)
            {
                ParameterInfo[] parameters = ctor.GetParameters();
                object[] args = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    args[i] = ResolveMemberFor(resolver, parameters[i].ParameterType,
                        parameters[i].GetCustomAttribute<DependencyInjectAttribute>(),
                        parameters[i].HasDefaultValue ? parameters[i].DefaultValue : null);
                }

                instance = args.Length == 0
                    ? Activator.CreateInstance(fromType)
                    : Activator.CreateInstance(fromType, args);
            }
            else
            {
                instance = Activator.CreateInstance(fromType);
            }

            if (doProp)
            {
                foreach (PropertyInfo prop in fromType.GetProperties()
                    .Where(p => p.IsDefined(typeof(DependencyInjectAttribute), true) && p.CanWrite))
                {
                    DependencyInjectAttribute attr = prop.GetCustomAttribute<DependencyInjectAttribute>();
                    object value = ResolveMemberFor(resolver, prop.PropertyType, attr, null);
                    prop.SetValue(instance, value);
                }
            }

            if (doMethod)
            {
                foreach (MethodInfo method in fromType.GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Where(m => m.IsDefined(typeof(DependencyInjectAttribute), true)))
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    object[] args = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        args[i] = ResolveMemberFor(resolver, parameters[i].ParameterType,
                            parameters[i].GetCustomAttribute<DependencyInjectAttribute>(),
                            parameters[i].HasDefaultValue ? parameters[i].DefaultValue : null);
                    }

                    method.Invoke(instance, args);
                }
            }

            return instance;
        }

        /// <summary>
        /// fallback 专用：解析单个成员依赖。基础类型 / 字符串返回 fallback 值（如参数默认值或 null）；
        /// 否则按 [DependencyInject] 的 Type/Key 或声明类型解析。
        /// </summary>
        private static object ResolveMemberFor(IResolver resolver, Type memberType,
            DependencyInjectAttribute attr, object fallback)
        {
            if (memberType.IsPrimitive || memberType == typeof(string))
            {
                return fallback;
            }

            Type resolveType = (attr != null && attr.Type != null) ? attr.Type : memberType;
            string resolveKey = attr != null ? attr.Key : null;
            return resolver.Resolve(resolveType, resolveKey);
        }

        /// <summary>
        /// 尝试解析：若类型已注册则返回实例；否则返回 default（不抛异常）。
        /// </summary>
        public static object TryResolve(this IResolver resolver, Type fromType)
        {
            if (resolver.IsRegistered(fromType))
            {
                return resolver.Resolve(fromType);
            }

            return null;
        }

        /// <summary>尝试解析（带命名 Key）：未注册返回 default。</summary>
        public static object TryResolve(this IResolver resolver, Type fromType, string key)
        {
            if (resolver.IsRegistered(fromType))
            {
                return resolver.Resolve(fromType, key);
            }

            return null;
        }

        /// <summary>泛型尝试解析：未注册返回 default。</summary>
        public static T TryResolve<T>(this IResolver resolver)
        {
            return (T)TryResolve(resolver, typeof(T));
        }

        /// <summary>泛型尝试解析（带命名 Key）：未注册返回 default。</summary>
        public static T TryResolve<T>(this IResolver resolver, string key)
        {
            return (T)TryResolve(resolver, typeof(T), key);
        }

        /// <summary>
        /// 显式释放一个由容器创建并跟踪的实例（仅对 Container 有效）。
        /// 非 Container 的 resolver 调用本方法为空操作。
        /// </summary>
        public static void Release(this IResolver resolver, object instance)
        {
            Container container = resolver as Container;
            if (container != null)
            {
                container.Release(instance);
            }
        }

        #endregion Resolve

        #region IsRegistered（查询）

        /// <summary>判断 T 类型（默认 Key）是否已注册。</summary>
        public static bool IsRegistered<T>(this IRegistered registered)
        {
            return registered.IsRegistered(typeof(T));
        }

        /// <summary>判断 T 类型（带命名 Key）是否已注册。</summary>
        public static bool IsRegistered<T>(this IRegistered registered, string key)
        {
            return registered.IsRegistered(typeof(T), key);
        }

        #endregion
    }
}
