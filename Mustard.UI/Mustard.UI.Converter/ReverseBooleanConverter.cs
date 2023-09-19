namespace Mustard.UI.Converter;

[ValueConversion(typeof(bool), typeof(bool))]
public class ReverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool v) return !v;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool v) return !v;
        return null;
    }
}
