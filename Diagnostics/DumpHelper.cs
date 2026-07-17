using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace Jory.Common
{
    /// <summary>
    /// Dump帮助类
    /// </summary>
    public static class DumpHelper
    {
        /// <summary>
        /// From dbghelp.h:
        /// </summary>
        [Flags]
        public enum DumpType : uint
        {
            // From dbghelp.h:
            MiniDumpNormal = 0x00000000,

            MiniDumpWithDataSegs = 0x00000001,
            MiniDumpWithFullMemory = 0x00000002,
            MiniDumpWithHandleData = 0x00000004,
            MiniDumpFilterMemory = 0x00000008,
            MiniDumpScanMemory = 0x00000010,
            MiniDumpWithUnloadedModules = 0x00000020,
            MiniDumpWithIndirectlyReferencedMemory = 0x00000040,
            MiniDumpFilterModulePaths = 0x00000080,
            MiniDumpWithProcessThreadData = 0x00000100,
            MiniDumpWithPrivateReadWriteMemory = 0x00000200,
            MiniDumpWithoutOptionalData = 0x00000400,
            MiniDumpWithFullMemoryInfo = 0x00000800,
            MiniDumpWithThreadInfo = 0x00001000,
            MiniDumpWithCodeSegs = 0x00002000,
            MiniDumpWithoutAuxiliaryState = 0x00004000,
            MiniDumpWithFullAuxiliaryState = 0x00008000,
            MiniDumpWithPrivateWriteCopyMemory = 0x00010000,
            MiniDumpIgnoreInaccessibleMemory = 0x00020000,
            MiniDumpValidTypeFlags = 0x0003ffff,
        };

        //typedef struct _MINIDUMP_EXCEPTION_INFORMATION {
        //    DWORD ThreadId;
        //    PEXCEPTION_POINTERS ExceptionPointers;
        //    BOOL ClientPointers;
        //} MINIDUMP_EXCEPTION_INFORMATION, *PMINIDUMP_EXCEPTION_INFORMATION;
        [StructLayout(LayoutKind.Sequential, Pack = 4)]  // Pack=4 is important! So it works also for x64!
        private struct MiniDumpExceptionInformation
        {
            public uint ThreadId;
            public IntPtr ExceptioonPointers;

            [MarshalAs(UnmanagedType.Bool)]
            public bool ClientPointers;
        }

        //BOOL
        //WINAPI
        //MiniDumpWriteDump(
        //    __in HANDLE hProcess,
        //    __in DWORD ProcessId,
        //    __in HANDLE hFile,
        //    __in MINIDUMP_TYPE DumpType,
        //    __in_opt PMINIDUMP_EXCEPTION_INFORMATION ExceptionParam,
        //    __in_opt PMINIDUMP_USER_STREAM_INFORMATION UserStreamParam,
        //    __in_opt PMINIDUMP_CALLBACK_INFORMATION CallbackParam
        //    );
        [DllImport("dbghelp.dll", EntryPoint = "MiniDumpWriteDump", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, IntPtr hFile, uint dumpType, ref MiniDumpExceptionInformation expParam, IntPtr userStreamParam, IntPtr callbackParam);

        [DllImport("kernel32.dll", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
        private static extern uint GetCurrentThreadId();

        [DllImport("kernel32.dll", EntryPoint = "GetCurrentProcess", ExactSpelling = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", EntryPoint = "GetCurrentProcessId", ExactSpelling = true)]
        private static extern uint GetCurrentProcessId();

        public static bool Write()
        {
            string dumpFile = Path.Combine(Environment.CurrentDirectory, "Dump", string.Format("{0}.dmp", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")));
            return Write(dumpFile);
        }

        public static bool Write(string fileName)
        {
            return Write(fileName, DumpType.MiniDumpWithFullMemory);
        }

        public static bool Write(string fileName, DumpType dumpType)
        {
            try
            {
                var path = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                ClearOldFile(path);
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    MiniDumpExceptionInformation exp;
                    exp.ThreadId = GetCurrentThreadId();
                    exp.ClientPointers = false;
                    exp.ExceptioonPointers = Marshal.GetExceptionPointers();
                    var ret = MiniDumpWriteDump(GetCurrentProcess(), GetCurrentProcessId(), fs.SafeFileHandle.DangerousGetHandle(), (uint)dumpType, ref exp, IntPtr.Zero, IntPtr.Zero);
                    return ret;
                }
            }
            catch
            {
                return false;
            }
        }

        private static void ClearOldFile(string path, int saveTimeout = 7)
        {
            var files = Directory.GetFiles(path, "*.dmp", SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
            {
                return;
            }
            var currentTime = DateTime.Now;
            foreach (var item in files)
            {
                var timeStr = Path.GetFileName(item).Split('.')[0];
                var cultureInfo = CultureInfo.CreateSpecificCulture("zh-CN");
                var format = "yyyy-MM-dd HH-mm-ss";
                var createTime = DateTime.ParseExact(timeStr, format, cultureInfo);
                TimeSpan span = currentTime - createTime;
                if (span.Days > saveTimeout)
                {
                    File.Delete(item);
                }
            }
        }
    }
}
