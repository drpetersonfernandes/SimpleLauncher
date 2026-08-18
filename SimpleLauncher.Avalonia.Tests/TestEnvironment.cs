using System.Text;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Shared test infrastructure: initializes the Avalonia headless platform on a
/// dedicated UI thread (whose dispatcher pump runs for the whole test session),
/// so Bitmap / TrayIcon / Dispatcher-dependent code can be tested without a display.
/// </summary>
internal static class HeadlessAvalonia
{
    private static readonly object Sync = new();
    private static bool _initialized;

    /// <summary>
    /// Initializes the headless Avalonia platform once per test session. The platform
    /// is set up on a dedicated background "UI thread" that continuously pumps the
    /// dispatcher, so worker threads can safely post to Dispatcher.UIThread.
    /// </summary>
    public static void EnsureInitialized()
    {
        if (_initialized) return;

        lock (Sync)
        {
            if (_initialized) return;

            var ready = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var uiThread = new Thread(() =>
            {
                AppBuilder.Configure<Application>()
                    .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                    .SetupWithoutStarting();

                // Force the UI dispatcher to be created on this thread BEFORE the
                // ready signal, so Dispatcher.UIThread is owned by the pump below.
                _ = Dispatcher.UIThread;
                ready.SetResult(true);

                while (true)
                {
                    Dispatcher.UIThread.RunJobs();
                    Thread.Sleep(1);
                }
            })
            { IsBackground = true, Name = "AvaloniaTestUiThread" };
            uiThread.Start();

            if (!ready.Task.Wait(TimeSpan.FromSeconds(30)))
            {
                throw new TimeoutException("The Avalonia headless platform failed to initialize within 30 seconds.");
            }

            _initialized = true;
        }
    }

    /// <summary>
    /// Runs a function on the UI thread and returns its result.
    /// </summary>
    public static T RunOnUiThread<T>(Func<T> action)
    {
        EnsureInitialized();
        return Dispatcher.UIThread.InvokeAsync(action).GetTask().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Runs an action on the UI thread.
    /// </summary>
    public static void RunOnUiThread(Action action)
    {
        EnsureInitialized();
        Dispatcher.UIThread.InvokeAsync(action).GetTask().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Polls until the given condition becomes true (UI work is pumped by the
    /// dedicated UI thread in the background), failing after the timeout.
    /// </summary>
    public static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 10000)
    {
        EnsureInitialized();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (!condition())
        {
            if (sw.ElapsedMilliseconds > timeoutMs)
            {
                throw new TimeoutException("Condition not met within the timeout.");
            }

            await Task.Delay(5);
        }
    }
}

/// <summary>
/// Filesystem-level test helpers (portable settings.xml in the test output dir,
/// in-memory configuration).
/// </summary>
internal static class TestEnvironment
{
    private static readonly object Sync = new();
    private static bool _portableSettingsReady;

    /// <summary>
    /// Ensures a settings.xml exists next to the test assembly so SettingsManagerService
    /// resolves to portable mode and never writes to the real %LOCALAPPDATA%\SimpleLauncher.
    /// </summary>
    public static void EnsurePortableSettings()
    {
        if (_portableSettingsReady) return;

        lock (Sync)
        {
            if (_portableSettingsReady) return;

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.xml");
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "<Settings />");
            }

            _portableSettingsReady = true;
        }
    }

    /// <summary>
    /// Builds an IConfiguration from a JSON string (AddInMemoryCollection is not
    /// referenced by this solution, so JSON streams are used instead).
    /// </summary>
    public static IConfiguration ConfigurationFromJson(string json)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return new ConfigurationBuilder().AddJsonStream(stream).Build();
    }
}