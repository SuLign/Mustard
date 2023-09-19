using System.ComponentModel;

namespace Mustard.UI.Converter;

[ValueConversion(typeof(Enum), typeof(string))]
public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum e)
        {
            var valAttrs = value.GetType().GetMember(e.ToString()).FirstOrDefault(m => m.DeclaringType == value.GetType()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (valAttrs != null)
            {
                foreach (var item in valAttrs)
                {
                    if (item is DescriptionAttribute des)
                    {
                        return des.Description;
                    }
                }
            }
        }
        return value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}
