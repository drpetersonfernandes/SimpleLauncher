using System.Text.Json;
using System.Text.Json.Serialization;
using SimpleLauncher.Core.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="BoolConverter"/>, which handles API booleans expressed as numbers (0/1).
/// </summary>
public class BoolConverterTests
{
    private sealed class Payload
    {
        [JsonConverter(typeof(BoolConverter))]
        public bool Flag { get; set; }

        [JsonConverter(typeof(BoolConverter))]
        public bool Other { get; set; }
    }

    private static Payload Deserialize(string json)
    {
        return JsonSerializer.Deserialize<Payload>(json)!;
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void Read_JsonBooleanTokens_AreParsed(string jsonToken, bool expected)
    {
        var payload = Deserialize($"{{\"Flag\":{jsonToken},\"Other\":false}}");
        Assert.Equal(expected, payload.Flag);
    }

    [Theory]
    [InlineData("0", false)]
    [InlineData("1", true)]
    [InlineData("-1", true)]
    [InlineData("5", true)]
    public void Read_NumericTokens_AreParsedAsBooleans(string number, bool expected)
    {
        var payload = Deserialize($"{{\"Flag\":{number},\"Other\":false}}");
        Assert.Equal(expected, payload.Flag);
    }

    [Fact]
    public void Read_StringToken_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => Deserialize("""{"Flag":"yes","Other":false}"""));
    }

    [Fact]
    public void Read_NullToken_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => Deserialize("""{"Flag":null,"Other":false}"""));
    }

    [Fact]
    public void Write_EmitsJsonBooleanTokens()
    {
        var json = JsonSerializer.Serialize(new Payload { Flag = true, Other = false });
        Assert.Contains("\"Flag\":true", json, StringComparison.Ordinal);
        Assert.Contains("\"Other\":false", json, StringComparison.Ordinal);
    }

    [Fact]
    public void RoundTrip_TrueAndFalse_ArePreserved()
    {
        var payload = Deserialize("""{"Flag":1,"Other":0}""");
        var json = JsonSerializer.Serialize(payload);
        var roundTripped = Deserialize(json);
        Assert.True(roundTripped.Flag);
        Assert.False(roundTripped.Other);
    }
}
