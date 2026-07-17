# Jory.IOC 依赖注入容器 — 代码解读

> 一个轻量级、基于特性的（Attribute 驱动）.NET 依赖注入（DI / IOC）容器实现。
> 命名空间：`Jory.IOC`

---

## 1. 概述

这套代码实现了一个完整的**控制反转（IOC）容器**，核心能力包括：

- **注册（Register）**：把"服务抽象"（接口/基类）与"实现"建立映射，支持单例（Singleton）与瞬态（Transient）两种生命周期。
- **解析（Resolve）**：根据注册信息自动创建实例，并**递归注入**其依赖（构造函数 / 属性 / 方法三种方式）。
- **Key 命名注册**：同一个服务类型可注册多个不同 `key` 的实现，按需取出。
- **开放泛型**：注册泛型定义（如 `IRepository<>`），解析时自动套用具体类型参数。
- **自定义工厂**：通过 `Func<IResolver, object>` 委托完全掌控实例创建过程。
- **兼容 .NET 标准**：实现 `IServiceProvider`，可被 `GetService(Type)` 调用。

---

## 2. 文件清单与职责

| 文件 | 类型 | 职责 |
|------|------|------|
| `IContainer.cs` | 接口 | 容器总接口，组合 `IResolver` + `IRegistrator` |
| `IRegistrator.cs` | 接口 | 注册器：增删注册、获取描述符、构建解析器 |
| `IResolver.cs` | 接口 | 解析器：按类型/Key 创建实例，兼容 `IServiceProvider` |
| `IRegistered.cs` | 接口 | 基础查询：判断某类型是否已注册 |
| `Container.cs` | 实现类 | 核心容器（`sealed`），注册表 + 解析 + 实例创建逻辑 |
| `ContainerExtension.cs` | 静态扩展类 | 大量泛型友好扩展方法（注册/解析/判断），降低调用成本 |
| `DependencyDescriptor.cs` | 数据类 | 一条注册记录的载体（FromType、ToType、生命周期、工厂、实例等） |
| `DependencyInjectAttribute.cs` | 特性/枚举 | `[DependencyInject]`、`[DependencyType]` 特性 + `DependencyType`/`Lifetime` 枚举 |

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

## 3. 核心概念

### 3.1 生命周期（Lifetime）

定义于 `DependencyInjectAttribute.cs`：

```csharp
public enum Lifetime
{
    Singleton,  // 单例：容器内唯一实例（线程安全双重检查锁）
    Transient   // 瞬态：每次 Resolve 都新建
}
```

- **Singleton**：首次解析时创建并缓存到 `DependencyDescriptor.ToInstance`；之后直接返回。并发创建由 `lock (descriptor)` 保护。
- **Transient**：每次解析都调用 `Create(...)` 走构造流程新建实例。

### 3.2 Key 命名注册

注册表内部是 `ConcurrentDictionary<string, DependencyDescriptor>`，键的格式为：

```
$"{fromType.FullName}{key}"     // 例如 "Jory.IDemo.Demo" 或 "Jory.IDemo.Demo#mysql"
```

- 不传 `key` 时即按 `FullName` 注册（默认实现）。
- 同一 `FromType` 可用不同 `key` 注册多个实现，解析时指定 `key` 精确取出。

### 3.3 注入方式（Constructor / Property / Method）

通过 `[DependencyInject]` 标记目标。**优先级与规则**：

1. **构造函数**：优先选择被 `[DependencyInject]` 标记的构造函数；若没有标记，则自动选择**参数最多**的公共构造函数。
2. **属性**：所有被 `[DependencyInject]` 标记且可写（`CanWrite`）的属性，在实例创建后赋值。
3. **方法**：所有被 `[DependencyInject]` 标记的方法，在实例创建后调用（参数同样按依赖解析）。
4. **参数简化**：`[DependencyInject]` 也可直接标在构造函数/方法参数上，用于指定注入的类型与 Key。

基础类型（`IsPrimitive`）与 `string` **不**作为依赖注入，取其默认值（`default` 或参数默认值）。

### 3.4 类型筛选优化 `[DependencyType]`

```csharp
[Flags]
public enum DependencyType { Constructor = 0, Property = 1, Method = 2 }

[DependencyType(DependencyType.Constructor)]  // 只做构造函数注入，跳过属性/方法扫描
class MyService { ... }
```

标记在类上可跳过不必要的属性/方法反射扫描，提升性能。

---

