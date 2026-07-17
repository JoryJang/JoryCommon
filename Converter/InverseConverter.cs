using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Jory.Common
{
    /// <summary>
    /// 通用“取反”转换器（合并自原 InverseBoolenConverter 与 OppositeConverter，并做了扩展与优化）。
    /// 支持：bool 取反、Visibility 在 Visible 与 Collapsed 之间切换、常用数值类型取相反数。
    /// 双向绑定安全：ConvertBack 与 Convert 互为逆操作（取反是自身的逆）。
    /// </summary>
    [ValueConversion(typeof(object), typeof(object))]
    public class InverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value is bool)
                return !((bool)value);

            if (value is Visibility)
                return ((Visibility)value == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;

            if (value is double) return -((double)value);
            if (value is float) return -((float)value);
            if (value is decimal) return -((decimal)value);
            if (value is long) return -((long)value);
            if (value is int) return -((int)value);
            if (value is short) return -((short)value);
            if (value is sbyte) return -((sbyte)value);

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}
