using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UdsTool.Utils.Converters
{
    public class ByteArrayToHexStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] bytes)
            {
                return BitConverter.ToString(bytes).Replace("-", " ");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hexString)
            {
                hexString = hexString.Replace(" ", "").Replace("-", "");

                if (hexString.Length % 2 != 0)
                {
                    return null;
                }

                var bytes = new byte[hexString.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (!byte.TryParse(hexString.Substring(i * 2, 2), NumberStyles.HexNumber, culture, out bytes[i]))
                    {
                        return null;
                    }
                }
                return bytes;
            }
            return null;
        }
    }
}
