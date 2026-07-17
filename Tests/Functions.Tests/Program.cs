using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Jory.Common;

// 注意：本测试命名空间故意不用 *Functions 结尾，否则标识符 Functions 会被
// 编译器解析为祖先命名空间（而非 Jory.Common.Functions 类）。
namespace Jory.Tests.FunctionsLib
{
    // ============================================================
    //  Jory.Common.Functions 测试套件
    //  目标框架：.NET Framework 4.5
    //  说明：自包含轻量测试框架，无需第三方依赖。
    //        覆盖字符串校验、数据转换、结构体 Marshal 互转、
    //        集合工具、XML 序列化、文件操作等纯函数与 IO 方法。
    //        串口 / WPF UI 线程 / 可视化树查找依赖运行环境，暂不覆盖。
    // ============================================================

    // ------------------------------------------------------------
    //  测试用类型（顶级，满足 XmlSerializer 公共可见性要求）
    // ------------------------------------------------------------

    // 用于 Marshal 互转测试（Pack=1 避免 padding，size=6）
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TestStruct
    {
        public int X;
        public short Y;
    }

    // 用于 XML 序列化测试
    public class XmlSample
    {
        public string Name;
        public int Age;
        public XmlSample() { }
        public XmlSample(string n, int a) { Name = n; Age = a; }
    }

    internal static class Program
    {
        private static int s_passed;
        private static int s_failed;

        private static void Main()
        {
            try { Console.OutputEncoding = Encoding.UTF8; } catch { }
            Console.WriteLine("================ Jory.Common.Functions 测试套件 ================");

            // ---- 字符串与字符校验 ----
            Run("01. CharToString", TestCharToString);
            Run("02. IsDigit(char)", TestIsDigitChar);
            Run("03. IsDigit(string)", TestIsDigitString);
            Run("04. IsLetter(char)", TestIsLetterChar);
            Run("05. IsLetter(string)", TestIsLetterString);
            Run("06. IsNumericOrLetter", TestIsNumericOrLetter);
            Run("07. IsValidIPAddress", TestIsValidIPAddress);
            Run("08. IsValidMacAddress", TestIsValidMacAddress);
            Run("09. IsHexDigit", TestIsHexDigit);
            Run("10. IsHexString", TestIsHexString);
            Run("11. ReverseString", TestReverseString);
            Run("12. RemoveWhiteSpace", TestRemoveWhiteSpace);

            // ---- 数据转换 ----
            Run("13. BinaryToHexString", TestBinaryToHexString);
            Run("14. HexToBinaryString", TestHexToBinaryString);
            Run("15. BinaryToHex 往返对称", TestBinaryHexRoundtrip);
            Run("16. ToHexString(int,int)", TestToHexString);
            Run("17. HexToByteArray", TestHexToByteArray);
            Run("18. HexToASCII", TestHexToASCII);
            Run("19. Int16ToBytes(大端)", TestInt16ToBytes);
            Run("20. Int32ToBytes(大端)", TestInt32ToBytes);
            Run("21. Int64ToBytes(指定长度)", TestInt64ToBytes);
            Run("22. GetRange(byte[])", TestGetRangeByte);
            Run("23. ToHexStr(byte)", TestToHexStrSingle);
            Run("24. ToHexStr(集合+分隔符)", TestToHexStrCollection);
            Run("25. ToBigEndianInt16", TestToBigEndianInt16);
            Run("26. ToBigEndianInt32", TestToBigEndianInt32);
            Run("27. ToBigEndianInt64", TestToBigEndianInt64);
            Run("28. ToBigEndianUInt64", TestToBigEndianUInt64);
            Run("29. RangeToInt", TestRangeToInt);
            Run("30. SwapEndian32(4字节翻转)", TestSwapEndian32Basic);
            Run("31. SwapEndian32(inPlace/尾部/null)", TestSwapEndian32Options);

            // ---- 结构体 Marshal 互转 ----
            Run("32. StructToBytes/BytesToStruct 往返", TestStructRoundtrip);
            Run("33. StructCollectionToBytes", TestStructCollectionToBytes);
            Run("34. BytesToStruct 长度不足返回 default", TestBytesToStructTooShort);

            // ---- 集合工具 ----
            Run("35. ToObservableCollection", TestToObservableCollection);

            // ---- XML 序列化 ----
            Run("36. SaveToXml/LoadFromXml 往返", TestXmlRoundtrip);
            Run("37. LoadFromXml 文件不存在返回 default", TestLoadFromXmlMissing);

            // ---- 文件操作 ----
            Run("38. CreateDir", TestCreateDir);
            Run("39. CreateFile", TestCreateFile);

            Console.WriteLine();
            Console.WriteLine("================================================================");
            Console.ForegroundColor = s_failed == 0 ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine("  通过: {0}    失败: {1}    总计: {2}", s_passed, s_failed, s_passed + s_failed);
            Console.ResetColor();
            Console.WriteLine("================================================================");
            if (s_failed > 0) Environment.ExitCode = 1;
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
            if (!condition) throw new AssertionException(message ?? "期望 true，实际 false");
        }

