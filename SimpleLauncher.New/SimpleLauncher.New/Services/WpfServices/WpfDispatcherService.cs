using System.Windows.Threading;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.New.Services.WpfServices;

/// <summary>
/// WPF implementation of IDispatcherService — dispatches work to the UI thread.
/// </summary>
public class WpfDispatcherService : IDispatcherService
{
    private readonly Dispatcher _dispatcher;

    public WpfDispatcherService()
    {
        _dispatcher = System.Windows.Application.Current?.Dispatcher
                      ?? Dispatcher.CurrentDispatcher;
    }

    public void Invoke(Action action)
    {
        if (_dispatcher.CheckAccess())
            action();
        else
            _dispatcher.Invoke(action);
    }

    public Task InvokeAsync(Action action)
    {
        if (_dispatcher.CheckAccess())
        {
            action();
            return Task.CompletedTask;
        }

        return _dispatcher.InvokeAsync(action).Task;
    }

    public Task<T> InvokeAsync<T>(Func<T> func)
    {
        if (_dispatcher.CheckAccess())
            return Task.FromResult(func());

        return _dispatcher.InvokeAsync(func).Task;
    }
}
