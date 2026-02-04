using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using IronHive.Providers.OpenAI.Compatible.Adapters;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;

namespace IronHive.Providers.OpenAI.Compatible.Clients;

/// <summary>
/// OpenAI 호환 서비스를 위한 Chat Completion 클라이언트입니다.
/// Adapter를 통해 제공자별 요청/응답 변환을 수행합니다.
/// </summary>
public class CompatibleChatCompletionClient : IDisposable
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IProviderAdapter _adapter;
    private readonly CompatibleConfig _config;

    private const string ChatCompletionPath = "chat/completions";

    public CompatibleChatCompletionClient(CompatibleConfig config)
    {
        _config = config;
        _adapter = ProviderAdapterFactory.GetAdapter(config);
        _client = CreateHttpClient(config, _adapter);
        _jsonOptions = CreateJsonOptions();
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Chat Completion 요청을 보냅니다.
    /// </summary>
    public async Task<ChatCompletionResponse> PostChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Stream = false;

        // 1. 요청을 JSON으로 직렬화
        var requestJson = JsonSerializer.SerializeToNode(request, _jsonOptions) as JsonObject
            ?? throw new InvalidOperationException("Failed to serialize request.");

        // 2. Adapter를 통해 요청 변환
        var transformedRequest = _adapter.TransformRequest(requestJson, _config);

        // 3. HTTP 요청 전송
        var json = transformedRequest.ToJsonString(_jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _client.PostAsync(ChatCompletionPath, content, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        // 4. 응답 파싱
        var responseJson = await response.Content.ReadFromJsonAsync<JsonObject>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");

        // 5. Adapter를 통해 응답 변환
        var transformedResponse = _adapter.TransformResponse(responseJson);

        // 6. 최종 응답 객체로 역직렬화
        return transformedResponse.Deserialize<ChatCompletionResponse>(_jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize transformed response.");
    }

    /// <summary>
    /// 스트리밍 Chat Completion 요청을 보냅니다.
    /// </summary>
    public async IAsyncEnumerable<StreamingChatCompletionResponse> PostStreamingChatCompletionAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request.Stream = true;
        request.StreamOptions = new ChatCompletionStreamOptions { IncludeUsage = true };

        // 1. 요청을 JSON으로 직렬화
        var requestJson = JsonSerializer.SerializeToNode(request, _jsonOptions) as JsonObject
            ?? throw new InvalidOperationException("Failed to serialize request.");

        // 2. Adapter를 통해 요청 변환
        var transformedRequest = _adapter.TransformRequest(requestJson, _config);

        // 3. HTTP 요청 전송
        var json = transformedRequest.ToJsonString(_jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ChatCompletionPath);
        httpRequest.Content = content;

        using var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        // 4. 스트리밍 응답 처리
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("data:"))
            {
                var data = line.Substring("data:".Length).Trim();
                if (!data.StartsWith('{') || !data.EndsWith('}'))
                    continue;

                // 청크 파싱
                var chunkJson = JsonSerializer.Deserialize<JsonObject>(data, _jsonOptions);
                if (chunkJson == null)
                    continue;

                // 5. Adapter를 통해 청크 변환
                var transformedChunk = _adapter.TransformStreamingChunk(chunkJson);

                // 6. 최종 응답 객체로 역직렬화
                var message = transformedChunk.Deserialize<StreamingChatCompletionResponse>(_jsonOptions);
                if (message != null)
                {
                    yield return message;
                }
            }

            // GPUStack 등 일부 플랫폼의 에러 처리
            if (line.StartsWith("error:"))
            {
                var data = line.Substring("error:".Length).Trim();
                throw new InvalidOperationException(data);
            }
        }
    }

    private HttpClient CreateHttpClient(CompatibleConfig config, IProviderAdapter adapter)
    {
        var baseUrl = adapter.GetBaseUrl(config);

        if (string.IsNullOrEmpty(baseUrl))
        {
            throw new InvalidOperationException(
                $"BaseUrl is required for provider '{config.Provider}'.");
        }

        // URL이 /로 끝나도록 보장
        if (!baseUrl.EndsWith('/'))
        {
            baseUrl += '/';
        }

        var client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };

        // Authorization 헤더
        if (!string.IsNullOrWhiteSpace(config.ApiKey))
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
        }

        // Adapter에서 제공하는 추가 헤더
        var additionalHeaders = adapter.GetAdditionalHeaders(config);
        foreach (var header in additionalHeaders)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        // Config의 DefaultHeaders 적용
        if (config.DefaultHeaders != null)
        {
            foreach (var header in config.DefaultHeaders)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        return client;
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            AllowOutOfOrderMetadataProperties = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)
            },
        };
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Request failed with status {response.StatusCode}: {errorContent}");
        }
    }
}
