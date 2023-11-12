using Mustard.Base.Attributes.UIAttributes;
using Mustard.Base.Toolset;
using Mustard.Interfaces.Framework;
using Mustard.UI.Sunflower;

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
        }

        private void OnDo()
        {
        }
    }
}
