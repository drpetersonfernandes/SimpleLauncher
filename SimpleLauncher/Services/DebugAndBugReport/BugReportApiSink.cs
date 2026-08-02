using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Events;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.DebugAndBugReport;

/// <summary>
/// A Serilog sink that collects warning and error log events, writes them to log files, and submits them to the bug report API.
/// </summary>
public class BugReportApiSink : ILogEventSink, IDisposable
{
    private readonly Channel<LogEvent> _channel = Channel.CreateBounded<LogEvent>(new BoundedChannelOptions(100)
    {
        FullMode = BoundedChannelFullMode.DropOldest
    });

    private readonly CancellationTokenSource _cts = new();
    private IHttpClientFactory _httpClientFactory = null!;
    private IConfiguration _configuration = null!;
    private IDeleteFilesService _deleteFilesService = null!;
    private string _logFolder = null!;
    private bool _disposed;

    private static readonly Lock InitLock = new();
    private bool _initialized;
    private Task _processTask = null!;

    /// <summary>
    /// Initializes the sink with the services and log folder needed to submit bug reports.
    /// </summary>
    /// <param name="httpClientFactory">The factory used to create the HTTP client for bug report submissions.</param>
    /// <param name="configuration">The application configuration containing API and log path settings.</param>
    /// <param name="deleteFilesService">The service used to delete log files after successful submission.</param>
    /// <param name="logFolder">The folder where log files are written.</param>
    public void Initialize(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IDeleteFilesService deleteFilesService,
        string logFolder)
    {
        lock (InitLock)
        {
            if (_initialized) return;

            _initialized = true;

            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _deleteFilesService = deleteFilesService;
            _logFolder = logFolder;

            _processTask = ProcessQueueAsync(_cts.Token);
        }
    }

