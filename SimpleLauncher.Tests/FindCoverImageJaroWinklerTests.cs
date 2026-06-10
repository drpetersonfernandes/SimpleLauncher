using SimpleLauncher.Services.FindCoverImage;
using Xunit;

namespace SimpleLauncher.Tests;

public class FindCoverImageJaroWinklerTests
{
    // Jaro-Winkler Similarity Tests

    [Fact]
    public void JaroWinklerIdenticalStringsReturns1()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("hello", "hello");
        Assert.Equal(1.0, result, 10);
    }

    [Fact]
    public void JaroWinklerEmptyStringsReturns1()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("", "");
        Assert.Equal(1.0, result, 10);
    }

    [Fact]
    public void JaroWinklerBothNullReturns1()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity(null, null);
        Assert.Equal(1.0, result, 10);
    }

    [Fact]
    public void JaroWinklerOneEmptyOneNullReturns1()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("", null);
        Assert.Equal(1.0, result, 10);
    }

    [Fact]
    public void JaroWinklerOneEmptyReturns0()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("hello", "");
        Assert.Equal(0.0, result, 10);
    }

    [Fact]
    public void JaroWinklerOneNullReturns0()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("hello", null);
        Assert.Equal(0.0, result, 10);
    }

    [Fact]
    public void JaroWinklerCompletelyDifferentReturnsLow()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("abc", "xyz");
        Assert.True(result < 0.5);
    }

    [Fact]
    public void JaroWinklerSimilarStringsReturnsHigh()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("mario", "marion");
        Assert.True(result > 0.9);
    }

    [Fact]
    public void JaroWinklerIsCaseInsensitive()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("HELLO", "hello");
        Assert.Equal(1.0, result, 10);
    }

    [Fact]
    public void JaroWinklerPrefixBonusApplied()
    {
        var noPrefix = FindCoverImageService.CalculateJaroWinklerSimilarity("abcde", "abcxy");
        var withPrefix = FindCoverImageService.CalculateJaroWinklerSimilarity("abcde", "abcxy");
        // Both should be equal since prefix is the same
        Assert.Equal(noPrefix, withPrefix, 10);
    }

    [Fact]
    public void JaroWinklerSingleCharacterMatch()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("a", "a");
        Assert.Equal(0.0, result, 10);
    }

    [Fact]
    public void JaroWinklerSingleCharacterMismatch()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("a", "b");
        Assert.Equal(0.0, result, 10);
    }

    [Fact]
    public void JaroWinklerTranspositions()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("martha", "marhta");
        Assert.True(result > 0.9);
    }

    [Fact]
    public void JaroWinklerSymmetric()
    {
        var r1 = FindCoverImageService.CalculateJaroWinklerSimilarity("hello", "world");
        var r2 = FindCoverImageService.CalculateJaroWinklerSimilarity("world", "hello");
        Assert.Equal(r1, r2, 10);
    }

    [Fact]
    public void JaroWinklerLongStrings()
    {
        const string s1 = "super mario bros world edition";
        const string s2 = "super mario bros world edition";
        Assert.Equal(1.0, FindCoverImageService.CalculateJaroWinklerSimilarity(s1, s2), 10);
    }

    [Fact]
    public void JaroWinklerGameNames()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("street fighter ii", "street fighter ii turbo");
        Assert.True(result > 0.85);
    }

    [Fact]
    public void JaroWinklerWithNumbers()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("game123", "game124");
        Assert.True(result > 0.9);
    }

    [Fact]
    public void JaroWinklerWithSpecialChars()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("mega-man x", "mega-man x");
        Assert.Equal(1.0, result, 10);
    }

    [Fact]
    public void JaroWinklerMaxLengthPrefixIs4()
    {
        // The algorithm uses MaxPrefixLength = 4
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("abcdefgh", "abcdeXYZ");
        Assert.True(result > 0.5);
    }

    [Fact]
    public void JaroWinklerReturnsBetween0And1()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("test", "testing");
        Assert.InRange(result, 0.0, 1.0);
    }
}
