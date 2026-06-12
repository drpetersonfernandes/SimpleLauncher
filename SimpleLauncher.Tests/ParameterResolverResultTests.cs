using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="ParameterResolverResult"/> model property defaults,
/// setters, special characters, multiline values, and instance independence.
/// </summary>
public class ParameterResolverResultTests
{
    /// <summary>
    /// Verifies that the default SuggestedParameter is an empty string.
    /// </summary>
    [Fact]
    public void DefaultSuggestedParameterIsEmpty()
    {
        var result = new ParameterResolverResult();
        Assert.Equal("", result.SuggestedParameter);
    }

    /// <summary>
    /// Verifies that the default Explanation is null.
    /// </summary>
    [Fact]
    public void DefaultExplanationIsNull()
    {
        var result = new ParameterResolverResult();
        Assert.Null(result.Explanation);
    }

    /// <summary>
    /// Verifies that SuggestedParameter can be set and retrieved.
    /// </summary>
    [Fact]
    public void SuggestedParameterCanBeSet()
    {
        var result = new ParameterResolverResult { SuggestedParameter = "-f" };
        Assert.Equal("-f", result.SuggestedParameter);
    }

    /// <summary>
    /// Verifies that Explanation can be set and retrieved.
    /// </summary>
    [Fact]
    public void ExplanationCanBeSet()
    {
        var result = new ParameterResolverResult { Explanation = "Fullscreen flag" };
        Assert.Equal("Fullscreen flag", result.Explanation);
    }

    /// <summary>
    /// Verifies that both properties can be set together via object initializer.
    /// </summary>
    [Fact]
    public void BothPropertiesCanBeSetTogether()
    {
        var result = new ParameterResolverResult
        {
            SuggestedParameter = "--rompath \"C:\\roms\"",
            Explanation = "ROM path specified"
        };

        Assert.Equal("--rompath \"C:\\roms\"", result.SuggestedParameter);
        Assert.Equal("ROM path specified", result.Explanation);
    }

    /// <summary>
    /// Verifies that SuggestedParameter supports special characters such as paths and DLL names.
    /// </summary>
    [Fact]
    public void SuggestedParameterSupportsSpecialCharacters()
    {
        var result = new ParameterResolverResult
        {
            SuggestedParameter = "-L \"C:\\cores\\fceumm_libretro.dll\""
        };

        Assert.Contains("fceumm_libretro.dll", result.SuggestedParameter);
    }

    /// <summary>
    /// Verifies that Explanation supports multiline strings with newline characters.
    /// </summary>
    [Fact]
    public void ExplanationSupportsMultiline()
    {
        var result = new ParameterResolverResult
        {
            Explanation = "Line 1\nLine 2\nLine 3"
        };

        Assert.Contains("\n", result.Explanation);
    }

    /// <summary>
    /// Verifies that multiple instances maintain independent property values.
    /// </summary>
    [Fact]
    public void CanCreateMultipleInstances()
    {
        var r1 = new ParameterResolverResult { SuggestedParameter = "a" };
        var r2 = new ParameterResolverResult { SuggestedParameter = "b" };

        Assert.NotEqual(r1.SuggestedParameter, r2.SuggestedParameter);
    }
}
