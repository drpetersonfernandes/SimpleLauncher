namespace SimpleLauncher.Core.Interfaces;

public interface IDispatcherService
{
    Task InvokeAsync(Action action);
    Task<T> InvokeAsync<T>(Func<T> func);
    void Invoke(Action action);
}
