using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Mustard.UI.Converter;

[ValueConversion(typeof(Color), typeof(Color))]
public class ColorOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Color color)
        {
            if (double.TryParse(parameter.ToString(), out var opacity))
            {
                color.ScA = (float)opacity;
            }
            return color;
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