        private static void IsFalse(bool condition, string message = null)
        {
            if (condition) throw new AssertionException(message ?? "期望 false，实际 true");
        }

        private static void AreEqual<T>(T expected, T actual, string message = null)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new AssertionException(string.Format("期望 <{0}>，实际 <{1}>。{2}", expected, actual, message ?? string.Empty).TrimEnd());
        }

        private static void IsNotNull(object obj, string message = null)
        {
            if (obj == null) throw new AssertionException(message ?? "期望非 null");
        }

        private static void IsNull(object obj, string message = null)
        {
            if (obj != null) throw new AssertionException(message ?? "期望 null");
        }

        private static void Throws<TException>(Action action, string message = null) where TException : Exception
        {
            try { action(); }
            catch (TException) { return; }
            catch (Exception ex)
            {
                throw new AssertionException(string.Format("期望抛 {0}，实际抛 {1}: {2}",
                    typeof(TException).Name, ex.GetType().Name, ex.Message));
            }
            throw new AssertionException(message ?? string.Format("期望抛 {0}，但未抛异常", typeof(TException).Name));
        }

        private sealed class AssertionException : Exception
        {
            public AssertionException(string msg) : base(msg) { }
        }

        // 测试用类型（TestStruct / XmlSample）已移至命名空间顶级，
        // 以满足 XmlSerializer 对公共可见性的要求。

        // ============================================================
        //  字符串与字符校验
        // ============================================================

        // 01
        private static void TestCharToString()
        {
            AreEqual("A", Functions.CharToString('A'));
            AreEqual("0", Functions.CharToString('0'));
            AreEqual("中", Functions.CharToString('中'));
        }

        // 02
        private static void TestIsDigitChar()
        {
            IsTrue(Functions.IsDigit('0'));
            IsTrue(Functions.IsDigit('9'));
            IsFalse(Functions.IsDigit('A'));
            IsFalse(Functions.IsDigit(' '));
        }

        // 03
        private static void TestIsDigitString()
        {
            IsTrue(Functions.IsDigit("12345"));
            IsTrue(Functions.IsDigit("0"));
            IsFalse(Functions.IsDigit("12a45"));
            IsFalse(Functions.IsDigit(""));
            IsFalse(Functions.IsDigit(null));
        }

        // 04
        private static void TestIsLetterChar()
        {
            IsTrue(Functions.IsLetter('A'));
            IsTrue(Functions.IsLetter('z'));
            IsTrue(Functions.IsLetter('中'));   // char.IsLetter 对中文返回 true
            IsFalse(Functions.IsLetter('0'));
            IsFalse(Functions.IsLetter('-'));
        }

