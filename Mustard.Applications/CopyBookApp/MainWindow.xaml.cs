using Mustard.Base.Attributes.UIAttributes;
using Mustard.Base.Toolset;
using Mustard.Interfaces.Framework;
using Mustard.UI.Sunflower;
using Mustard.UI.Sunflower.Controls;

using System;

namespace CopyBookApp;

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
        Console.WriteLine();
    }
}
