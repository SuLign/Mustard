using System.Windows;
using System.Windows.Media;

namespace JinWu.EasyStyle;

public static class TabItemStyle
{
    public static readonly DependencyProperty TabItemIconProperty =
        DependencyProperty.RegisterAttached(
            "TabItemIcon",
            typeof(Geometry),
            typeof(TabItemStyle));

    public static void SetTabItemIcon(DependencyObject groupBox, Geometry element)
        => groupBox.SetValue(TabItemIconProperty, element);

    public static Geometry GetTabItemIcon(DependencyObject groupBox)
        => (Geometry)groupBox.GetValue(TabItemIconProperty);
}
