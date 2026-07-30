using Serilog;
using Serilog.Events;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.DebugAndBugReport;

public class LogErrorsService : ILogErrors
{
    private readonly ILogger _logger;

    public LogErrorsService()
    {
        _logger = Log.ForContext<LogErrorsService>();
    }

    public Task LogErrorAsync(Exception ex, string contextMessage = null)
    {
        if (ex != null)
        {
            _logger.Error(ex, contextMessage ?? ex.Message);
        }
        else if (!string.IsNullOrWhiteSpace(contextMessage))
        {
            _logger.Warning(contextMessage);
        }

        return Task.CompletedTask;
    }
}
