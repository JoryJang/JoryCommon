using System;
using System.Collections.Generic;
using System.Text;

namespace Jory.IOC.Tests
{
    // ============================================================
    //  Jory.IOC 测试套件
    //  目标框架：.NET Framework 4.5
    //  说明：自包含轻量测试框架，无需第三方依赖。
    //        重点覆盖上一轮修复的 Bug（工厂注册）与新增能力
    //        （Release / trackTransients / Dispose 防护 / ResolveWithoutRoot 三级注入）。
    // ============================================================
    internal static class Program
    {
        private static int s_passed;
        private static int s_failed;

        private static void Main()
        {
            // 控制台用 UTF-8 输出，避免中文乱码
            try { Console.OutputEncoding = Encoding.UTF8; } catch { }
            Console.WriteLine("==================== Jory.IOC 测试套件 ====================");
            Console.WriteLine();

            // 按 feature 分组运行
            Run("01. 基础注册与解析", TestBasicResolve);
            Run("02. 单例唯一性", TestSingletonUniqueness);
            Run("03. 瞬态每次新建", TestTransientNewInstance);
            Run("04. 构造函数注入", TestConstructorInjection);
            Run("05. 属性注入", TestPropertyInjection);
            Run("06. 方法注入", TestMethodInjection);
            Run("07. 参数级 [DependencyInject] (Type+Key)", TestParameterInjection);
            Run("08. Key 命名多实现", TestKeyedMultipleImpl);
            Run("09. 开放泛型", TestOpenGeneric);
            Run("10. 自定义工厂单例 [Bug 修复验证]", TestFactorySingleton);
            Run("11. 自定义工厂瞬态", TestFactoryTransient);
            Run("12. 工厂单例缓存唯一性", TestFactorySingletonCached);
            Run("13. ResolveWithoutRoot 三级注入 [修复验证]", TestResolveWithoutRootThreeInjects);
            Run("14. ResolveWithoutRoot 循环检测", TestResolveWithoutRootCircular);
            Run("15. 已注册类型循环依赖检测", TestCircularDetection);
            Run("16. TryResolve 未注册返回 null", TestTryResolveUnregistered);
            Run("17. IsRegistered / 泛型 IsRegistered", TestIsRegistered);
            Run("18. IDisposable 单例释放", TestDisposableSingletonRelease);
            Run("19. Release 方法显式释放 [新增]", TestReleaseMethod);
            Run("20. trackTransients=false 不跟踪瞬态 [新增]", TestTrackTransientsFalse);
            Run("21. trackTransients=true 跟踪瞬态释放 [新增]", TestTrackTransientsTrue);
            Run("22. Dispose 后 Resolve 抛异常 [新增]", TestDisposedGuard);
            Run("23. IServiceProvider.GetService 兼容", TestServiceProviderCompat);
            Run("24. DependencyType 限定只构造注入", TestDependencyTypeLimit);
            Run("25. OnResolved 回调（单例一次/瞬态每次）", TestOnResolvedCallback);
            Run("26. Unregister 后无法解析", TestUnregister);
            Run("27. 容器自身可解析为 IResolver/IServiceProvider", TestContainerSelfResolve);
            Run("28. 基础类型/字符串不注入", TestPrimitiveNotInjected);
            Run("29. 覆盖注册（重复 Register）", TestOverrideRegister);
            Run("30. 自映射单例", TestSelfMappingSingleton);

            Console.WriteLine();
            Console.WriteLine("===========================================================");
            Console.ForegroundColor = s_failed == 0 ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine("  通过: {0}    失败: {1}    总计: {2}", s_passed, s_failed, s_passed + s_failed);
            Console.ResetColor();
            Console.WriteLine("===========================================================");

            if (s_failed > 0)
            {
                Environment.ExitCode = 1;
            }
        }

