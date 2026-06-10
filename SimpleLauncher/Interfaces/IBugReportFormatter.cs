namespace SimpleLauncher.Interfaces;

public interface IBugReportFormatter
{
    string BuildReport(Exception ex, string contextMessage = null);
}
