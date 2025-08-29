using IronHive.Abstractions.Tools;
using Microsoft.OpenApi;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IronHive.Plugins.OpenAPI;

/// <summary>
/// 최신 OpenAPI.NET(2.x) 기반의 범용 OpenAPI 호출 도구
/// 필수 입력: operationId
/// 선택 입력: path/query/header 파라미터명 그대로, body는 Json 직렬화 가능한 객체 또는 JsonElement/JsonNode
/// </summary>
public sealed class OpenApiTool : ITool, IDisposable
{
    private readonly OpenApiDocument _document;
    private readonly Dictionary<string, (OpenApiOperation op, string path)> _byOperationId;
    private readonly HttpClient _http;

    public string Name { get; init; } = "openapi-invoker";
    public string Description { get; init; } =
        "OpenAPI 스펙을 로드해 operationId로 HTTP 요청을 구성/호출하는 도구";
    public object? Parameters { get; init; }
    public bool RequiresApproval { get; set; }

    /// <summary>
    /// 스펙을 URL/파일 경로에서 직접 로드하는 생성자 (권장)
    /// </summary>
    public static async Task<OpenApiTool> CreateAsync(
        string openApiLocation,
        HttpClient? httpClient = null,
        CancellationToken ct = default)
    {
        // Microsoft.OpenApi 2.x는 LoadAsync 제공
        // 예시: var (doc, _) = await OpenApiDocument.LoadAsync("https://.../openapi.yaml");
        var (doc, _) = await OpenApiDocument.LoadAsync(openApiLocation);
        return new OpenApiTool(doc, httpClient);
    }

    /// <summary>
    /// 스펙 Stream 기반 (파일/리소스 스트림 등)
    /// </summary>
    public static async Task<OpenApiTool> CreateAsync(
        Stream openApiStream,
        HttpClient? httpClient = null,
        CancellationToken ct = default)
    {
        var (doc, _) = await OpenApiDocument.LoadAsync(openApiStream);
        return new OpenApiTool(doc, httpClient);
    }

    private OpenApiTool(OpenApiDocument document, HttpClient? httpClient)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _http = httpClient ?? new HttpClient();

        _byOperationId = new(StringComparer.Ordinal);
        foreach (var (path, item) in _document.Paths)
        {
            foreach (var (method, op) in item.Operations)
            {
                var id = op.OperationId ?? $"{method}_{path}";
                //_byOperationId[id] = (op, path, method);
            }
        }
    }

    public void Dispose()
    {
        _http.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<ToolOutput> InvokeAsync(ToolInput input, CancellationToken cancellationToken = default)
    {
        try
        {
            using var resp = await _http.SendAsync(new HttpRequestMessage(), cancellationToken);
            var text = await resp.Content.ReadAsStringAsync(cancellationToken);

            if (!resp.IsSuccessStatusCode)
                return ToolOutput.Failure($"API 호출 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{text}");

            // JSON이면 pretty-print
            if (IsJson(resp.Content.Headers.ContentType))
            {
                try
                {
                    var node = JsonNode.Parse(text);
                    text = node?.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) ?? text;
                }
                catch { /* JSON이 아닐 수 있음 - 원문 반환 */ }
            }

            return ToolOutput.Success(text);
        }
        catch (Exception ex)
        {
            return ToolOutput.Failure($"예외 발생: {ex.GetType().Name}: {ex.Message}");
        }
    }

    // ===== Helpers =====

    private static bool TryGetString(ToolInput input, string key, [NotNullWhen(true)] out string? value)
    {
        if (input.TryGetValue(key, out var raw) && raw is not null)
        {
            value = raw.ToString();
            return true;
        }
        value = null;
        return false;
    }

    private static bool TryExtractParam(ToolInput input, OpenApiParameter p, out List<string> values)
    {
        values = new();
        if (!input.TryGetValue(p.Name, out var val) || val is null)
            return false;

        switch (val)
        {
            case string s:
                if (!string.IsNullOrEmpty(s)) values.Add(s);
                break;
            case IEnumerable<object?> list:
                foreach (var x in list)
                    if (x is not null) values.Add(x.ToString()!);
                break;
            default:
                values.Add(val.ToString()!);
                break;
        }
        return values.Count > 0;
    }

    private static OpenApiMediaType? PickBestContentType(IDictionary<string, OpenApiMediaType> content)
    {
        // 선호도: json > form > 기타
        if (content.TryGetValue("application/json", out var json)) return json;
        if (content.TryGetValue("application/*+json", out var j2)) return j2;
        if (content.TryGetValue("application/x-www-form-urlencoded", out var form)) return form;
        if (content.TryGetValue("multipart/form-data", out var multi)) return multi;

        return content.Values.FirstOrDefault();
    }

    private static bool IsJson(MediaTypeHeaderValue? ct)
        => ct?.MediaType is string s && (s.Contains("json", StringComparison.OrdinalIgnoreCase) || s.EndsWith("+json", StringComparison.OrdinalIgnoreCase));

    private static bool TryBuildRequestBody(
        OpenApiMediaType mediaType,
        ToolInput input,
        out HttpContent content,
        out string? error)
    {
        // 사용자가 body 전체를 'body' 키로 넘기면 그대로 사용
        if (input.TryGetValue("body", out var bodyObj) && bodyObj is not null)
        {
            if (mediaType is not null && mediaType.Schema is not null)
            {
                // 단순히 직렬화; 스키마 유효성까지는 여기서 처리하지 않음
            }

            var json = JsonSerializer.Serialize(bodyObj);
            content = new StringContent(json, Encoding.UTF8, "application/json");
            error = null;
            return true;
        }

        // 개별 속성으로 왔다면 스키마 기반으로 JSON 구성(필수만 체크)
        var node = new JsonObject();
        var schema = mediaType.Schema;

        if (schema == null)
        {
            content = new StringContent("{}", Encoding.UTF8, "application/json");
            error = null;
            return true;
        }

        foreach (var prop in schema.Properties)
        {
            var name = prop.Key;
            if (input.TryGetValue(name, out var val) && val is not null)
            {
                node[name] = JsonSerializer.SerializeToNode(val);
            }
            else if (schema.Required.Contains(name))
            {
                content = new StringContent("", Encoding.UTF8, "application/json");
                error = $"요청 본문 필수 속성 '{name}'가 필요합니다.";
                return false;
            }
        }

        var jsonText = node.ToJsonString();
        content = new StringContent(jsonText, Encoding.UTF8, "application/json");
        error = null;
        return true;
    }
}
