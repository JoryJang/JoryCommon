using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Jory.Common
{
    /// <summary>
    /// 判断绑定值与参数是否相等 -> bool（目标类型为 Visibility 时自动转为 Visible / Collapsed）。
    /// 优先按数值比较（例如 int 1 与 double 1.0 视为相等），失败时回退到引用/值相等。
    /// 常用于高亮选中项、Tab 选中态等。单向转换器（ConvertBack 返回 UnsetValue）。
    /// </summary>
    [ValueConversion(typeof(object), typeof(bool))]
    public class EqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool equal = AreEqual(value, parameter);

            if (targetType == typeof(Visibility))
                return equal ? Visibility.Visible : Visibility.Collapsed;
            if (targetType == typeof(bool))
                return equal;

            return equal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        private static bool AreEqual(object value, object parameter)
        {
            if (value == null && parameter == null) return true;
            if (value == null || parameter == null) return false;

            if (value is IComparable && parameter is IConvertible)
            {
                try
                {
                    object converted = System.Convert.ChangeType(parameter, value.GetType(), CultureInfo.InvariantCulture);
                    return ((IComparable)value).CompareTo(converted) == 0;
                }
                catch
                {
                    // 回退到引用/值相等比较
                }
            }

            return object.Equals(value, parameter);
        }
    }
}
