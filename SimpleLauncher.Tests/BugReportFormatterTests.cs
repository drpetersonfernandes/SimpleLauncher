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

    [Fact]
    public void BuildReportDeeplyNestedExceptionContainsAllLevels()
    {
        var level3 = new ArgumentException("Level 3 error");
        var level2 = new InvalidOperationException("Level 2 error", level3);
        var level1 = new ApplicationException("Level 1 error", level2);
        var report = BugReportFormatter.BuildReport(level1);

        Assert.Contains("ApplicationException", report);
        Assert.Contains("Level 1 error", report);
        Assert.Contains("--- Inner Exception ---", report);
        Assert.Contains("InvalidOperationException", report);
        Assert.Contains("Level 2 error", report);
    }

    [Fact]
    public void BuildReportExceptionWithNullSourceHandlesGracefully()
    {
        var ex = new Exception("test error");
        // Source is null by default for manually created exceptions
        Assert.Null(ex.Source);

        var report = BugReportFormatter.BuildReport(ex);

        Assert.Contains("test error", report);
        Assert.Contains("Source:", report);
    }

    [Fact]
    public void BuildReportWithContextMessageAndExceptionUsesContextMessage()
    {
        var ex = new InvalidOperationException("Exception message");
        var report = BugReportFormatter.BuildReport(ex, "Context takes priority");

        Assert.Contains("Context takes priority", report);
        Assert.Contains("Exception message", report); // Exception details still included
    }

    [Fact]
    public void BuildReportEmptyContextMessageFallsBackToExceptionMessage()
    {
        var ex = new InvalidOperationException("Fallback message");
        var report = BugReportFormatter.BuildReport(ex, "");

        Assert.Contains("Fallback message", report);
    }
}
