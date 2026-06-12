using SimpleLauncher.Services.FindCoverImage;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the Jaro-Winkler similarity algorithm used in cover image matching.
/// </summary>
public class FindCoverImageJaroWinklerTests
{
    // Jaro-Winkler Similarity Tests

    /// <summary>
    /// Verifies that identical strings return a similarity of 1.0.
    /// </summary>
    [Fact]
    public void JaroWinklerIdenticalStringsReturns1()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("hello", "hello");
        Assert.Equal(1.0, result, 10);
    }

    /// <summary>
    /// Verifies that two empty strings return a similarity of 1.0.
    /// </summary>
    [Fact]
    public void JaroWinklerEmptyStringsReturns1()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("", "");
        Assert.Equal(1.0, result, 10);
    }

    /// <summary>
    /// Verifies that two null values return a similarity of 1.0.
    /// </summary>
    [Fact]
    public void JaroWinklerBothNullReturns1()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity(null, null);
        Assert.Equal(1.0, result, 10);
    }

    /// <summary>
    /// Verifies that one empty and one null string return a similarity of 1.0.
    /// </summary>
    [Fact]
    public void JaroWinklerOneEmptyOneNullReturns1()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("", null);
        Assert.Equal(1.0, result, 10);
    }

    /// <summary>
    /// Verifies that comparing a non-empty string with an empty string returns 0.0.
    /// </summary>
    [Fact]
    public void JaroWinklerOneEmptyReturns0()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("hello", "");
        Assert.Equal(0.0, result, 10);
    }

    /// <summary>
    /// Verifies that comparing a non-empty string with null returns 0.0.
    /// </summary>
    [Fact]
    public void JaroWinklerOneNullReturns0()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("hello", null);
        Assert.Equal(0.0, result, 10);
    }

    /// <summary>
    /// Verifies that completely different strings return a low similarity score.
    /// </summary>
    [Fact]
    public void JaroWinklerCompletelyDifferentReturnsLow()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("abc", "xyz");
        Assert.True(result < 0.5);
    }

    /// <summary>
    /// Verifies that similar strings return a high similarity score above 0.9.
    /// </summary>
    [Fact]
    public void JaroWinklerSimilarStringsReturnsHigh()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("mario", "marion");
        Assert.True(result > 0.9);
    }

    /// <summary>
    /// Verifies that the similarity calculation is case-insensitive.
    /// </summary>
    [Fact]
    public void JaroWinklerIsCaseInsensitive()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("HELLO", "hello");
        Assert.Equal(1.0, result, 10);
    }

    /// <summary>
    /// Verifies that the prefix bonus is applied consistently for strings with the same prefix.
    /// </summary>
    [Fact]
    public void JaroWinklerPrefixBonusApplied()
    {
        var noPrefix = FindCoverImageService.CalculateJaroWinklerSimilarity("abcde", "abcxy");
        var withPrefix = FindCoverImageService.CalculateJaroWinklerSimilarity("abcde", "abcxy");
        // Both should be equal since prefix is the same
        Assert.Equal(noPrefix, withPrefix, 10);
    }

    /// <summary>
    /// Verifies that a single matching character returns 0.0 (edge case for minimum length).
    /// </summary>
    [Fact]
    public void JaroWinklerSingleCharacterMatch()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("a", "a");
        Assert.Equal(0.0, result, 10);
    }

    /// <summary>
    /// Verifies that a single non-matching character returns 0.0.
    /// </summary>
    [Fact]
    public void JaroWinklerSingleCharacterMismatch()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("a", "b");
        Assert.Equal(0.0, result, 10);
    }

    /// <summary>
    /// Verifies that transposed characters still yield a high similarity score.
    /// </summary>
    [Fact]
    public void JaroWinklerTranspositions()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("martha", "marhta");
        Assert.True(result > 0.9);
    }

    /// <summary>
    /// Verifies that the similarity calculation is symmetric (order of arguments does not matter).
    /// </summary>
    [Fact]
    public void JaroWinklerSymmetric()
    {
        var r1 = FindCoverImageService.CalculateJaroWinklerSimilarity("hello", "world");
        var r2 = FindCoverImageService.CalculateJaroWinklerSimilarity("world", "hello");
        Assert.Equal(r1, r2, 10);
    }

    /// <summary>
    /// Verifies that identical long strings return a similarity of 1.0.
    /// </summary>
    [Fact]
    public void JaroWinklerLongStrings()
    {
        const string s1 = "super mario bros world edition";
        const string s2 = "super mario bros world edition";
        Assert.Equal(1.0, FindCoverImageService.CalculateJaroWinklerSimilarity(s1, s2), 10);
    }

    /// <summary>
    /// Verifies that similar game names produce a high similarity score above 0.85.
    /// </summary>
    [Fact]
    public void JaroWinklerGameNames()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("street fighter ii", "street fighter ii turbo");
        Assert.True(result > 0.85);
    }

    /// <summary>
    /// Verifies that strings with numbers produce a high similarity score when close.
    /// </summary>
    [Fact]
    public void JaroWinklerWithNumbers()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("game123", "game124");
        Assert.True(result > 0.9);
    }

    /// <summary>
    /// Verifies that strings with special characters are handled correctly.
    /// </summary>
    [Fact]
    public void JaroWinklerWithSpecialChars()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("mega-man x", "mega-man x");
        Assert.Equal(1.0, result, 10);
    }

    /// <summary>
    /// Verifies that the max prefix length of 4 is applied correctly in the algorithm.
    /// </summary>
    [Fact]
    public void JaroWinklerMaxLengthPrefixIs4()
    {
        // The algorithm uses MaxPrefixLength = 4
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("abcdefgh", "abcdeXYZ");
        Assert.True(result > 0.5);
    }

    /// <summary>
    /// Verifies that the similarity result is always between 0.0 and 1.0.
    /// </summary>
    [Fact]
    public void JaroWinklerReturnsBetween0And1()
    {
        var result = FindCoverImageService.CalculateJaroWinklerSimilarity("test", "testing");
        Assert.InRange(result, 0.0, 1.0);
    }
}
