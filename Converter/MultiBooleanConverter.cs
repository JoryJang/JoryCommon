using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Jory.Common
{
    /// <summary>
    /// 多值 bool 组合转换器（IMultiValueConverter），用于 MultiBinding。
    /// 参数（不区分大小写）：
    ///   "Any" ：任意一个为 true 即 true（默认 "All"：全部为 true 才为 true）。
    /// 目标类型为 Visibility 时，true -> Visible，false -> Collapsed。
    /// ConvertBack 不支持（返回 null）。
    /// </summary>
    [ValueConversion(typeof(object[]), typeof(bool))]
    public class MultiBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool any = BooleanToVisibilityConverter.HasFlag(parameter, "any");
            bool result;

            if (any)
            {
                result = false;
                if (values != null)
                {
                    foreach (object v in values)
                    {
                        if (v is bool && (bool)v)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                result = true;
                if (values != null)
                {
                    foreach (object v in values)
                    {
                        if (!(v is bool && (bool)v))
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }

            if (targetType == typeof(Visibility))
                return result ? Visibility.Visible : Visibility.Collapsed;

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
