using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mustard.UI.Sunflower.Controls;

public static class ButtonStyle
{
    public static readonly DependencyProperty MouseOverBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "MouseOverBackground",
                typeof(SolidColorBrush),
                typeof(ButtonStyle),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x3C, 0x7F, 0xB1))));
    public static DependencyProperty MouseOverForegroundProperty =
            DependencyProperty.RegisterAttached(
                "MouseOverForeground",
                typeof(SolidColorBrush),
                typeof(ButtonStyle));
    public static DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached(
                "CornerRadius",
                typeof(double),
                typeof(ButtonStyle),
                new PropertyMetadata(10.0));

    public static void SetMouseOverBackground(Button button, SolidColorBrush color) => button.SetValue(MouseOverBackgroundProperty, color);

    public static SolidColorBrush GetMouseOverBackground(Button button) => (SolidColorBrush)button.GetValue(MouseOverBackgroundProperty);

    public static void SetMouseOverForeground(Button button, SolidColorBrush color) => button.SetValue(MouseOverForegroundProperty, color);

    public static SolidColorBrush GetMouseOverForeground(Button button) => (SolidColorBrush)button.GetValue(MouseOverForegroundProperty);

    public static double GetCornerRadius(Button button) => (double)button.GetValue(CornerRadiusProperty);

    public static void SetCornerRadius(Button button, double value) => button.SetValue(CornerRadiusProperty, value);
}
