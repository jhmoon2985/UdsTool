using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace UdsTool.Utils.Converters
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible = false;

            if (value is bool boolean)
            {
                visible = boolean;
            }
            else if (value != null)
            {
                visible = true;
            }

            // 파라미터가 "Invert"인 경우 결과를 반전
            if (parameter is string param && param == "Invert")
            {
                visible = !visible;
            }

            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool result = visibility == Visibility.Visible;

                // 파라미터가 "Invert"인 경우 결과를 반전
                if (parameter is string param && param == "Invert")
                {
                    result = !result;
                }

                return result;
            }

            return false;
        }
    }
}
