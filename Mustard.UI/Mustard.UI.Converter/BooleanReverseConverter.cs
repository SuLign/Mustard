using System.Windows;

namespace Mustard.UI.Converter;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BooleanReverseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool v)
        {
            return !v;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool v)
        {
            return !v;
        }
        return false;
    }
}

