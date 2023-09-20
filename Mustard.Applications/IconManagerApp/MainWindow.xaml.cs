using Mustard.Base.Attributes.UIAttributes;
using Mustard.Base.Toolset;
using Mustard.Interfaces.Framework;
using Mustard.UI.Sunflower;

namespace IconManagerApp;

/// <summary>
/// Window1.xaml 的交互逻辑
/// </summary>
[MustardWindow(true)]
public partial class MainWindow : SunFlowerWindow
{
    public MainWindow()
    {
        InitializeComponent();
        //SingletonContainer<IMustardMessageManager>.Instance.Error("Test Error");
        OnDo();
    }

    private void OnDo()
    {
    }

    private void SelectedChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        lstIconHost.ScrollIntoView(e.AddedItems[0]);
        lstAllIconHost.ScrollIntoView(e.AddedItems[0]);
    }
}
