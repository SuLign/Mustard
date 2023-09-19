using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace JinWu.EasyStyle;

public static class ButtonStyle
{
    public static readonly DependencyProperty MouseOverBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "MouseOverBackground",
                typeof(SolidColorBrush),
                typeof(Button),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(25,255,255,255))));
    public static DependencyProperty MouseOverForegroundProperty =
            DependencyProperty.RegisterAttached(
                "MouseOverForeground",
                typeof(SolidColorBrush),
                typeof(Button));

    public static void SetMouseOverBackground(Button button, SolidColorBrush color)
    {
        button.SetValue(MouseOverBackgroundProperty, color);
    }

    public static SolidColorBrush GetMouseOverBackground(Button button)
    {
        return (SolidColorBrush)button.GetValue(MouseOverBackgroundProperty);
    }

    public static void SetMouseOverForeground(Button button, SolidColorBrush color)
    {
        button.SetValue(MouseOverForegroundProperty, color);
    }

    public static SolidColorBrush GetMouseOverForeground(Button button)
    {
        return (SolidColorBrush)button.GetValue(MouseOverForegroundProperty);
    }

}
