namespace System.Text.Json.Nodes;

internal static class JsonNodeExtensions
{
    /// <summary>
    /// 지정한 키에 해당하는 첫 번째 문자열 값을 트리 전체에서 재귀적으로 찾습니다.
    /// </summary>
    public static string? FindString(this JsonNode? node, string key)
        => node.TryFindValue<string>(key, out var value) ? value : null;

    /// <summary>
    /// 지정한 키에 해당하는 첫 번째 정수 값을 트리 전체에서 재귀적으로 찾습니다.
    /// </summary>
    public static int? FindInt(this JsonNode? node, string key)
        => node.TryFindValue<int>(key, out var value) ? value : null;

    // Deliberately a Try-pattern (not "T? FindValue<T>"): an unconstrained T? doesn't wrap
    // value types in Nullable<T>, so a not-found int would come back as default(int) == 0
    // instead of null, indistinguishable from an actual 0 in the JSON.
    private static bool TryFindValue<T>(this JsonNode? node, string key, out T value)
    {
        if (node is JsonObject obj)
        {
            foreach (var kvp in obj)
            {
                if (kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase) && kvp.Value is JsonValue jsonValue
                    && jsonValue.TryGetValue<T>(out var found))
                {
                    value = found!;
                    return true;
                }
                if (kvp.Value.TryFindValue(key, out value))
                    return true;
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                if (item.TryFindValue(key, out value))
                    return true;
            }
        }

        value = default!;
        return false;
    }
}
