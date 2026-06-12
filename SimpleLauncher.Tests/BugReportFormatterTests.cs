using SimpleLauncher.Services.DebugAndBugReport;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests the <see cref="BugReportFormatter"/> ability to build structured bug report strings.
/// </summary>
public class BugReportFormatterTests
{
    /// <summary>
    /// Verifies that building a report with a null exception produces all expected sections with "None" placeholders.
    /// </summary>
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

    /// <summary>
    /// Verifies that building a report with an exception includes the exception type and message.
    /// </summary>
    [Fact]
    public void BuildReportWithExceptionContainsExceptionDetails()
    {
        var ex = new InvalidOperationException("Test error message");
        var report = BugReportFormatter.BuildReport(ex);

        Assert.Contains("InvalidOperationException", report);
        Assert.Contains("Test error message", report);
        Assert.Contains("Source:", report);
    }

    /// <summary>
    /// Verifies that a custom context message is used instead of the exception message.
    /// </summary>
    [Fact]
    public void BuildReportWithContextMessageUsesContextMessage()
    {
        var report = BugReportFormatter.BuildReport(null, "Custom context");

        Assert.Contains("Custom context", report);
    }

    /// <summary>
    /// Verifies that a report with an inner exception includes the inner exception details.
    /// </summary>
    [Fact]
    public void BuildReportWithInnerExceptionContainsInnerException()
    {
        var inner = new ArgumentException("Inner error");
        var ex = new InvalidOperationException("Outer error", inner);
        var report = BugReportFormatter.BuildReport(ex);

        Assert.Contains("--- Inner Exception ---", report);
        Assert.Contains("Inner error", report);
    }

    /// <summary>
    /// Verifies that the report contains environment details such as application name, version, and OS.
    /// </summary>
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

    /// <summary>
    /// Verifies that a deeply nested exception chain includes all inner exception levels.
    /// </summary>
    [Fact]
    public void BuildReportDeeplyNestedExceptionContainsAllLevels()
    {
        var level3 = new ArgumentException("Level 3 error");
        var level2 = new InvalidOperationException("Level 2 error", level3);
        var level1 = new InvalidOperationException("Level 1 error", level2);
        var report = BugReportFormatter.BuildReport(level1);

        Assert.Contains("InvalidOperationException", report);
        Assert.Contains("Level 1 error", report);
        Assert.Contains("--- Inner Exception ---", report);
        Assert.Contains("InvalidOperationException", report);
        Assert.Contains("Level 2 error", report);
    }

    /// <summary>
    /// Verifies that an exception with a null source is handled gracefully in the report.
    /// </summary>
    [Fact]
    public void BuildReportExceptionWithNullSourceHandlesGracefully()
    {
        var ex = new InvalidOperationException("test error");
        // Source is null by default for manually created exceptions
        Assert.Null(ex.Source);

        var report = BugReportFormatter.BuildReport(ex);

        Assert.Contains("test error", report);
        Assert.Contains("Source:", report);
    }

    /// <summary>
    /// Verifies that a context message takes priority over the exception message in the report.
    /// </summary>
    [Fact]
    public void BuildReportWithContextMessageAndExceptionUsesContextMessage()
    {
        var ex = new InvalidOperationException("Exception message");
        var report = BugReportFormatter.BuildReport(ex, "Context takes priority");

        Assert.Contains("Context takes priority", report);
        Assert.Contains("Exception message", report); // Exception details still included
    }

    /// <summary>
    /// Verifies that an empty context message falls back to the exception message.
    /// </summary>
    [Fact]
    public void BuildReportEmptyContextMessageFallsBackToExceptionMessage()
    {
        var ex = new InvalidOperationException("Fallback message");
        var report = BugReportFormatter.BuildReport(ex, "");

        Assert.Contains("Fallback message", report);
    }
}
