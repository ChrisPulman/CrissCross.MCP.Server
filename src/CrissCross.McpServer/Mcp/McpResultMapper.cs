using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrissCross.McpServer.Mcp;

public static class McpResultMapper
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string ToJson<T>(T value, string? type = null)
    {
        if (type is null)
        {
            return JsonSerializer.Serialize(value, JsonOptions);
        }

        return JsonSerializer.Serialize(new { type, value }, JsonOptions);
    }
}
