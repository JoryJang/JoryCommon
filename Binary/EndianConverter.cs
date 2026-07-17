using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Jory.Common
{
    /// <summary>
    /// 将基数据类型转换为指定端的一个字节数组，
    /// 或将一个字节数组转换为指定端基数据类型。
    /// </summary>
    public class EndianConverter
    {
        private readonly bool _isSystemEndianMatch;

        /// <summary>
        /// 以大端
        /// </summary>
        public static readonly EndianConverter BigEndian;

        /// <summary>
        /// 以小端
        /// </summary>
        public static readonly EndianConverter LittleEndian;

        static EndianConverter()
        {
            BigEndian = new EndianConverter(EndianType.Big);
            LittleEndian = new EndianConverter(EndianType.Little);
            Default = LittleEndian;
            DefaultEndianType = EndianType.Little;
        }

        /// <summary>
        /// 以默认小端，可通过<see cref="EndianConverter.DefaultEndianType"/>重新指定默认端。
        /// </summary>
        public static EndianConverter Default { get; private set; }

        private static EndianType m_defaultEndianType;

        /// <summary>
        /// 默认大小端切换。
        /// </summary>
        public static EndianType DefaultEndianType
        {
            get => m_defaultEndianType;
            set
            {
                m_defaultEndianType = value;
                switch (value)
                {
                    case EndianType.Little:
                        Default = LittleEndian;
                        break;

                    case EndianType.Big:
                        Default = BigEndian;
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="endianType"></param>
        public EndianConverter(EndianType endianType)
        {
            this.EndianType = endianType;
            _isSystemEndianMatch = BitConverter.IsLittleEndian == (endianType == EndianType.Little);
        }

        /// <summary>
        /// 指定大小端。
        /// </summary>
        public EndianType EndianType { get; private set; }

        /// <summary>
        /// 判断当前系统是否为设置的大小端
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSystemEndianMatch()
        {
            return _isSystemEndianMatch;
        }

        #region ushort

        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(ushort value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSystemEndianMatch())
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// 转换为指定端模式的2字节转换为UInt16数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public ushort ToUInt16(byte[] buffer, int offset)
        {
            if (this.EndianType == EndianType.Little)
            {
                return (ushort)(buffer[offset] | (buffer[offset + 1] << 8));
            }
            else
            {
                return (ushort)((buffer[offset] << 8) | buffer[offset + 1]);
            }
        }

        #endregion ushort

        #region ulong

        /// <summary>
        /// 转换为指定端8字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(ulong value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSystemEndianMatch())
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// 转换为指定端模式的Ulong数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public ulong ToUInt64(byte[] buffer, int offset)
        {
            if (this.EndianType == EndianType.Little)
            {
                return ((ulong)buffer[offset]) |
                       ((ulong)buffer[offset + 1] << 8) |
                       ((ulong)buffer[offset + 2] << 16) |
                       ((ulong)buffer[offset + 3] << 24) |
                       ((ulong)buffer[offset + 4] << 32) |
                       ((ulong)buffer[offset + 5] << 40) |
                       ((ulong)buffer[offset + 6] << 48) |
                       ((ulong)buffer[offset + 7] << 56);
            }
            else
            {
                return ((ulong)buffer[offset] << 56) |
                       ((ulong)buffer[offset + 1] << 48) |
                       ((ulong)buffer[offset + 2] << 40) |
                       ((ulong)buffer[offset + 3] << 32) |
                       ((ulong)buffer[offset + 4] << 24) |
                       ((ulong)buffer[offset + 5] << 16) |
                       ((ulong)buffer[offset + 6] << 8) |
                       ((ulong)buffer[offset + 7]);
            }
        }

        #endregion ulong

        #region bool

        /// <summary>
        /// 转换为指定端1字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(bool value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// 转换为指定端模式的bool数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public bool ToBoolean(byte[] buffer, int offset)
        {
            return BitConverter.ToBoolean(buffer, offset);
        }

        #endregion bool

        #region char

        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(char value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSystemEndianMatch())
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// 转换为指定端模式的Char数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public char ToChar(byte[] buffer, int offset)
        {
            if (this.EndianType == EndianType.Little)
            {
                return (char)(buffer[offset] | (buffer[offset + 1] << 8));
            }
            else
            {
                return (char)((buffer[offset] << 8) | buffer[offset + 1]);
            }
        }

        #endregion char

        #region short

        /// <summary>
        /// 转换为指定端2字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(short value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSystemEndianMatch())
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// 转换为指定端模式的Short数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public short ToInt16(byte[] buffer, int offset)
        {
            if (this.EndianType == EndianType.Little)
            {
                return (short)(buffer[offset] | (buffer[offset + 1] << 8));
            }
            else
            {
                return (short)((buffer[offset] << 8) | buffer[offset + 1]);
            }
        }

        #endregion short

        #region int

        /// <summary>
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSystemEndianMatch())
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// 转换为指定端模式的int数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int ToInt32(byte[] buffer, int offset)
        {
            if (this.EndianType == EndianType.Little)
            {
                return buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24);
            }
            else
            {
                return (buffer[offset] << 24) | (buffer[offset + 1] << 16) | (buffer[offset + 2] << 8) | buffer[offset + 3];
            }
        }

        #endregion int

        #region long

        /// <summary>
        /// 转换为指定端8字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(long value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSystemEndianMatch())
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// 转换为指定端模式的long数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public long ToInt64(byte[] buffer, int offset)
        {
            if (this.EndianType == EndianType.Little)
            {
                return ((long)buffer[offset]) |
                       ((long)buffer[offset + 1] << 8) |
                       ((long)buffer[offset + 2] << 16) |
                       ((long)buffer[offset + 3] << 24) |
                       ((long)buffer[offset + 4] << 32) |
                       ((long)buffer[offset + 5] << 40) |
                       ((long)buffer[offset + 6] << 48) |
                       ((long)buffer[offset + 7] << 56);
            }
            else
            {
                return ((long)buffer[offset] << 56) |
                       ((long)buffer[offset + 1] << 48) |
                       ((long)buffer[offset + 2] << 40) |
                       ((long)buffer[offset + 3] << 32) |
                       ((long)buffer[offset + 4] << 24) |
                       ((long)buffer[offset + 5] << 16) |
                       ((long)buffer[offset + 6] << 8) |
                       ((long)buffer[offset + 7]);
            }
        }

        #endregion long

        #region uint

        /// <summary>
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSystemEndianMatch())
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// 转换为指定端模式的Uint数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public uint ToUInt32(byte[] buffer, int offset)
        {
            if (this.EndianType == EndianType.Little)
            {
                return (uint)(buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24));
            }
            else
            {
                return (uint)((buffer[offset] << 24) | (buffer[offset + 1] << 16) | (buffer[offset + 2] << 8) | buffer[offset + 3]);
            }
        }

        #endregion uint

        #region float

        /// <summary>
        /// 转换为指定端4字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(float value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSystemEndianMatch())
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// 转换为指定端模式的float数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public float ToSingle(byte[] buffer, int offset)
        {
            if (this.IsSystemEndianMatch())
            {
                return BitConverter.ToSingle(buffer, offset);
            }
            else
            {
                // 避免一次 Array.Copy，直接反转索引
                var bytes = new byte[4];
                bytes[0] = buffer[offset + 3];
                bytes[1] = buffer[offset + 2];
                bytes[2] = buffer[offset + 1];
                bytes[3] = buffer[offset];
                return BitConverter.ToSingle(bytes, 0);
            }
        }

        #endregion float

        #region double

        /// <summary>
        /// 转换为指定端8字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (!this.IsSystemEndianMatch())
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// 转换为指定端模式的double数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public double ToDouble(byte[] buffer, int offset)
        {
            if (this.IsSystemEndianMatch())
            {
                return BitConverter.ToDouble(buffer, offset);
            }
            else
            {
                // 避免一次 Array.Copy，直接反转索引
                var bytes = new byte[8];
                bytes[0] = buffer[offset + 7];
                bytes[1] = buffer[offset + 6];
                bytes[2] = buffer[offset + 5];
                bytes[3] = buffer[offset + 4];
                bytes[4] = buffer[offset + 3];
                bytes[5] = buffer[offset + 2];
                bytes[6] = buffer[offset + 1];
                bytes[7] = buffer[offset];
                return BitConverter.ToDouble(bytes, 0);
            }
        }

        #endregion double

        #region decimal

        /// <summary>
        /// 转换为指定端16字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetBytes(decimal value)
        {
            var bytes = DecimalConverter.ToBytes(value);
            if (!this.IsSystemEndianMatch())
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// 转换为指定端模式的<see cref="decimal"/>数据。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public decimal ToDecimal(byte[] buffer, int offset)
        {
            var bytes = new byte[16];
            if (this.IsSystemEndianMatch())
            {
                Array.Copy(buffer, offset, bytes, 0, 16);
                return DecimalConverter.FromBytes(bytes);
            }
            else
            {
                for (int i = 0; i < 16; i++)
                {
                    bytes[i] = buffer[offset + 15 - i];
                }
                return DecimalConverter.FromBytes(bytes);
            }
        }

        #endregion decimal
    }

    /// <summary>
    /// <see cref="decimal"/>与字节数组转换
    /// </summary>
    public static class DecimalConverter
    {
        /// <summary>
        /// 将<see cref="decimal"/>对象转换为固定字节长度（16）数组。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytes(decimal value)
        {
            var bits = decimal.GetBits(value);
            var bytes = new byte[16];
            // 使用 BlockCopy 替代循环，大幅提升性能
            Buffer.BlockCopy(bits, 0, bytes, 0, 16);
            return bytes;
        }

        /// <summary>
        /// 将固定字节长度（16）数组转换为<see cref="decimal"/>对象。
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static decimal FromBytes(byte[] array)
        {
            int[] bits = new int[4];
            // 使用 BlockCopy 替代循环，大幅提升性能
            Buffer.BlockCopy(array, 0, bits, 0, 16);
            return new decimal(bits);
        }
    }

    /// <summary>
    /// 大小端类型
    /// </summary>
    public enum EndianType
    {
        /// <summary>
        /// 小端模式
        /// </summary>
        Little,

        /// <summary>
        /// 大端模式
        /// </summary>
        Big
    }
}