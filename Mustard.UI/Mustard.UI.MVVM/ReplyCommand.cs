using System;
using System.Windows.Input;

namespace Mustard.UI.MVVM;
public class ReplyCommand : ICommand
{
    public event EventHandler CanExecuteChanged;

    private Action action;

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        action?.Invoke();
    }

    public ReplyCommand(Action action)
    {
        this.action = action;
    }
}
