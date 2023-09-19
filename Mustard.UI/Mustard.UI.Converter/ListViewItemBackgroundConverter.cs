using System.Windows.Controls;
using System.Windows.Media;

namespace Mustard.UI.Converter;

[ValueConversion(typeof(ListViewItem), typeof(SolidColorBrush))]
public class ListViewItemBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ListViewItem lineItem)
        {
            if (lineItem.Parent == null) new SolidColorBrush(Colors.Transparent);
            var lineIndex = lineItem.TabIndex;
            if (lineIndex % 2 == 0)
            {
                return new SolidColorBrush(Color.FromArgb(0x05, 0xda, 0xda, 0xda));
            }
        }
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

