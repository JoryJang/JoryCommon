using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Jory.Common
{
    /// <summary>
    /// bool -> Visibility 转换器。
    /// 参数（不区分大小写，可组合，以逗号分隔）：
    ///   "Hidden"             ：不可见时使用 Visibility.Hidden 而非默认的 Collapsed。
    ///   "Inverse"/"!"/"Not"  ：反转结果（true -> 折叠，false -> 可见）。
    /// ConvertBack 支持从 Visibility 还原为 bool（同样遵循参数）。
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible = value is bool && (bool)value;
            if (HasFlag(parameter, "inverse", "!", "not"))
                visible = !visible;

            Visibility collapsed = HasFlag(parameter, "hidden") ? Visibility.Hidden : Visibility.Collapsed;
            Visibility result = visible ? Visibility.Visible : collapsed;

            if (targetType == typeof(Visibility)) return result;
            if (targetType == typeof(bool)) return visible;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible = value is Visibility && (Visibility)value == Visibility.Visible;
            if (HasFlag(parameter, "inverse", "!", "not"))
                visible = !visible;

            if (targetType == typeof(bool)) return visible;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        internal static bool HasFlag(object parameter, params string[] keywords)
        {
            string p = parameter as string;
            if (string.IsNullOrEmpty(p)) return false;
            p = p.ToLowerInvariant();
            foreach (string k in keywords)
            {
                if (p.IndexOf(k, StringComparison.Ordinal) >= 0)
                    return true;
            }
            return false;
        }
    }
}
