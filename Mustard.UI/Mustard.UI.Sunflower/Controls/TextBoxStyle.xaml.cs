using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Mustard.UI.Sunflower.Controls;

public static class TextBoxStyle
{
    public static readonly DependencyProperty PrefixProperty =
        DependencyProperty.RegisterAttached(
            "Prefix",
            typeof(object),
            typeof(TextBoxStyle));

    public static readonly DependencyProperty PrefixOpacityProperty =
       DependencyProperty.RegisterAttached(
           "PrefixOpacity",
           typeof(double),
           typeof(TextBoxStyle),
           new PropertyMetadata(0.5));

    public static readonly DependencyProperty HidePrefixWhenTextNullProperty =
        DependencyProperty.RegisterAttached(
           "HidePrefixWhenTextNull",
           typeof(bool),
           typeof(TextBoxStyle),
           new PropertyMetadata(true));

    public static readonly DependencyProperty SuperfixProperty =
        DependencyProperty.RegisterAttached(
            "Superfix",
            typeof(object),
            typeof(TextBoxStyle));

    public static readonly DependencyProperty SuperfixOpacityProperty =
       DependencyProperty.RegisterAttached(
           "SuperfixOpacity",
           typeof(double),
           typeof(TextBoxStyle),
           new PropertyMetadata(0.5));

    public static readonly DependencyProperty HideSuperfixWhenTextNullProperty =
        DependencyProperty.RegisterAttached(
           "HideSuperfixWhenTextNull",
           typeof(bool),
           typeof(TextBoxStyle),
           new PropertyMetadata(true));

    public static void SetPrefix(DependencyObject textBox, object element)
        => textBox.SetValue(PrefixProperty, element);

    public static object GetPrefix(DependencyObject textBox)
        => textBox.GetValue(PrefixProperty);

    public static void SetSuperfix(DependencyObject textBox, object element)
        => textBox.SetValue(SuperfixProperty, element);

    public static object GetSuperfix(DependencyObject textBox)
        => textBox.GetValue(SuperfixProperty);

    public static void SetPrefixOpacity(DependencyObject textBox, double opacity)
        => textBox.SetValue(PrefixOpacityProperty, opacity);

    public static double GetPrefixOpacity(DependencyObject textBox)
        => (double)textBox.GetValue(PrefixOpacityProperty);

    public static void SetSuperfixOpacity(DependencyObject textBox, double opacity)
        => textBox.SetValue(SuperfixOpacityProperty, opacity);

    public static double GetSuperfixOpacity(DependencyObject textBox)
        => (double)textBox.GetValue(SuperfixOpacityProperty);

    public static void SetHidePrefixWhenTextNull(DependencyObject textBox, bool state)
        => textBox.SetValue(HidePrefixWhenTextNullProperty, state);

    public static bool GetHidePrefixWhenTextNull(DependencyObject textBox)
        => (bool)textBox.GetValue(HidePrefixWhenTextNullProperty);

    public static void SetHideSuperfixWhenTextNull(DependencyObject textBox, bool state)
        => textBox.SetValue(HideSuperfixWhenTextNullProperty, state);

    public static bool GetHideSuperfixWhenTextNull(DependencyObject textBox)
        => (bool)textBox.GetValue(HideSuperfixWhenTextNullProperty);
}
