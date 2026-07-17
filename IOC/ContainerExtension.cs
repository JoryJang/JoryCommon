using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UEM.IOC
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

        /// <summary>泛型解析：按默认 Key 创建 T 类型实例。</summary>
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
        /// 创建未注册的根类型实例（瞬态）：适用于“目标类型未注册，但其成员依赖已注册”的场景，
        /// 按构造函数 / 属性 / 方法注入其成员依赖。不进入容器注册表。
        /// </summary>
        /// <exception cref="Exception">目标类型没有任何公共构造函数时抛出。</exception>
        public static object ResolveWithoutRoot(this IResolver resolver, Type fromType)
        {
            // 选择构造函数：优先带 [DependencyInject]；否则取参数最多的公共构造函数。
            var ctor = fromType.GetConstructors().FirstOrDefault(x => x.IsDefined(typeof(DependencyInjectAttribute), true));
            if (ctor == null)
            {
                if (fromType.GetConstructors().Length == 0)
                {
                    throw new Exception(string.Format("没有找到类型{0}的公共构造函数。", fromType.FullName));
                }

                ctor = fromType.GetConstructors().OrderByDescending(x => x.GetParameters().Length).First();
            }

            // 读取类上 [DependencyType]，决定是否进行构造注入。
            DependencyTypeAttribute dependencyTypeAttribute = null;
            if (fromType.IsDefined(typeof(DependencyTypeAttribute), true))
            {
                dependencyTypeAttribute = fromType.GetCustomAttribute<DependencyTypeAttribute>();
            }

            var parameters = ctor.GetParameters();
            var ps = new object[parameters.Length];

            if (dependencyTypeAttribute == null || dependencyTypeAttribute.Type.HasFlag(DependencyType.Constructor))
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType.IsPrimitive || parameters[i].ParameterType == typeof(string))
                    {
                        ps[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : null;
                    }
                    else if (parameters[i].IsDefined(typeof(DependencyInjectAttribute), true))
                    {
                        var attribute = parameters[i].GetCustomAttribute<DependencyInjectAttribute>();
                        var type = attribute.Type ?? parameters[i].ParameterType;
                        ps[i] = resolver.Resolve(type, attribute.Key);
                    }
                    else
                    {
                        ps[i] = resolver.Resolve(parameters[i].ParameterType);
                    }
                }
            }

            if (ps == null || ps.Length == 0)
            {
                return Activator.CreateInstance(fromType);
            }

            return Activator.CreateInstance(fromType, ps);
        }

        /// <summary>泛型版本：创建未注册的根类型 T 实例（瞬态）。</summary>
        public static T ResolveWithoutRoot<T>(this IResolver resolver)
        {
            return (T)ResolveWithoutRoot(resolver, typeof(T));
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
