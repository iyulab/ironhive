using System.Text.Json;
using Microsoft.Extensions.AI;

namespace IronHive.Providers.Endpoints;

/// <summary>Small readers shared by the wire codecs: absent or mistyped properties read as <see langword="null"/>.</summary>
internal static class WireJson
{
    // AIJsonUtilities.DefaultOptions indents; tool arguments travel as one line.
    private static readonly JsonSerializerOptions CompactOptions = new(AIJsonUtilities.DefaultOptions) { WriteIndented = false };

    /// <summary>The arguments of a tool call as one line of JSON (an empty object for none).</summary>
    public static string Arguments(FunctionCallContent call) =>
        JsonSerializer.Serialize(call.Arguments ?? new Dictionary<string, object?>(), CompactOptions);

    public static string? GetString(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    public static float? GetFloat(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetSingle()
            : null;

    public static int? GetInt(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
            ? number
            : null;
}
