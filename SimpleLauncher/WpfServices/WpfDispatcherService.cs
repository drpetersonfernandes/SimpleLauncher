using System.Windows;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.WpfServices;

/// <summary>
/// WPF implementation of IDispatcherService, marshaling calls to the UI thread via Application.Current.Dispatcher.
/// </summary>
public class WpfDispatcherService : IDispatcherService
{
    /// <summary>Asynchronously invokes the specified action on the UI thread.</summary>
    public Task InvokeAsync(Action action)
    {
        return Application.Current.Dispatcher.InvokeAsync(action).Task;
    }

    /// <summary>Asynchronously invokes the specified function on the UI thread and returns its result.</summary>
    public Task<T> InvokeAsync<T>(Func<T> func)
    {
        return Application.Current.Dispatcher.InvokeAsync(func).Task;
    }

    /// <summary>Synchronously invokes the specified action on the UI thread.</summary>
    public void Invoke(Action action)
    {
        Application.Current.Dispatcher.Invoke(action);
    }
}
