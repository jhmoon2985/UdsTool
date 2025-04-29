using System;
using System.Globalization;
using System.Windows.Data;
using UdsTool.Models;

namespace UdsTool.Converters
{
    public class EqualityToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;

            if (parameter is RequestResponseType paramType && value is RequestResponseType valueType)
            {
                return valueType == paramType;
            }

            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}