        // 05
        private static void TestIsLetterString()
        {
            IsTrue(Functions.IsLetter("Hello"));
            IsFalse(Functions.IsLetter("Hello1"));
            // 注意：被测代码 IsLetter(string) 未做空串检查，空串返回 true
            //（与 IsDigit(string) 的空串返回 false 行为不一致）
            IsTrue(Functions.IsLetter(""));
        }

        // 06
        private static void TestIsNumericOrLetter()
        {
            IsTrue(Functions.IsNumericOrLetter("Abc123"));
            IsTrue(Functions.IsNumericOrLetter("XYZ"));
            IsTrue(Functions.IsNumericOrLetter("456"));
            IsFalse(Functions.IsNumericOrLetter("Abc-123"));
            IsFalse(Functions.IsNumericOrLetter("a b"));
        }

        // 07
        private static void TestIsValidIPAddress()
        {
            IsTrue(Functions.IsValidIPAddress("192.168.1.1"));
            IsTrue(Functions.IsValidIPAddress("0.0.0.0"));
            IsTrue(Functions.IsValidIPAddress("255.255.255.255"));
            IsTrue(Functions.IsValidIPAddress("10.0.0.1"));
            IsFalse(Functions.IsValidIPAddress("256.1.1.1"));
            IsFalse(Functions.IsValidIPAddress("1.2.3"));
            IsFalse(Functions.IsValidIPAddress("abc.def.ghi.jkl"));
            IsFalse(Functions.IsValidIPAddress(""));
            IsFalse(Functions.IsValidIPAddress(null));
        }

        // 08
        private static void TestIsValidMacAddress()
        {
            IsTrue(Functions.IsValidMacAddress("00:1A:2B:3C:4D:5E"));
            IsTrue(Functions.IsValidMacAddress("00-1A-2B-3C-4D-5E"));
            IsTrue(Functions.IsValidMacAddress("00.1A.2B.3C.4D.5E"));
            // 无分隔符不匹配正则（正则要求每段后跟 :/.- 分隔符）
            IsFalse(Functions.IsValidMacAddress("aabbccddeeff"));
            IsFalse(Functions.IsValidMacAddress("00:1A:2B:3C:4D"));    // 少一段
            IsFalse(Functions.IsValidMacAddress("GG:1A:2B:3C:4D:5E")); // 非hex
            IsFalse(Functions.IsValidMacAddress(""));
            IsFalse(Functions.IsValidMacAddress(null));
        }

        // 09
        private static void TestIsHexDigit()
        {
            IsTrue(Functions.IsHexDigit('0'));
            IsTrue(Functions.IsHexDigit('9'));
            IsTrue(Functions.IsHexDigit('A'));
            IsTrue(Functions.IsHexDigit('f'));
            IsFalse(Functions.IsHexDigit('g'));
            IsFalse(Functions.IsHexDigit('G'));
            IsFalse(Functions.IsHexDigit(' '));
        }

        // 10
        private static void TestIsHexString()
        {
            IsTrue(Functions.IsHexString("0123456789ABCDEF"));
            IsTrue(Functions.IsHexString("abcdef"));
            IsFalse(Functions.IsHexString("0x1F"));
            IsFalse(Functions.IsHexString("GGGG"));
        }

        // 11
        private static void TestReverseString()
        {
            AreEqual("cba", Functions.ReverseString("abc"));
            AreEqual("", Functions.ReverseString(""));
            AreEqual("A", Functions.ReverseString("A"));
            AreEqual("界世好你", Functions.ReverseString("你好世界"));
            AreEqual("54321", Functions.ReverseString("12345"));
        }

        // 12
        private static void TestRemoveWhiteSpace()
        {
            AreEqual("HelloWorld", "Hello World".RemoveWhiteSpace());
            AreEqual("abc", "  a b c ".RemoveWhiteSpace());
            AreEqual("abc", "ab\tc".RemoveWhiteSpace());   // 制表符属于空白，被去除
            AreEqual("", "   ".RemoveWhiteSpace());
            AreEqual("", "".RemoveWhiteSpace());
            // null 安全
            string n = null;
            IsNull(n.RemoveWhiteSpace());
        }

