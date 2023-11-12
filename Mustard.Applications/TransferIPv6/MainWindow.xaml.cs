using Mustard.Base.Attributes.UIAttributes;
using Mustard.Base.Toolset;
using Mustard.Interfaces.Framework;
using Mustard.UI.Sunflower;

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace TransferIPv6;

/// <summary>
/// Window1.xaml 的交互逻辑
/// </summary>
[MustardWindow(true)]
public partial class MainWindow : SunFlowerWindow
{
    private Regex ipv6Regex;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void IPv6Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        ipv6Regex = new Regex("^((([0-9A-Fa-f]{1,4}:){1,6}:)|(([0-9A-Fa-f]{1,4}:){7}))([0-9A-Fa-f]{1,4})");
        if (!ipv6Regex.IsMatch((sender as TextBox).Text))
        {
            (sender as TextBox).BorderBrush = Brushes.OrangeRed;
        }
        else
        {
            (sender as TextBox).BorderBrush = new SolidColorBrush(Color.FromArgb(0x30, 0xDD, 0xDD, 0xDD));
        }
    }
}