## 4. 数据结构：DependencyDescriptor

每条注册的核心载体：

| 成员 | 说明 |
|------|------|
| `Type FromType` | 注册的服务抽象类型（如接口） |
| `Type ToType` | 实际实现类型 |
| `Lifetime Lifetime` | 生命周期 |
| `object ToInstance` | 单例实例缓存（仅 Singleton 使用） |
| `Func<IResolver, object> ImplementationFactory` | 自定义工厂委托（存在时优先于 ToType 创建） |
| `Action<object> OnResolved` | 解析完成回调（单例仅触发一次，瞬态每次触发；已注册单例实例不触发） |

---

## 5. 注册 API（ContainerExtension 扩展方法）

扩展方法挂在 `IRegistrator` 上，全部返回 `IRegistrator`（可链式调用）。

### 5.1 RegisterSingleton（单例）

```csharp
// 自映射（TFrom -> TFrom）
container.RegisterSingleton<MyService>();
container.RegisterSingleton<MyService>("keyA");

// 接口 -> 实现
container.RegisterSingleton<IService, ServiceImpl>();
container.RegisterSingleton<IService, ServiceImpl>("keyA");

// 基于运行时类型 / 实例
container.RegisterSingleton<IService>(existingInstance);
container.RegisterSingleton(myInstance);                 // 用实例实际类型注册
container.RegisterSingleton(typeof(IService), typeof(ServiceImpl));

// 自定义工厂
container.RegisterSingleton<IService>(r => new ServiceImpl(r.Resolve<IOther>()));
container.RegisterSingleton<IService>(r => new ServiceImpl(), "keyA");
```

### 5.2 RegisterTransient（瞬态）

签名与 Singleton 完全对称，每次解析新建：

```csharp
container.RegisterTransient<IService, ServiceImpl>();
container.RegisterTransient<IService, ServiceImpl>("keyA");
container.RegisterTransient<MyService>();
container.RegisterTransient<IService>(r => new ServiceImpl());
container.RegisterTransient(typeof(IService), typeof(ServiceImpl));
```

### 5.3 Unregister（移除）

```csharp
container.Unregister<IService>();
container.Unregister<IService>("keyA");
container.Unregister(typeof(IService));
container.Unregister(typeof(IService), "keyA");
```

---

## 6. 解析 API（ContainerExtension 扩展方法）

扩展方法挂在 `IResolver`（容器本身即解析器）上。

```csharp
// 泛型解析
var svc = container.Resolve<IService>();
var svcA = container.Resolve<IService>("keyA");

// 反射式解析
object obj = container.Resolve(typeof(IService));
object objA = container.Resolve(typeof(IService), "keyA");

// 尝试解析（未注册返回 default / null，不抛异常）
var maybe = container.TryResolve<IService>();
var maybeA = container.TryResolve<IService>("keyA");

// 解析未注册的根类型（其成员已注册时可用，按构造函数/属性/方法注入）
var root = container.ResolveWithoutRoot<UnregisteredRoot>();

// 判断是否已注册
bool ok = container.IsRegistered<IService>();
bool okA = container.IsRegistered<IService>("keyA");
```

---

## 7. 特性与枚举速查

| 特性/枚举 | 位置 | 作用 |
|-----------|------|------|
| `[DependencyInject]` | 类/构造/属性/方法/参数 | 标记需要注入的目标；支持 `Type` 与 `Key` 指定 |
| `[DependencyType(DependencyType)]` | 类 | 限定注入扫描范围（构造/属性/方法），优化性能 |
| `DependencyType`（`[Flags]`） | — | `Constructor=0`, `Property=1`, `Method=2` |
| `Lifetime` | — | `Singleton`, `Transient` |

`[DependencyInject]` 常用构造：

```csharp
[DependencyInject]                       // 默认
[DependencyInject("keyA")]               // 按 Key 注入
[DependencyInject(typeof(ServiceImpl))]  // 指定具体类型
[DependencyInject(typeof(ServiceImpl), "keyA")]
```

---

## 8. 完整用法示例

### 8.1 基础接口 + 实现 + 注入

