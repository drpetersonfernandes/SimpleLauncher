using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

/// <summary>
/// Custom converter to handle API returning boolean values as numbers (0/1).
/// </summary>
public class BoolConverter : JsonConverter<bool>
{
    /// <summary>
    /// Reads and converts a JSON token to a boolean, handling true, false, and numeric (0/1) representations.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The target type to convert to.</param>
    /// <param name="options">Serialization options.</param>
    /// <returns>The parsed boolean value.</returns>
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

    /// <summary>
    /// Writes a boolean value as a JSON boolean token.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The boolean value to write.</param>
    /// <param name="options">Serialization options.</param>
    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }
}
