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
        var path = string.Format(GoogleAIConstants.PostStreamGenerateContentPath, request.Model).RemovePrefix('/');
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

        int n;
        while ((n = await reader.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
        {
            for (int i = 0; i < n; i++)
            {
                char c = buffer[i];

                // JsonObject 시작
                if (c == '{')
                {
                    sb.Append(c);
                    depth++;
                    continue;
                }

                // 잘못된 스트림
                if (depth < 0)
                    throw new JsonException("Invalid JSON stream");

                // JsonObject 시작 전
                if (depth == 0)
                    continue;

                // JsonObject 종료
                if (c == '}')
                {
                    depth--;
                }

                // JsonObject 채우기
                sb.Append(c);

                // 오브젝트 완성시
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