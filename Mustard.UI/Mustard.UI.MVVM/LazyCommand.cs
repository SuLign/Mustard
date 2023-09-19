using System;
using System.Linq.Expressions;
using System.Windows.Input;

namespace Mustard.UI.MVVM;

public class LazyCommand : ICommand
{
    private Lazy<Action> _action;

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        _action?.Value?.Invoke();
    }

    public LazyCommand(Action action)
    {
        _action = new Lazy<Action>(() => action);
    }

    public static implicit operator LazyCommand (Action action)
    {
        return new LazyCommand(action);
    }
}
