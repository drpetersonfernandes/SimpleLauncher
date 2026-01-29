using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Custom converter to handle API returning boolean values as numbers (0/1).
/// </summary>
public class BoolConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number => reader.GetInt32() != 0,
            _ => throw new JsonException($"Unexpected token type: {reader.TokenType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }
}