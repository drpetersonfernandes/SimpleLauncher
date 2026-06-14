using System.Diagnostics.CodeAnalysis;
using SimpleLauncher.Services.DebugAndBugReport;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for <see cref="BugReportFormatter"/> covering additional exception types,
/// edge cases, and report structure validation.
/// </summary>
[SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
public class BugReportFormatterExtendedTests
{
    [Fact]
    public void BuildReportContainsSectionDelimiters()
    {
        var report = BugReportFormatter.BuildReport(null);

        Assert.Contains("=== Environment Details ===", report);
        Assert.Contains("=== Error Details ===", report);
        Assert.Contains("=== Exception Details ===", report);
    }

    [Fact]
    public void BuildReportNullExceptionContainsAllNoneFields()
    {
        var report = BugReportFormatter.BuildReport(null);

        Assert.Contains("Type: None", report);
        Assert.Contains("Message: None", report);
        Assert.Contains("Source: None", report);
        Assert.Contains("StackTrace: None", report);
    }

    [Fact]
    public void BuildReportWithAggregateExceptionContainsType()
    {
        var ex = new AggregateException("Multiple errors",
            new InvalidOperationException("Error 1"),
            new ArgumentException("Error 2"));
        var report = BugReportFormatter.BuildReport(ex);

        Assert.Contains("AggregateException", report);
        Assert.Contains("Multiple errors", report);
    }

    [Fact]
    public void BuildReportWithTaskCanceledException()
    {
        var ex = new TaskCanceledException("Task was cancelled");
        var report = BugReportFormatter.BuildReport(ex);

        Assert.Contains("TaskCanceledException", report);
        Assert.Contains("Task was cancelled", report);
    }

    [Fact]
    public void BuildReportWithNullReferenceException()
    {
        var ex = new NullReferenceException("Object reference not set");
        var report = BugReportFormatter.BuildReport(ex);

        Assert.Contains("NullReferenceException", report);
        Assert.Contains("Object reference not set", report);
    }

    [Fact]
    public void BuildReportWithFileNotFoundException()
    {
        var ex = new FileNotFoundException("File not found", "test.txt");
        var report = BugReportFormatter.BuildReport(ex);

        Assert.Contains("FileNotFoundException", report);
        Assert.Contains("File not found", report);
    }

    [Fact]
    public void BuildReportWithContextMessageAndNullException()
    {
        var report = BugReportFormatter.BuildReport(null, "Something went wrong during initialization");

        Assert.Contains("Something went wrong during initialization", report);
        Assert.Contains("Type: None", report);
    }

    [Fact]
    public void BuildReportWithContextMessageContainingSpecialCharacters()
    {
        var report = BugReportFormatter.BuildReport(null, "Error at C:\\Users\\test\\file.cs:42");

        Assert.Contains("Error at C:\\Users\\test\\file.cs:42", report);
    }

    [Fact]
    public void BuildReportExceptionWithStackTrace()
    {
        try
        {
            throw new InvalidOperationException("Test exception");
        }
        catch (Exception ex)
        {
            var report = BugReportFormatter.BuildReport(ex);
            Assert.Contains("StackTrace:", report);
            Assert.Contains("BuildReportExceptionWithStackTrace", report);
        }
    }

    [Fact]
    public void BuildReportContainsApplicationName()
    {
        var report = BugReportFormatter.BuildReport(null);
        Assert.Contains("Application Name:", report);
    }

    [Fact]
    public void BuildReportContainsApplicationVersion()
    {
        var report = BugReportFormatter.BuildReport(null);
        Assert.Contains("Application Version:", report);
    }

    [Fact]
    public void BuildReportContainsOsVersion()
    {
        var report = BugReportFormatter.BuildReport(null);
        Assert.Contains("OS Version:", report);
    }

    [Fact]
    public void BuildReportContainsArchitecture()
    {
        var report = BugReportFormatter.BuildReport(null);
        Assert.Contains("Architecture:", report);
    }

    [Fact]
    public void BuildReportContainsBitness()
    {
        var report = BugReportFormatter.BuildReport(null);
        Assert.Contains("Bitness:", report);
        Assert.True(report.Contains("64-bit") || report.Contains("32-bit"));
    }

    [Fact]
    public void BuildReportContainsProcessorCount()
    {
        var report = BugReportFormatter.BuildReport(null);
        Assert.Contains("Processor Count:", report);
    }

    [Fact]
    public void BuildReportContainsBaseDirectory()
    {
        var report = BugReportFormatter.BuildReport(null);
        Assert.Contains("Base Directory:", report);
    }

    [Fact]
    public void BuildReportContainsTempPath()
    {
        var report = BugReportFormatter.BuildReport(null);
        Assert.Contains("Temp Path:", report);
    }

    [Fact]
    public void BuildReportContainsDate()
    {
        var report = BugReportFormatter.BuildReport(null);
        Assert.Contains("Date:", report);
    }

    [Fact]
    public void BuildReportWithFormatExceptionContainsDetails()
    {
        var ex = new FormatException("Invalid format");
        var report = BugReportFormatter.BuildReport(ex);

        Assert.Contains("FormatException", report);
        Assert.Contains("Invalid format", report);
    }

    [Fact]
    public void BuildReportWithNotSupportedExceptionContainsDetails()
    {
        var ex = new NotSupportedException("Operation not supported");
        var report = BugReportFormatter.BuildReport(ex);

        Assert.Contains("NotSupportedException", report);
        Assert.Contains("Operation not supported", report);
    }

    [Fact]
    public void BuildReportWithEmptyContextMessageFallsBackToException()
    {
        var ex = new InvalidOperationException("Fallback message");
        var report = BugReportFormatter.BuildReport(ex, "");

        Assert.Contains("Fallback message", report);
    }

    [Fact]
    public void BuildReportWithWhitespaceContextMessageFallsBackToException()
    {
        var ex = new InvalidOperationException("Fallback message");
        var report = BugReportFormatter.BuildReport(ex, "   ");

        Assert.Contains("Fallback message", report);
    }
}
