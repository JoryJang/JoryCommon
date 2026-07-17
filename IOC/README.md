# Jory.IOC 依赖注入容器 — 使用说明

> 一个轻量级、基于特性（Attribute）驱动的 .NET 依赖注入（IOC/DI）容器。
> 命名空间：`Jory.IOC` ｜ 目标框架：.NET Framework 4.5+ ｜ 线程安全

---

## 1. 这是什么 / 有什么用

`Jory.IOC` 是一个**自研的轻量级 IOC 容器**，用来管理对象的创建与依赖关系，让你不再手动 `new` 并手工传递依赖。它的核心价值：

| 痛点 | 它怎么解决 |
|------|-----------|
| 类与类之间强耦合（直接 new 实现） | 注册“接口 → 实现”映射，运行时由容器解析，解耦调用方与实现 |
| 依赖层层传递，构造函数参数越堆越多 | 容器自动递归注入构造函数 / 属性 / 方法三级依赖 |
| 单例管理混乱、多线程下重复创建 | 内置 Singleton 生命周期，双重检查锁保证唯一实例 |
| 同一接口有多个实现（如多种日志） | 支持 Key 命名注册，按需取出 |
| 泛型仓储 `IRepository<T>` 重复注册 | 支持开放泛型注册，解析时自动套用类型参数 |
| 实例化逻辑复杂（需读配置/连数据库） | 支持自定义工厂委托 `Func<IResolver, object>` |

**适用场景**：WPF/WinForms 桌面应用、小型服务、测试替身组装、任何需要解耦组件的 .NET Framework 项目。它不依赖任何第三方库，单文件级即可集成。

**不适合**：需要 ASP.NET Core 原生 `Microsoft.Extensions.DependencyInjection` 集成、或需要 Scoped 生命周期（每请求一个作用域）的场景——本容器只提供 Singleton / Transient 两种生命周期。

---

## 2. 核心能力一览

- **两种生命周期**：`Singleton`（容器内唯一）/ `Transient`（每次新建）
- **三级注入**：构造函数、属性、方法（用 `[DependencyInject]` 标记）
- **Key 命名注册**：同一接口注册多个实现，按 Key 区分
- **开放泛型**：注册 `IRepository<>`，解析 `IRepository<User>` 自动还原
- **自定义工厂**：`Func<IResolver, object>` 完全掌控实例化
- **循环依赖检测**：自动抛出带完整类型链的异常
- **反射缓存**：构造函数选择 / 注入属性 / 注入方法只反射一次
- **IDisposable 自动回收**：容器释放时统一 Dispose 其创建的实例
- **兼容 IServiceProvider**：可接入任何 `GetService(Type)` 的框架代码

---

## 3. 快速开始（3 分钟上手）

### 第 1 步：引用命名空间

```csharp
using Jory.IOC;
```

### 第 2 步：定义服务与实现

```csharp
public interface ILog { void Write(string msg); }

public class ConsoleLog : ILog
{
    public void Write(string msg) { Console.WriteLine(msg); }
}

public interface IService { void Run(); }

public class MyService : IService
{
    private readonly ILog _log;

    // 构造函数注入（无标记时自动选参数最多的公共构造函数）
    public MyService(ILog log) { _log = log; }

    public void Run() { _log.Write("running..."); }
}
```

### 第 3 步：注册 + 解析

```csharp
var container = new Container();
container
    .RegisterSingleton<ILog, ConsoleLog>()        // 单例日志
    .RegisterTransient<IService, MyService>();    // 瞬态业务服务

var svc = container.Resolve<IService>();
svc.Run();   // 输出: running...
```

就这么简单。下面是完整的 API 速查。

---

## 4. API 速查

### 4.1 注册（Register）

所有注册方法挂在 `IRegistrator` 上（`Container` 即实现），返回 `IRegistrator` 可链式调用。

#### 单例 `RegisterSingleton`

```csharp
// 接口 -> 实现（容器创建并缓存）
container.RegisterSingleton<ILog, ConsoleLog>();
container.RegisterSingleton<ILog, ConsoleLog>("keyA");          // 带 Key

// 自映射（TFrom -> TFrom）
container.RegisterSingleton<MyService>();
container.RegisterSingleton<MyService>("keyA");

// 已有实例直接托管为单例
container.RegisterSingleton<ILog>(existingInstance);
container.RegisterSingleton<ILog>(existingInstance, "keyA");
container.RegisterSingleton(myInstance);                        // 用实例实际类型注册

// 运行时 Type
container.RegisterSingleton(typeof(ILog), typeof(ConsoleLog));
container.RegisterSingleton(typeof(ILog), typeof(ConsoleLog), "keyA");

// 自定义工厂（单例：工厂只调用一次，结果被缓存）
container.RegisterSingleton<ILog>(r => new ConsoleLog());
container.RegisterSingleton<ILog>(r => new ConsoleLog(), "keyA");
container.RegisterSingleton(typeof(ILog), r => new ConsoleLog());
```

