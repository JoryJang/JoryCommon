using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
                FileStream fs;
                fs = File.Create(FilePathFull);
                fs.Close();
                File.SetAttributes(FilePathFull, FileAttributes.Normal);
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
        public static bool IsNumeric(char ch)
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
        public static bool IsNumeric(string str)
        {
            if (string.IsNullOrEmpty(str)) 
                return false;

            for (int i = 0; i < str.Length; i++)
            {
                if (IsNumeric(str[i]) == false)
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
            for (int i = 0; i < str.Length; i++)
            {
                if (IsNumeric(str[i]) == false && IsLetter(str[i]) == false)
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
        public static bool IsValidIP(string ipStr)
        {
            if (string.IsNullOrEmpty(ipStr))
            {
                return false;
            }

            Regex r = new Regex(@"^((0|1[0-9]{0,2}|2[0-9]{0,1}|2[0-4][0-9]|25[0-5]|[3-9][0-9]{0,1})\.){3}(0|1[0-9]{0,2}|2[0-9]{0,1}|2[0-4][0-9]|25[0-5]|[3-9][0-9]{0,1})$");

            return r.IsMatch(ipStr);
        }

        public static bool IsValidMac(string macStr)
        {
            if (string.IsNullOrEmpty(macStr))
            {
                return false;
            }
            string pattern = @"^([0-9A-Fa-f]{2}[:.-]){5}([0-9A-Fa-f]{2})$";
            Regex regex = new Regex(pattern);
            bool result = regex.IsMatch(macStr);
            return result;
        }

        /// <summary>
        /// 判断字符是否是16进制
        /// </summary>
        /// <param name="ch">字符串</param>
        /// <returns></returns>
        public static bool IsHexChar(char ch)
        {
            return (ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F');
            //string str = CharToString(ch);

            //Regex regex = new Regex("^[A-Fa-f]+$");
            //return regex.IsMatch(str.Trim());
        }

        /// <summary>
        /// 判断字符串是否是16进制字符
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static bool IsHexStr(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (IsHexChar(str[i]) == false)
                {
                    return false;
                }
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
            char[] arr = str.ToCharArray();
            Array.Reverse(arr);
            string reverseStr = new string(arr);
            return reverseStr;
        }

        /// <summary>
        /// 去除字符串里所有空格
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveWhiteSpace(this string str)
        {
            return new string(str.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        #endregion 字符串与字符校验


        #region XML 序列化

        /// <summary>
        ///xml转集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T LoadFromXml<T>(string path)
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
        public static bool SaveToXml<T>(T ob, string path)
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
        public static string[] GetComList()
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
        /// 更新UI数据
        /// </summary>
        /// <param name="action"></param>
        public static void UpdateUIData(Action action)
        {
            if (System.Windows.Application.Current == null || action == null)
            {
                return;
            }

            if (IsMainThread(Application.Current.Dispatcher.Thread))
            {
                action();
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.Invoke(action);
            }
        }


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
        /// 在 UI 线程上异步执行操作（不阻塞调用线程），返回可 await 的 Task。
        /// 若当前已在 UI 线程或调度器不可用，则直接执行。
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

        /// <summary>
        /// WPF当前线程是否主线程
        /// </summary>
        /// <returns></returns>
        public static bool IsMainThread(System.Threading.Thread thread)
        {
            return thread == System.Threading.Thread.CurrentThread;
        }

        #endregion UI 线程调度


        #region 可视化树查找

        //查找对象下第一个指定类型和名称的可视子对象
        public static T FindFirstVisualChild<T>(DependencyObject obj, string childName) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if ((child != null) && (child is T) && child.GetValue(FrameworkElement.NameProperty).ToString() == childName)
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

        //查找对象之上第一个指定类型的祖先对象
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
        /// <param name="strcutType">结构体类型</param>
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
        public static byte[] ReversePerUInt32(byte[] source, int length = 0, bool inPlace = true, bool reverseTail = false)
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
        public static string Convert10To16String(int dataInt, int Len = 2)
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
            if (hex.Length <= 0)
            {
                return new byte[] { };
            }

            hex = hex.Replace(" ", "").Replace("-", "");

            if (!IsHexStr(hex))
            {
                return new byte[] { };
            }

            if (hex.Length % 2 != 0)
            {
                hex = string.Format("0{0}", hex);
            }

            byte[] buff = new byte[hex.Length / 2];
            int index = 0;
            for (int i = 0; i < hex.Length; i += 2)
            {
                buff[index] = Convert.ToByte(hex.Substring(i, 2), 16);
                ++index;
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
            if (sourceIndex < 0 || sourceIndex >= sourceArray.Length)
            {
                throw new ArgumentOutOfRangeException("起始点小于0或大于数组长度");
            }

            if (sourceIndex + length > sourceArray.Length)
            {
                throw new ArgumentOutOfRangeException("起始点加截取长度大于数组长度");
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
            Array destinationArray = new Array[length];

            if (sourceIndex < 0 || sourceIndex >= sourceArray.Length)
            {
                throw new ArgumentOutOfRangeException("起始点小于0或大于数组长度");
            }

            if (sourceIndex + length > sourceArray.Length)
            {
                throw new ArgumentOutOfRangeException("起始点加截取长度大于数组长度");
            }

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
            StringBuilder sb = new StringBuilder();
            foreach (byte b in array)
            {
                sb.Append(b.ToHexStr());

                sb.Append(separator);
            }

            string hexStr = sb.Length > 1 ? sb.ToString(0, sb.Length - 1) : sb.ToString();

            return hexStr;
        }

        /// <summary>
        /// 把byte数组转化为short，采用大端字节序，与C#相反
        /// </summary>
        /// <param name="array"></param>
        /// <param name="begIndex"></param>
        /// <returns></returns>
        public static short ToShort(this byte[] array, int begIndex = 0)
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
        public static int ToInt32(this byte[] data, int begIndex = 0)
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
                result = result | (dValue << ((length - i - 1) * 8));
            }

            return result;
        }

        /// <summary>
        /// byte数据转换成int64
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static long ToInt64(this byte[] data, int begIndex = 0)
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

        public static ulong ToUInt64(this byte[] data)
        {
            return (ulong)data.ToInt64();
        }

        /// <summary>
        /// 把指定位置和长度的byte数组转换成整形,高位在前,指定长度不能大于4
        /// </summary>
        /// <param name="array"></param>
        /// <param name="begIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int RangeToInt(this byte[] array, int begIndex, int length)
        {
            if (length > 4 || begIndex + length > array.Length)
            {
                throw new ArgumentOutOfRangeException("length");
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
