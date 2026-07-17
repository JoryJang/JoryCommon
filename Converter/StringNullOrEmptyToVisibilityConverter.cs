using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Jory.Common
{
    /// <summary>
    /// 字符串为空（null / 空 / 仅空白） -> Collapsed（或 Hidden），否则 -> Visible。
    /// 参数（不区分大小写，可组合，以逗号分隔）：
    ///   "Inverse"/"!"/"Not" ：反转（空串 -> Visible）。
    ///   "Hidden"            ：不可见时使用 Visibility.Hidden。
    /// 单向转换器（ConvertBack 返回 UnsetValue）。
    /// </summary>
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool empty = string.IsNullOrWhiteSpace(value as string);
            bool visible = BooleanToVisibilityConverter.HasFlag(parameter, "inverse", "!", "not")
                ? empty
                : !empty;

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
