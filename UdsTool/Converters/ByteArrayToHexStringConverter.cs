﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace UdsTool.Converters
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
            throw new NotImplementedException();
        }
    }
}