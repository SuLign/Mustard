using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mustard.UI.MVVM;

public class ViewModuleBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public void Set<T>(ref T refObj, T value, [CallerMemberName] string name = null)
    {
        if (name != null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            refObj = value;
            return;
        }
        throw new ArgumentNullException("成员属性名字为空");
    }

    public void Set([CallerMemberName] string name = null)
    {
        if (name != null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            return;
        }
        throw new ArgumentNullException("成员属性名字为空");
    }
}
