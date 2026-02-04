using IronHive.Providers.GoogleAI.Payloads.GenerateContent;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace IronHive.Providers.GoogleAI.Clients;

internal class GoogleAIGenerateContentClient : GoogleAIClientBase
{
    public GoogleAIGenerateContentClient(GoogleAIConfig config) : base(config)
    { }

    public GoogleAIGenerateContentClient(string apiKey) : base(apiKey)
    { }

    public async Task<GenerateContentResponse> GenerateContentAsync(
        GenerateContentRequest request, 
        CancellationToken cancellationToken = default)
    {
        request.Model = request.Model.EnsurePrefix("models/");
        var path = string.Format(GoogleAIConstants.PostGenerateContentPath, request.Model).RemovePrefix('/');
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(path, content, cancellationToken);
        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        var res = await response.Content.ReadFromJsonAsync<GenerateContentResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
        return res;
    }

    public async IAsyncEnumerable<GenerateContentResponse> StreamGenerateContentAsync(
        GenerateContentRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request.Model = request.Model.EnsurePrefix("models/");
        var path = string.Format(GoogleAIConstants.PostStreamGenerateContentPath, request.Model).RemovePrefix('/') + "?alt=sse";
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var _request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        using var response = await _client.SendAsync(_request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8, true, 8192, false);

        // --- 상태 머신 ---
        var sb = new StringBuilder();
        var buffer = new char[4096];
        int depth = 0;
        bool inString = false;
        bool escape = false;

        int n;
        while ((n = await reader.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
        {
            for (int i = 0; i < n; i++)
            {
                char c = buffer[i];

                // 오브젝트 시작 전: 첫 '{' 찾기
                if (depth == 0)
                {
                    if (c == '{')
                    {
                        sb.Append(c);
                        depth = 1;
                        inString = false;
                        escape = false;
                    }
                    continue;
                }

                // 오브젝트 진행 중: 일단 append
                sb.Append(c);

                // \" \\ \n 등 이스케이프 처리 중이면 이번 글자는 의미 해석 안 함
                if (escape)
                {   
                    escape = false;
                    continue;
                }

                // 문자열 안에서만 의미 있지만, 안전하게 처리
                if (c == '\\')
                {
                    escape = true;
                    continue;
                }

                // 문자열 내부 토글
                if (c == '"')
                {
                    inString = !inString;
                    continue;
                }

                // 문자열 밖에서만 depth 카운트
                if (!inString)
                {
                    if (c == '{')
                    {
                        depth++;
                    }
                    else if (c == '}')
                    {
                        depth--;
                        if (depth < 0)
                            throw new JsonException("Invalid JSON stream");

                        // 오브젝트 완성
                        if (depth == 0)
                        {
                            var jsonStr = sb.ToString();
                            sb.Clear();

                            var item = JsonSerializer.Deserialize<GenerateContentResponse>(jsonStr, _jsonOptions)
                                       ?? throw new JsonException("null deserialized");

                            yield return item;
                        }
                    }
                }
            }
        }

        // 스트림이 끝났는데 오브젝트가 덜 닫혔으면 예외
        if (depth != 0)
            throw new JsonException("Truncated JSON stream");
    }
}