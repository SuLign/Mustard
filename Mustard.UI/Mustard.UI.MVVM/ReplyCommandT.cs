using System;
using System.Windows.Input;

namespace Mustard.UI.MVVM;

public class ReplyCommand<T> : ICommand
{
    public event EventHandler CanExecuteChanged;

    private Action<T> action;

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        if (parameter is T tval)
        {
            action?.Invoke(tval);
        }
    }

    public ReplyCommand(Action<T> action)
    {
        this.action = action;
    }
}
