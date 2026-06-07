using SimpleLauncher.Core.Services.SearchOrchestrator;
using Xunit;

namespace SimpleLauncher.Tests;

public class SearchValidationResultTests
{
    [Fact]
    public void SuccessCreatesValidResult()
    {
        var result = SearchValidationResult.Success("mario");
        Assert.True(result.IsValid);
        Assert.Equal("mario", result.ValidatedQuery);
    }

    [Fact]
    public void FailureCreatesInvalidResult()
    {
        var result = SearchValidationResult.Failure();
        Assert.False(result.IsValid);
        Assert.Null(result.ValidatedQuery);
    }

    [Fact]
    public void SuccessTrimsQuery()
    {
        var result = SearchValidationResult.Success("  mario  ");
        Assert.Equal("  mario  ", result.ValidatedQuery);
    }

    [Fact]
    public void SuccessWithEmptyQueryStillValid()
    {
        var result = SearchValidationResult.Success("");
        Assert.True(result.IsValid);
        Assert.Equal("", result.ValidatedQuery);
    }

    [Fact]
    public void SuccessWithSpecialCharacters()
    {
        var result = SearchValidationResult.Success("mega man x2 (usa)");
        Assert.True(result.IsValid);
        Assert.Equal("mega man x2 (usa)", result.ValidatedQuery);
    }

    [Fact]
    public void SuccessWithUnicodeCharacters()
    {
        var result = SearchValidationResult.Success("ポケモン");
        Assert.True(result.IsValid);
        Assert.Equal("ポケモン", result.ValidatedQuery);
    }

    [Fact]
    public void FailureIsValidFalseByDefault()
    {
        var result = SearchValidationResult.Failure();
        Assert.False(result.IsValid);
    }

    [Fact]
    public void SuccessPreservesInternalWhitespace()
    {
        var result = SearchValidationResult.Success("  super  mario  world  ");
        Assert.Equal("  super  mario  world  ", result.ValidatedQuery);
    }

    [Fact]
    public void MultipleSuccessResultsAreIndependent()
    {
        var r1 = SearchValidationResult.Success("zelda");
        var r2 = SearchValidationResult.Success("mario");

        Assert.Equal("zelda", r1.ValidatedQuery);
        Assert.Equal("mario", r2.ValidatedQuery);
    }

    [Fact]
    public void FailureResultHasNullValidatedQuery()
    {
        var result = SearchValidationResult.Failure();
        Assert.Null(result.ValidatedQuery);
    }
}
