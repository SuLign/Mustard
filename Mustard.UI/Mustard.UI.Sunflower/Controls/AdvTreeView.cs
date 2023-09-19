using System.Windows;
using System.Windows.Controls;

namespace Mustard.UI.Sunflower.Controls;

public class AdvTreeView : TreeView
{
    public static new DependencyProperty SelectedItemProperty;

    static AdvTreeView()
    {
        SelectedItemProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItem",
                typeof(object),
                typeof(AdvTreeView),
                new FrameworkPropertyMetadata());
    }

    public new object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public AdvTreeView() =>
        SelectedItemChanged += AdvTreeView_SelectedItemChanged;

    private void AdvTreeView_SelectedItemChanged(
        object sender,
        RoutedPropertyChangedEventArgs<object> e)
    {
        this.SetValue(SelectedItemProperty, e.NewValue);
        SelectedItem = e.NewValue;
    }
}

