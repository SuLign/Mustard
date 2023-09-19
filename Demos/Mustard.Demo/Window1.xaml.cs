using Mustard.Base.Attributes.UIAttributes;
using Mustard.Base.Toolset;
using Mustard.Interfaces.Framework;
using Mustard.UI.Sunflower;

using OpenCvSharp;

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Mustard.Demo
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    [MustardWindow(true)]
    public partial class Window1 : SunFlowerWindow
    {
        VideoCapture capture;
        public Window1()
        {
            InitializeComponent();
            SingletonContainer<IMustardMessageManager>.Instance.Error("Test Error");
            OnDo();
        }

        private void OnDo()
        {
        }
    }
}
