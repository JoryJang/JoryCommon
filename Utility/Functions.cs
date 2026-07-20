using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Jory.Common
{
    public static class Functions
    {
        #region 文件与目录操作

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="DirPathFull">文件夹全路径</param>
        /// <returns></returns>
        public static bool CreateDir(string DirPathFull)
        {
            if (!Directory.Exists(DirPathFull))
            {
                Directory.CreateDirectory(DirPathFull);
            }

            return true;
        }

        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="FilePathFull">文件全路径</param>
        /// <returns></returns>
        public static bool CreateFile(string FilePathFull)
        {
            if (!File.Exists(FilePathFull))
            {
                using (FileStream fs = File.Create(FilePathFull))
                {
                    // using 确保即便后续操作异常，句柄也会被释放
                }
                //File.SetAttributes(FilePathFull, FileAttributes.Normal);
            }
            return true;
        }

        #endregion 文件与目录操作


        #region 字符串与字符校验

        /// <summary>
        /// 字符转换成字符串
        /// </summary>
        /// <param name="ch">字符</param>
        /// <returns></returns>
        public static string CharToString(char ch)
        {
            return ch.ToString();

            //StringBuilder sb = new StringBuilder();
            //sb.Append(ch);
            //string str = sb.ToString();
            //return str;
        }

        /// <summary>
        /// 判断是否是数字
        /// </summary>
        /// <param name="ch">字符</param>
        /// <returns></returns>
        public static bool IsDigit(char ch)
        {
            return char.IsDigit(ch);

            //string str = CharToString(ch);

            //Regex regex = new Regex("^[0-9]*[0-9][0-9]*$");
            //return regex.IsMatch(str.Trim());
        }

        /// <summary>
        /// 判断string是否全是数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDigit(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            for (int i = 0; i < str.Length; i++)
            {
                if (IsDigit(str[i]) == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 判断是否是字母
        /// </summary>
        /// <param name="ch">字符</param>
        /// <returns></returns>
        public static bool IsLetter(char ch)
        {
            return char.IsLetter(ch);

            //string str = CharToString(ch);
            //Regex regex = new Regex("^[A-Za-z]+$");
            //return regex.IsMatch(str.Trim());
        }

        /// <summary>
        /// 判断string是否全是字母
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsLetter(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            for (int i = 0; i < str.Length; i++)
            {
                if (IsLetter(str[i]) == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 判断字符串是否是由字母和数字组成
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static bool IsNumericOrLetter(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            for (int i = 0; i < str.Length; i++)
            {
                if (IsDigit(str[i]) == false && IsLetter(str[i]) == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 验证指定字符串是否是IP地址
        /// </summary>
        /// <param name="ipStr"></param>
        /// <returns></returns>
        private static readonly Regex IpAddressRegex = new Regex(
            @"^((0|1[0-9]{0,2}|2[0-9]{0,1}|2[0-4][0-9]|25[0-5]|[3-9][0-9]{0,1})\.){3}(0|1[0-9]{0,2}|2[0-9]{0,1}|2[0-4][0-9]|25[0-5]|[3-9][0-9]{0,1})$",
            RegexOptions.Compiled);

        public static bool IsValidIPAddress(string ipStr)
        {
            if (string.IsNullOrEmpty(ipStr))
            {
                return false;
            }

            return IpAddressRegex.IsMatch(ipStr);
        }

        /// <summary>
        /// 验证指定字符串是否是合法的 MAC 地址。
        /// 支持的分隔符为 : . -，例如 00:1A:2B:3C:4D:5E、00-1A-2B-3C-4D-5E。
        /// </summary>
        /// <param name="macStr">待验证的 MAC 地址字符串。</param>
        /// <returns>合法返回 true；空串或不匹配格式返回 false。</returns>
        private static readonly Regex MacAddressRegex = new Regex(
            @"^([0-9A-Fa-f]{2}[:.-]){5}([0-9A-Fa-f]{2})$",
            RegexOptions.Compiled);

        public static bool IsValidMacAddress(string macStr)
        {
            if (string.IsNullOrEmpty(macStr))
            {
                return false;
            }

            return MacAddressRegex.IsMatch(macStr);
        }

        /// <summary>
        /// 判断字符是否是16进制
        /// </summary>
        /// <param name="ch">字符串</param>
        /// <returns></returns>
        public static bool IsHexDigit(char ch)
        {
            return (ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f');
            //string str = CharToString(ch);

            //Regex regex = new Regex("^[A-Fa-f]+$");
            //return regex.IsMatch(str.Trim());
        }

        /// <summary>
        /// 判断字符串是否是16进制字符
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static bool IsHexString(string str)
        {
            // 1. 处理空字符串情况
            if (string.IsNullOrEmpty(str)) return false;

            int startIndex = 0;

            // 2. 检查并跳过 "0x" 或 "0X" 前缀
            if (str.Length >= 2 && str[0] == '0' && (str[1] == 'x' || str[1] == 'X'))
            {
                startIndex = 2;
            }

            // 3. 如果去掉前缀后字符串为空（即输入刚好是 "0x"），则不合法
            if (startIndex >= str.Length) return false;

            // 4. 从 startIndex 开始遍历检查剩余字符
            for (int i = startIndex; i < str.Length; i++)
            {
                if (!IsHexDigit(str[i])) return false;
            }

            return true;
        }

        /// <summary>
        /// 字符串反转
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReverseString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            char[] arr = str.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        /// <summary>
        /// 去除字符串里所有空格
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveWhiteSpace(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            StringBuilder sb = new StringBuilder(str.Length);

            foreach (char c in str)
            {
                if (!char.IsWhiteSpace(c))
                    sb.Append(c);
            }

            return sb.ToString();

            //return new string(str.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        #endregion 字符串与字符校验


        #region XML 序列化

        /// <summary>
        ///xml转集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T DeserializeFromXml<T>(string path)
        {
            if (!File.Exists(path))
            {
                return default(T);
            }
            try
            {
                XmlSerializer xmlzer = new XmlSerializer(typeof(T));
                using (StreamReader sr = new StreamReader(path))
                {
                    T result = (T)xmlzer.Deserialize(sr);
                    return result;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("解析文件失败，" + ex.Message);
                System.Diagnostics.Trace.TraceError(ex.Message, ex);
                return default(T);
            }
        }

        /// <summary>
        ///集合转xml
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ob"></param>
        /// <param name="path"></param>
        public static bool SerializeToXml<T>(T ob, string path)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                var dPath = Path.GetDirectoryName(path);
                if (!Directory.Exists(dPath)) Directory.CreateDirectory(dPath);

                // 使用 using 自动管理资源
                using (var fs = new FileStream(path, FileMode.Create))
                using (var sw = new StreamWriter(fs))
                {
                    serializer.Serialize(sw, ob);
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message, ex);
                return false;
            }
        }

        #endregion XML 序列化


        #region 集合工具

        /// <summary>
        /// 其他集合转ObservableCollection
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ObservableCollection<TSource> ToObservableCollection<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                return new ObservableCollection<TSource>();
            }
            return new ObservableCollection<TSource>(source);
        }

        #endregion 集合工具


        #region 串口

        /// <summary>
        /// 获取串口列表
        /// </summary>
        /// <returns></returns>
        public static string[] GetSerialPortNames()
        {
            try
            {
                return SerialPort.GetPortNames();
            }
            catch
            {
            }
            return new string[0];
        }

        #endregion 串口


        #region UI 线程调度

        /// <summary>
        /// 在 UI 线程上同步执行操作。
        /// 若当前已在 UI 线程或调度器不可用，则直接执行。
        /// </summary>
        public static void InvokeOnUiThread(Action action)
        {
            if (action == null) return;

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
                dispatcher.Invoke(action);
            else
                action();
        }

        /// <summary>
        /// 在 UI 线程上异步执行操作（不阻塞调用线程，不等待执行完成）。
        /// 若当前已在 UI 线程或调度器不可用，则直接同步执行。
        /// </summary>
        public static void BeginInvokeOnUiThread(Action action)
        {
            if (action == null) return;
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null) return;

            if (!dispatcher.CheckAccess())
            {
                // 不在UI线程，异步投递
                dispatcher.BeginInvoke(action);
            }
            else
            {
                // 在UI线程，直接执行
                action();
            }
        }

        #endregion UI 线程调度


        #region 可视化树查找

        /// <summary>
        /// 在可视化树中深度优先查找第一个匹配指定类型且 Name 相符的可视子对象。
        /// </summary>
        /// <typeparam name="T">目标依赖对象类型。</typeparam>
        /// <param name="obj">查找的起点对象。</param>
        /// <param name="childName">子对象的 Name（FrameworkElement.Name 属性值）。</param>
        /// <returns>匹配到的子对象；未找到返回 null。</returns>
        public static T FindFirstVisualChild<T>(DependencyObject obj, string childName) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T && child is FrameworkElement fe && fe.Name == childName)
                {
                    return (T)child;
                }
                else
                {
                    T childOfChild = FindFirstVisualChild<T>(child, childName);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 沿可视化树向上查找第一个匹配指定类型的祖先对象（从起点的父级开始向上搜索）。
        /// </summary>
        /// <typeparam name="T">目标祖先类型。</typeparam>
        /// <param name="obj">起始对象。</param>
        /// <returns>匹配到的祖先对象；未找到或父级链不满足类型时返回 default(T)。</returns>
        public static T FindFirstVisualAncestor<T>(DependencyObject obj) where T : DependencyObject
        {
            DependencyObject CurObj = obj;
            while ((CurObj != null) && (CurObj is T == false))
            {
                CurObj = VisualTreeHelper.GetParent(CurObj);
            }

            return (T)CurObj;
        }

        #endregion 可视化树查找


        #region 结构体与字节互转 (Marshal)

        /// <summary>
        /// 将字节数组转换为结构体
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <param name="startIndex">起始索引</param>
        /// <returns>转换出的结构体</returns>
        public static T BytesToStruct<T>(byte[] bytes, int startIndex = 0) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));

            if (size > bytes.Length - startIndex)
            {
                //返回空
                return default(T);
            }

            IntPtr buffer = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.Copy(bytes, startIndex, buffer, size);
                return (T)Marshal.PtrToStructure(buffer, typeof(T));
            }
            catch
            {
                throw;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        /// <summary>
        /// 将结构转换为字节数组
        /// </summary>
        /// <param name="structObj">结构对象</param>
        /// <returns>字节数组</returns>
        public static byte[] StructToBytes<T>(T structObj) where T : struct
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(typeof(T));
            //创建byte数组
            byte[] bytes = new byte[size];
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将结构体拷到分配好的内存空间
            Marshal.StructureToPtr(structObj, structPtr, false);
            //从内存空间拷到byte数组
            Marshal.Copy(structPtr, bytes, 0, size);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回byte数组
            return bytes;
        }

        /// <summary>
        /// 结构体集合转字节数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static byte[] StructCollectionToBytes<T>(IList<T> collection) where T : struct
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            int count = collection.Count;
            if (count == 0)
            {
                return new byte[0];
            }

            int size = Marshal.SizeOf(typeof(T));
            int totalSize = size * count;
            byte[] array = new byte[totalSize];

            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                for (int i = 0; i < count; i++)
                {
                    T item = collection[i];
                    Marshal.StructureToPtr(item, ptr, false);
                    Marshal.Copy(ptr, array, i * size, size);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return array;
        }

        #endregion 结构体与字节互转 (Marshal)


        #region 数据转换

        /// <summary>
        /// 从索引 0 开始，每 4 字节执行一次大端↔小端转换。
        /// </summary>
        /// <param name="source">原始数据。</param>
        /// <param name="length">需要转换的区域长度；≤0 表示全部。</param>
        /// <param name="inPlace">true 直接修改并返回原数组；false 返回新数组。</param>
        /// <param name="reverseTail">当数据长度不是 4 的倍数时，是否把剩余字节顺序整体翻转。</param>
        /// <returns></returns>
        public static byte[] SwapEndian32(byte[] source, int length = 0, bool inPlace = true, bool reverseTail = false)
        {
            if (source == null) throw new ArgumentNullException("source");

            int effectiveLen = length <= 0 ? source.Length : Math.Min(length, source.Length);
            byte[] target = inPlace ? source : (byte[])source.Clone();
            unsafe
            {
                fixed (byte* p = target)
                {
                    uint* pU32 = (uint*)p;               // 4-byte 对齐视图
                    int countU32 = effectiveLen >> 2;    // 整除 4 的块数

                    for (int i = 0; i < countU32; i++)
                    {
                        uint raw = pU32[i];
                        pU32[i] = (raw << 24) | ((raw & 0xFF00) << 8)
                                | ((raw >> 8) & 0xFF00) | (raw >> 24);
                    }
                }
            }

            // 尾部处理
            int tailLen = effectiveLen & 3;          // effectiveLen % 4
            if (tailLen > 0 && reverseTail)
            {
                int tailStart = effectiveLen - tailLen;
                Array.Reverse(target, tailStart, tailLen);
            }

            return target;
        }

        /// <summary>
        /// 二进制字符串转换成十六进制字符串
        /// </summary>
        /// <param name="dataBinary">二进制字符串</param>
        /// <returns></returns>
        public static string BinaryToHexString(string dataBinary)
        {
            if (string.IsNullOrEmpty(dataBinary)) return "";

            // 预估长度，减少 StringBuilder 扩容次数
            StringBuilder sb = new StringBuilder(dataBinary.Length / 4);

            for (int i = 0; i < dataBinary.Length; i += 8)
            {
                // 防止最后不足8位越界
                if (i + 8 > dataBinary.Length) break;

                string strByte = dataBinary.Substring(i, 8);
                int iValue = Convert.ToInt32(strByte, 2);
                sb.Append(iValue.ToString("X2")); // 使用格式化字符串替代手动补0
            }
            return sb.ToString();
        }

        /// <summary>
        /// 十六进制字符串转换成二进制字符串
        /// </summary>
        /// <param name="dataHex">16进制字符串</param>
        /// <returns></returns>
        public static string HexToBinaryString(string dataHex)
        {
            if (string.IsNullOrEmpty(dataHex)) return "";

            dataHex = dataHex.Replace(" ", "").Replace("-", "");
            StringBuilder sb = new StringBuilder(dataHex.Length * 4);

            for (int i = 0; i < dataHex.Length; i += 2)
            {
                if (i + 2 > dataHex.Length) break;

                string strByte = dataHex.Substring(i, 2);
                int iValue = Convert.ToInt32(strByte, 16);
                sb.Append(Convert.ToString(iValue, 2).PadLeft(8, '0')); // 使用 PadLeft 替代 while 循环
            }
            return sb.ToString();
        }

        /// <summary>
        /// 10进制整数转换成十六进制字符串
        /// </summary>
        /// <param name="DataInt">10进制</param>
        /// <param name="Len">长度</param>
        /// <returns></returns>
        public static string ToHexString(int dataInt, int Len = 2)
        {
            //string Value16 = Convert.ToString(DataInt, 16);
            ////补足位数
            //while (Value16.Length < Len)
            //{
            //    Value16 = "0" + Value16;
            //}
            ////转换成大写
            //Value16 = Value16.ToUpper();
            //return Value16;

            return dataInt.ToString($"X{Len}");
        }

        /// <summary>
        /// 16进制转Byte数组
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] HexToByteArray(string hex)
        {
            if (string.IsNullOrEmpty(hex))
            {
                return new byte[0];
            }

            // 1. 去除空格和连字符
            hex = hex.Replace(" ", "").Replace("-", "");

            // 2. 检查并跳过 "0x" 或 "0X" 前缀
            int startIndex = 0;
            if (hex.Length >= 2 && hex[0] == '0' && (hex[1] == 'x' || hex[1] == 'X'))
            {
                startIndex = 2;
            }

            int effectiveLength = hex.Length - startIndex;
            if (effectiveLength <= 0)
            {
                return new byte[0];
            }

            // 3. 奇数长度补0（在有效内容前面补0，而不是整个字符串前面）
            if (effectiveLength % 2 != 0)
            {
                hex = hex.Insert(startIndex, "0");
                effectiveLength++;
            }

            byte[] buff = new byte[effectiveLength / 2];
            int index = 0;

            // 4. 单次遍历解析，若遇到非法字符直接返回空数组
            for (int i = startIndex; i < hex.Length; i += 2)
            {
                // 提取2个字符
                string byteStr = hex.Substring(i, 2);

                // 校验这两个字符是否为合法的16进制字符
                if (!IsHexDigit(byteStr[0]) || !IsHexDigit(byteStr[1]))
                {
                    return new byte[0]; // 发现非法字符，直接终止并返回空数组
                }

                buff[index] = Convert.ToByte(byteStr, 16);
                index++;
            }

            return buff;
        }

        /// <summary>
        /// 16进制转ASCII
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static string HexToASCII(string hex)
        {
            byte[] buff = HexToByteArray(hex);
            string result = Encoding.ASCII.GetString(buff);
            return result;
        }

        /// <summary>
        /// 把整形转换成2个byte的数组,高位在前低位在后,05--&gt;0005
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Int16ToBytes(short data)
        {
            byte[] array = new byte[2];
            array[0] = (byte)(data >> 8);
            array[1] = (byte)data;
            return array;

            //byte[] array = BitConverter.GetBytes(data);
            //Array.Reverse(array);
            //return array;
        }

        /// <summary>
        /// 把整形转换成4个byte的数组,高位在前低位在后,05--&gt;00000005
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Int32ToBytes(int data)
        {
            byte[] array = new byte[4];
            array[0] = (byte)(data >> 24);
            array[1] = (byte)(data >> 16);
            array[2] = (byte)(data >> 8);
            array[3] = (byte)data;
            return array;
        }

        /// <summary>
        /// 把数据转换成指定长度2,4,8的比特数组
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Int64ToBytes(long data, int length = 8)
        {
            byte[] array = new byte[length];
            for (int i = 0; i < length; i++)
            {
                int shift = (length - i - 1) * 8;
                array[i] = (byte)(data >> shift);
            }

            return array;
        }

        /// <summary>
        /// 根据起始位置和长度获取byte数组
        /// </summary>
        /// <param name="sourceArray"></param>
        /// <param name="sourceIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] GetRange(this byte[] sourceArray, int sourceIndex, int length)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException(nameof(sourceArray));
            }

            if (sourceIndex < 0 || sourceIndex > sourceArray.Length || length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex), "起始点小于0或大于数组长度");
            }

            if (sourceIndex + length > sourceArray.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "起始点加截取长度大于数组长度");
            }

            if (length == 0)
            {
                return new byte[0];
            }

            byte[] destinationArray = new byte[length];
            Buffer.BlockCopy(sourceArray, sourceIndex, destinationArray, 0, length);
            return destinationArray;
        }

        /// <summary>
        /// 根据起始位置和长度获取数组
        /// </summary>
        /// <param name="sourceArray"></param>
        /// <param name="sourceIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static Array GetRange(this Array sourceArray, int sourceIndex, int length)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException(nameof(sourceArray));
            }

            if (sourceIndex < 0 || sourceIndex > sourceArray.Length || length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex), "起始点小于0或大于数组长度");
            }

            if (sourceIndex + length > sourceArray.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "起始点加截取长度大于数组长度");
            }

            Array destinationArray = Array.CreateInstance(sourceArray.GetType().GetElementType(), length);
            Array.Copy(sourceArray, sourceIndex, destinationArray, 0, length);
            return destinationArray;
        }

        #region Byte[]操作

        /// <summary>
        /// 将当前System.Byte对象的值转换为它的等效16进制字符串表示形式。
        /// </summary>
        /// <param name="b">一个System.Byte对象</param>
        /// <returns>返回转换后等效的16进制的字符串</returns>
        public static string ToHexStr(this byte b)
        {
            return b.ToString("X2");
        }

        /// <summary>
        /// 将此实例的字节集合转成为它的等效的16进制字符串表示形式。
        /// </summary>
        /// <param name="array">字节集合</param>
        /// <returns>返回转换后等效的16进制的字符串</returns>
        public static string ToHexStr(this IEnumerable<byte> array, string separator = "")
        {
            if (array == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            bool isFirst = true;
            foreach (byte b in array)
            {
                if (!isFirst && !string.IsNullOrEmpty(separator))
                {
                    sb.Append(separator);
                }
                sb.AppendFormat("{0:X2}", b);
                isFirst = false;
            }

            return sb.ToString();
        }

        /// <summary>
        /// 把byte数组转化为short，采用大端字节序，与C#相反
        /// </summary>
        /// <param name="array"></param>
        /// <param name="begIndex"></param>
        /// <returns></returns>
        public static short ToBigEndianInt16(this byte[] array, int begIndex = 0)
        {
            if (begIndex + 2 <= array.Length)
            {
                //高位在前
                return (short)(array[begIndex] << 8 | array[begIndex + 1]);
            }
            else if (begIndex == array.Length - 1)
            {
                return array[begIndex];
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// byte数据转换成整形,高位在前
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int ToBigEndianInt32(this byte[] data, int begIndex = 0)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (begIndex < 0 || begIndex >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(begIndex));

            int length = data.Length - begIndex > 4 ? 4 : data.Length - begIndex;

            int result = 0;

            for (int i = 0; i < length; i++)
            {
                byte dValue = data[i + begIndex];
                result = result |(dValue << ((length - i - 1) * 8));
            }

            return result;
        }

        /// <summary>
        /// byte数据转换成int64
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static long ToBigEndianInt64(this byte[] data, int begIndex = 0)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (begIndex < 0 || begIndex >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(begIndex));

            int length = data.Length - begIndex > 8 ? 8 : data.Length - begIndex;

            long result = 0;

            for (int i = 0; i < length; i++)
            {
                long dValue = data[i + begIndex];
                result = result | (dValue << ((length - i - 1) * 8));
            }

            return result;
        }

        /// <summary>
        /// 将字节数组按大端字节序转换为 64 位无符号整数（UInt64）。
        /// </summary>
        /// <param name="data">字节数组（一般应为 8 字节，不足时按实际长度读取）。</param>
        /// <returns>转换后的 UInt64 值。</returns>
        public static ulong ToBigEndianUInt64(this byte[] data)
        {
            return (ulong)data.ToBigEndianInt64();
        }

        /// <summary>
        /// 把指定位置和长度的byte数组转换成整形,高位在前,指定长度不能大于4
        /// </summary>
        /// <param name="array"></param>
        /// <param name="begIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int SubarrayToBigEndianInt32(this byte[] array, int begIndex, int length)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (begIndex < 0 || length < 0 || length > 4 || begIndex + length > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "length不能大于4，且起始点加长度不能超过数组长度");
            }

            int result = 0;

            for (int i = 0; i < length; i++)
            {
                byte dValue = array[i + begIndex];
                result = result | (dValue << ((length - i - 1) * 8));
            }

            return result;
        }

        #endregion Byte[]操作

        #endregion 数据转换
    }
}
