using System.Text.Json.Nodes;

namespace System.Net.Http;

public static class HttpMessageExtensions
{
    /// <summary>
    /// HTTP 응답 메시지에서 재귀적으로 "message" 속성을 탐색하여 해당 메시지를 추출합니다.
    /// 응답이 실패했을 경우에만 메시지를 추출합니다.
    /// </summary>
    /// <param name="message">찾아낸 메시지 문자열 (없으면 빈 문자열)</param>
    /// <returns>"message" 속성을 성공적으로 추출했으면 true, 그렇지 않으면 false</returns>
    public static bool TryExtractMessage(this HttpResponseMessage response, out string message)
    {
        const string name = "message";

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
    /// JsonNode를 재귀적으로 순회하여 지정된 속성 이름과 일치하는 값을 찾아 반환합니다.
    /// </summary>
    /// <param name="node">탐색할 JsonNode 객체</param>
    /// <param name="name">찾고자 하는 속성 이름</param>
    /// <returns>속성 값이 존재하면 해당 문자열, 없으면 빈 문자열</returns>
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
