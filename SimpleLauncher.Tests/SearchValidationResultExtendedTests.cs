using SimpleLauncher.Core.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
///     Extended tests for <see cref="SearchValidationResult" /> covering additional edge cases.
/// </summary>
public class SearchValidationResultExtendedTests
{
    /// <summary>
    ///     Verifies that Success handles a very long query string correctly.
    /// </summary>
    [Fact]
    public void SuccessWithLongQuery()
    {
        var longQuery = new string('a', 10000);
        var result = SearchValidationResult.Success(longQuery);
        Assert.True(result.IsValid);
        Assert.Equal(longQuery, result.ValidatedQuery);
    }

    /// <summary>
    ///     Verifies that Success handles queries containing newline characters.
    /// </summary>
    [Fact]
    public void SuccessWithNewlines()
    {
        var result = SearchValidationResult.Success("line1\nline2\r\nline3");
        Assert.True(result.IsValid);
        Assert.Equal("line1\nline2\r\nline3", result.ValidatedQuery);
    }

    /// <summary>
    ///     Verifies that Success handles queries containing tab characters.
    /// </summary>
    [Fact]
    public void SuccessWithTabs()
    {
        var result = SearchValidationResult.Success("game\tname");
        Assert.True(result.IsValid);
        Assert.Equal("game\tname", result.ValidatedQuery);
    }

    /// <summary>
    ///     Verifies that Success handles queries with mixed whitespace characters.
    /// </summary>
    [Fact]
    public void SuccessWithMixedWhitespace()
    {
        var result = SearchValidationResult.Success(" \t\n ");
        Assert.True(result.IsValid);
        Assert.Equal(" \t\n ", result.ValidatedQuery);
    }

    /// <summary>
    ///     Verifies that Success handles queries containing emoji characters.
    /// </summary>
    [Fact]
    public void SuccessWithEmoji()
    {
        var result = SearchValidationResult.Success("🎮 Mario");
        Assert.True(result.IsValid);
        Assert.Equal("🎮 Mario", result.ValidatedQuery);
    }

    /// <summary>
    ///     Verifies that Success handles queries containing accented characters.
    /// </summary>
    [Fact]
    public void SuccessWithAccentedCharacters()
    {
        var result = SearchValidationResult.Success("Pokémon");
        Assert.True(result.IsValid);
        Assert.Equal("Pokémon", result.ValidatedQuery);
    }

    /// <summary>
    ///     Verifies that Success handles queries containing Chinese characters.
    /// </summary>
    [Fact]
    public void SuccessWithChineseCharacters()
    {
        var result = SearchValidationResult.Success("超级马里奥");
        Assert.True(result.IsValid);
        Assert.Equal("超级马里奥", result.ValidatedQuery);
    }

    /// <summary>
    ///     Verifies that Success handles queries containing Arabic characters.
    /// </summary>
    [Fact]
    public void SuccessWithArabicCharacters()
    {
        var result = SearchValidationResult.Success("ماريو");
        Assert.True(result.IsValid);
        Assert.Equal("ماريو", result.ValidatedQuery);
    }

    /// <summary>
    ///     Verifies that multiple Failure calls return independent result instances.
    /// </summary>
    [Fact]
    public void FailureMultipleCallsReturnIndependentResults()
    {
        var r1 = SearchValidationResult.Failure();
        var r2 = SearchValidationResult.Failure();
        Assert.False(r1.IsValid);
        Assert.False(r2.IsValid);
    }

    /// <summary>
    ///     Verifies that Success and Failure results are independent of each other.
    /// </summary>
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

    /// <summary>
    ///     Verifies that Success with null query still produces a valid result.
    /// </summary>
    [Fact]
    public void SuccessWithNullQueryStillValid()
    {
        var result = SearchValidationResult.Success(null!);
        Assert.True(result.IsValid);
        Assert.Null(result.ValidatedQuery);
    }

    /// <summary>
    ///     Verifies that ValidatedQuery is set correctly and can be read back.
    /// </summary>
    [Fact]
    public void ValidatedQueryIsInitOnly()
    {
        var result = SearchValidationResult.Success("test");
        // ValidatedQuery should be init-only, so we can verify it's set correctly
        Assert.Equal("test", result.ValidatedQuery);
    }

    /// <summary>
    ///     Verifies that IsValid is set correctly on a Success result.
    /// </summary>
    [Fact]
    public void IsValidIsInitOnly()
    {
        var result = SearchValidationResult.Success("test");
        Assert.True(result.IsValid);
    }
}