    /// <summary>
    /// Emits a log event to the sink, queuing warning and error events for reporting.
    /// </summary>
    /// <param name="logEvent">The log event to emit.</param>
    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Level < LogEventLevel.Warning) return;

        _channel.Writer.TryWrite(logEvent);
    }

    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        while (await _channel.Reader.WaitToReadAsync(cancellationToken))
        {
            while (_channel.Reader.TryRead(out var logEvent))
            {
                try
                {
                    await SendReportAsync(logEvent);
                }
                catch
                {
                    WriteCriticalError(logEvent);
                }
            }
        }
    }

    private async Task SendReportAsync(LogEvent logEvent)
    {
        if (_httpClientFactory == null || _configuration == null) return;

        var report = BuildReport(logEvent);

        var apiKey = _configuration.GetValue<string>("ApiKey") ?? "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";
        if (string.IsNullOrEmpty(apiKey)) return;

        var errorLogPath = Path.Combine(_logFolder,
            _configuration.GetValue<string>("LogPathForAdmin") ?? "error.log");

        var userLogPath = Path.Combine(_logFolder,
            _configuration.GetValue<string>("LogPath") ?? "error_user.log");

        if (errorLogPath != null)
        {
            await File.AppendAllTextAsync(errorLogPath, report);
            if (userLogPath != null)
            {
                await File.AppendAllTextAsync(userLogPath,
                    report + "--------------------------------------------------------------------------------------------------------------\n\n\n");
            }
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient("LogErrorsClient");
            httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

            var assembly = Assembly.GetExecutingAssembly();
            var payload = new
            {
                message = report,
                applicationName = assembly.GetName().Name ?? "SimpleLauncher",
                version = assembly.GetName().Version?.ToString() ?? "Unknown",
                userInfo = GetUserInfo(),
                environment = GetEnvironmentName(),
                stackTrace = BuildStackTrace(logEvent)
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var apiUrl = _configuration.GetValue<string>("BugReportApiUrl") ??
                         "https://www.purelogiccode.com/bugreport/api/send-bug-report";

            using var response = await httpClient.PostAsync(apiUrl, jsonContent, cts.Token);

            if (response.IsSuccessStatusCode && File.Exists(errorLogPath) && _deleteFilesService != null)
            {
                try
                {
                    await _deleteFilesService.TryDeleteFileAsync(errorLogPath);
                }
                catch
                {
                    // Ignore deletion failures
                }
            }
        }
        catch
        {
            WriteCriticalError(logEvent);
        }
    }

    private void WriteCriticalError(LogEvent logEvent)
    {
        try
        {
            var criticalLogPath = Path.Combine(_logFolder,
                _configuration?.GetValue<string>("LogPathCritical") ?? "critical_error.log");
            var report = BuildReport(logEvent) +
                         "\n--------------------------------------------------------------------------------------------------------------\n\n\n";

            if (criticalLogPath != null)
            {
                File.AppendAllText(criticalLogPath, report);
            }
        }
        catch
        {
            // Can't do anything more
        }
    }

    private static string BuildReport(LogEvent logEvent)
    {
        var message = new StringBuilder();

        message.AppendLine("=== Environment Details ===");
        message.AppendLine(CultureInfo.InvariantCulture, $"Date: {DateTime.Now}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Application Name: {Assembly.GetExecutingAssembly().GetName().Name ?? "SimpleLauncher"}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Application Version: {Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown"}");
        message.AppendLine(CultureInfo.InvariantCulture, $"OS Version: {RuntimeInformation.OSDescription}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Architecture: {RuntimeInformation.OSArchitecture}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Bitness: {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Windows Version: {GetMicrosoftWindowsVersion.GetVersion()}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Processor Count: {Environment.ProcessorCount}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Base Directory: {AppContext.BaseDirectory}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Temp Path: {Path.GetTempPath()}");
        message.AppendLine();

        message.AppendLine("=== Error Details ===");
        message.AppendLine(CultureInfo.InvariantCulture, $"Log Level: {logEvent.Level}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Error message: {logEvent.RenderMessage()}");
        message.AppendLine();

        message.AppendLine("=== Exception Details ===");
        if (logEvent.Exception == null)
        {
            message.AppendLine("Type: None");
            message.AppendLine("Message: None");
            message.AppendLine("Source: None");
            message.AppendLine("StackTrace: None");
        }
        else
        {
            AppendException(message, logEvent.Exception);
            if (logEvent.Exception.InnerException != null)
            {
                message.AppendLine();
                message.AppendLine("--- Inner Exception ---");
                AppendException(message, logEvent.Exception.InnerException);
            }
        }

        return message.ToString();
    }

    private static void AppendException(StringBuilder message, Exception exception)
    {
        message.AppendLine(CultureInfo.InvariantCulture, $"Type: {exception.GetType().FullName}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Message: {exception.Message}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Source: {exception.Source}");
        message.AppendLine(CultureInfo.InvariantCulture, $"StackTrace: {exception.StackTrace}");
    }

    private static string? BuildStackTrace(LogEvent logEvent)
    {
        if (logEvent.Exception == null) return null;

        var sb = new StringBuilder();
        var currentEx = logEvent.Exception;
        var depth = 0;
        const int maxDepth = 10;

        while (currentEx != null && depth < maxDepth)
        {
            if (depth > 0)
            {
                sb.AppendLine();
                sb.AppendLine("--- INNER EXCEPTION ---");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"Exception Type: {currentEx.GetType().FullName}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Message: {currentEx.Message}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Source: {currentEx.Source}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"StackTrace: {currentEx.StackTrace}");

            currentEx = currentEx.InnerException;
            depth++;
        }

        if (currentEx != null)
        {
            sb.AppendLine();
            sb.AppendLine("--- ADDITIONAL INNER EXCEPTIONS TRUNCATED ---");
        }

        return sb.ToString();
    }

    private static string? GetUserInfo()
    {
        try
        {
            return Environment.MachineName;
        }
        catch
        {
            return null;
        }
    }

    private static string GetEnvironmentName()
    {
#if DEBUG
        return "Debug";
#else
        return "Release";
#endif
    }

    /// <summary>
    /// Releases all resources used by the sink, stopping the report processing queue.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _cts.Cancel();
        _cts.Dispose();
    }
}