```csharp
using Jory.IOC;

public interface ILog { void Write(string msg); }
public class ConsoleLog : ILog { public void Write(string msg) => Console.WriteLine(msg); }

public interface IService { void Run(); }

public class MyService : IService
{
    private readonly ILog _log;

    // 构造函数注入（无标记时自动选参数最多的构造函数）
    public MyService(ILog log) { _log = log; }

    // 属性注入
    [DependencyInject] public ILog ExtraLog { get; set; }

    // 方法注入
    [DependencyInject]
    public void Init(ILog initLog) => initLog.Write("MyService initialized");

    public void Run() => _log.Write("run");
}

// —— 使用 ——
var container = new Container();
container
    .RegisterSingleton<ILog, ConsoleLog>()        // 单例日志
    .RegisterTransient<IService, MyService>();    // 瞬态业务服务

var svc = container.Resolve<IService>();
svc.Run();
```

### 8.2 Key 命名多实现

```csharp
public class MySqlLog : ILog { /* ... */ }
public class FileLog  : ILog { /* ... */ }

container
    .RegisterSingleton<ILog, ConsoleLog>("console")
    .RegisterSingleton<ILog, MySqlLog>("mysql")
    .RegisterSingleton<ILog, FileLog>("file");

var mysqlLog = container.Resolve<ILog>("mysql");
```

### 8.3 开放泛型

```csharp
public interface IRepository<T> { }
public class Repository<T> : IRepository<T> { }

container.RegisterTransient(typeof(IRepository<>), typeof(Repository<>));

var repo = container.Resolve<IRepository<User>>();  // 自动套用 User
```

### 8.4 自定义工厂 + 解析回调

```csharp
container.RegisterSingleton<IService>(r =>
{
    var log = r.Resolve<ILog>();
    return new MyService(log);
});

// 解析完成后触发（瞬态每次触发，单例仅一次）
var descriptor = new DependencyDescriptor(typeof(IService), typeof(MyService), Lifetime.Transient)
{
    OnResolved = obj => Console.WriteLine("resolved: " + obj.GetType().Name)
};
container.Register(descriptor);
```

---

## 9. 关键实现细节（Container.cs）

1. **注册表线程安全**：使用 `ConcurrentDictionary`，`AddOrUpdate` 覆盖式更新（重复注册后者生效）。
2. **解析入口统一 + 循环依赖检测**：两个公共 `Resolve` 均委托给 `ResolveCore`，进入时把当前类型压入“每线程独立的解析栈”（`ThreadLocal<Stack<Type>>`），递归解析发现类型已在栈中即抛出带完整类型链的 `InvalidOperationException`；`finally` 中出栈，保证异常时栈不残留。
3. **单例并发创建**：`lock (descriptor)` + 双重检查保证唯一实例；**单例工厂**结果同样在锁内缓存（修复了旧版“单例工厂每次都新建”的隐蔽问题）。
4. **反射缓存**：`ConcurrentDictionary<Type, InjectionInfo>` 缓存每个实现类型的“选中构造函数 / 需注入属性 / 需注入方法 / `[DependencyType]`”，瞬态高频创建不再每次反射，性能显著提升。
5. **构造函数选择策略**：优先取被 `[DependencyInject]` 标记的构造函数；否则取参数最多的公共构造函数；无任何公共构造函数则抛 `InvalidOperationException`（含类型名）。
6. **泛型解析**：闭合泛型先转开放泛型定义去注册表匹配，命中后用 `MakeGenericType(...)` 还原具体类型再构造。
7. **未注册类型**：基础类型 / `string` 返回 `null`；其它类型抛出含类型全名的 `InvalidOperationException`（提示先注册或用 `ResolveWithoutRoot`）。
8. **`OnResolved` 触发时机**：`Create(...)` 末尾调用；已直接注册的单例实例（`ToInstance`）不走 `Create`，不触发。
9. **`IDisposable` 管理**：`Container` 实现 `IDisposable`，自动登记其创建且可释放的实例（单例与瞬态），`Dispose()` 时统一释放；用户经 `RegisterSingleton(instance)` 传入的实例不纳入释放（由用户管理）。
10. **自身可解析**：构造时注册 `IResolver` / `IServiceProvider` 为单例。
11. **`GetService` 契约**：兼容 `IServiceProvider`，未注册类型返回 `null`；已注册但构造失败仍冒泡异常。

---

## 10. 注意事项与局限

