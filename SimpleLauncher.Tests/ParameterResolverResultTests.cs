using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class ParameterResolverResultTests
{
    [Fact]
    public void DefaultSuggestedParameterIsEmpty()
    {
        var result = new ParameterResolverResult();
        Assert.Equal(string.Empty, result.SuggestedParameter);
    }

    [Fact]
    public void DefaultExplanationIsNull()
    {
        var result = new ParameterResolverResult();
        Assert.Null(result.Explanation);
    }

    [Fact]
    public void SuggestedParameterCanBeSet()
    {
        var result = new ParameterResolverResult { SuggestedParameter = "-f" };
        Assert.Equal("-f", result.SuggestedParameter);
    }

    [Fact]
    public void ExplanationCanBeSet()
    {
        var result = new ParameterResolverResult { Explanation = "Fullscreen flag" };
        Assert.Equal("Fullscreen flag", result.Explanation);
    }

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

    [Fact]
    public void SuggestedParameterSupportsSpecialCharacters()
    {
        var result = new ParameterResolverResult
        {
            SuggestedParameter = "-L \"C:\\cores\\fceumm_libretro.dll\""
        };

        Assert.Contains("fceumm_libretro.dll", result.SuggestedParameter);
    }

    [Fact]
    public void ExplanationSupportsMultiline()
    {
        var result = new ParameterResolverResult
        {
            Explanation = "Line 1\nLine 2\nLine 3"
        };

        Assert.Contains("\n", result.Explanation);
    }

    [Fact]
    public void CanCreateMultipleInstances()
    {
        var r1 = new ParameterResolverResult { SuggestedParameter = "a" };
        var r2 = new ParameterResolverResult { SuggestedParameter = "b" };

        Assert.NotEqual(r1.SuggestedParameter, r2.SuggestedParameter);
    }
}