#### 瞬态 `RegisterTransient`（每次解析新建）

签名与 Singleton 完全对称：

```csharp
container.RegisterTransient<IService, MyService>();
container.RegisterTransient<IService, MyService>("keyA");
container.RegisterTransient<MyService>();
container.RegisterTransient<ILog>(r => new ConsoleLog());
container.RegisterTransient(typeof(IService), typeof(MyService));
```

#### 开放泛型

```csharp
public interface IRepository<T> { }
public class Repository<T> : IRepository<T> { }

// 注册开放泛型定义
container.RegisterTransient(typeof(IRepository<>), typeof(Repository<>));

// 解析时自动套用类型参数
var userRepo = container.Resolve<IRepository<User>>();
```

#### 移除注册 `Unregister`

```csharp
container.Unregister<IService>();
container.Unregister<IService>("keyA");
container.Unregister(typeof(IService));
container.Unregister(typeof(IService), "keyA");
```

#### 直接使用描述符（高级）

```csharp
var descriptor = new DependencyDescriptor(typeof(IService), typeof(MyService), Lifetime.Transient)
{
    OnResolved = obj => Console.WriteLine("resolved: " + obj.GetType().Name)
};
container.Register(descriptor);
```

### 4.2 解析（Resolve）

```csharp
// 泛型解析
var svc  = container.Resolve<IService>();
var svcA = container.Resolve<IService>("keyA");

// 反射式解析
object obj  = container.Resolve(typeof(IService));
object objA = container.Resolve(typeof(IService), "keyA");

// 尝试解析（未注册返回 default/null，不抛异常）
var maybe  = container.TryResolve<IService>();
var maybeA = container.TryResolve<IService>("keyA");

// 解析未注册的根类型（本身不注册，但成员依赖已注册；自动三级注入）
var root = container.ResolveWithoutRoot<UnregisteredRoot>();

// 判断是否已注册
bool ok  = container.IsRegistered<IService>();
bool okA = container.IsRegistered<IService>("keyA");
```

### 4.3 特性（Attribute）

| 特性 | 标记位置 | 作用 |
|------|---------|------|
| `[DependencyInject]` | 类 / 构造函数 / 属性 / 方法 / 参数 | 标记需要注入的目标；可用 `Type` 与 `Key` 精确指定 |
| `[DependencyType(...)]` | 类 | 限定只做哪些注入方式（跳过不必要的反射，提升性能） |

`[DependencyInject]` 的常用形式：

```csharp
[DependencyInject]                          // 按成员声明类型自动解析
[DependencyInject("mysql")]                // 按命名 Key 解析
[DependencyInject(typeof(ImplA))]          // 指定具体类型
[DependencyInject(typeof(ImplA), "k")]     // 类型 + Key
```

`[DependencyType]` 限定注入范围（位标志枚举）：

```csharp
[Flags]
public enum DependencyType { None = 0, Constructor = 1, Property = 2, Method = 4 }

[DependencyType(DependencyType.Constructor)]                          // 只构造注入
[DependencyType(DependencyType.Constructor | DependencyType.Property)] // 构造 + 属性
```

> ⚠️ 枚举值设计为 `None=0 / Constructor=1 / Property=2 / Method=4`（2 的幂）。早期版本曾把 `Constructor=0`，导致 `HasFlag(Constructor)` 恒为 true，已修复。

---

## 5. 完整用法示例

### 5.1 三级注入（构造 + 属性 + 方法）

```csharp
public class MyService : IService
{
    private readonly ILog _log;

    public MyService(ILog log) { _log = log; }   // 构造注入

    [DependencyInject] public ILog ExtraLog { get; set; }   // 属性注入

    [DependencyInject]
    public void Init(ILog initLog)                         // 方法注入
    {
        initLog.Write("MyService initialized");
    }

    public void Run() { _log.Write("run"); }
}

var container = new Container();
container.RegisterSingleton<ILog, ConsoleLog>();
container.RegisterTransient<IService, MyService>();

var svc = container.Resolve<IService>();
svc.Run();
```

### 5.2 Key 命名多实现

```csharp
public class MySqlLog : ILog { /* ... */ }
public class FileLog  : ILog { /* ... */ }

container
    .RegisterSingleton<ILog, ConsoleLog>("console")
    .RegisterSingleton<ILog, MySqlLog>("mysql")
    .RegisterSingleton<ILog, FileLog>("file");

var mysqlLog = container.Resolve<ILog>("mysql");
```

### 5.3 自定义工厂 + 解析回调

