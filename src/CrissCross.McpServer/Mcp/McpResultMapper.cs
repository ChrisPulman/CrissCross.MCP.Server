using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrissCross.McpServer.Mcp;

/// <summary>
/// Serializes MCP tool results using the server's stable JSON settings.
/// </summary>
public static class McpResultMapper
{
    /// <summary>
    /// Gets the JSON serializer options used by MCP tool result payloads.
    /// </summary>
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Serializes a value directly or wraps it with an explicit type discriminator.
    /// </summary>
    /// <typeparam name="T">The value type being serialized.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <param name="type">Optional type discriminator included in the payload.</param>
    /// <returns>A JSON string.</returns>
    public static string ToJson<T>(T value, string? type = null)
    {
        if (type is null)
        {
            return JsonSerializer.Serialize(value, JsonOptions);
        }

        return JsonSerializer.Serialize(new { type, value }, JsonOptions);
    }
}
