using Mustard.Base.Attributes.UIAttributes;
using Mustard.Base.Toolset;
using Mustard.Interfaces.Framework;
using Mustard.UI.Sunflower;
using Mustard.UI.Sunflower.ExControls;
using Mustard.UIExtension.PlotControl;

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Mustard.Demo
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    [MustardWindow(true)]
    public partial class Window1 : SunFlowerWindow
    {
        public Window1()
        {
            InitializeComponent();
            SingletonContainer<IMustardMessageManager>.Instance.Error("Test Error");
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ColorPicker.PickColor(out var color);
        }

        private void ShowWaitControl(object sender, System.Windows.RoutedEventArgs e)
        {
            using var w = new WaitControl();
            w.Wait("Message");
            Thread.Sleep(1000);
        }
    }
}
