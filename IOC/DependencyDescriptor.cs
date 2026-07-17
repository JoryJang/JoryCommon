using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jory.IOC
{
    /// <summary>
    /// 依赖描述符：承载“一条注册记录”的全部元信息。
    /// 它描述了“服务抽象(FromType)”对应的“实现(ToType / ToInstance)”、生命周期、工厂与回调等。
    /// </summary>
    public class DependencyDescriptor
    {
        /// <summary>
        /// 以“已有实例”初始化一条单例注册。
        /// 常用于把外部创建好的对象直接作为单例托管给容器。
        /// </summary>
        /// <param name="fromType">服务抽象类型。</param>
        /// <param name="instance">已创建好的实例（将作为单例缓存）。</param>
        public DependencyDescriptor(Type fromType, object instance)
        {
            this.FromType = fromType;
            this.ToInstance = instance;
            this.Lifetime = Lifetime.Singleton;   // 持有实例即视为单例
            this.ToType = instance.GetType();     // 实现类型取实例的实际类型
        }

        /// <summary>
        /// 以“抽象类型 + 实现类型 + 生命周期”初始化一条完整注册。
        /// 适用于类型映射（如接口 -> 实现类），且由容器负责实例化。
        /// </summary>
        /// <param name="fromType">服务抽象类型。</param>
        /// <param name="toType">实现类型（容器需要实例化时使用）。</param>
        /// <param name="lifetime">生命周期（单例 / 瞬态）。</param>
        public DependencyDescriptor(Type fromType, Type toType, Lifetime lifetime)
        {
            this.FromType = fromType;
            this.Lifetime = lifetime;
            this.ToType = toType;
        }

        /// <summary>
        /// 以“抽象类型”初始化一条最简描述。主要用于反注册、查询等场景的键匹配。
        /// </summary>
        /// <param name="fromType">服务抽象类型。</param>
        public DependencyDescriptor(Type fromType)
        {
            this.FromType = fromType;
        }

        /// <summary>注册的服务抽象类型（如接口、基类）。只读。</summary>
        public Type FromType { get; }

        /// <summary>
        /// 实例化工厂委托。当不为 null 时，解析将优先调用它来创建实例，
        /// 从而完全覆盖 ToType 的默认构造流程（适合需要复杂初始化逻辑的场景）。
        /// </summary>
        public Func<IResolver, object> ImplementationFactory { get; set; }

        /// <summary>生命周期：Singleton（单例）或 Transient（瞬态）。只读。</summary>
        public Lifetime Lifetime { get; }

        /// <summary>
        /// 解析完成后的回调。
        /// <para>说明：已直接注册的实例不会触发；单例经由 Create 创建时仅触发一次；瞬态每次解析都触发。</para>
        /// </summary>
        public Action<object> OnResolved { get; set; }

        /// <summary>
        /// 单例缓存的实例。Singleton 首次解析成功后由 Container 写入，之后直接复用。
        /// 由 Container 在 lock 内赋值，保证线程安全。瞬态不使用此字段。
        /// </summary>
        public object ToInstance { get; set; }

        /// <summary>实现类型（容器需要实例化时使用的类型）。只读。</summary>
        public Type ToType { get; }
    }
}
