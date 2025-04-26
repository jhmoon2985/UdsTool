using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UdsTool.Utils.Converters
{
    public class HexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] bytes)
            {
                return BitConverter.ToString(bytes).Replace("-", " ");
            }
            else if (value is uint uintValue)
            {
                return string.Format("0x{0:X}", uintValue);
            }
            else if (value is int intValue)
            {
                return string.Format("0x{0:X}", intValue);
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hexString)
            {
                hexString = hexString.Trim();

                if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    hexString = hexString.Substring(2);
                }

                if (targetType == typeof(uint))
                {
                    if (uint.TryParse(hexString, NumberStyles.HexNumber, culture, out uint result))
                    {
                        return result;
                    }
                }
                else if (targetType == typeof(int))
                {
                    if (int.TryParse(hexString, NumberStyles.HexNumber, culture, out int result))
                    {
                        return result;
                    }
                }
                else if (targetType == typeof(byte[]))
                {
                    // 공백으로 분리된 16진수 문자열을 바이트 배열로 변환
                    string[] hexValues = hexString.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    byte[] result = new byte[hexValues.Length];

                    for (int i = 0; i < hexValues.Length; i++)
                    {
                        if (!byte.TryParse(hexValues[i], NumberStyles.HexNumber, culture, out result[i]))
                        {
                            return null;
                        }
                    }

                    return result;
                }
            }

            return null;
        }
    }
}