```csharp
// 工厂单例：工厂只调用一次，结果被缓存
container.RegisterSingleton<IService>(r =>
{
    var log = r.Resolve<ILog>();
    return new MyService(log);
});

// 带 OnResolved 回调的描述符（瞬态每次触发，单例仅一次）
var descriptor = new DependencyDescriptor(typeof(IService), typeof(MyService), Lifetime.Transient)
{
    OnResolved = obj => Console.WriteLine("resolved: " + obj.GetType().Name)
};
container.Register(descriptor);
```

### 5.4 把容器本身作为依赖注入

容器构造时已把自己注册为 `IResolver` / `IServiceProvider` 单例，可在需要“动态解析”的服务里注入：

```csharp
public class LazyResolver
{
    private readonly IResolver _resolver;
    public LazyResolver(IResolver resolver) { _resolver = resolver; }

    public void DoSomething(string key)
    {
        // 运行时按 Key 动态取出
        var log = _resolver.Resolve<ILog>(key);
        log.Write("...");
    }
}
```

### 5.5 释放瞬态实例（本次新增能力）

```csharp
var container = new Container();
container.RegisterTransient<IDisposable, MyResource>();

var r1 = container.Resolve<IDisposable>();
// ... 使用 r1 ...
container.Release(r1);   // 显式释放（从跟踪集合移除并 Dispose）

container.Dispose();     // 释放所有仍被跟踪的实例
```

---

## 6. 生命周期与释放说明（重要）

### 6.1 两种生命周期

| 生命周期 | 行为 | 线程安全 | 容器是否跟踪释放 |
|---------|------|---------|----------------|
| `Singleton` | 容器内唯一实例，首次解析创建并缓存 | 双重检查锁保证 | ✅ 始终跟踪 |
| `Transient` | 每次解析新建 | 每次独立 | ⚠️ 受 `trackTransients` 控制 |

### 6.2 `trackTransients` 开关与 `Release` 方法

> 本次优化新增。详见第 8 节“本次优化变更”。

```csharp
// 默认：不跟踪瞬态（推荐，符合主流 DI 容器约定，避免内存泄漏）
var container = new Container();

// 启用旧行为：容器自动跟踪并释放瞬态（注意长时间运行可能累积内存）
var container = new Container(trackTransients: true);
```

- 默认 `trackTransients=false`：瞬态实例由调用方自行管理（用完自行 Dispose，或交给 GC）。
- 启用 `trackTransients=true`：瞬态实例也会被登记，容器 `Dispose()` 时统一释放——但长期运行的服务会因瞬态无限累积导致内存增长，此时应配合 `container.Release(instance)` 主动回收。

### 6.3 容器自身的释放

```csharp
using (var container = new Container())
{
    container.RegisterSingleton<ILog, ConsoleLog>();
    // ... 使用 ...
}  // 离开 using 自动 Dispose：释放所有由容器创建且被跟踪的 IDisposable 实例
```

注意：用户通过 `RegisterSingleton(instance)` 显式传入的实例**不**由容器释放（由用户管理其生命周期）。

---

## 7. 注意事项与局限

| 项 | 说明 |
|----|------|
| **属性/方法注入顺序** | 在构造函数之后；同类别内按反射返回顺序，不保证特定先后 |
| **多实现集合解析** | 暂不支持 `Resolve<IEnumerable<IService>>()` 一次取出同类型所有注册（注册表为单值映射，多实现会覆盖） |
| **程序集批量扫描** | 暂未提供 `RegisterAssemblyTypes(...)` 自动扫描注册 |
| **循环依赖** | 检测到即抛 `InvalidOperationException`（含完整类型链 A -> B -> A），需在设计上避免构造期相互依赖 |
| **Scoped 生命周期** | 不支持（仅 Singleton / Transient） |
| **基础类型注入** | `int`/`double`/`bool`/`string` 等不作为依赖注入，取参数默认值或 `null` |
| **构造函数选择** | 优先选 `[DependencyInject]` 标记的；否则选参数最多的公共构造函数 |

---

## 8. 本次优化变更说明（相对原版）

### 8.1 修复的 Bug

