using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MuhAimLabScoresViewer
{
    public class InvertedBoolToVisibilityConverterCollapse : IValueConverter
    {
        private object GetVisibility(object value)
        {
            if (!(value is bool)) return Visibility.Visible;
            bool objValue = (bool)value;
            if (objValue) return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetVisibility(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
