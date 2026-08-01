using System.IO;
using System.Windows;
using Mame.DatCreator.Services.DebugAndBugReport;
using Serilog.Events;

namespace Mame.DatCreator;

/// <summary>
/// Entry point for the MAME DAT Creator application.
/// </summary>
/// <summary>
/// Entry point for the MAME DAT Creator application.
/// </summary>
public static class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    [STAThread]
    public static void Main(string[] args)
    {
        var appDataLogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        Directory.CreateDirectory(appDataLogFolder);

        var bugReportSink = new BugReportApiSink(appDataLogFolder);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Debug(outputTemplate: "[{Level}] {Timestamp:HH:mm:ss.fff} - {Message}{NewLine}{Exception}")
            .WriteTo.Async(a => a.File(
                Path.Combine(appDataLogFolder, "error_user.log"),
                LogEventLevel.Warning,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}"))
            .WriteTo.Sink(bugReportSink)
            .CreateLogger();

        try
        {
            var app = new Application();
            var mainWindow = new MainWindow();
            app.Run(mainWindow);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An unhandled error occurred");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
