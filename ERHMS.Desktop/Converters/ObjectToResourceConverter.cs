﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ERHMS.Desktop.Converters
{
    public class ObjectToResourceConverter : IValueConverter
    {
        public ResourceDictionary Resources { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Resources[value.GetType()];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