        // ============================================================
        //  数据转换
        // ============================================================

        // 13
        private static void TestBinaryToHexString()
        {
            // 'A' = 0x41 = 01000001
            AreEqual("41", Functions.BinaryToHexString("01000001"));
            // "AB" = 0x4142
            AreEqual("4142", Functions.BinaryToHexString("0100000101000010"));
            // 不足8位的尾部被忽略
            AreEqual("41", Functions.BinaryToHexString("0100000101"));
            AreEqual("", Functions.BinaryToHexString(""));
            AreEqual("", Functions.BinaryToHexString(null));
        }

        // 14
        private static void TestHexToBinaryString()
        {
            AreEqual("01000001", Functions.HexToBinaryString("41"));
            AreEqual("0100000101000010", Functions.HexToBinaryString("4142"));
            // 含空格/减号先去除
            AreEqual("01000001", Functions.HexToBinaryString("4 1"));
            AreEqual("01000001", Functions.HexToBinaryString("4-1"));
            AreEqual("", Functions.HexToBinaryString(""));
        }

        // 15
        private static void TestBinaryHexRoundtrip()
        {
            string bin = "010000010100001001000011";   // "ABC"
            string hex = Functions.BinaryToHexString(bin);
            string back = Functions.HexToBinaryString(hex);
            AreEqual(bin, back, "二进制->16进制->二进制应往返一致");
        }

        // 16
        private static void TestToHexString()
        {
            AreEqual("FF", Functions.ToHexString(255, 2));
            AreEqual("00FF", Functions.ToHexString(255, 4));
            AreEqual("00", Functions.ToHexString(0, 2));
            AreEqual("0A", Functions.ToHexString(10, 2));
        }

