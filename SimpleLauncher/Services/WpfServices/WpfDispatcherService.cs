using System.Windows;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Services.WpfServices;

/// <summary>
///     WPF implementation of IDispatcherService, marshaling calls to the UI thread via Application.Current.Dispatcher.
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

    /// <summary>Asynchronously invokes the specified async function on the UI thread and awaits its completion.</summary>
    public Task InvokeAsync(Func<Task> func)
    {
        return Application.Current.Dispatcher.InvokeAsync(func).Task.Unwrap();
    }

    /// <summary>Synchronously invokes the specified action on the UI thread.</summary>
    public void Invoke(Action action)
    {
        Application.Current.Dispatcher.Invoke(action);
    }
}