| # | 问题 | 影响 | 修复 |
|---|------|------|------|
| 1 | **工厂注册完全不工作（严重）** | `RegisterSingleton<T>(func)` / `RegisterTransient<T>(func)` 调用 `new DependencyDescriptor(typeof(T), Lifetime.Singleton)`，因无 `(Type, Lifetime)` 构造函数，被错误匹配到 `(Type, object)` 重载，把 `Lifetime` 枚举值装箱当作单例实例——**工厂委托永远不会执行，解析直接返回一个枚举值** | 新增 `DependencyDescriptor(Type, Lifetime)` 构造函数，工厂场景 ToType 设为 fromType 占位 |
| 2 | **`ResolveWithoutRoot` 只做构造注入** | 扩展方法 `ResolveWithoutRoot` 注释声称“构造/属性/方法注入”，实际只做了构造函数注入，属性与方法注入被遗漏 | Container 新增同名实例方法，复用 `InstantiateAndInject` 完整三级注入；扩展方法转发，fallback 也补齐 |
| 3 | **`ResolveWithoutRoot` 不共享缓存、不做循环检测** | 每次调用重复反射；且无法检测根类型参与的循环依赖 | 实例方法复用 `m_injectionCache` 与线程解析栈 |
| 4 | **瞬态实例内存泄漏** | 所有瞬态实例被无条件登记到 `m_disposables`，长期运行无限累积 | 新增 `trackTransients` 构造参数（默认 false）+ `Release(instance)` 方法 |
| 5 | **覆盖注册时旧单例 disposable 未清理** | `Register` 覆盖或 `Unregister` 后，旧单例实例仍残留在释放集合，可能被双重释放或泄漏 | `Register`/`Unregister` 覆盖时调用 `UntrackDisposable` 摘除旧实例 |
| 6 | **容器释放后无防护** | `Dispose()` 后再调用 `Resolve`/`Register` 行为未定义 | 新增 `m_disposed` 标志 + `ThrowIfDisposed()`，释放后抛 `ObjectDisposedException` |
| 7 | **`DependencyDescriptor` 缺参数校验** | `fromType`/`instance`/`toType` 为 null 时不抛异常，后续 NRE 难排查 | 三个构造函数均加 `ArgumentNullException` 校验 |
| 8 | **`GetDescriptors` 枚举期间变更风险** | 直接返回 `Values`，外部枚举时若并发注册会抛异常 | 改为 `ToArray()` 返回快照 |

### 8.2 新增的能力

- **`Container.ResolveWithoutRoot(Type)`** 实例方法：复用反射缓存 + 三级注入 + 循环检测
- **`Container.Release(object)`** 实例方法 + `IResolver.Release(object)` 扩展：显式释放被跟踪的瞬态实例
- **`Container(bool trackTransients)`** 构造重载：控制是否跟踪瞬态
- **`InstantiateAndInject`** 内部方法：统一三级注入逻辑，`Create` 与 `ResolveWithoutRoot` 共用

### 8.3 不变项（向后兼容）

- 所有原有 public 方法签名保持不变
- `new Container()` 默认行为：仅瞬态跟踪策略由“跟踪”改为“不跟踪”（修复泄漏），如需旧行为用 `new Container(true)`
- 原有 `IOC代码解读.md` 保留，作为实现层面的深度解读参考

---

## 9. 文件清单

| 文件 | 类型 | 职责 |
|------|------|------|
| `IContainer.cs` | 接口 | 容器总接口，组合 `IResolver` + `IRegistrator` |
| `IRegistrator.cs` | 接口 | 注册器：增删注册、获取描述符、构建解析器 |
| `IResolver.cs` | 接口 | 解析器：按类型/Key 创建实例，兼容 `IServiceProvider` |
| `IRegistered.cs` | 接口 | 基础查询：判断某类型是否已注册 |
| `Container.cs` | 实现类 | 核心容器（`sealed`），注册表 + 解析 + 实例创建 + 释放管理 |
| `ContainerExtension.cs` | 静态扩展类 | 大量泛型友好扩展方法（注册/解析/释放/判断），降低调用成本 |
| `DependencyDescriptor.cs` | 数据类 | 一条注册记录的载体（FromType、ToType、生命周期、工厂、实例、回调） |
| `DependencyInjectAttribute.cs` | 特性/枚举 | `[DependencyInject]`、`[DependencyType]` 特性 + `DependencyType`/`Lifetime` 枚举 |
| `README.md` | 文档 | 本使用说明 |
| `IOC代码解读.md` | 文档 | 实现层面的深度解读 |

**接口依赖关系**

```
IRegistered                (IsRegistered 查询)
   ├── IRegistrator        (Register / Unregister / GetDescriptors / BuildResolver)
   └── IResolver : IServiceProvider
                       (Resolve + IsRegistered)

IContainer : IResolver, IRegistrator   (总接口)
Container : IContainer                 (唯一实现)
```

---

## 10. 一句话总结

`Container` 负责存储与创建，`ContainerExtension` 把常用操作包装成易用的泛型方法，`DependencyDescriptor` 承载每条注册的全部元信息，`[DependencyInject]` / `[DependencyType]` 驱动“在哪注入、注入什么”——四者配合实现了一个麻雀虽小、五脏俱全的特性化 IOC 容器。

> 更多实现细节请参阅同目录下的 `IOC代码解读.md`。
