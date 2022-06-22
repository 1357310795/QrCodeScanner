using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MyQrCodeScanner.Converters
{
    public class CheckConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return false;
            }
            string checkvalue = value.ToString();
            string targetvalue = parameter.ToString();
            bool r = checkvalue.Equals(targetvalue);
            return r;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return null;
            }

            if ((bool)value)
            {
                return parameter.ToString();
            }
            return null;
        }
    }

    public class BoolToInverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? tmp = (bool?)value;
            return !tmp.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }

}
