using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UdsTool.Utils.Converters
{
    public class BoolToDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTransmitted)
            {
                return isTransmitted ? "송신" : "수신";
            }
            return "알 수 없음";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 문자열을 bool로 변환
            if (value is string direction)
            {
                if (direction == "송신")
                    return true;
                else if (direction == "수신")
                    return false;
            }

            // 기본값 반환
            return false;
        }
    }
}