        // ------------------------------------------------------------
        //  轻量测试框架
        // ------------------------------------------------------------
        private static void Run(string name, Action test)
        {
            try
            {
                test();
                s_passed++;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("  [PASS] ");
                Console.ResetColor();
                Console.WriteLine(name);
            }
            catch (Exception ex)
            {
                s_failed++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("  [FAIL] ");
                Console.ResetColor();
                Console.WriteLine(name);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("         -> " + (ex is AssertionException ? ex.Message : ex.GetType().Name + ": " + ex.Message));
                Console.ResetColor();
            }
        }

        private static void IsTrue(bool condition, string message = null)
        {
            if (!condition)
            {
                throw new AssertionException(message ?? "期望 true，实际 false");
            }
        }

        private static void IsFalse(bool condition, string message = null)
        {
            if (condition)
            {
                throw new AssertionException(message ?? "期望 false，实际 true");
            }
        }

        private static void AreEqual<T>(T expected, T actual, string message = null)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new AssertionException(string.Format("期望 <{0}>，实际 <{1}>。{2}",
                    expected, actual, message ?? string.Empty).TrimEnd());
            }
        }

        private static void IsNotNull(object obj, string message = null)
        {
            if (obj == null)
            {
                throw new AssertionException(message ?? "期望非 null，实际 null");
            }
        }

        private static void IsNull(object obj, string message = null)
        {
            if (obj != null)
            {
                throw new AssertionException(message ?? "期望 null，实际非 null");
            }
        }

        private static void IsInstanceOf<T>(object obj, string message = null)
        {
            if (!(obj is T))
            {
                throw new AssertionException(string.Format("期望类型 {0}，实际 {1}。{2}",
                    typeof(T).Name, obj == null ? "null" : obj.GetType().Name, message ?? string.Empty).TrimEnd());
            }
        }

        private static void Throws<TException>(Action action, string message = null) where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return;
            }
            catch (Exception ex)
            {
                throw new AssertionException(string.Format("期望抛 {0}，实际抛 {1}: {2}",
                    typeof(TException).Name, ex.GetType().Name, ex.Message));
            }
            throw new AssertionException(message ?? string.Format("期望抛 {0}，但未抛任何异常", typeof(TException).Name));
        }

        private sealed class AssertionException : Exception
        {
            public AssertionException(string msg) : base(msg) { }
        }

        // ============================================================
        //  测试用类型定义
        // ============================================================

        // ---- 日志族 ----
        public interface ILog { string Name { get; } }
        public class ConsoleLog : ILog { public string Name { get { return "console"; } } }
        public class MySqlLog : ILog { public string Name { get { return "mysql"; } } }
        public class FileLog : ILog { public string Name { get { return "file"; } } }

        // ---- 业务服务族 ----
        public interface IService { ILog Log { get; } }
        public class ServiceImpl : IService
        {
            public ILog Log { get; private set; }
            public ServiceImpl(ILog log) { Log = log; }
        }

        // 带三级注入的服务
        public class FullInjectService : IService
        {
            public ILog CtorLog { get; private set; }
            [DependencyInject] public ILog PropLog { get; set; }
            public bool MethodCalled { get; private set; }
            public ILog MethodLog { get; private set; }

            public ILog Log { get { return CtorLog; } }

            public FullInjectService(ILog log) { CtorLog = log; }

            [DependencyInject]
            public void Initialize(ILog log) { MethodCalled = true; MethodLog = log; }
        }

        // ---- 泛型仓储 ----
        public interface IRepository<T> { Type EntityType { get; } }
        public class Repository<T> : IRepository<T>
        {
            public Type EntityType { get { return typeof(T); } }
        }
        public class User { }

        // ---- 可释放服务 ----
        public class DisposableService : IDisposable
        {
            public bool Disposed { get; private set; }
            public void Dispose() { Disposed = true; }
        }

        // ---- 循环依赖（已注册）----
        public interface ICircularA { }
        public interface ICircularB { }
        public class CircularAImpl : ICircularA
        {
            public CircularAImpl(ICircularB b) { }
        }
        public class CircularBImpl : ICircularB
        {
            public CircularBImpl(ICircularA a) { }
        }

        // ---- ResolveWithoutRoot 用的未注册根类型（带三级注入）----
        public class RootWithThreeInjects
        {
            public ILog CtorLog { get; private set; }
            [DependencyInject] public ILog PropLog { get; set; }
            public bool MethodCalled { get; private set; }

            public RootWithThreeInjects(ILog log) { CtorLog = log; }

            [DependencyInject]
            public void Setup(ILog log) { MethodCalled = true; }
        }

        // 循环依赖的根类型（根参与环）
        public class RootInCircle
        {
            public RootInCircle(IWatcher w) { }
        }
        public interface IWatcher { }
        public class WatcherImpl : IWatcher
        {
            public WatcherImpl(RootInCircle r) { }
        }

        // ---- 参数级注入目标 ----
        public interface IParamTarget { string Which { get; } }
        public class ParamTargetImpl : IParamTarget
        {
            public string Which { get; private set; }
            // 参数上指定 Key
            public ParamTargetImpl([DependencyInject("mysql")] ILog log)
            {
                Which = log.Name;
            }
        }

        // ---- DependencyType 限定 ----
        [DependencyType(DependencyType.Constructor)]
        public class OnlyCtorInject
        {
            [DependencyInject] public ILog ShouldNotBeInjected { get; set; }
            public OnlyCtorInject(ILog log) { CtorLog = log; }
            public ILog CtorLog { get; private set; }
        }

        // ---- 自映射 ----
        public class SelfMapped { public int Id { get; set; } }

        // ============================================================
        //  测试方法
        // ============================================================

        // 01. 基础注册与解析
        private static void TestBasicResolve()
        {
            var c = new Container();
            c.RegisterSingleton<ILog, ConsoleLog>();
            var log = c.Resolve<ILog>();
            IsNotNull(log);
            IsInstanceOf<ConsoleLog>(log);
            AreEqual("console", log.Name);

            // 反射式 Resolve(Type)
            object obj = c.Resolve(typeof(ILog));
            IsNotNull(obj);
        }

        // 02. 单例唯一性
        private static void TestSingletonUniqueness()
        {
            var c = new Container();
            c.RegisterSingleton<ILog, ConsoleLog>();
            var a = c.Resolve<ILog>();
            var b = c.Resolve<ILog>();
            AreEqual<object>(a, b, "单例两次解析应返回同一实例");
        }

        // 03. 瞬态每次新建
        private static void TestTransientNewInstance()
        {
            var c = new Container();
            c.RegisterTransient<ILog, ConsoleLog>();
            var a = c.Resolve<ILog>();
            var b = c.Resolve<ILog>();
            IsFalse(ReferenceEquals(a, b), "瞬态两次解析应返回不同实例");
        }

        // 04. 构造函数注入
        private static void TestConstructorInjection()
        {
            var c = new Container();
            c.RegisterSingleton<ILog, ConsoleLog>();
            c.RegisterTransient<IService, ServiceImpl>();
            var svc = c.Resolve<IService>();
            IsNotNull(svc.Log, "构造函数注入的依赖不应为 null");
            AreEqual("console", svc.Log.Name);
        }

        // 05. 属性注入
        private static void TestPropertyInjection()
        {
            var c = new Container();
            c.RegisterSingleton<ILog, ConsoleLog>();
            c.RegisterTransient<FullInjectService>();
            var svc = c.Resolve<FullInjectService>();
            IsNotNull(svc.PropLog, "属性注入应成功");
            AreEqual("console", svc.PropLog.Name);
        }

        // 06. 方法注入
        private static void TestMethodInjection()
        {
            var c = new Container();
            c.RegisterSingleton<ILog, ConsoleLog>();
            c.RegisterTransient<FullInjectService>();
            var svc = c.Resolve<FullInjectService>();
            IsTrue(svc.MethodCalled, "方法注入应被调用");
            IsNotNull(svc.MethodLog, "方法注入的依赖不应为 null");
        }

        // 07. 参数级 [DependencyInject] (Type+Key)
        private static void TestParameterInjection()
        {
            var c = new Container();
            c.RegisterSingleton<ILog, ConsoleLog>("console");
            c.RegisterSingleton<ILog, MySqlLog>("mysql");
            c.RegisterTransient<IParamTarget, ParamTargetImpl>();
            var t = c.Resolve<IParamTarget>();
            AreEqual("mysql", t.Which, "参数级 [DependencyInject(\"mysql\")] 应解析到 MySqlLog");
        }

        // 08. Key 命名多实现
        private static void TestKeyedMultipleImpl()
        {
            var c = new Container();
            c.RegisterSingleton<ILog, ConsoleLog>("console");
            c.RegisterSingleton<ILog, MySqlLog>("mysql");
            c.RegisterSingleton<ILog, FileLog>("file");

            AreEqual("console", c.Resolve<ILog>("console").Name);
            AreEqual("mysql", c.Resolve<ILog>("mysql").Name);
            AreEqual("file", c.Resolve<ILog>("file").Name);
        }

        // 09. 开放泛型
        private static void TestOpenGeneric()
        {
            var c = new Container();
            c.RegisterTransient(typeof(IRepository<>), typeof(Repository<>));
            var repo = c.Resolve<IRepository<User>>();
            IsNotNull(repo);
            AreEqual(typeof(User), repo.EntityType, "开放泛型应正确套用类型参数");
        }

        // 10. 自定义工厂单例 [Bug 修复验证] —— 修复前会返回 Lifetime 枚举值
        private static void TestFactorySingleton()
        {
            var c = new Container();
            int invokeCount = 0;
            c.RegisterSingleton<ILog>(r =>
            {
                invokeCount++;
                return new ConsoleLog();
            });

            var log = c.Resolve<ILog>();
            IsNotNull(log, "工厂注册应返回实例（非 null）");
            IsInstanceOf<ConsoleLog>(log, "工厂注册应返回 ConsoleLog 实例");
            IsFalse(log.GetType() == typeof(Lifetime), "工厂注册不应返回 Lifetime 枚举值（修复前 Bug）");
            AreEqual(1, invokeCount, "单例工厂应只调用一次");
        }

        // 11. 自定义工厂瞬态
        private static void TestFactoryTransient()
        {
            var c = new Container();
            int invokeCount = 0;
            c.RegisterTransient<ILog>(r =>
            {
                invokeCount++;
                return new ConsoleLog();
            });

            var a = c.Resolve<ILog>();
            var b = c.Resolve<ILog>();
            AreEqual(2, invokeCount, "瞬态工厂每次解析都应调用");
            IsFalse(ReferenceEquals(a, b), "瞬态应返回不同实例");
        }

        // 12. 工厂单例缓存唯一性
        private static void TestFactorySingletonCached()
        {
            var c = new Container();
            c.RegisterSingleton<ILog>(r => new ConsoleLog());
            var a = c.Resolve<ILog>();
            var b = c.Resolve<ILog>();
            AreEqual<object>(a, b, "单例工厂结果应被缓存，多次解析返回同一实例");
        }

        // 13. ResolveWithoutRoot 三级注入 [修复验证] —— 修复前只做构造注入
        private static void TestResolveWithoutRootThreeInjects()
        {
            var c = new Container();
            c.RegisterSingleton<ILog, ConsoleLog>();

            var root = c.ResolveWithoutRoot<RootWithThreeInjects>();
            IsNotNull(root, "ResolveWithoutRoot 应返回实例");
            IsNotNull(root.CtorLog, "构造注入应成功");
            IsNotNull(root.PropLog, "属性注入应成功（修复前缺失）");
            IsTrue(root.MethodCalled, "方法注入应被调用（修复前缺失）");
        }

        // 14. ResolveWithoutRoot 循环检测
        private static void TestResolveWithoutRootCircular()
        {
            var c = new Container();
            c.RegisterSingleton<IWatcher, WatcherImpl>();
            // RootInCircle 未注册，其依赖 WatcherImpl 依赖 RootInCircle —— 形成环
            Throws<InvalidOperationException>(() => c.ResolveWithoutRoot<RootInCircle>(),
                "ResolveWithoutRoot 应检测根类型参与的循环依赖");
        }

        // 15. 已注册类型循环依赖检测
        private static void TestCircularDetection()
        {
            var c = new Container();
            c.RegisterTransient<ICircularA, CircularAImpl>();
            c.RegisterTransient<ICircularB, CircularBImpl>();
            Throws<InvalidOperationException>(() => c.Resolve<ICircularA>(),
                "循环依赖应抛 InvalidOperationException");
        }

        // 16. TryResolve 未注册返回 null
        private static void TestTryResolveUnregistered()
        {
            var c = new Container();
            var maybe = c.TryResolve<ILog>();
            IsNull(maybe, "未注册类型 TryResolve 应返回 null");

            c.RegisterSingleton<ILog, ConsoleLog>();
            IsNotNull(c.TryResolve<ILog>(), "已注册类型 TryResolve 应返回实例");
        }

        // 17. IsRegistered / 泛型 IsRegistered
        private static void TestIsRegistered()
        {
            var c = new Container();
            IsFalse(c.IsRegistered<ILog>(), "注册前默认 Key 应返回 false");
            c.RegisterSingleton<ILog, ConsoleLog>("mysql");
            IsFalse(c.IsRegistered<ILog>(), "仅注册命名 Key 时默认 Key 仍为 false");
            IsTrue(c.IsRegistered<ILog>("mysql"), "已注册的命名 Key 应返回 true");
            IsFalse(c.IsRegistered<ILog>("file"), "未注册的 Key 应返回 false");
            // 再注册默认 Key
            c.RegisterSingleton<ILog, ConsoleLog>();
            IsTrue(c.IsRegistered<ILog>(), "注册默认 Key 后应返回 true");
        }

        // 18. IDisposable 单例释放
        private static void TestDisposableSingletonRelease()
        {
            DisposableService svc;
            using (var c = new Container())
            {
                c.RegisterSingleton<DisposableService>();
                svc = c.Resolve<DisposableService>();
            }
            IsTrue(svc.Disposed, "容器 Dispose 应释放其创建的单例实例");
        }

        // 19. Release 方法显式释放 [新增]
        private static void TestReleaseMethod()
        {
            var c = new Container(trackTransients: true);
            c.RegisterTransient<DisposableService>();
            var svc = c.Resolve<DisposableService>();
            c.Release(svc);
            IsTrue(svc.Disposed, "Release 应立即 Dispose 被跟踪的瞬态实例");

            // 再次 Release 不应抛异常
            c.Release(svc);
        }

        // 20. trackTransients=false 不跟踪瞬态 [新增]
        private static void TestTrackTransientsFalse()
        {
            DisposableService svc;
            using (var c = new Container())  // 默认 trackTransients=false
            {
                c.RegisterTransient<DisposableService>();
                svc = c.Resolve<DisposableService>();
            }
            IsFalse(svc.Disposed, "trackTransients=false 时，容器 Dispose 不应释放瞬态实例");
        }

        // 21. trackTransients=true 跟踪瞬态释放 [新增]
        private static void TestTrackTransientsTrue()
        {
            DisposableService svc;
            using (var c = new Container(trackTransients: true))
            {
                c.RegisterTransient<DisposableService>();
                svc = c.Resolve<DisposableService>();
            }
            IsTrue(svc.Disposed, "trackTransients=true 时，容器 Dispose 应释放瞬态实例");
        }

        // 22. Dispose 后 Resolve 抛异常 [新增]
        private static void TestDisposedGuard()
        {
            var c = new Container();
            c.RegisterSingleton<ILog, ConsoleLog>();
            c.Resolve<ILog>();  // 正常使用
            c.Dispose();

            Throws<ObjectDisposedException>(() => c.Resolve<ILog>(), "Dispose 后 Resolve 应抛 ObjectDisposedException");
            Throws<ObjectDisposedException>(() => c.RegisterSingleton<ILog, MySqlLog>(), "Dispose 后 Register 应抛 ObjectDisposedException");
        }

        // 23. IServiceProvider.GetService 兼容
        private static void TestServiceProviderCompat()
        {
            var c = new Container();
            IServiceProvider sp = c;
            IsNull(sp.GetService(typeof(ILog)), "未注册类型 GetService 应返回 null（ISP 约定）");

            c.RegisterSingleton<ILog, ConsoleLog>();
            IsNotNull(sp.GetService(typeof(ILog)), "已注册类型 GetService 应返回实例");
        }

        // 24. DependencyType 限定只构造注入
        private static void TestDependencyTypeLimit()
        {
            var c = new Container();
            c.RegisterSingleton<ILog, ConsoleLog>();
            c.RegisterTransient<OnlyCtorInject>();
            var svc = c.Resolve<OnlyCtorInject>();
            IsNotNull(svc.CtorLog, "限定 Constructor 时仍应构造注入");
            IsNull(svc.ShouldNotBeInjected, "限定 Constructor 时应跳过属性注入");
        }

        // 25. OnResolved 回调（单例一次/瞬态每次）
        private static void TestOnResolvedCallback()
        {
            var c = new Container();

            int singletonCount = 0;
            var singletonDesc = new DependencyDescriptor(typeof(ILog), typeof(ConsoleLog), Lifetime.Singleton)
            {
                OnResolved = obj => singletonCount++
            };
            c.Register(singletonDesc);
            c.Resolve<ILog>();
            c.Resolve<ILog>();
            AreEqual(1, singletonCount, "单例 OnResolved 应只触发一次");

            int transientCount = 0;
            var transientDesc = new DependencyDescriptor(typeof(IService), typeof(ServiceImpl), Lifetime.Transient)
            {
                OnResolved = obj => transientCount++
            };
            c.Register(transientDesc);
            c.Resolve<IService>();
            c.Resolve<IService>();
            AreEqual(2, transientCount, "瞬态 OnResolved 应每次解析都触发");
        }

        // 26. Unregister 后无法解析
        private static void TestUnregister()
        {
            var c = new Container();
            c.RegisterSingleton<ILog, ConsoleLog>();
            IsTrue(c.IsRegistered<ILog>());
            c.Unregister<ILog>();
            IsFalse(c.IsRegistered<ILog>(), "Unregister 后应返回 false");
            Throws<InvalidOperationException>(() => c.Resolve<ILog>(), "Unregister 后 Resolve 应抛异常");
        }

        // 27. 容器自身可解析为 IResolver/IServiceProvider
        private static void TestContainerSelfResolve()
        {
            var c = new Container();
            var resolver = c.Resolve<IResolver>();
            AreEqual<object>(c, resolver, "容器应把自身注册为 IResolver 单例");

            var sp = c.Resolve<IServiceProvider>();
            AreEqual<object>(c, sp, "容器应把自身注册为 IServiceProvider 单例");
        }

        // 28. 基础类型/字符串不注入
        private static void TestPrimitiveNotInjected()
        {
            var c = new Container();
            // 基础类型未注册，Resolve 内部对 IsPrimitive 返回 null（不抛异常）
            object i = c.Resolve(typeof(int));
            IsNull(i, "int 未注册应返回 null（基础类型不注入）");

            object s = c.Resolve(typeof(string));
            IsNull(s, "string 未注册应返回 null");
        }

        // 29. 覆盖注册（重复 Register）
        private static void TestOverrideRegister()
        {
            var c = new Container();
            c.RegisterSingleton<ILog, ConsoleLog>();
            AreEqual("console", c.Resolve<ILog>().Name);

            // 覆盖为 MySqlLog
            c.RegisterSingleton<ILog, MySqlLog>();
            AreEqual("mysql", c.Resolve<ILog>().Name, "覆盖注册后应返回新实现");
        }

        // 30. 自映射单例
        private static void TestSelfMappingSingleton()
        {
            var c = new Container();
            c.RegisterSingleton<SelfMapped>();
            var a = c.Resolve<SelfMapped>();
            var b = c.Resolve<SelfMapped>();
            AreEqual<object>(a, b, "自映射单例应返回同一实例");
        }
    }
}
