using SimpleLauncher.Services.DebugAndBugReport;
using Xunit;

namespace SimpleLauncher.Tests;

public class BugReportFormatterTests
{
    [Fact]
    public void BuildReportNullExceptionContainsNoneSections()
    {
        var report = BugReportFormatter.BuildReport(null);

        Assert.Contains("=== Environment Details ===", report);
        Assert.Contains("=== Error Details ===", report);
        Assert.Contains("=== Exception Details ===", report);
        Assert.Contains("Type: None", report);
        Assert.Contains("Message: None", report);
        Assert.Contains("Source: None", report);
        Assert.Contains("StackTrace: None", report);
    }

    [Fact]
    public void BuildReportWithExceptionContainsExceptionDetails()
    {
        var ex = new InvalidOperationException("Test error message");
        var report = BugReportFormatter.BuildReport(ex);

        Assert.Contains("InvalidOperationException", report);
        Assert.Contains("Test error message", report);
        Assert.Contains("Source:", report);
    }

    [Fact]
    public void BuildReportWithContextMessageUsesContextMessage()
    {
        var report = BugReportFormatter.BuildReport(null, "Custom context");

        Assert.Contains("Custom context", report);
    }

    [Fact]
    public void BuildReportWithInnerExceptionContainsInnerException()
    {
        var inner = new ArgumentException("Inner error");
        var ex = new InvalidOperationException("Outer error", inner);
        var report = BugReportFormatter.BuildReport(ex);

        Assert.Contains("--- Inner Exception ---", report);
        Assert.Contains("Inner error", report);
    }

    [Fact]
    public void BuildReportContainsEnvironmentDetails()
    {
        var report = BugReportFormatter.BuildReport(null);

        Assert.Contains("Application Name:", report);
        Assert.Contains("Application Version:", report);
        Assert.Contains("OS Version:", report);
        Assert.Contains("Architecture:", report);
        Assert.Contains("Base Directory:", report);
    }
}
