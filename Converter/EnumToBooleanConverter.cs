using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Jory.Common
{
    /// <summary>
    /// 枚举 &lt;-&gt; bool 转换器，常用于 RadioButton 与枚举属性双向绑定。
    /// 用法：将 ConverterParameter 设为枚举成员名（或基础值）。
    ///   Convert     ：绑定值等于参数时返回 true，否则 false。
    ///   ConvertBack ：当 bool 为 true 时返回参数对应的枚举值；为 false 时返回 UnsetValue。
    /// </summary>
    [ValueConversion(typeof(Enum), typeof(bool))]
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value && parameter != null)
            {
                try
                {
                    return Enum.Parse(targetType, parameter.ToString(), true);
                }
                catch
                {
                    return DependencyProperty.UnsetValue;
                }
            }

            return DependencyProperty.UnsetValue;
        }
    }
}
