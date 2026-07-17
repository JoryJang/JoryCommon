using System;

namespace Jory.IOC
{
    /// <summary>
    /// IOC 容器总接口。
    /// 它组合了“解析器（IResolver）”与“注册器（IRegistrator）”两大职责，
    /// 因此一个 IContainer 实例既能注册服务，也能解析服务。
    /// 业务代码通常只需持有 IContainer，即可完成依赖注入的全部操作。
    /// </summary>
    public interface IContainer : IResolver, IRegistrator
    {
        // 本接口本身不声明额外成员，仅作为“容器”的统一抽象。
        // 所有能力均由 IResolver 与 IRegistrator 提供。
    }
}
