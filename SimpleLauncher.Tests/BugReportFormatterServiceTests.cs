using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.DebugAndBugReport;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="BugReportFormatterService"/> which wraps <see cref="BugReportFormatter"/>
/// with an injectable <see cref="IWindowsVersionService"/>.
/// </summary>
public class BugReportFormatterServiceTests
{
    private readonly BugReportFormatterService _service = new(new NoOpWindowsVersionService());

    [Fact]
    public void BuildReportNullExceptionContainsNoneSections()
    {
        var report = _service.BuildReport(null);

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
        var report = _service.BuildReport(ex);

        Assert.Contains("InvalidOperationException", report);
        Assert.Contains("Test error message", report);
        Assert.Contains("Source:", report);
    }

    [Fact]
    public void BuildReportWithContextMessageUsesContextMessage()
    {
        var report = _service.BuildReport(null, "Custom context");

        Assert.Contains("Custom context", report);
    }

    [Fact]
    public void BuildReportWithInnerExceptionContainsInnerException()
    {
        var inner = new ArgumentException("Inner error");
        var ex = new InvalidOperationException("Outer error", inner);
        var report = _service.BuildReport(ex);

        Assert.Contains("--- Inner Exception ---", report);
        Assert.Contains("Inner error", report);
    }

    [Fact]
    public void BuildReportContainsEnvironmentDetails()
    {
        var report = _service.BuildReport(null);

        Assert.Contains("Application Name:", report);
        Assert.Contains("Application Version:", report);
        Assert.Contains("OS Version:", report);
        Assert.Contains("Architecture:", report);
        Assert.Contains("Base Directory:", report);
    }

    [Fact]
    public void BuildReportContainsWindowsVersion()
    {
        var report = _service.BuildReport(null);
        Assert.Contains("Windows Version:", report);
    }

    [Fact]
    public void BuildReportUsesInjectedWindowsVersionService()
    {
        var service = new BugReportFormatterService(new CustomWindowsVersionService());
        var report = service.BuildReport(null);
        Assert.Contains("Custom Windows 99", report);
    }

    [Fact]
    public void BuildReportContainsDate()
    {
        var report = _service.BuildReport(null);
        Assert.Contains("Date:", report);
    }

    [Fact]
    public void BuildReportContainsProcessorCount()
    {
        var report = _service.BuildReport(null);
        Assert.Contains("Processor Count:", report);
    }

    [Fact]
    public void BuildReportContainsBitness()
    {
        var report = _service.BuildReport(null);
        Assert.Contains("Bitness:", report);
        Assert.True(report.Contains("64-bit") || report.Contains("32-bit"));
    }

    [Fact]
    public void BuildReportContainsTempPath()
    {
        var report = _service.BuildReport(null);
        Assert.Contains("Temp Path:", report);
    }

    [Fact]
    public void BuildReportWithContextMessageAndException()
    {
        var ex = new InvalidOperationException("Exception message");
        var report = _service.BuildReport(ex, "Context takes priority");

        Assert.Contains("Context takes priority", report);
        Assert.Contains("Exception message", report);
    }

    [Fact]
    public void BuildReportDeeplyNestedException()
    {
        var level3 = new ArgumentException("Level 3 error");
        var level2 = new InvalidOperationException("Level 2 error", level3);
        var level1 = new InvalidOperationException("Level 1 error", level2);
        var report = _service.BuildReport(level1);

        Assert.Contains("Level 1 error", report);
        Assert.Contains("--- Inner Exception ---", report);
        Assert.Contains("Level 2 error", report);
    }

    [Fact]
    public void BuildReportWithNullSourceHandlesGracefully()
    {
        var ex = new InvalidOperationException("test error");
        Assert.Null(ex.Source);

        var report = _service.BuildReport(ex);
        Assert.Contains("test error", report);
        Assert.Contains("Source:", report);
    }

    [Fact]
    public void BuildReportEmptyContextMessageFallsBackToExceptionMessage()
    {
        var ex = new InvalidOperationException("Fallback message");
        var report = _service.BuildReport(ex, "");

        Assert.Contains("Fallback message", report);
    }

    private sealed class NoOpWindowsVersionService : IWindowsVersionService
    {
        public string GetVersion()
        {
            return "Test Windows Version";
        }
    }

    private sealed class CustomWindowsVersionService : IWindowsVersionService
    {
        public string GetVersion()
        {
            return "Custom Windows 99";
        }
    }
}
