using System.Text.Json.Nodes;

namespace IronHive.Providers.OpenAI.Compatible;

/// <summary>
/// The one merge rule for a caller's <c>ExtraBody</c>, shared by the chat and embedding paths: an object merges into an
/// object; any other value replaces the one there, including a field this library set.
/// </summary>
internal static class JsonObjectMerge
{
    public static void DeepMerge(JsonObject target, JsonObject source)
    {
        foreach (var (key, value) in source)
        {
            if (target[key] is JsonObject existing && value is JsonObject incoming)
                DeepMerge(existing, incoming);
            else
                target[key] = value?.DeepClone();
        }
    }
}
