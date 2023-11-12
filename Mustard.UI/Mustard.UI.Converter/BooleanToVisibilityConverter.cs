using System.Windows;

namespace Mustard.UI.Converter;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool v)
        {
            if (parameter is bool p)
            {
                return (v & (!p)) ? Visibility.Visible : Visibility.Collapsed;
            }
            return v ? Visibility.Visible : Visibility.Collapsed;
        }
        if (value is double vi)
        {
            if (parameter is bool p)
            {
                return (vi > 0 & (!p)) ? Visibility.Visible : Visibility.Collapsed;
            }
            return vi > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}
