using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Serilog.Core;
using Serilog.Events;

namespace RetroAchievements.DataFetcher.Services.DebugAndBugReport;

public class BugReportApiSink : ILogEventSink, IDisposable
{
    private const string ApiUrl = "https://www.purelogiccode.com/bugreport/api/send-bug-report";
    private const string ApiKey = "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";

    private readonly Channel<LogEvent> _channel = Channel.CreateBounded<LogEvent>(new BoundedChannelOptions(100)
    {
        FullMode = BoundedChannelFullMode.DropOldest
    });

    private readonly CancellationTokenSource _cts = new();
    private readonly string _logFolder;
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };
    private bool _disposed;
    private Task _processTask = null!;

    public BugReportApiSink(string logFolder)
    {
        _logFolder = logFolder;
        _processTask = ProcessQueueAsync(_cts.Token);
    }

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
        var report = BuildReport(logEvent);

        var errorLogPath = Path.Combine(_logFolder, "error.log");
        await File.AppendAllTextAsync(errorLogPath, report);

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var payload = new
            {
                message = report,
                applicationName = assembly.GetName().Name ?? "RetroAchievements.DataFetcher",
                version = assembly.GetName().Version?.ToString() ?? "Unknown",
                userInfo = GetUserInfo(),
                environment = GetEnvironmentName(),
                stackTrace = logEvent.Exception?.StackTrace
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            request.Headers.Add("X-API-KEY", ApiKey);
            request.Content = jsonContent;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var response = await _httpClient.SendAsync(request, cts.Token);

            if (response.IsSuccessStatusCode && File.Exists(errorLogPath))
            {
                try { File.Delete(errorLogPath); }
                catch { /* Ignore */ }
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
            var criticalLogPath = Path.Combine(_logFolder, "critical_error.log");
            var report = BuildReport(logEvent) + "\n---\n\n\n";
            File.AppendAllText(criticalLogPath, report);
        }
        catch { /* Can't do anything more */ }
    }

    private static string BuildReport(LogEvent logEvent)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Environment Details ===");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Date: {DateTime.Now}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Application: {Assembly.GetExecutingAssembly().GetName().Name ?? "RetroAchievements.DataFetcher"}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Version: {Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown"}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"OS: {RuntimeInformation.OSDescription}");
        sb.AppendLine();
        sb.AppendLine("=== Error Details ===");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Level: {logEvent.Level}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Message: {logEvent.RenderMessage()}");
        if (logEvent.Exception != null)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"Exception: {logEvent.Exception}");
        }
        sb.AppendLine();
        return sb.ToString();
    }

    private static string? GetUserInfo()
    {
        try { return Environment.MachineName; }
        catch { return null; }
    }

    private static string GetEnvironmentName()
    {
#if DEBUG
        return "Debug";
#else
        return "Release";
#endif
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cts.Cancel();
        _cts.Dispose();
        _httpClient.Dispose();
    }
}
