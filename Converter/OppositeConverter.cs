using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Jory.Common
{
    public class OppositeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var valueType = value.GetType();
            if (valueType == typeof(Visibility))
            {
                if ((Visibility)value == Visibility.Visible) return Visibility.Collapsed;
                else return Visibility.Visible;
            }
            else if (valueType == typeof(bool))
            {
                if ((bool)value == true) return false;
                else return true;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("单向转换。");
        }
    }
}
