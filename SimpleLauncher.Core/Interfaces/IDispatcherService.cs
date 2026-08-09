namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Provides methods to marshal execution to the UI thread.
/// </summary>
public interface IDispatcherService
{
    /// <summary>
    /// Asynchronously invokes the specified action on the UI thread.
    /// </summary>
    /// <param name="action">The action to execute on the UI thread.</param>
    Task InvokeAsync(Action action);

    /// <summary>
    /// Asynchronously invokes the specified function on the UI thread and returns its result.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute on the UI thread.</param>
    /// <returns>The result of the function.</returns>
    Task<T> InvokeAsync<T>(Func<T> func);

    /// <summary>
    /// Asynchronously invokes the specified async function on the UI thread and
    /// awaits its completion (e.g. an awaited modal <c>ShowDialog</c>).
    /// </summary>
    /// <param name="func">The async function to execute on the UI thread.</param>
    /// <returns>A task that completes when the function completes.</returns>
    Task InvokeAsync(Func<Task> func);

    /// <summary>
    /// Synchronously invokes the specified action on the UI thread.
    /// </summary>
    /// <param name="action">The action to execute on the UI thread.</param>
    void Invoke(Action action);
}
