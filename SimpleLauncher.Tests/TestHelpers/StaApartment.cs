using System.Runtime.ExceptionServices;
using System.Windows.Threading;

namespace SimpleLauncher.Tests.TestHelpers;

/// <summary>
/// Runs test actions on a dedicated STA thread so WPF objects (Application, MenuItem, Label, Dispatcher)
/// can be created headlessly. xUnit runs tests on MTA threads by default, which WPF does not allow.
/// <para>
/// A single persistent STA thread hosts the message pump for the whole test process. The
/// <see cref="System.Windows.Application"/> (when created via <see cref="EnsureApplication"/>) lives on
/// that thread, so <c>Application.Current.Dispatcher</c> always has a live, pumping dispatcher — code under
/// test that hops onto it (BeginInvoke/Invoke) completes instead of silently dropping work or hanging forever.
/// </para>
/// </summary>
internal static class StaApartment
{
    /// <summary>
    /// Lazily creates the process-wide STA dispatcher thread and keeps it pumping until the test process exits.
    /// </summary>
    private static readonly Lazy<Dispatcher> AppDispatcher = new(CreateAppDispatcherThread);

    private static Dispatcher CreateAppDispatcherThread()
    {
        Dispatcher? dispatcher = null;
        var ready = new TaskCompletionSource();

        var thread = new Thread(() =>
        {
            try
            {
                dispatcher = Dispatcher.CurrentDispatcher;
                ready.SetResult();
                Dispatcher.Run(); // pumps messages until the process exits
            }
            catch (Exception ex)
            {
                ready.TrySetException(ex);
            }
        })
        {
            IsBackground = true,
            Name = "SimpleLauncher.Tests STA pump"
        };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        ready.Task.GetAwaiter().GetResult();
        return dispatcher!;
    }

    /// <summary>
    /// Executes the specified action on the shared STA dispatcher thread and rethrows any exception on the calling thread.
    /// </summary>
    /// <param name="action">The test action to execute.</param>
    public static void Run(Action action)
    {
        Exception? error = null;

        AppDispatcher.Value.Invoke(() =>
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

        if (error != null)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
        }
    }

    /// <summary>
    /// Executes the specified async action on the shared STA dispatcher thread, blocking until it completes,
    /// and rethrows any exception on the calling thread.
    /// </summary>
    /// <param name="action">The async test action to execute.</param>
    public static void RunAsync(Func<Task> action)
    {
        Exception? error = null;
        var done = new TaskCompletionSource();

        try
        {
            AppDispatcher.Value.BeginInvoke(async () =>
            {
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                finally
                {
                    done.SetResult();
                }
            });
        }
        catch (Exception ex)
        {
            // Dispatcher could not accept the callback (e.g. pump unavailable) - surface instead of hanging.
            done.TrySetException(ex);
        }

        done.Task.GetAwaiter().GetResult();

        if (error != null)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
        }
    }

    /// <summary>
    /// Ensures a WPF <see cref="System.Windows.Application"/> exists on the shared STA dispatcher thread so that
    /// <c>Application.Current</c> resource lookups do not throw. Creates one if none exists yet.
    /// </summary>
    public static void EnsureApplication()
    {
        AppDispatcher.Value.Invoke(() =>
        {
            if (System.Windows.Application.Current == null)
            {
                _ = new System.Windows.Application();
            }
        });
    }
}