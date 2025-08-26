using System;
using System.Threading.Tasks;
using System.Windows.Input;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public class AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null) : ICommand
{
    private readonly Func<Task> _execute = execute;
    private readonly Func<bool> _canExecute = canExecute;

    public bool CanExecute(object parameter)
    {
        return _canExecute?.Invoke() ?? true;
    }

    public async void Execute(object parameter)
    {
        try
        {
            await _execute();
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error executing command.");
        }
    }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}