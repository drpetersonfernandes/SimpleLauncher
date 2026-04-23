using SimpleLauncher.Services.RetroAchievements;
using Xunit;

namespace SimpleLauncher.Tests;

public class EncryptDuckStationTokenTests
{
    [Fact]
    public void EncryptDuckStationTokenMethodNullTokenReturnsEmpty()
    {
        var result = EncryptDuckStationToken.EncryptDuckStationTokenMethod(null, "user", true);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void EncryptDuckStationTokenMethodNullUsernameReturnsEmpty()
    {
        var result = EncryptDuckStationToken.EncryptDuckStationTokenMethod("token", null, true);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void EncryptDuckStationTokenMethodEmptyTokenReturnsEmpty()
    {
        var result = EncryptDuckStationToken.EncryptDuckStationTokenMethod("", "user", true);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void EncryptDuckStationTokenMethodEmptyUsernameReturnsEmpty()
    {
        var result = EncryptDuckStationToken.EncryptDuckStationTokenMethod("token", "", true);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void EncryptDuckStationTokenMethodSameInputsReturnsSameOutput()
    {
        var result1 = EncryptDuckStationToken.EncryptDuckStationTokenMethod("mytoken", "myuser", true);
        var result2 = EncryptDuckStationToken.EncryptDuckStationTokenMethod("mytoken", "myuser", true);
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void EncryptDuckStationTokenMethodDifferentUsernamesReturnsDifferentOutput()
    {
        var result1 = EncryptDuckStationToken.EncryptDuckStationTokenMethod("mytoken", "user1", true);
        var result2 = EncryptDuckStationToken.EncryptDuckStationTokenMethod("mytoken", "user2", true);
        Assert.NotEqual(result1, result2);
    }

    [Fact]
    public void EncryptDuckStationTokenMethodDifferentTokensReturnsDifferentOutput()
    {
        var result1 = EncryptDuckStationToken.EncryptDuckStationTokenMethod("token1", "myuser", true);
        var result2 = EncryptDuckStationToken.EncryptDuckStationTokenMethod("token2", "myuser", true);
        Assert.NotEqual(result1, result2);
    }

    [Fact]
    public void EncryptDuckStationTokenMethodValidInputReturnsNonEmptyBase64()
    {
        var result = EncryptDuckStationToken.EncryptDuckStationTokenMethod("mytoken123", "testuser", true);
        Assert.False(string.IsNullOrEmpty(result));
        // Verify it's valid Base64
        var bytes = Convert.FromBase64String(result);
        Assert.NotEmpty(bytes);
    }
}
