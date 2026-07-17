using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UEM.IOC
{
    /// <summary>
    /// 服务解析器接口：根据已注册的映射关系创建（解析）出服务实例，并自动完成依赖注入。
    /// 继承 IServiceProvider（.NET 标准服务定位接口）与 IRegistered（查询能力），
    /// 因此可无缝接入任何依赖 IServiceProvider 的框架代码（如 GetService(Type)）。
    /// </summary>
    public interface IResolver : IServiceProvider, IRegistered
    {
        /// <summary>
        /// 按命名 Key 解析目标类型的实例。
        /// </summary>
        /// <param name="fromType">服务抽象类型。</param>
        /// <param name="key">命名 Key。</param>
        /// <returns>解析得到的实例。</returns>
        object Resolve(Type fromType, string key);

        /// <summary>
        /// 按默认 Key 解析目标类型的实例。
        /// </summary>
        /// <param name="fromType">服务抽象类型。</param>
        /// <returns>解析得到的实例。</returns>
        object Resolve(Type fromType);
    }
}
