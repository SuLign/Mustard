using System.Windows.Media;

namespace Mustard.UI.Converter;

[ValueConversion(typeof(Color), typeof(Color))]
internal class ColorMaskConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Color origionColor)
        {
            if (parameter is Color toMixColor)
            {
                return Color.Add(origionColor, toMixColor);
            }
            else
            {
                return Color.Add(origionColor, new Color() { A = 25, R = 255, G = 255, B = 255 });
            }
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

