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
            OnDo();

            ContentRendered += Window1_ContentRendered;
        }

        private void Window1_ContentRendered(object sender, System.EventArgs e)
        {
            new Thread(() =>
            {
                var dta = new Random();
                while (true)
                {
                    var datas = new Point[2000];
                    for (int i = 0; i < datas.Length; i++)
                    {
                        datas[i] = new Point(i, dta.NextDouble());
                    }
                    //chart.SetLineData(datas, 0);
                }
            })
            { IsBackground = true }.Start();
        }

        private void OnDo()
        {
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
