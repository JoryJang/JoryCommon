using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Jory.Common
{
    /// <summary>
    /// 对象为 null -> Collapsed（或 Hidden），非 null -> Visible。
    /// 参数（不区分大小写，可组合，以逗号分隔）：
    ///   "Inverse"/"!"/"Not" ：反转（null -> Visible，非 null -> 折叠）。
    ///   "Hidden"            ：不可见时使用 Visibility.Hidden。
    /// 单向转换器（ConvertBack 返回 UnsetValue）。
    /// </summary>
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = value == null;
            bool visible = BooleanToVisibilityConverter.HasFlag(parameter, "inverse", "!", "not")
                ? isNull
                : !isNull;

            Visibility collapsed = BooleanToVisibilityConverter.HasFlag(parameter, "hidden")
                ? Visibility.Hidden
                : Visibility.Collapsed;

            return visible ? Visibility.Visible : collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
