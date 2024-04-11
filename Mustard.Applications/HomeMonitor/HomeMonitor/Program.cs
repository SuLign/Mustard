using Mustard.Base.Core;
using Mustard.Base.Toolset;
using Mustard.Interfaces.Framework;
using Mustard.UI.MVVM;
using Mustard.UI.Sunflower.Controls;

using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace HomeMonitor
{
    public class Program
    {
        [STAThread]
        static void Main(params string[] args)
        {
            Startup.AppActived += Startup_AppActived; ;
            Startup.Run();
        }

        private static void Startup_AppActived()
        {
            SingletonContainer<IMustardMessageManager>.Instance.MessageBoxEvent += Instance_MessageBoxEvent;
        }

        private static void Instance_MessageBoxEvent(string arg1, Mustard.Base.BaseDefinitions.TResult arg2)
        {
            MessageBoxResult res = MessageBoxResult.None;
            res = MustardMessageBox.Show("提示", "提示内容。", messageBoxButton: MessageBoxButton.YesNoCancel, MessageBoxResult.None, MessageBoxImage.Information);
            Debug.WriteLine(res);
        }
    }
}