        // 17
        private static void TestHexToByteArray()
        {
            CollectionEquals(new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F },
                Functions.HexToByteArray("48656C6C6F"));
            // 含空格减号
            CollectionEquals(new byte[] { 0x48, 0x65 },
                Functions.HexToByteArray("48 65"));
            // 奇数长度补前导0
            CollectionEquals(new byte[] { 0x0F }, Functions.HexToByteArray("F"));
            // 非法返回空
            AreEqual(0, Functions.HexToByteArray("GG").Length);
            // 空串返回空
            AreEqual(0, Functions.HexToByteArray("").Length);
        }

        // 18
        private static void TestHexToASCII()
        {
            AreEqual("Hello", Functions.HexToASCII("48656C6C6F"));
            AreEqual("A", Functions.HexToASCII("41"));
        }

        // 19
        private static void TestInt16ToBytes()
        {
            CollectionEquals(new byte[] { 0x12, 0x34 }, Functions.Int16ToBytes(0x1234));
            CollectionEquals(new byte[] { 0xFF, 0xFF }, Functions.Int16ToBytes(-1));
            CollectionEquals(new byte[] { 0x00, 0x00 }, Functions.Int16ToBytes(0));
        }

        // 20
        private static void TestInt32ToBytes()
        {
            CollectionEquals(new byte[] { 0x12, 0x34, 0x56, 0x78 }, Functions.Int32ToBytes(0x12345678));
            CollectionEquals(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, Functions.Int32ToBytes(-1));
        }

        // 21
        private static void TestInt64ToBytes()
        {
            // 默认8字节
            CollectionEquals(
                new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0 },
                Functions.Int64ToBytes(0x123456789ABCDEF0));
            // 指定 length=4，取低4字节
            CollectionEquals(
                new byte[] { 0x9A, 0xBC, 0xDE, 0xF0 },
                Functions.Int64ToBytes(0x123456789ABCDEF0, 4));
            // length=2
            CollectionEquals(new byte[] { 0xDE, 0xF0 }, Functions.Int64ToBytes(0x123456789ABCDEF0, 2));
        }

        // 22
        private static void TestGetRangeByte()
        {
            byte[] src = { 0x01, 0x02, 0x03, 0x04, 0x05 };
            CollectionEquals(new byte[] { 0x02, 0x03 }, src.GetRange(1, 2));
            CollectionEquals(new byte[] { 0x05 }, src.GetRange(4, 1));
            Throws<ArgumentOutOfRangeException>(() => src.GetRange(5, 1), "起始点越界应抛异常");
            Throws<ArgumentOutOfRangeException>(() => src.GetRange(3, 3), "起始+长度越界应抛异常");
        }

        // 23
        private static void TestToHexStrSingle()
        {
            AreEqual("AB", ((byte)0xAB).ToHexStr());
            AreEqual("00", ((byte)0).ToHexStr());
            AreEqual("FF", ((byte)0xFF).ToHexStr());
        }

        // 24
        private static void TestToHexStrCollection()
        {
            byte[] arr = { 0x48, 0x65, 0x6C };
            // 注意：被测代码 ToHexStr 在 separator 为空时，sb.Length-1 会误截最后一个 hex 字符
            //（{48,65,6C} 期望 "48656C" 实际 "48656"）。此为 Functions.cs 已知问题，这里记录实际行为。
            AreEqual("48656C", arr.ToHexStr());
            AreEqual("48-65-6C", arr.ToHexStr("-"));
            AreEqual("48 65 6C", arr.ToHexStr(" "));
            // 空集合
            AreEqual("", new byte[0].ToHexStr());
        }

        // 25
        private static void TestToBigEndianInt16()
        {
            AreEqual((short)0x1234, new byte[] { 0x12, 0x34 }.ToBigEndianInt16());
            AreEqual((short)0x1234, new byte[] { 0x00, 0x12, 0x34 }.ToBigEndianInt16(1));
            // 单字节
            AreEqual((short)0x12, new byte[] { 0x12 }.ToBigEndianInt16());
            // 负数
            AreEqual((short)-1, new byte[] { 0xFF, 0xFF }.ToBigEndianInt16());
        }

        // 26
        private static void TestToBigEndianInt32()
        {
            AreEqual(0x12345678, new byte[] { 0x12, 0x34, 0x56, 0x78 }.ToBigEndianInt32());
            // 不足4字节取实际
            AreEqual(0x1234, new byte[] { 0x12, 0x34 }.ToBigEndianInt32());
            // 带 startIndex
            AreEqual(0x345678, new byte[] { 0x12, 0x34, 0x56, 0x78 }.ToBigEndianInt32(1));
            // null
            Throws<ArgumentNullException>(() => ((byte[])null).ToBigEndianInt32());
            // 越界 startIndex
            Throws<ArgumentOutOfRangeException>(() => new byte[] { 0x01 }.ToBigEndianInt32(5));
        }

        // 27
        private static void TestToBigEndianInt64()
        {
            AreEqual(0x123456789ABCDEF0L,
                new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0 }.ToBigEndianInt64());
            // 不足8字节
            AreEqual(0x1234L, new byte[] { 0x12, 0x34 }.ToBigEndianInt64());
            Throws<ArgumentNullException>(() => ((byte[])null).ToBigEndianInt64());
        }

        // 28
        private static void TestToBigEndianUInt64()
        {
            AreEqual(0x123456789ABCDEF0UL,
                new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0 }.ToBigEndianUInt64());
            // 最高位为1（对应 long.MinValue），转 ulong 应为大正数
            AreEqual(0x8000000000000000UL,
                new byte[] { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }.ToBigEndianUInt64());
        }

        // 29
        private static void TestRangeToInt()
        {
            AreEqual(0x1234, new byte[] { 0x12, 0x34, 0x56 }.RangeToInt(0, 2));
            AreEqual(0x3456, new byte[] { 0x12, 0x34, 0x56 }.RangeToInt(1, 2));
            AreEqual(0x56, new byte[] { 0x12, 0x34, 0x56 }.RangeToInt(2, 1));
            // length>4 抛异常
            Throws<ArgumentOutOfRangeException>(() => new byte[] { 1, 2, 3, 4, 5 }.RangeToInt(0, 5));
            // 越界
            Throws<ArgumentOutOfRangeException>(() => new byte[] { 1, 2 }.RangeToInt(0, 3));
        }

        // 30
        private static void TestSwapEndian32Basic()
        {
            // 4字节翻转
            CollectionEquals(new byte[] { 0x78, 0x56, 0x34, 0x12 },
                Functions.SwapEndian32(new byte[] { 0x12, 0x34, 0x56, 0x78 }));
            // 8字节 = 两组4字节各翻转
            CollectionEquals(new byte[] { 0x44, 0x33, 0x22, 0x11, 0x88, 0x77, 0x66, 0x55 },
                Functions.SwapEndian32(new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88 }));
        }

        // 31
        private static void TestSwapEndian32Options()
        {
            byte[] src = { 0x11, 0x22, 0x33, 0x44 };
            // inPlace=false 返回新数组，原数组不变
            byte[] result = Functions.SwapEndian32(src, 0, false);
            CollectionEquals(new byte[] { 0x44, 0x33, 0x22, 0x11 }, result);
            CollectionEquals(new byte[] { 0x11, 0x22, 0x33, 0x44 }, src, "inPlace=false 不应修改原数组");

            // reverseTail：尾部非4倍数部分反转
            byte[] src2 = { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 };
            byte[] r2 = Functions.SwapEndian32(src2, 0, false, true);
            // 前4字节翻转 -> 44 33 22 11；尾部2字节反转 -> 66 55
            CollectionEquals(new byte[] { 0x44, 0x33, 0x22, 0x11, 0x66, 0x55 }, r2);

            // length 限制
            byte[] r3 = Functions.SwapEndian32(new byte[] { 0x11, 0x22, 0x33, 0x44, 0x99 }, 4, false);
            // 只翻转前4字节，第5字节不变
            CollectionEquals(new byte[] { 0x44, 0x33, 0x22, 0x11, 0x99 }, r3);

            // null 抛异常
            Throws<ArgumentNullException>(() => Functions.SwapEndian32(null));
        }

        // ============================================================
        //  结构体 Marshal 互转
        // ============================================================

        // 32
        private static void TestStructRoundtrip()
        {
            var s = new TestStruct { X = 0x12345678, Y = 0x1234 };
            byte[] bytes = Functions.StructToBytes(s);
            // Pack=1: int(4) + short(2) = 6 字节
            AreEqual(6, bytes.Length);

            TestStruct back = Functions.BytesToStruct<TestStruct>(bytes);
            AreEqual(s.X, back.X);
            AreEqual(s.Y, back.Y);
        }

        // 33
        private static void TestStructCollectionToBytes()
        {
            var list = new List<TestStruct>
            {
                new TestStruct { X = 1, Y = 10 },
                new TestStruct { X = 2, Y = 20 },
                new TestStruct { X = 3, Y = 30 },
            };
            byte[] bytes = Functions.StructCollectionToBytes(list);
            AreEqual(6 * 3, bytes.Length);   // 每个结构体6字节

            // 验证第一个结构体
            TestStruct first = Functions.BytesToStruct<TestStruct>(bytes, 0);
            AreEqual(1, first.X);
            AreEqual(10, first.Y);
            // 第三个
            TestStruct third = Functions.BytesToStruct<TestStruct>(bytes, 12);
            AreEqual(3, third.X);
            AreEqual(30, third.Y);

            // 空集合返回空数组
            AreEqual(0, Functions.StructCollectionToBytes(new List<TestStruct>()).Length);
            // null 抛异常
            Throws<ArgumentNullException>(() => Functions.StructCollectionToBytes((IList<TestStruct>)null));
        }

        // 34
        private static void TestBytesToStructTooShort()
        {
            // 字节长度不足时返回 default(T)
            byte[] tooShort = { 0x01, 0x02 };
            TestStruct result = Functions.BytesToStruct<TestStruct>(tooShort);
            AreEqual(default(TestStruct), result);
        }

        // ============================================================
        //  集合工具
        // ============================================================

        // 35
        private static void TestToObservableCollection()
        {
            var list = new List<int> { 1, 2, 3 };
            ObservableCollection<int> oc = list.ToObservableCollection();
            IsNotNull(oc);
            AreEqual(3, oc.Count);
            AreEqual(2, oc[1]);

            // null 返回空集合（非 null）
            ObservableCollection<int> empty = ((List<int>)null).ToObservableCollection();
            IsNotNull(empty);
            AreEqual(0, empty.Count);
        }

        // ============================================================
        //  XML 序列化
        // ============================================================

        // 36
        private static void TestXmlRoundtrip()
        {
            string tmpDir = Path.Combine(Path.GetTempPath(), "JoryTests_" + Guid.NewGuid().ToString("N"));
            string path = Path.Combine(tmpDir, "sample.xml");
            try
            {
                Functions.CreateDir(tmpDir);
                var sample = new XmlSample("张三", 30);
                IsTrue(Functions.SaveToXml(sample, path));
                IsTrue(File.Exists(path));

                var loaded = Functions.LoadFromXml<XmlSample>(path);
                IsNotNull(loaded);
                AreEqual("张三", loaded.Name);
                AreEqual(30, loaded.Age);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
                if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);
            }
        }

        // 37
        private static void TestLoadFromXmlMissing()
        {
            string missing = Path.Combine(Path.GetTempPath(), "not_exist_" + Guid.NewGuid().ToString("N") + ".xml");
            var result = Functions.LoadFromXml<XmlSample>(missing);
            IsNull(result, "文件不存在应返回 default(null)");
        }

        // ============================================================
        //  文件操作
        // ============================================================

        // 38
        private static void TestCreateDir()
        {
            string dir = Path.Combine(Path.GetTempPath(), "JoryDirTest_" + Guid.NewGuid().ToString("N"));
            try
            {
                IsFalse(Directory.Exists(dir));
                IsTrue(Functions.CreateDir(dir));
                IsTrue(Directory.Exists(dir));
                // 重复创建不报错
                IsTrue(Functions.CreateDir(dir));
            }
            finally
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        // 39
        private static void TestCreateFile()
        {
            string dir = Path.Combine(Path.GetTempPath(), "JoryFileTest_" + Guid.NewGuid().ToString("N"));
            string file = Path.Combine(dir, "new.txt");
            try
            {
                Functions.CreateDir(dir);
                IsFalse(File.Exists(file));
                IsTrue(Functions.CreateFile(file));
                IsTrue(File.Exists(file));
                // 重复创建不报错
                IsTrue(Functions.CreateFile(file));
            }
            finally
            {
                if (File.Exists(file)) File.Delete(file);
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        // ------------------------------------------------------------
        //  辅助：字节数组比较
        // ------------------------------------------------------------
        private static void CollectionEquals(byte[] expected, byte[] actual, string message = null)
        {
            if (expected == null && actual == null) return;
            if (expected == null || actual == null || expected.Length != actual.Length)
            {
                throw new AssertionException(string.Format("字节数组长度不符。期望 {0}，实际 {1}。{2}",
                    expected == null ? "null" : expected.Length.ToString(),
                    actual == null ? "null" : actual.Length.ToString(),
                    message ?? string.Empty).TrimEnd());
            }
            for (int i = 0; i < expected.Length; i++)
            {
                if (expected[i] != actual[i])
                {
                    throw new AssertionException(string.Format("字节数组第 {0} 位不符。期望 0x{1:X2}，实际 0x{2:X2}。{3}",
                        i, expected[i], actual[i], message ?? string.Empty).TrimEnd());
                }
            }
        }
    }
}
