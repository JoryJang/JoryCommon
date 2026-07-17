using System;

namespace Jory.IOC
{
    /// <summary>
    /// 提供“是否已注册”的查询能力。
    /// 被 IRegistrator 与 IResolver 共同继承，因此注册器与解析器都能判断某类型是否已登记。
    /// </summary>
    public interface IRegistered
    {
        /// <summary>
        /// 判断指定服务类型（带命名 Key）是否已注册。
        /// </summary>
        /// <param name="fromType">服务抽象类型（如接口、基类）。</param>
        /// <param name="key">命名 Key，用于区分同一类型的多个实现。</param>
        /// <returns>已注册返回 true，否则 false。</returns>
        bool IsRegistered(Type fromType, string key);

        /// <summary>
        /// 判断指定服务类型（默认 Key）是否已注册。
        /// </summary>
        /// <param name="fromType">服务抽象类型。</param>
        /// <returns>已注册返回 true，否则 false。</returns>
        bool IsRegistered(Type fromType);
    }
}
