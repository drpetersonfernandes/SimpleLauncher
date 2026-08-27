using System;
using System.IO;
using Serilog.Events;
using XmlToBinaryConverter.Services.DebugAndBugReport;

namespace XmlToBinaryConverter;

/// <summary>
/// Application entry point for the XML to Binary Converter.
/// </summary>
public partial class App
{
    protected override void OnStartup(System.Windows.StartupEventArgs e)
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

        base.OnStartup(e);
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}