- **属性/方法注入顺序**：属性与方法注入在构造函数执行**之后**；同类别内按反射返回顺序，不保证特定先后。
- **单例写可见性**：`ToInstance` 在 `lock` 内赋值，实践中安全；极高并发如需绝对可见性可加 `Volatile`（当前未做，因 `lock` 已提供 happens-before 保证）。
- **`ResolveWithoutRoot` 未共享缓存**：该方法在 `ContainerExtension` 中复制了构造逻辑，未复用 `Container` 的反射缓存，高频调用时反射开销略高（功能正确）。
- **多实现集合解析**：暂不支持 `Resolve<IEnumerable<IService>>` 一次取出同一类型的多个注册（注册表为单值映射，多实现会被覆盖）。如需支持需将注册表改为 `List<DependencyDescriptor>`。
- **程序集批量扫描注册**：暂未提供自动扫描程序集批量注册的能力（如 `RegisterAssemblyTypes`）。
- **循环依赖处理**：检测到循环时抛出 `InvalidOperationException`（含完整类型链），需在设计中避免构造期相互依赖。

---

## 11. 一句话总结

`Container` 负责存储与创建，`ContainerExtension` 负责把常用操作包装成易用的泛型方法，`DependencyDescriptor` 承载每条注册的全部元信息，`[DependencyInject]` / `[DependencyType]` 驱动"在哪注入、注入什么"，三者配合实现了一个麻雀虽小、五脏俱全的特性化 IOC 容器。

---

## 12. 本轮优化与增添（FW4.5 兼容改造）

> 目标框架 .NET Framework 4.5，并尽量兼容 C# 5 编译器：已移除 `nameof` 与字符串插值 `$"..."`，改用字符串字面量 / `string.Format`；`is null` 改为 `== null`；无类型 `default` 字面量改为 `null`。

### 12.1 已修复的问题（Bug Fix）

| 问题 | 原实现 | 新实现 |
|------|--------|--------|
| **`DependencyType` 枚举 Flags 失效** | `Constructor = 0` 导致任意值 `HasFlag(Constructor)` 恒为 `true`，即 `[DependencyType(Property)]` 仍会执行构造注入 | 改为 `None=0 / Constructor=1 / Property=2 / Method=4`，判断逻辑正确 |
| **单例工厂不缓存** | `RegisterSingleton(func)` 每次解析都调用工厂，单例不“单” | 单例工厂结果在 `lock` 内写入 `ToInstance`，真正唯一 |
| **异常信息缺失** | 未注册 / 无构造函数时 `throw new Exception()`（无类型信息） | 改为 `InvalidOperationException`，消息含类型全名与完整循环链 |
| **循环依赖无限递归** | 构造期循环依赖会栈溢出 | `ThreadLocal<Stack<Type>>` 检测并抛清晰异常 |
| **`IsRegistered` 不支持泛型开放定义** | 闭合泛型查不到开放泛型注册 | 自动转 `GetGenericTypeDefinition()` 后匹配 |
| **`GetService` 契约不符** | 未注册类型抛异常（违反 `IServiceProvider` 约定） | 未注册返回 `null`，已注册构造失败仍冒泡 |
| **`ResolveWithoutRoot` 冗余代码** | 残留永远为 `null` 的 `ops` 分支 | 已清理 |

### 12.2 已增添的能力（Enhancement）

- **反射缓存**：`ConcurrentDictionary<Type, InjectionInfo>` 缓存构造函数选择、需注入属性/方法，瞬态高频创建不再每次反射。
- **`IDisposable` 生命周期管理**：`Container` 实现 `IDisposable`，统一释放其创建的可释放实例（单例 + 瞬态），用户显式传入的单例实例不纳入。
- **单例并发更严谨**：双重检查锁内统一处理“工厂 / 类型构造 / 泛型还原”三种路径。

### 12.3 仍可进一步做的事（建议，未实现）

1. **多实现集合解析**：`Resolve<IEnumerable<IService>>` 一次取出同类型所有注册 → 需把注册表由单值改为 `List<DependencyDescriptor>`，并新增 `Register` 的“追加”语义。
2. **程序集批量扫描**：`RegisterAssemblyTypes(Assembly, predicate)` 按约定/接口自动注册。
3. **单例可见性**：用 `Volatile.Write/Read` 显式保证 `ToInstance` 的跨线程可见性（当前依赖 `lock` 已足够）。
4. **`ResolveWithoutRoot` 复用缓存**：将其逻辑迁到 `Container` 内部，共享 `InjectionInfo` 缓存。
5. **拦截 / AOP**：在 `Create` 中支持代理包装（如接口拦截），实现横切能力。
6. **开放式终结器警告**：`Dispose` 已 `try/catch` 单个释放异常，必要时可记录日志而非静默忽略。
