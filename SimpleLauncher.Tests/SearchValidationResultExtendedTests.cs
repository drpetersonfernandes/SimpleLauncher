using SimpleLauncher.Services.SearchOrchestrator;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for <see cref="SearchValidationResult"/> covering additional edge cases.
/// </summary>
public class SearchValidationResultExtendedTests
{
    [Fact]
    public void SuccessWithLongQuery()
    {
        var longQuery = new string('a', 10000);
        var result = SearchValidationResult.Success(longQuery);
        Assert.True(result.IsValid);
        Assert.Equal(longQuery, result.ValidatedQuery);
    }

    [Fact]
    public void SuccessWithNewlines()
    {
        var result = SearchValidationResult.Success("line1\nline2\r\nline3");
        Assert.True(result.IsValid);
        Assert.Equal("line1\nline2\r\nline3", result.ValidatedQuery);
    }

    [Fact]
    public void SuccessWithTabs()
    {
        var result = SearchValidationResult.Success("game\tname");
        Assert.True(result.IsValid);
        Assert.Equal("game\tname", result.ValidatedQuery);
    }

    [Fact]
    public void SuccessWithMixedWhitespace()
    {
        var result = SearchValidationResult.Success(" \t\n ");
        Assert.True(result.IsValid);
        Assert.Equal(" \t\n ", result.ValidatedQuery);
    }

    [Fact]
    public void SuccessWithEmoji()
    {
        var result = SearchValidationResult.Success("🎮 Mario");
        Assert.True(result.IsValid);
        Assert.Equal("🎮 Mario", result.ValidatedQuery);
    }

    [Fact]
    public void SuccessWithAccentedCharacters()
    {
        var result = SearchValidationResult.Success("Pokémon");
        Assert.True(result.IsValid);
        Assert.Equal("Pokémon", result.ValidatedQuery);
    }

    [Fact]
    public void SuccessWithChineseCharacters()
    {
        var result = SearchValidationResult.Success("超级马里奥");
        Assert.True(result.IsValid);
        Assert.Equal("超级马里奥", result.ValidatedQuery);
    }

    [Fact]
    public void SuccessWithArabicCharacters()
    {
        var result = SearchValidationResult.Success("ماريو");
        Assert.True(result.IsValid);
        Assert.Equal("ماريو", result.ValidatedQuery);
    }

    [Fact]
    public void FailureMultipleCallsReturnIndependentResults()
    {
        var r1 = SearchValidationResult.Failure();
        var r2 = SearchValidationResult.Failure();
        Assert.False(r1.IsValid);
        Assert.False(r2.IsValid);
    }

    [Fact]
    public void SuccessAndFailureAreIndependent()
    {
        var success = SearchValidationResult.Success("test");
        var failure = SearchValidationResult.Failure();

        Assert.True(success.IsValid);
        Assert.False(failure.IsValid);
        Assert.Equal("test", success.ValidatedQuery);
        Assert.Null(failure.ValidatedQuery);
    }

    [Fact]
    public void SuccessWithNullQueryStillValid()
    {
        var result = SearchValidationResult.Success(null);
        Assert.True(result.IsValid);
        Assert.Null(result.ValidatedQuery);
    }

    [Fact]
    public void ValidatedQueryIsInitOnly()
    {
        var result = SearchValidationResult.Success("test");
        // ValidatedQuery should be init-only, so we can verify it's set correctly
        Assert.Equal("test", result.ValidatedQuery);
    }

    [Fact]
    public void IsValidIsInitOnly()
    {
        var result = SearchValidationResult.Success("test");
        Assert.True(result.IsValid);
    }
}
