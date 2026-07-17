using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jory.IOC
{
    /// <summary>
    /// 依赖注入标记特性。
    /// 可标记在：类、构造函数、属性、方法、参数 上，用于告诉容器“这里需要注入依赖”。
    /// 支持通过 Type / Key 精确指定要注入的服务。
    /// </summary>
    /// <example>
    /// [DependencyInject]                      // 按成员声明类型自动解析
    /// [DependencyInject("mysql")]             // 按命名 Key 解析
    /// [DependencyInject(typeof(ImplA))]       // 指定具体类型
    /// [DependencyInject(typeof(ImplA), "k")]  // 指定类型 + Key
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = false)]
    public class DependencyInjectAttribute : Attribute
    {
        /// <summary>默认注入配置：按成员声明的类型自动解析。</summary>
        public DependencyInjectAttribute()
        {
        }

        /// <summary>使用指定命名 Key 注入。</summary>
        /// <param name="key">命名 Key。</param>
        public DependencyInjectAttribute(string key)
        {
            this.Key = key;
        }

        /// <summary>指定注入类型与命名 Key。</summary>
        /// <param name="type">要注入的具体类型。</param>
        /// <param name="key">命名 Key。</param>
        public DependencyInjectAttribute(Type type, string key)
        {
            this.Key = key;
            this.Type = type;
        }

        /// <summary>指定注入类型（按默认 Key）。</summary>
        /// <param name="type">要注入的具体类型。</param>
        public DependencyInjectAttribute(Type type)
        {
            this.Key = string.Empty;
            this.Type = type;
        }

        /// <summary>命名 Key；用于在多个同类型实现中精确定位。为 null/空表示默认 Key。</summary>
        public string Key { get; }

        /// <summary>指定要注入的具体类型；为 null 时按成员声明类型解析。</summary>
        public Type Type { get; }
    }

    /// <summary>
    /// 依赖注入“范围”限定特性（仅可标记在类上）。
    /// 用于告诉容器：本类型的依赖只通过哪些方式注入，从而跳过不必要的反射扫描，提升性能。
    /// </summary>
    /// <example>
    /// [DependencyType(DependencyType.Constructor)]                       // 只做构造函数注入
    /// [DependencyType(DependencyType.Constructor | DependencyType.Property)] // 构造 + 属性
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DependencyTypeAttribute : Attribute
    {
        /// <summary>
        /// 初始化依赖类型。
        /// </summary>
        /// <param name="type">可叠加的位域组合，例如 DependencyType.Constructor | DependencyType.Property。</param>
        public DependencyTypeAttribute(DependencyType type)
        {
            this.Type = type;
        }

        /// <summary>启用的注入方式组合。</summary>
        public DependencyType Type { get; }
    }

    /// <summary>
    /// 依赖注入方式枚举（位标志，可组合）。
    /// <para>
    /// 重要：使用 [Flags] 时不要把有效位设为 0。旧实现中 Constructor = 0 会导致
    /// 任意值与 HasFlag(Constructor) 比较都为 true，从而“指定 Property 却仍执行构造注入”。
    /// 这里改为 None=0 / Constructor=1 / Property=2 / Method=4，逻辑才正确。
    /// </para>
    /// </summary>
    [Flags]
    public enum DependencyType
    {
        /// <summary>不通过任何方式自动注入（需完全手动构造）。</summary>
        None = 0,

        /// <summary>构造函数注入。</summary>
        Constructor = 1,

        /// <summary>属性注入。</summary>
        Property = 2,

        /// <summary>方法注入。</summary>
        Method = 4
    }

    /// <summary>
    /// 注入项的生命周期。
    /// </summary>
    public enum Lifetime
    {
        /// <summary>单例：容器内唯一实例（线程安全，双重检查锁）。</summary>
        Singleton,

        /// <summary>瞬态：每次解析都创建新实例。</summary>
        Transient
    }
}
