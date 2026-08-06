using System.Runtime.ExceptionServices;

namespace SimpleLauncher.Tests.TestHelpers;

/// <summary>
/// Runs a test action on a dedicated STA thread so WPF objects (Application, MenuItem, Label, Dispatcher)
/// can be created headlessly. xUnit runs tests on MTA threads by default, which WPF does not allow.
/// </summary>
internal static class StaApartment
{
    /// <summary>
    /// Executes the specified action on a new STA thread and rethrows any exception on the calling thread.
    /// </summary>
    /// <param name="action">The test action to execute.</param>
    public static void Run(Action action)
    {
        Exception? error = null;

        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                error = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (error != null)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
        }
    }

    /// <summary>
    /// Executes the specified async action on a new STA thread, blocking until it completes,
    /// and rethrows any exception on the calling thread.
    /// </summary>
    /// <param name="action">The async test action to execute.</param>
    public static void RunAsync(Func<Task> action)
    {
        Exception? error = null;

        var thread = new Thread(() =>
        {
            try
            {
                action().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                error = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (error != null)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
        }
    }

    /// <summary>
    /// Ensures a WPF <see cref="System.Windows.Application"/> exists on the current (STA) thread so that
    /// <c>Application.Current</c> resource lookups do not throw. Creates one if none exists yet.
    /// </summary>
    public static void EnsureApplication()
    {
        if (System.Windows.Application.Current == null)
        {
            _ = new System.Windows.Application();
        }
    }
}
