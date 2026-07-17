using System;
using System.Collections.Generic;

namespace Jory.IOC
{
    /// <summary>
    /// 容器注册器接口：负责把“服务抽象”与“实现”建立映射关系，并提供反注册与查询能力。
    /// 继承 IRegistered，因此注册器也具备“是否已注册”的查询能力。
    /// </summary>
    public interface IRegistrator : IRegistered
    {
        /// <summary>
        /// 使用命名 Key 注册一条类型描述符（DependencyDescriptor）。
        /// 若 Key 已存在则覆盖。
        /// </summary>
        /// <param name="descriptor">依赖描述符。</param>
        /// <param name="key">命名 Key。</param>
        void Register(DependencyDescriptor descriptor, string key);

        /// <summary>
        /// 注册一条类型描述符（DependencyDescriptor，使用默认 Key）。
        /// 若已存在则覆盖。
        /// </summary>
        /// <param name="descriptor">依赖描述符。</param>
        void Register(DependencyDescriptor descriptor);

        /// <summary>
        /// 使用命名 Key 移除一条注册信息。
        /// </summary>
        /// <param name="descriptor">依赖描述符（用于提取 FromType 作为键）。</param>
        /// <param name="key">命名 Key。</param>
        void Unregister(DependencyDescriptor descriptor, string key);

        /// <summary>
        /// 移除一条注册信息（默认 Key）。
        /// </summary>
        /// <param name="descriptor">依赖描述符（用于提取 FromType 作为键）。</param>
        void Unregister(DependencyDescriptor descriptor);

        /// <summary>
        /// 获取所有已注册的类型描述符，用于调试、检视容器内容。
        /// </summary>
        /// <returns>已注册的描述符集合。</returns>
        IEnumerable<DependencyDescriptor> GetDescriptors();

        /// <summary>
        /// 构建一个“服务解析器”。本容器自身即实现 IResolver，因此直接返回 this。
        /// </summary>
        /// <returns>解析器实例。</returns>
        IResolver BuildResolver();
    }
}
