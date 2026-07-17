using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Jory.Common
{
    /// <summary>
    /// 提供对控制台窗口的动态显示与隐藏功能。
    /// 适用于 Windows 平台下无控制台的 GUI 应用程序（如 WinForms/WPF），
    /// 可在运行时动态创建或销毁关联的控制台窗口。
    /// </summary>
    public static class ConsoleManager
    {
        private const string Kernel32_DllName = "kernel32.dll";

        [DllImport(Kernel32_DllName, SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport(Kernel32_DllName, SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport(Kernel32_DllName, SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

      
        [DllImport(Kernel32_DllName, SetLastError = true)]
        private static extern int GetConsoleOutputCP();

        /// <summary>
        /// 判断当前进程是否已附加到一个控制台窗口。
        /// </summary>
        public static bool HasConsole => GetConsoleWindow() != IntPtr.Zero;

        /// <summary>
        /// 如果当前进程未附加控制台，则创建一个新的控制台窗口并重定向标准输出和错误流。
        /// </summary>
        /// <remarks>
        /// 调用 <see cref="AllocConsole"/> 后必须重新初始化 <see cref="Console.Out"/> 和 <see cref="Console.Error"/>，
        /// 否则后续的 <c>Console.WriteLine</c> 不会输出到新控制台。
        /// </remarks>
        public static void Show()
        {
            if (!HasConsole)
            {
                bool allocated = AllocConsole();
                if (allocated)
                {
                    InvalidateOutAndError();
                }
            
            }
        }

        /// <summary>
        /// 如果当前进程已附加控制台，则将其分离并隐藏控制台窗口。
        /// 分离后，向 <see cref="Console"/> 写入内容将不会显示（但不会抛出异常）。
        /// </summary>
        public static void Hide()
        {
            if (HasConsole)
            {
                SetOutAndErrorToNull();
                FreeConsole();
            }
        }

        /// <summary>
        /// 切换控制台窗口的可见状态：
        /// - 若已显示，则隐藏；
        /// - 若未显示，则创建并显示。
        /// </summary>
        public static void Toggle()
        {
            if (HasConsole)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        /// <summary>
        /// 强制重新初始化 <see cref="Console.Out"/> 和 <see cref="Console.Error"/>，
        /// 使其指向新分配的控制台的标准输出/错误句柄。
        /// </summary>
        /// <remarks>
        /// 此方法通过反射调用 .NET Framework 内部方法 <c>Console.InitializeStdOutError</c>，
        /// 仅在 .NET Framework 下有效（.NET Core/.NET 5+ 行为不同）。
        /// </remarks>
        private static void InvalidateOutAndError()
        {
            Type consoleType = typeof(Console);

            // 获取内部字段 _out 和 _error（静态、非公开）
            FieldInfo outFile = consoleType.GetField("_out", BindingFlags.Static | BindingFlags.NonPublic);
            FieldInfo errorFile = consoleType.GetField("_error", BindingFlags.Static | BindingFlags.NonPublic);

            // 获取内部方法 InitializeStdOutError（静态、非公开）
            MethodInfo initMethod = consoleType.GetMethod("InitializeStdOutError",
                BindingFlags.Static | BindingFlags.NonPublic);

            // 安全断言（调试模式下检查）
            Debug.Assert(outFile != null, "无法找到 Console._out 字段");
            Debug.Assert(errorFile != null, "无法找到 Console._error 字段");
            Debug.Assert(initMethod != null, "无法找到 Console.InitializeStdOutError 方法");

            // 清空现有引用，触发重新初始化
            outFile?.SetValue(null, null);
            errorFile?.SetValue(null, null);
            initMethod?.Invoke(null, new object[] { true });
        }

        /// <summary>
        /// 将 <see cref="Console.Out"/> 和 <see cref="Console.Error"/> 重定向到 <see cref="TextWriter.Null"/>，
        /// 避免在控制台被释放后写入无效句柄导致异常或静默失败。
        /// </summary>
        private static void SetOutAndErrorToNull()
        {
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }
    }

}
