using NLog;
using NLog.Conditions;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UEM.Log
{
    public class NLogger
    {
        private static readonly Logger nLogger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 日志初始化，只在程序启动时启动一次
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="useAsyncFile"></param>
        public static void Init(string fileName = "", bool useAsyncFile = true)
        {
            string configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppDomain.CurrentDomain.BaseDirectory, "NLog.config");

            NLog.Config.LoggingConfiguration config = null;
            if (File.Exists(configPath))
            {
                config = new NLog.Config.XmlLoggingConfiguration(configPath, true);
            }
            else
            {
                config = new NLog.Config.LoggingConfiguration();
                Target consoleTarget = ConfigConsoleTarget();
                Target fileTarget;

                if (useAsyncFile)
                {
                    fileTarget = ConfigAsyncFileTarget(fileName);
                }
                else
                {
                    fileTarget = ConfigSyncFileTarget(fileName);
                }

                config.AddTarget(consoleTarget);
                config.AddTarget(fileTarget);

                config.LoggingRules.Add(new NLog.Config.LoggingRule("*", LogLevel.Debug, consoleTarget));
                config.LoggingRules.Add(new NLog.Config.LoggingRule("*", LogLevel.Info, fileTarget));
            }

            LogManager.Configuration = config;
        }

        public static void ShutDown()
        {
            LogManager.Flush(TimeSpan.FromSeconds(2));
            LogManager.Shutdown();
        }

        private static Target ConfigConsoleTarget()
        {
            ColoredConsoleTarget consoleTarget = new ColoredConsoleTarget();
            consoleTarget.Name = "console";
            consoleTarget.Layout = "[${longdate} ${level}] [Thread Id:${threadid}] ${message} ${exception:format=ToString}";
            consoleTarget.UseDefaultRowHighlightingRules = true;

            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Debug", ConsoleOutputColor.DarkGray, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Info", ConsoleOutputColor.Gray, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Warn", ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Error", ConsoleOutputColor.Red, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Fatal", ConsoleOutputColor.Red, ConsoleOutputColor.White));

            return consoleTarget;
        }

        private static Target ConfigAsyncFileTarget(string fileName)
        {
            string name = fileName;
            FileTarget fileTarget = new FileTarget();
            fileTarget.Name = "file";
            fileTarget.FileName = "${basedir}/Logs/" + name + "${level}-${shortdate}.log";
            fileTarget.Layout = "${longdate} [${level}] [Thread Id:${threadid}] ${message} ${exception:format=ToString}";
            fileTarget.ArchiveFileName = "${basedir}/Logs/" + name + "${level}-${shortdate}-{###}.log";
            fileTarget.ArchiveAboveSize = 5120000;
            fileTarget.ArchiveNumbering = ArchiveNumberingMode.Sequence;
            fileTarget.ConcurrentWrites = true;
            fileTarget.KeepFileOpen = false;
            fileTarget.Encoding = Encoding.UTF8;

            NLog.Targets.Wrappers.AsyncTargetWrapper asyncTarget = new NLog.Targets.Wrappers.AsyncTargetWrapper("asyncFile", fileTarget);
            asyncTarget.Name = "asyncFile";
            asyncTarget.QueueLimit = 10240;
            return asyncTarget;
        }

        private static Target ConfigSyncFileTarget(string fileName)
        {
            string name = fileName;
            FileTarget fileTarget = new FileTarget();
            fileTarget.Name = "syncFile";
            fileTarget.FileName = "${basedir}/Logs/" + name + "${level}-${shortdate}.log";
            fileTarget.Layout = "${longdate} [${level}] [Thread Id:${threadid}] ${message} ${exception:format=ToString}";
            fileTarget.ArchiveFileName = "${basedir}/Logs/" + name + "${level}-${shortdate}-{###}.log";
            fileTarget.ArchiveAboveSize = 5120000;
            fileTarget.ArchiveNumbering = ArchiveNumberingMode.Sequence;
            fileTarget.ConcurrentWrites = true;
            fileTarget.KeepFileOpen = false;
            fileTarget.Encoding = Encoding.UTF8;

            return fileTarget;
        }

        public static void WriteDebug(string msg, Exception ex = null)
        {
            try
            {
                nLogger.Debug(ex, msg);
            }
            catch (Exception e)
            {
                nLogger.Error(e, msg);
            }
        }

        public static void WriteInfo(string msg, Exception ex = null)
        {
            try
            {
                nLogger.Info(ex, msg);
            }
            catch (Exception e)
            {
                nLogger.Error(e, msg);
            }
        }

        public static void WriteWarn(string msg, Exception ex = null)
        {
            nLogger.Warn(ex, msg);
        }

        public static void WriteError(string msg, Exception ex = null)
        {
            nLogger.Error(ex, msg);
        }

        public static void WriteFatal(string msg, Exception ex = null)
        {
            nLogger.Fatal(ex, msg);
        }
    }
}
