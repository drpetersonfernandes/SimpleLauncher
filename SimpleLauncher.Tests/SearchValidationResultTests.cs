using SimpleLauncher.Services.SearchOrchestrator;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests the SearchValidationResult model for creating success and failure validation results.
/// </summary>
public class SearchValidationResultTests
{
    /// <summary>
    /// Verifies that the Success factory creates a valid result with the correct query.
    /// </summary>
    [Fact]
    public void SuccessCreatesValidResult()
    {
        var result = SearchValidationResult.Success("mario");
        Assert.True(result.IsValid);
        Assert.Equal("mario", result.ValidatedQuery);
    }

    /// <summary>
    /// Verifies that the Failure factory creates an invalid result with a null query.
    /// </summary>
    [Fact]
    public void FailureCreatesInvalidResult()
    {
        var result = SearchValidationResult.Failure();
        Assert.False(result.IsValid);
        Assert.Null(result.ValidatedQuery);
    }

    /// <summary>
    /// Verifies that the Success factory preserves the query as-is including whitespace.
    /// </summary>
    [Fact]
    public void SuccessTrimsQuery()
    {
        var result = SearchValidationResult.Success("  mario  ");
        Assert.Equal("  mario  ", result.ValidatedQuery);
    }

    /// <summary>
    /// Verifies that Success with an empty string still produces a valid result.
    /// </summary>
    [Fact]
    public void SuccessWithEmptyQueryStillValid()
    {
        var result = SearchValidationResult.Success("");
        Assert.True(result.IsValid);
        Assert.Equal("", result.ValidatedQuery);
    }

    /// <summary>
    /// Verifies that Success correctly stores queries containing special characters.
    /// </summary>
    [Fact]
    public void SuccessWithSpecialCharacters()
    {
        var result = SearchValidationResult.Success("mega man x2 (usa)");
        Assert.True(result.IsValid);
        Assert.Equal("mega man x2 (usa)", result.ValidatedQuery);
    }

    /// <summary>
    /// Verifies that Success correctly stores queries containing Unicode characters.
    /// </summary>
    [Fact]
    public void SuccessWithUnicodeCharacters()
    {
        var result = SearchValidationResult.Success("ポケモン");
        Assert.True(result.IsValid);
        Assert.Equal("ポケモン", result.ValidatedQuery);
    }

    /// <summary>
    /// Verifies that a Failure result always has IsValid set to false.
    /// </summary>
    [Fact]
    public void FailureIsValidFalseByDefault()
    {
        var result = SearchValidationResult.Failure();
        Assert.False(result.IsValid);
    }

    /// <summary>
    /// Verifies that internal whitespace within the query is preserved.
    /// </summary>
    [Fact]
    public void SuccessPreservesInternalWhitespace()
    {
        var result = SearchValidationResult.Success("  super  mario  world  ");
        Assert.Equal("  super  mario  world  ", result.ValidatedQuery);
    }

    /// <summary>
    /// Verifies that multiple Success results maintain independent query values.
    /// </summary>
    [Fact]
    public void MultipleSuccessResultsAreIndependent()
    {
        var r1 = SearchValidationResult.Success("zelda");
        var r2 = SearchValidationResult.Success("mario");

        Assert.Equal("zelda", r1.ValidatedQuery);
        Assert.Equal("mario", r2.ValidatedQuery);
    }

    /// <summary>
    /// Verifies that a Failure result has a null ValidatedQuery property.
    /// </summary>
    [Fact]
    public void FailureResultHasNullValidatedQuery()
    {
        var result = SearchValidationResult.Failure();
        Assert.Null(result.ValidatedQuery);
    }
}
