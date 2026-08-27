using SimpleLauncher.Core.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="SystemHelper"/> model covering property defaults, assignment, and edge cases.
/// </summary>
public class SystemHelperTests
{
    /// <summary>
    /// Verifies that default property values of a new SystemHelper are null.
    /// </summary>
    [Fact]
    public void DefaultValuesAreNull()
    {
        var helper = new SystemHelper();
        Assert.Null(helper.SystemName);
        Assert.Null(helper.SystemHelperText);
    }

    /// <summary>
    /// Verifies that SystemName and SystemHelperText properties can be set and retrieved.
    /// </summary>
    [Fact]
    public void PropertiesCanBeSet()
    {
        var helper = new SystemHelper
        {
            SystemName = "NES",
            SystemHelperText = "Nintendo Entertainment System helper text"
        };

        Assert.Equal("NES", helper.SystemName);
        Assert.Equal("Nintendo Entertainment System helper text", helper.SystemHelperText);
    }

    /// <summary>
    /// Verifies that SystemName and SystemHelperText support Unicode characters.
    /// </summary>
    [Fact]
    public void SystemNameSupportsUnicode()
    {
        var helper = new SystemHelper
        {
            SystemName = "ファミコン",
            SystemHelperText = "ファミリーコンピュータ"
        };

        Assert.Equal("ファミコン", helper.SystemName);
        Assert.Equal("ファミリーコンピュータ", helper.SystemHelperText);
    }

    /// <summary>
    /// Verifies that SystemHelperText supports multiline strings.
    /// </summary>
    [Fact]
    public void SystemHelperTextSupportsMultiline()
    {
        var helper = new SystemHelper
        {
            SystemName = "NES",
            SystemHelperText = "Line 1\nLine 2\nLine 3"
        };

        Assert.Contains("\n", helper.SystemHelperText, StringComparison.Ordinal);
        Assert.Equal(3, helper.SystemHelperText.Split('\n').Length);
    }

    /// <summary>
    /// Verifies that SystemHelperText supports very long text strings.
    /// </summary>
    [Fact]
    public void SystemHelperTextSupportsLongText()
    {
        var longText = new string('A', 10000);
        var helper = new SystemHelper
        {
            SystemHelperText = longText
        };

        Assert.Equal(longText, helper.SystemHelperText);
    }

    /// <summary>
    /// Verifies that SystemHelperText supports special characters such as angle brackets and pipes.
    /// </summary>
    [Fact]
    public void SystemHelperTextSupportsSpecialCharacters()
    {
        var helper = new SystemHelper
        {
            SystemHelperText = "Use <config> with \"quotes\" and |pipes|"
        };

        Assert.Contains("<config>", helper.SystemHelperText, StringComparison.Ordinal);
        Assert.Contains("\"quotes\"", helper.SystemHelperText, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that properties are init-only and can be set during object initialization.
    /// </summary>
    [Fact]
    public void PropertiesAreInitOnly()
    {
        var helper = new SystemHelper { SystemName = "NES", SystemHelperText = "text" };
        Assert.Equal("NES", helper.SystemName);
        Assert.Equal("text", helper.SystemHelperText);
    }

    /// <summary>
    /// Verifies that multiple SystemHelper instances maintain independent property values.
    /// </summary>
    [Fact]
    public void MultipleInstancesAreIndependent()
    {
        var h1 = new SystemHelper { SystemName = "NES" };
        var h2 = new SystemHelper { SystemName = "SNES" };

        Assert.NotEqual(h1.SystemName, h2.SystemName, StringComparer.Ordinal);
    }
}