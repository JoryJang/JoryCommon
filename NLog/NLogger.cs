using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Jory.Log
{
    /// <summary>
    /// 基于 NLog 的日志工具类，提供统一的日志写入接口。
    /// 支持控制台彩色输出 + 文件日志（同步/异步可选）。
    /// 使用方式：程序启动时调用 <see cref="Init"/>，退出时调用 <see cref="ShutDown"/>。
    /// </summary>
    public static class NLogger
    {
        /// <summary>NLog 日志实例（静态只读，线程安全）</summary>
        private static readonly Logger nLogger = LogManager.GetCurrentClassLogger();

        /// <summary>标记是否已初始化，防止重复初始化</summary>
        private static int _initialized = 0;

        /// <summary>单个日志文件最大大小（字节），默认 5MB</summary>
        private const int ArchiveAboveSize = 5 * 1024 * 1024;

        /// <summary>异步写入队列最大容量</summary>
        private const int AsyncQueueLimit = 10240;

        /// <summary>
        /// 日志初始化，应在程序启动时调用且仅调用一次。
        /// 优先加载程序目录下的 NLog.config 配置文件；
        /// 若配置文件不存在，则使用内置默认配置（控制台 + 文件）。
        /// </summary>
        /// <param name="fileName">日志文件名前缀，为空则不添加前缀</param>
        /// <param name="useAsyncFile">是否使用异步文件写入（默认 true，高吞吐场景推荐）</param>
        public static void Init(string fileName = "", bool useAsyncFile = true)
        {
            // 防止重复初始化（线程安全）
            if (Interlocked.CompareExchange(ref _initialized, 1, 0) != 0)
            {
                nLogger.Warn("NLogger.Init 被重复调用，已忽略。");
                return;
            }

            // 拼接配置文件路径：优先使用程序集所在目录，回退到 AppDomain 基础目录
            string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                             ?? AppDomain.CurrentDomain.BaseDirectory;
            string configPath = Path.Combine(baseDir, "NLog.config");

            LoggingConfiguration config;

            // 优先尝试加载外部配置文件
            if (!TryLoadExternalConfig(configPath, out config))
            {
                // 加载失败时使用内置默认配置
                config = BuildDefaultConfig(fileName, useAsyncFile);
            }

            LogManager.Configuration = config;
        }

        /// <summary>
        /// 关闭日志系统，刷新缓冲区中未写入的日志并释放资源。
        /// 应在程序退出前调用。
        /// </summary>
        public static void ShutDown()
        {
            // 等待最多 2 秒将缓冲区日志刷入目标
            LogManager.Flush(TimeSpan.FromSeconds(2));
            LogManager.Shutdown();
        }

        #region 私有方法 - Target 配置

        /// <summary>
        /// 尝试从外部 NLog.config 文件加载配置。
        /// </summary>
        /// <param name="configPath">配置文件完整路径。</param>
        /// <param name="config">输出参数，成功时返回加载的配置。</param>
        /// <returns>加载成功返回 true，否则返回 false。</returns>
        private static bool TryLoadExternalConfig(string configPath, out LoggingConfiguration config)
        {
            if (!File.Exists(configPath))
            {
                config = null;
                return false;
            }

            try
            {
                config = new XmlLoggingConfiguration(configPath);
                return true;
            }
            catch (Exception ex)
            {
                nLogger.Warn(ex, "加载外部 NLog.config 失败，将使用内置默认配置。");
                config = null;
                return false;
            }
        }

        /// <summary>
        /// 构建内置默认日志配置：控制台彩色输出 + 文件日志（同步/异步可选）。
        /// </summary>
        /// <param name="fileName">日志文件名前缀。</param>
        /// <param name="useAsyncFile">是否使用异步文件写入。</param>
        /// <returns>构建完成的 LoggingConfiguration 对象。</returns>
        private static LoggingConfiguration BuildDefaultConfig(string fileName, bool useAsyncFile)
        {
            var config = new LoggingConfiguration();

            string namePrefix = string.IsNullOrWhiteSpace(fileName) ? "" : fileName + "-";

            Target consoleTarget = CreateConsoleTarget();
            Target fileTarget = useAsyncFile
                ? CreateAsyncFileTarget(namePrefix)
                : CreateSyncFileTarget(namePrefix);

            config.AddTarget(consoleTarget);
            config.AddTarget(fileTarget);

            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, consoleTarget));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, fileTarget));

            return config;
        }

        /// <summary>
        /// 创建彩色控制台输出目标，按日志级别显示不同颜色。
        /// </summary>
        private static Target CreateConsoleTarget()
        {
            var consoleTarget = new ColoredConsoleTarget("console")
            {
                Layout = "[${longdate} ${level}] [Thread Id:${threadid}] ${message} ${exception:format=ToString}",
                UseDefaultRowHighlightingRules = true
            };

            // 按级别设置前景色：Debug=深灰, Info=灰, Warn=黄, Error=红, Fatal=红底白字
            consoleTarget.RowHighlightingRules.Add(
                new ConsoleRowHighlightingRule("level == LogLevel.Debug", ConsoleOutputColor.DarkGray, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(
                new ConsoleRowHighlightingRule("level == LogLevel.Info", ConsoleOutputColor.Gray, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(
                new ConsoleRowHighlightingRule("level == LogLevel.Warn", ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(
                new ConsoleRowHighlightingRule("level == LogLevel.Error", ConsoleOutputColor.Red, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(
                new ConsoleRowHighlightingRule("level == LogLevel.Fatal", ConsoleOutputColor.Red, ConsoleOutputColor.White));

            return consoleTarget;
        }

        /// <summary>
        /// 创建异步文件写入目标（内部包装同步 FileTarget）。
        /// 适用于高并发/高吞吐场景，日志先入队列再批量写入磁盘。
        /// </summary>
        /// <param name="namePrefix">日志文件名前缀</param>
        private static Target CreateAsyncFileTarget(string namePrefix)
        {
            FileTarget fileTarget = CreateFileTarget("file", namePrefix);

            // 使用 AsyncTargetWrapper 包装，实现异步批量写入
            var asyncTarget = new AsyncTargetWrapper("asyncFile", fileTarget)
            {
                QueueLimit = AsyncQueueLimit,  // 队列满时丢弃最旧日志（默认行为）
                OverflowAction = AsyncTargetWrapperOverflowAction.Discard
            };

            return asyncTarget;
        }

        /// <summary>
        /// 创建同步文件写入目标。
        /// 适用于对日志实时性要求高、不允许丢失的场景。
        /// </summary>
        /// <param name="namePrefix">日志文件名前缀</param>
        private static Target CreateSyncFileTarget(string namePrefix)
        {
            return CreateFileTarget("syncFile", namePrefix);
        }

        /// <summary>
        /// 创建 FileTarget 并配置公共属性（文件路径、归档策略、编码等）。
        /// 提取公共逻辑，避免同步/异步目标之间的代码重复。
        /// </summary>
        /// <param name="targetName">Target 名称标识</param>
        /// <param name="namePrefix">日志文件名前缀</param>
        private static FileTarget CreateFileTarget(string targetName, string namePrefix)
        {
            return new FileTarget(targetName)
            {
                // 日志文件路径：{basedir}/Logs/{前缀}{级别}-{日期}.log
                FileName = "${basedir}/Logs/" + namePrefix + "${level}-${shortdate}.log",
                Layout = "${longdate} [${level}] [Thread Id:${threadid}] ${message} ${exception:format=ToString}",

                // 归档配置：文件超过 ArchiveAboveSize 时按序号归档
                ArchiveFileName = "${basedir}/Logs/" + namePrefix + "${level}-${shortdate}.log",
                ArchiveAboveSize = ArchiveAboveSize,
                ArchiveSuffixFormat = "_{0:000}",

                // 允许多进程并发写入同一文件
                KeepFileOpen = true,
                AutoFlush = false,
                OpenFileFlushTimeout = 1,
                Encoding = Encoding.UTF8
            };
        }

        #endregion 私有方法 - Target 配置

        #region 公共方法 - 日志写入

        /// <summary>
        /// 写入 Debug 级别日志（仅开发/调试阶段使用）。
        /// </summary>
        /// <param name="msg">日志消息</param>
        /// <param name="ex">关联的异常对象（可选）</param>
        public static void WriteDebug(string msg, Exception ex = null)
        {
            nLogger.Debug(ex, msg);
        }

        public static void WriteDebug(string msg, params object[] args) 
        { 
            nLogger.Debug(msg, args); 
        }

        /// <summary>
        /// 写入 Info 级别日志（常规运行信息）。
        /// </summary>
        /// <param name="msg">日志消息</param>
        /// <param name="ex">关联的异常对象（可选）</param>
        public static void WriteInfo(string msg, Exception ex = null)
        {
            nLogger.Info(ex, msg);
        }

        public static void WriteInfo(string msg, params object[] args)
        {
            nLogger.Info(msg, args);
        }

        /// <summary>
        /// 写入 Warn 级别日志（潜在问题警告）。
        /// </summary>
        /// <param name="msg">日志消息</param>
        /// <param name="ex">关联的异常对象（可选）</param>
        public static void WriteWarn(string msg, Exception ex = null)
        {
            nLogger.Warn(ex, msg);
        }

        public static void WriteWarn(string msg, params object[] args)
        {
            nLogger.Warn(msg, args);
        }

        /// <summary>
        /// 写入 Error 级别日志（运行时错误）。
        /// </summary>
        /// <param name="msg">日志消息</param>
        /// <param name="ex">关联的异常对象（可选）</param>
        public static void WriteError(string msg, Exception ex = null)
        {
            nLogger.Error(ex, msg);
        }

        public static void WriteError(string msg, params object[] args)
        {
            nLogger.Error(msg, args);
        }

        /// <summary>
        /// 写入 Fatal 级别日志（致命错误，通常导致程序终止）。
        /// </summary>
        /// <param name="msg">日志消息</param>
        /// <param name="ex">关联的异常对象（可选）</param>
        public static void WriteFatal(string msg, Exception ex = null)
        {
            nLogger.Fatal(ex, msg);
        }

        public static void WriteFatal(string msg, params object[] args)
        {
            nLogger.Fatal(msg, args);
        }

        #endregion 公共方法 - 日志写入
    }
}