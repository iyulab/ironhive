using System.Text.Json.Nodes;

namespace System.Net.Http;

public static class HttpResonseMessageExtensions
{
    /// <summary>
    /// HttP 응답 메시지에서 재귀적으로 "message"라는 속성이 존재하는 경우 해당 메시지를 반환합니다.
    /// </summary>
    public static bool TryExtractMessage(this HttpResponseMessage response, out string message)
    {
        var name = "message";

        message = string.Empty;
        if (response.IsSuccessStatusCode)
            return false;

        try
        {
            using var stream = response.Content.ReadAsStream();
            using var reader = new StreamReader(stream);
            var str = reader.ReadToEnd();
            if (!str.Contains(name, StringComparison.OrdinalIgnoreCase))
                return false;

            var json = JsonNode.Parse(str);
            if (json == null)
                return false;

            message = FindPropertyValue(json, name);
            return !string.IsNullOrEmpty(message);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// JsonNode를 재귀적으로 탐색하여 해당하는 속서의 값을 찾습니다.
    /// </summary>
    private static string FindPropertyValue(JsonNode? node, string name)
    {
        if (node is JsonObject obj)
        {
            foreach (var kvp in obj)
            {
                if (kvp.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return kvp.Value?.ToString() ?? string.Empty;

                var result = FindPropertyValue(kvp.Value, name);
                if (!string.IsNullOrEmpty(result))
                    return result;
            }
        }
        
        if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                var result = FindPropertyValue(item, name);
                if (!string.IsNullOrEmpty(result))
                    return result;
            }
        }

        return string.Empty;
    }
}
