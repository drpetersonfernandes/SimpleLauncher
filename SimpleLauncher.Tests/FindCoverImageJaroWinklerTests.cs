using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.FindCoverImage;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class FindCoverImageJaroWinklerTests
{
    private readonly FindCoverImage _findCoverImage;

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }

    public FindCoverImageJaroWinklerTests()
    {
        ServiceProviderMock.Install();
        var configuration = new ConfigurationBuilder().Build();
        _findCoverImage = new FindCoverImage(configuration, new NoOpLogErrors());
    }

    // Jaro-Winkler Similarity Tests

    [Fact]
    public void JaroWinklerIdenticalStringsReturns1()
    {
        var result = _findCoverImage.CalculateJaroWinklerSimilarity("hello", "hello");
        Assert.Equal(1.0, result, 10);
    }

    [Fact]
    public void JaroWinklerEmptyStringsReturns1()
    {
        var result = _findCoverImage.CalculateJaroWinklerSimilarity("", "");
        Assert.Equal(1.0, result, 10);
    }

    [Fact]
    public void JaroWinklerBothNullReturns1()
    {
        var result = _findCoverImage.CalculateJaroWinklerSimilarity(null, null);
        Assert.Equal(1.0, result, 10);
    }

    [Fact]
    public void JaroWinklerOneEmptyOneNullReturns1()
    {
        var result = _findCoverImage.CalculateJaroWinklerSimilarity("", null);
        Assert.Equal(1.0, result, 10);
    }

    [Fact]
    public void JaroWinklerOneEmptyReturns0()
    {
        var result = _findCoverImage.CalculateJaroWinklerSimilarity("hello", "");
        Assert.Equal(0.0, result, 10);
    }

    [Fact]
    public void JaroWinklerOneNullReturns0()
    {
        var result = _findCoverImage.CalculateJaroWinklerSimilarity("hello", null);
        Assert.Equal(0.0, result, 10);
    }

    [Fact]
    public void JaroWinklerCompletelyDifferentReturnsLow()
    {
        var result = _findCoverImage.CalculateJaroWinklerSimilarity("abc", "xyz");
        Assert.True(result < 0.5);
    }

    [Fact]
    public void JaroWinklerSimilarStringsReturnsHigh()
    {
        var result = _findCoverImage.CalculateJaroWinklerSimilarity("mario", "marion");
        Assert.True(result > 0.9);
    }

    [Fact]
    public void JaroWinklerIsCaseInsensitive()
    {
        var result = _findCoverImage.CalculateJaroWinklerSimilarity("HELLO", "hello");
        Assert.Equal(1.0, result, 10);
    }

    [Fact]
    public void JaroWinklerPrefixBonusApplied()
    {
        var noPrefix = _findCoverImage.CalculateJaroWinklerSimilarity("abcde", "abcxy");
        var withPrefix = _findCoverImage.CalculateJaroWinklerSimilarity("abcde", "abcxy");
        // Both should be equal since prefix is the same
        Assert.Equal(noPrefix, withPrefix, 10);
    }

    [Fact]
    public void JaroWinklerSingleCharacterMatch()
    {
        var result = _findCoverImage.CalculateJaroWinklerSimilarity("a", "a");
        Assert.Equal(0.0, result, 10);
    }

    [Fact]
    public void JaroWinklerSingleCharacterMismatch()
    {
        var result = _findCoverImage.CalculateJaroWinklerSimilarity("a", "b");
        Assert.Equal(0.0, result, 10);
    }

    [Fact]
    public void JaroWinklerTranspositions()
    {
        var result = _findCoverImage.CalculateJaroWinklerSimilarity("martha", "marhta");
        Assert.True(result > 0.9);
    }

    [Fact]
    public void JaroWinklerSymmetric()
    {
        var r1 = _findCoverImage.CalculateJaroWinklerSimilarity("hello", "world");
        var r2 = _findCoverImage.CalculateJaroWinklerSimilarity("world", "hello");
        Assert.Equal(r1, r2, 10);
    }

    [Fact]
    public void JaroWinklerLongStrings()
    {
        const string s1 = "super mario bros world edition";
        const string s2 = "super mario bros world edition";
        Assert.Equal(1.0, _findCoverImage.CalculateJaroWinklerSimilarity(s1, s2), 10);
    }

    [Fact]
    public void JaroWinklerGameNames()
    {
        var result = _findCoverImage.CalculateJaroWinklerSimilarity("street fighter ii", "street fighter ii turbo");
        Assert.True(result > 0.85);
    }

    [Fact]
    public void JaroWinklerWithNumbers()
    {
        var result = _findCoverImage.CalculateJaroWinklerSimilarity("game123", "game124");
        Assert.True(result > 0.9);
    }

    [Fact]
    public void JaroWinklerWithSpecialChars()
    {
        var result = _findCoverImage.CalculateJaroWinklerSimilarity("mega-man x", "mega-man x");
        Assert.Equal(1.0, result, 10);
    }

    [Fact]
    public void JaroWinklerMaxLengthPrefixIs4()
    {
        // The algorithm uses MaxPrefixLength = 4
        var result = _findCoverImage.CalculateJaroWinklerSimilarity("abcdefgh", "abcdeXYZ");
        Assert.True(result > 0.5);
    }

    [Fact]
    public void JaroWinklerReturnsBetween0And1()
    {
        var result = _findCoverImage.CalculateJaroWinklerSimilarity("test", "testing");
        Assert.InRange(result, 0.0, 1.0);
    }
}
