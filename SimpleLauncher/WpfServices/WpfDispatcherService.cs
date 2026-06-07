using System.Windows;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.WpfServices;

public class WpfDispatcherService : IDispatcherService
{
    public Task InvokeAsync(Action action)
    {
        return Application.Current.Dispatcher.InvokeAsync(action).Task;
    }

    public Task<T> InvokeAsync<T>(Func<T> func)
    {
        return Application.Current.Dispatcher.InvokeAsync(func).Task;
    }

    public void Invoke(Action action)
    {
        Application.Current.Dispatcher.Invoke(action);
    }
}
