using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Jory.Common
{
    /// <summary>
    ///  INI文件操作辅助类，仅支持Windows系统
    /// </summary>
    public class INIFile
    {
        #region 字段与构造

        /// <summary>
        /// 文件路径
        /// </summary>
        public readonly string path;

        /// <summary>
        /// 传入INI文件路径构造对象
        /// </summary>
        /// <param name="iniPath">INI文件路径</param>
        public INIFile(string iniPath)
        {
            path = iniPath;
        }

        #endregion 字段与构造


        #region Win32 API (P/Invoke)

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, Byte[] retVal, int size, string filePath);

        #endregion Win32 API (P/Invoke)


        #region 基础读写

        /// <summary>
        /// 写INI文件
        /// </summary>
        /// <param name="section">分组节点</param>
        /// <param name="key">关键字</param>
        /// <param name="value">值</param>
        public void WriteValue(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, path);
        }

        /// <summary>
        /// 读取INI文件，支持指定默认值和自定义缓冲区大小（避免长值被截断）
        /// </summary>
        /// <param name="section">分组节点</param>
        /// <param name="key">关键字</param>
        /// <param name="defaultValue">键不存在或为空时返回的默认值</param>
        /// <param name="bufferSize">缓冲区字符数，默认 1024，超过将按此长度截断</param>
        /// <returns>值；未找到时返回 defaultValue</returns>
        public string ReadValue(string section, string key, string defaultValue="", int bufferSize = 1024)
        {
            StringBuilder temp = new StringBuilder(bufferSize);
            GetPrivateProfileString(section, key, defaultValue, temp, bufferSize, path);
            return temp.ToString();
        }

        /// <summary>
        /// 以字节形式读取INI文件中的值
        /// </summary>
        /// <param name="section">分组节点</param>
        /// <param name="key">关键字</param>
        /// <returns>值的字节表现形式</returns>
        public byte[] ReadBytes(string section, string key)
        {
            byte[] temp = new byte[255];
            GetPrivateProfileString(section, key, "", temp, 255, path);
            return temp;
        }

        /// <summary>
        /// 删除指定段落下的单个键
        /// </summary>
        /// <param name="section">分组节点</param>
        /// <param name="key">关键字</param>
        public void DeleteKey(string section, string key)
        {
            WritePrivateProfileString(section, key, null, path);
        }

        #endregion 基础读写


        #region 段落与键管理

        /// <summary>
        /// 删除ini文件下所有段落
        /// </summary>
        public void DeleteAllSections()
        {
            WriteValue(null, null, null);
        }

        /// <summary>
        /// 删除ini文件下指定段落下的所有键（段头仍保留）
        /// </summary>
        /// <param name="section">分组节点</param>
        public void ClearSection(string section)
        {
            WriteValue(section, null, null);
        }

        /// <summary>
        /// 批量写入一个段落下的多个键值对
        /// </summary>
        /// <param name="section">分组节点</param>
        /// <param name="values">键值对集合</param>
        public void WriteSection(string section, IDictionary<string, string> values)
        {
            if (values == null) return;
            foreach (var kvp in values)
            {
                WriteValue(section, kvp.Key, kvp.Value);
            }
        }

        #endregion 段落与键管理


        #region 枚举与查询

        /// <summary>
        /// 读取段名或键名列表（section 与 key 同时为 null 时取所有段名；key 为 null 时取指定段下所有键名）。
        /// 返回结果以 '\0' 分隔、双 '\0' 结尾，这里拆分为字符串数组。
        /// </summary>
        private string[] GetProfileStringList(string section, string key, int bufferSize = 65536)
        {
            StringBuilder temp = new StringBuilder(bufferSize);
            GetPrivateProfileString(section, key, null, temp, bufferSize, path);
            return temp.ToString().Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// 获取INI文件中所有的段（Section）名称
        /// </summary>
        /// <returns>段名数组，无段时返回空数组</returns>
        public string[] GetSectionNames()
        {
            return GetProfileStringList(null, null);
        }

        /// <summary>
        /// 获取指定段落下所有的键（Key）名称
        /// </summary>
        /// <param name="section">分组节点</param>
        /// <returns>键名数组，段不存在或无键时返回空数组</returns>
        public string[] GetKeys(string section)
        {
            return GetProfileStringList(section, null);
        }

        /// <summary>
        /// 判断指定段是否存在
        /// </summary>
        /// <param name="section">分组节点</param>
        /// <returns>存在返回 true</returns>
        public bool IsSectionExists(string section)
        {
            return GetSectionNames().Contains(section);
        }

        /// <summary>
        /// 判断指定段下是否存在某个键
        /// </summary>
        /// <param name="section">分组节点</param>
        /// <param name="key">关键字</param>
        /// <returns>存在返回 true</returns>
        public bool IsKeyExists(string section, string key)
        {
            return GetKeys(section).Contains(key);
        }

        /// <summary>
        /// 读取指定段落下所有键值对，返回字典（键→值）
        /// </summary>
        /// <param name="section">分组节点</param>
        /// <returns>键名到值的字典</returns>
        public Dictionary<string, string> GetSection(string section)
        {
            var dict = new Dictionary<string, string>();
            foreach (var key in GetKeys(section))
            {
                dict[key] = ReadValue(section, key, "", 1024);
            }
            return dict;
        }

        #endregion 枚举与查询


        #region 类型化读写

        /// <summary>
        /// 读取整数值
        /// </summary>
        /// <param name="section">分组节点</param>
        /// <param name="key">关键字</param>
        /// <param name="defaultValue">解析失败或不存在时的默认值</param>
        /// <returns>整数值</returns>
        public int ReadInt(string section, string key, int defaultValue = 0)
        {
            var s = ReadValue(section, key, defaultValue.ToString(), 1024);
            return int.TryParse(s, out var v) ? v : defaultValue;
        }

        /// <summary>
        /// 写入整数值
        /// </summary>
        public void WriteInt(string section, string key, int value)
        {
            WriteValue(section, key, value.ToString());
        }

        /// <summary>
        /// 读取布尔值（兼容 "true/false"、"1/0"、"yes/no"，大小写不敏感）
        /// </summary>
        /// <param name="section">分组节点</param>
        /// <param name="key">关键字</param>
        /// <param name="defaultValue">解析失败或不存在时的默认值</param>
        /// <returns>布尔值</returns>
        public bool ReadBool(string section, string key, bool defaultValue = false)
        {
            var s = ReadValue(section, key, "", 1024).Trim().ToLower();
            if (s == "true" || s == "1" || s == "yes") return true;
            if (s == "false" || s == "0" || s == "no") return false;
            return defaultValue;
        }

        /// <summary>
        /// 写入布尔值
        /// </summary>
        public void WriteBool(string section, string key, bool value)
        {
            WriteValue(section, key, value ? "true" : "false");
        }

        /// <summary>
        /// 读取浮点值
        /// </summary>
        /// <param name="section">分组节点</param>
        /// <param name="key">关键字</param>
        /// <param name="defaultValue">解析失败或不存在时的默认值</param>
        /// <returns>双精度浮点值</returns>
        public double ReadDouble(string section, string key, double defaultValue = 0)
        {
            var s = ReadValue(section, key, defaultValue.ToString(), 1024);
            return double.TryParse(s, out var v) ? v : defaultValue;
        }

        /// <summary>
        /// 写入浮点值
        /// </summary>
        public void WriteDouble(string section, string key, double value)
        {
            WriteValue(section, key, value.ToString());
        }

        #endregion 类型化读写
    }
}
