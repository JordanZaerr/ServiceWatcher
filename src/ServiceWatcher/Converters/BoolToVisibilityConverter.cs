using System;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace ServiceWatcher.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool param;
            if (parameter != null 
                && bool.TryParse(parameter.ToString(), out param))
            {
                if (!param)
                {
                    return !((bool)value) ? Visibility.Visible : Visibility.Hidden;
                }
            }
            return ((bool)value) ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
