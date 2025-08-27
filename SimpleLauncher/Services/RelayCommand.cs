using System;
using System.Windows.Input;

namespace SimpleLauncher.Services;

public class RelayCommand(Action execute, Func<bool> canExecute = null) : ICommand
{
    private readonly Action _execute = execute;
    private readonly Func<bool> _canExecute = canExecute;

    public bool CanExecute(object parameter)
    {
        return _canExecute?.Invoke() ?? true;
    }

    public void Execute(object parameter)
    {
        _execute();
    }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}