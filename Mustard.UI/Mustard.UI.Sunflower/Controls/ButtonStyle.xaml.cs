using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mustard.UI.Sunflower.Controls;

public static class ButtonStyle
{
    // Mouse Over
    public static readonly DependencyProperty MouseOverBackgroundProperty =
        DependencyProperty.RegisterAttached(
            "MouseOverBackground",
            typeof(Brush),
            typeof(ButtonStyle),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x3C, 0x7F, 0xB1))));
    public static DependencyProperty MouseOverForegroundProperty =
        DependencyProperty.RegisterAttached(
            "MouseOverForeground",
            typeof(Brush),
            typeof(ButtonStyle));
    public static DependencyProperty MouseOverBorderBrushProperty =
        DependencyProperty.RegisterAttached(
            "MouseOverBorderBrush",
            typeof(Brush),
            typeof(ButtonStyle));

    // Mouse Pressed
    public static readonly DependencyProperty MousePressedBackgroundProperty =
    DependencyProperty.RegisterAttached(
        "MousePressedBackground",
        typeof(Brush),
        typeof(ButtonStyle),
        new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x3C, 0x7F, 0xB1))));
    public static DependencyProperty MousePressedForegroundProperty =
        DependencyProperty.RegisterAttached(
            "MousePressedForeground",
            typeof(Brush),
            typeof(ButtonStyle));
    public static DependencyProperty MousePressedBorderBrushProperty =
        DependencyProperty.RegisterAttached(
            "MousePressedBorderBrush",
            typeof(Brush),
            typeof(ButtonStyle));

    // Corner
    public static DependencyProperty CornerRadiusProperty =
        DependencyProperty.RegisterAttached(
            "CornerRadius",
            typeof(double),
            typeof(ButtonStyle),
            new PropertyMetadata(0.0));

    // Icon
    public static DependencyProperty ButtonIconProperty =
        DependencyProperty.RegisterAttached(
            "ButtonIcon",
            typeof(Geometry),
            typeof(ButtonStyle),
            new PropertyMetadata(null));

    public static DependencyProperty ButtonIconFillProperty =
        DependencyProperty.RegisterAttached(
            "ButtonIconFill",
            typeof(Brush),
            typeof(ButtonStyle),
            new PropertyMetadata(null));


    // Mouse Over
    public static void SetMouseOverBackground(DependencyObject button, Brush color) => button.SetValue(MouseOverBackgroundProperty, color);
    public static Brush GetMouseOverBackground(DependencyObject button) => (Brush)button.GetValue(MouseOverBackgroundProperty);
    public static void SetMouseOverForeground(DependencyObject button, Brush color) => button.SetValue(MouseOverForegroundProperty, color);
    public static Brush GetMouseOverForeground(DependencyObject button) => (Brush)button.GetValue(MouseOverForegroundProperty);
    public static void SetMouseOverBorderBrush(DependencyObject button, Brush color) => button.SetValue(MouseOverBorderBrushProperty, color);
    public static Brush GetMouseOverBorderBrush(DependencyObject button) => (Brush)button.GetValue(MouseOverBorderBrushProperty);

    // Mouse Pressed
    public static void SetMousePressedBackground(DependencyObject button, Brush color) => button.SetValue(MousePressedBackgroundProperty, color);
    public static Brush GetMousePressedBackground(DependencyObject button) => (Brush)button.GetValue(MousePressedBackgroundProperty);
    public static void SetMousePressedForeground(DependencyObject button, Brush color) => button.SetValue(MousePressedForegroundProperty, color);
    public static Brush GetMousePressedForeground(DependencyObject button) => (Brush)button.GetValue(MousePressedForegroundProperty);
    public static void SetMousePressedBorderBrush(DependencyObject button, Brush color) => button.SetValue(MousePressedBorderBrushProperty, color);
    public static Brush GetMousePressedBorderBrush(DependencyObject button) => (Brush)button.GetValue(MousePressedBorderBrushProperty);

    // Corner
    public static double GetCornerRadius(DependencyObject button) => (double)button.GetValue(CornerRadiusProperty);
    public static void SetCornerRadius(DependencyObject button, double value) => button.SetValue(CornerRadiusProperty, value);
}
