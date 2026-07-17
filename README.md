# JoryCommonLibrary

一个面向 **.NET Framework 4.5** 的 C# 通用工具库，涵盖依赖注入容器、日志封装、文件/目录操作、字节与大小端转换、MVVM 基类、WPF 值转换器、INI/XML 读写、枚举扩展、字符串/字节工具等，适用于桌面（WPF/WinForms）与控制台程序。

> 仓库地址：https://github.com/JoryJang/JoryCommon

---

## 目录

- [环境要求](#环境要求)
- [获取与引用](#获取与引用)
- [目录结构](#目录结构)
- [命名空间说明](#命名空间说明)
- [模块用法](#模块用法)
  - [1. IOC 依赖注入容器（UEM.IOC）](#1-ioc-依赖注入容器uemioc)
  - [2. 日志封装 NLogger（UEM.Log）](#2-日志封装-nloggeruemlog)
  - [3. 文件与目录操作（Jory.Common）](#3-文件与目录操作jorycommon)
  - [4. 字节与大小端转换 SocketBitConverter（Jory.Common）](#4-字节与大小端转换-socketbitconverterjorycommon)
  - [5. MVVM 基类 ViewModelBase / PropertyChangedModel（Jory.Common）](#5-mvvm-基类-viewmodelbase--propertychangedmodeljorycommon)
  - [6. WPF 值转换器（Jory.Common）](#6-wpf-值转换器jorycommon)
  - [7. INI 文件读写 INIFile（Jory.Common）](#7-ini-文件读写-inifilejorycommon)
  - [8. XML 读写 XmlHelper（Jory.Common）](#8-xml-读写-xmlhelperjorycommon)
  - [9. 枚举扩展 EnumExt（Jory.Common）](#9-枚举扩展-enumextjorycommon)
  - [10. 通用函数 Functions（Jory.Common）](#10-通用函数-functionsjorycommon)
  - [11. 其它小工具](#11-其它小工具)
- [构建](#构建)
- [许可证](#许可证)

---

## 环境要求

| 依赖 | 说明 |
|------|------|
| .NET Framework 4.5+ | 项目以 `TargetFrameworkVersion=v4.5` 编译（兼容 Mono / .NET Core 通过兼容包有限使用） |
| Visual Studio 2012+ | 工程为旧式 `.csproj` 格式（MSBuild 15.0） |
| NLog | 依赖的 `NLog.dll` 已内置在仓库 `NLog\NLog.dll`，无需通过 NuGet 安装 |
| WPF 程序集 | `PresentationCore` / `PresentationFramework` / `WindowsBase`（转换器、部分类型需要） |

---

## 获取与引用

```bash
git clone https://github.com/JoryJang/JoryCommon.git
```

引用方式（二选一）：

1. **直接引用工程**：在解决方案中“添加现有项目” `JoryCommonLibrary.csproj`，然后对被引用的工程“添加引用 → 项目 → JoryCommonLibrary”。
2. **引用编译产物**：生成 `Jory.Common.dll` 后，连同 `NLog\NLog.dll` 一起复制到你的输出目录（`csproj` 通过 `HintPath` 引用 `NLog\NLog.dll`，请确保该文件随程序发布）。

---

## 目录结构

```
JoryCommonLibrary/
├── JoryCommonLibrary.csproj      # 工程文件（FW 4.5，输出 Jory.Common.dll）
├── JoryCommonLibrary.slnx        # 解决方案
├── NLog/                         # 内置 NLog.dll 及 NLog 封装
│   ├── NLog.dll
│   └── NLogger.cs
├── IOC/                          # 轻量级特性化依赖注入容器（UEM.IOC）
│   ├── Container.cs / ContainerExtension.cs
│   ├── DependencyDescriptor.cs / DependencyInjectAttribute.cs
│   ├── IContainer.cs / IRegistrator.cs / IResolver.cs / IRegistered.cs
│   └── IOC代码解读.md            # 容器完整实现文档（建议深入阅读）
├── Converter/                    # WPF IValueConverter 实现
│   ├── InverseBoolenConverter.cs
│   └── OppositeConverter.cs
├── ConsoleManager.cs             # 控制台窗口显隐
├── DirFile.cs                    # 目录 / 文件操作
├── FileOperate.cs                # 文件读写与文件夹遍历
├── Functions.cs                  # 通用函数（字符串/字节/字节序/UI 线程）
├── EnumExt.cs                    # 枚举扩展
├── INIFile.cs                    # INI 文件读写
├── XmlHelper.cs                  # XML 读写
├── SocketBitConverter.cs         # 大小端字节转换
├── ViewModelBase.cs              # MVVM ViewModel 基类
├── PropertyChangedModel.cs       # 通知属性基类
├── DumpHelper.cs                 # 进程 MiniDump
├── StopwatchHelper.cs            # 代码计时
└── Properties/AssemblyInfo.cs
```

---

## 命名空间说明

本库命名空间尚未完全统一，引用时请注意：

| 命名空间 | 包含模块 |
|----------|----------|
| `Jory.Common` | 除 IOC 与 NLog 封装外的全部模块（文件、字节、MVVM、转换器、INI/XML、枚举、函数等） |
| `UEM.IOC` | IOC 容器（`Container`、`ContainerExtension`、`DependencyDescriptor`、特性与接口） |
| `UEM.Log` | `NLogger` 日志封装 |

---

## 模块用法

### 1. IOC 依赖注入容器（UEM.IOC）

轻量级、特性驱动的依赖注入容器，支持**单例 / 瞬态**生命周期、**构造 / 属性 / 方法**三种注入方式、**Key 命名多实现**、**开放泛型**、**自定义工厂**与 `Func<IResolver,object>` 回调，并兼容 `IServiceProvider`。完整实现解读见仓库内 [`IOC/IOC代码解读.md`](IOC/IOC代码解读.md)。

```csharp
using UEM.IOC;

public interface ILog { void Write(string msg); }
public class ConsoleLog : ILog { public void Write(string msg) => Console.WriteLine(msg); }

public interface IService { void Run(); }
public class MyService : IService
{
    private readonly ILog _log;
    public MyService(ILog log) { _log = log; }               // 构造函数注入
    [DependencyInject] public ILog ExtraLog { get; set; }    // 属性注入
    [DependencyInject] public void Init(ILog log) => log.Write("init"); // 方法注入
    public void Run() => _log.Write("run");
}

// —— 使用 ——
var container = new Container();
container
    .RegisterSingleton<ILog, ConsoleLog>()       // 单例
    .RegisterTransient<IService, MyService>();   // 瞬态

var svc = container.Resolve<IService>();
svc.Run();
```

常用 API：

```csharp
container.RegisterSingleton<IFoo, FooImpl>();
container.RegisterSingleton<IFoo, FooImpl>("keyA");
container.RegisterTransient<IService, MyService>();
container.RegisterTransient<IService>(r => new MyService(r.Resolve<ILog>())); // 工厂
container.Unregister<IFoo>();

var svc   = container.Resolve<IService>();
var svcA  = container.Resolve<IService>("keyA");
var maybe = container.TryResolve<IService>();   // 未注册返回 null，不抛异常
bool ok   = container.IsRegistered<IService>();
container.Dispose();                            // 统一释放其创建的可释放实例
```

> 循环依赖检测：构造期循环依赖会抛出含完整类型链的 `InvalidOperationException`，请在设计中避免。

### 2. 日志封装 NLogger（UEM.Log）

对 NLog 的极简封装。程序启动时调用一次 `Init()`，之后用 `WriteXxx` 写日志；退出前调用 `ShutDown()` 刷新。若程序目录存在 `NLog.config`，则自动采用该配置；否则内置控制台 + 异步文件（按级别分文件，存放于 `Logs/`）输出。

```csharp
using UEM.Log;

NLogger.Init();                       // 可选 fileName 指定日志文件名前缀
try
{
    NLogger.WriteInfo("程序启动");
    NLogger.WriteDebug("调试信息");
    NLogger.WriteWarn("警告");
    NLogger.WriteError("出错了", ex); // ex 为 Exception，可空
    NLogger.WriteFatal("致命错误", ex);
}
finally
{
    NLogger.ShutDown();
}
```

### 3. 文件与目录操作（Jory.Common）

`DirFile` 与 `FileOperate` 提供常用静态方法（功能略有重叠，按习惯选用）。

```csharp
using Jory.Common;

// DirFile
if (!DirFile.IsExistDirectory(@"D:\data")) DirFile.CreateDir(@"D:\data");
DirFile.WriteText(@"D:\data\a.txt", "hello", Encoding.UTF8);
string[] files = DirFile.GetFileNames(@"D:\data", "*.txt", isSearchChild: true);
long size = DirFile.GetFileSize(@"D:\data\a.txt");
DirFile.CopyFile(@"D:\data\a.txt", @"D:\data\b.txt");
DirFile.DeleteFile(@"D:\data\b.txt");

// FileOperate
FileOperate.WriteFile(@"D:\data\c.txt", "内容");
string text = FileOperate.ReadFile(@"D:\data\c.txt");
FileOperate.FileCoppy(@"D:\data\c.txt", @"D:\data\c2.txt");
long dirLen = FileOperate.GetDirectoryLength(@"D:\data");
```

### 4. 字节与大小端转换 SocketBitConverter（Jory.Common）

可在**大端 / 小端**之间转换基元类型与字节数组，自动判断是否与本机字节序一致（`IsSystemEndianMatch`）。内置 `BigEndian`、`LittleEndian`、`Default` 三个静态实例。

```csharp
using Jory.Common;

var conv = SocketBitConverter.BigEndian;     // 或 LittleEndian / Default
byte[] bytes = conv.GetBytes(0x1234);        // ushort -> 大端字节
ushort v = conv.ToUInt16(bytes, 0);

int  i  = conv.ToInt32(conv.GetBytes(123456), 0);
double d = conv.ToDouble(conv.GetBytes(3.14), 0);
bool b  = conv.ToBoolean(conv.GetBytes(true), 0);

// 切换默认端
SocketBitConverter.DefaultEndianType = EndianType.Big;  // Default 随之变为 BigEndian
```

### 5. MVVM 基类 ViewModelBase / PropertyChangedModel（Jory.Common）

用于 WPF/Silverlight 数据绑定的通知属性基类。

- `ViewModelBase`：抽象基类，实现 `INotifyPropertyChanged` + `IDisposable`，通过 `OnPropertyChanged()` 通知（支持 `[CallerMemberName]`）。
- `PropertyChangedModel`：更轻量的基类，提供 `SetProperty<T>(ref field, value)`（值变化时才赋值并通知），以及 `OnPropertyChanged()`。

```csharp
using Jory.Common;

public class MainViewModel : ViewModelBase
{
    private string _name;
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }   // 自动取属性名
    }
}

public class Setting : PropertyChangedModel
{
    private int _count;
    public int Count
    {
        get => _count;
        set => SetProperty(ref _count, value);        // 仅在变化时通知
    }
}
```

### 6. WPF 值转换器（Jory.Common）

实现 `IValueConverter`，可在 XAML 中直接引用。

- `InverseBoolenConverter`：布尔取反（`true<->false`）。
- `OppositeConverter`：对数值/布尔取相反数（按目标类型转换）。

```xml
<Window.Resources>
    <local:InverseBoolenConverter x:Key="InverseBool"/>
    <local:OppositeConverter     x:Key="Opposite"/>
</Window.Resources>

<Button Content="开始" IsEnabled="{Binding IsBusy, Converter={StaticResource InverseBool}}"/>
```

```csharp
var inverse = new InverseBoolenConverter();
bool hidden = (bool)inverse.Convert(true, typeof(bool), null, null); // false
```

### 7. INI 文件读写 INIFile（Jory.Common）

读写标准 Windows INI 文件（基于 Win32 API），支持分区、键、类型化读写（int/bool/double）与字节数组。

```csharp
using Jory.Common;

var ini = new INIFile(@"D:\config.ini");
ini.WriteValue("Server", "IP", "127.0.0.1");
ini.WriteInt("Server", "Port", 8080);
ini.WriteBool("Server", "Enabled", true);

string ip   = ini.ReadValue("Server", "IP", "0.0.0.0");
int    port = ini.ReadInt("Server", "Port", 80);
bool   en   = ini.ReadBool("Server", "Enabled", false);

string[] sections = ini.GetSectionNames();
ini.DeleteKey("Server", "IP");
ini.DeleteAllSections();
```

### 8. XML 读写 XmlHelper（Jory.Common）

基于 XPath 的 XML 读写，支持实例方式与静态方式。

```csharp
using Jory.Common;

var xml = new XmlHelper(@"D:\data.xml");      // 构造时加载/创建文件
xml.AppendNode(...);                           // 追加节点
string val = xml.GetValue("/root/name");       // XPath 取值
string attr = xml.GetAttributeValue("/root/item", "id");
xml.RemoveNode("/root/old");
xml.Save();

// 静态方式
string s = XmlHelper.GetValue(@"D:\data.xml", "/root/name");
```

### 9. 枚举扩展 EnumExt（Jory.Common）

枚举与字典/字符串/描述互转，支持 `[Description]` 特性。

```csharp
using Jory.Common;

enum Color { [Description("红色")] Red, [Description("绿色")] Green }

string desc = EnumExt.GetDescription(Color.Red);              // "红色"
Dictionary<int, string> dict = EnumExt.GetDictionary(typeof(Color));
Dictionary<string, int> items = EnumExt.GetValueItems(typeof(Color));
int v = EnumExt.GetValue(typeof(Color), "Red");               // 0
string name = 0.ToEnumString(typeof(Color));                 // "Red"
```

### 10. 通用函数 Functions（Jory.Common）

字符串、字节、十六进制、字节序与 UI 线程辅助。

```csharp
using Jory.Common;

// 校验
bool a = Functions.IsNumeric("123");        // true
bool b = Functions.IsValidIP("192.168.1.1");
bool c = Functions.IsValidMac("00-1B-2C-3D");
bool d = Functions.IsHexStr("1A2F");

// 字节 / 十六进制
byte[] buf = Functions.HexToByteArray("1A2F");
string hex = buf.ToHexStr();                 // 扩展方法："1A2F"
string asc = Functions.HexToASCII("4869");   // "Hi"
string rev = Functions.ReverseString("abcd");

// 字节数组扩展
short  s = buf.ToShort(0);
int    i = buf.ToInt32(0);
byte[] slice = buf.GetRange(0, 2);

// UI 线程（WPF/Silverlight/WinForms 可用）
Functions.InvokeOnUiThread(() => { textBox.Text = "更新"; });
Functions.BeginInvokeOnUiThread(() => { /* 异步 */ });
```

### 11. 其它小工具

```csharp
using Jory.Common;

// StopwatchHelper —— 计量代码耗时（返回毫秒）
long ms = StopwatchHelper.Execute(() =>
{
    // 被测代码
});

// DumpHelper —— 写出进程 MiniDump（用于崩溃现场保存）
DumpHelper.Write();                          // 默认文件名
DumpHelper.Write("crash.dmp", DumpType.MiniDumpNormal);

// ConsoleManager —— 控制台窗口显隐（适用于“Windows 应用程序”附带控制台调试）
ConsoleManager.Show();
ConsoleManager.Hide();
ConsoleManager.Toggle();
```

---

## 构建

使用 Visual Studio 打开 `JoryCommonLibrary.slnx`（或 `JoryCommonLibrary.csproj`），选择 `Debug` / `Release` 后生成即可，产出 `Jory.Common.dll`。也可使用命令行：

```bash
msbuild JoryCommonLibrary.csproj /p:Configuration=Release
```

> 注意：本机若访问 `github.com` 出现 TLS 证书吊销检查报错（`CRYPT_E_NO_REVOCATION_CHECK`），属网络/证书环境限制，与代码无关。

---

## 许可证

本仓库**尚未指定许可证**。如需开源使用，请自行添加 `LICENSE` 文件（如 MIT）后再分发。
