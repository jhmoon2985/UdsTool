using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UdsTool.Utils.Converters
{
    public class StringToHexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte byteValue)
            {
                return $"0x{byteValue:X2}";
            }
            else if (value is uint uintValue)
            {
                return $"0x{uintValue:X3}";
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hexString)
            {
                hexString = hexString.Replace("0x", "").Trim();

                if (targetType == typeof(byte))
                {
                    if (byte.TryParse(hexString, NumberStyles.HexNumber, culture, out byte result))
                    {
                        return result;
                    }
                }
                else if (targetType == typeof(uint))
                {
                    if (uint.TryParse(hexString, NumberStyles.HexNumber, culture, out uint result))
                    {
                        return result;
                    }
                }
            }
            return 0;
        }
    }
}
