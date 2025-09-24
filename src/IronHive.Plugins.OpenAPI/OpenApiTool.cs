using System.Text;
using System.Text.Json;
using Microsoft.OpenApi;
using IronHive.Abstractions.Tools;

namespace IronHive.Plugins.OpenAPI;

/// <inheritdoc />
public sealed class OpenApiTool : ITool
{
    private readonly HttpClient _http;
    private readonly IDictionary<string, IOpenApiCredential>? _credentials;

    public OpenApiTool(HttpClient http, IDictionary<string, IOpenApiCredential>? credentials = null)
    {
        _http = http;
        _credentials = credentials;
    }

    /// <summary> 이 도구의 클라이언트 이름입니다. </summary>
    public required string ClientName { get; init; }
    /// <summary> 이 도구에서 호출하는 API의 operationId입니다. </summary>
    public required string OperationId { get; init; }
    /// <summary> 이 도구에서 호출하는 API의 기본 URI들입니다. 성공할 때까지 순차적으로 시도합니다. </summary>
    public required IEnumerable<Uri> BaseUris { get; init; }
    /// <summary> 이 도구에서 호출하는 API의 HTTP 메서드입니다. </summary>
    public required HttpMethod Method { get; init; }
    /// <summary> 이 도구에서 호출하는 API의 경로(패스)입니다. </summary>
    public required string Path { get; init; }
    /// <summary> 이 도구에서 사용되는 요청 파라미터들입니다. </summary>
    public required OpenApiProperties Properties { get; init; }
    /// <summary> 이 도구에서 사용되는 보안 요구사항(스키마)들입니다. </summary>
    public IEnumerable<OpenApiSecurityRequirement>? Requirements { get; init; }

    /// <inheritdoc />
    public string UniqueName => $"openapi_{ClientName}_{OperationId}";
    /// <inheritdoc />
    public string? Description { get; init; }
    /// <inheritdoc />
    public object? Parameters { get; init; }
    /// <inheritdoc />
    public bool RequiresApproval { get; set; } = true;

    /// <inheritdoc />
    public async Task<ToolOutput> InvokeAsync(
        ToolInput input, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queue = new Queue<Uri>(BaseUris);
            var errors = new List<Exception>();

            while (queue.TryDequeue(out var baseUri) && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var req = BuildHttpRequest(baseUri, input);
                    using var res = await _http.SendAsync(req, cancellationToken).ConfigureAwait(false);
                    var text = await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    if (!res.IsSuccessStatusCode)
                        throw new Exception($"HTTP {(int)res.StatusCode} {res.ReasonPhrase} {text}");

                    // ToolOutput의 payload 크기 제한(120,000자) 옵션 도입 필요
                    var payload = text.Length > 120_000 ? text[..120_000] : text;
                    return ToolOutput.Success(payload);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }

            return errors.Count > 0
                ? ToolOutput.Failure("Request failed on all servers: " + string.Join(" | ", errors.Select(e => e.Message)))
                : ToolOutput.Failure("Request failed on all servers.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ToolOutput.Failure("Request failed: " + ex.Message);
        }
    }

    /// <summary> 전달받은 input을 바탕으로 HttpRequestMessage를 생성합니다. </summary>
    private HttpRequestMessage BuildHttpRequest(Uri baseUrl, ToolInput input)
    {
        // 1) 준비 - path, query, header, cookie, body
        var path = Path;
        var query = new List<KeyValuePair<string, string>>();
        var headers = new List<KeyValuePair<string, string>>();
        var cookies = new List<KeyValuePair<string, string>>();
        HttpContent? body = null;

        // 2) PATH
        if (input.TryGetValue<Dictionary<string, object?>>("path", out var pItem))
        {
            foreach (var (name, param) in Properties.Path)
            {
                if (pItem.TryGetValue(name, out var value))
                {
                    var segment = OpenApiParameterSerializer.SerializePath(param, value);
                    path = path.Replace($"{{{name}}}", segment);
                }
            }
        }

        // 3) QUERY
        if (input.TryGetValue<Dictionary<string, object?>>("query", out var qItem))
        {
            foreach (var (name, param) in Properties.Query)
            {
                if (qItem.TryGetValue(name, out var value))
                {
                    foreach (var pair in OpenApiParameterSerializer.SerializeQuery(param, value))
                        query.Add(pair);
                }
            }
        }

        // 4) HEADER
        if (input.TryGetValue<Dictionary<string, object?>>("header", out var hItem))
        {
            foreach (var (name, param) in Properties.Header)
            {
                if (hItem.TryGetValue(name, out var value))
                {
                    foreach (var pair in OpenApiParameterSerializer.SerializeHeader(param, value))
                        headers.Add(pair);
                }
            }
        }

        // 5) COOKIE
        if (input.TryGetValue<Dictionary<string, object?>>("cookie", out var cItem))
        {
            foreach (var (name, param) in Properties.Cookie)
            {
                if (cItem.TryGetValue(name, out var value))
                {
                    foreach (var pair in OpenApiParameterSerializer.SerializeCookie(param, value))
                        cookies.Add(pair);
                }
            }
        }

        // 6) BODY (우선순위: application/json > text/plain > 기타)
        if (input.TryGetValue<object>("body", out var bItem))
        {
            var contentType = Properties.Body?.Content is null ? string.Empty
                : Properties.Body.Content.ContainsKey("application/json") ? "application/json"
                : Properties.Body.Content.ContainsKey("text/plain") ? "text/plain"
                : Properties.Body.Content.Keys.First();

            if (!string.IsNullOrWhiteSpace(contentType))
            {
                if (contentType == "application/json")
                {
                    var json = JsonSerializer.Serialize(bItem);
                    body = new StringContent(json, Encoding.UTF8, contentType);
                }
                else
                {
                    body = new StringContent(bItem?.ToString() ?? string.Empty, Encoding.UTF8, contentType);
                }
            }
        }

        // 7) SECURITY (apiKey, basic, bearer, oauth2, oidc)
        if (Requirements is not null && _credentials is not null)
        {
            foreach (var require in Requirements)
            {
                foreach (var (schemeRef, _scope) in require)
                {
                    var name = schemeRef.Reference?.Id;
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    if (!_credentials.TryGetValue(name, out var cred)) continue;
                    if (!cred.Match(schemeRef)) continue;

                    switch (cred)
                    {
                        case ApiKeyCredential apiKey:
                            switch (schemeRef.In)
                            {
                                case ParameterLocation.Path:
                                    path = path.Replace($"{{{schemeRef.Name}}}", Uri.EscapeDataString(apiKey.Value));
                                    break;
                                case ParameterLocation.Query:
                                    query.Add(new(schemeRef.Name!, apiKey.Value));
                                    break;
                                case ParameterLocation.Header:
                                    headers.Add(new(schemeRef.Name!, apiKey.Value));
                                    break;
                                case ParameterLocation.Cookie:
                                    cookies.Add(new(schemeRef.Name!, apiKey.Value));
                                    break;
                            }
                            break;
                        case HttpBasicCredential basic:
                            var basicVal = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{basic.Username}:{basic.Password}"));
                            headers.Add(new("Authorization", $"Basic {basicVal}"));
                            break;
                        case HttpBearerCredential bearer:
                            headers.Add(new("Authorization", $"Bearer {bearer.Token}"));
                            break;
                        case OAuth2Credential oauth:
                            headers.Add(new("Authorization", $"Bearer {oauth.AccessToken}"));
                            break;
                        case OpenIdConnectCredential oid:
                            headers.Add(new("Authorization", $"Bearer {oid.AccessToken}"));
                            break;
                    }
                }
            }
        }

        // 8) BUILD URI + REQUEST
        var uri = BuildUri(baseUrl, path, query);
        var req = new HttpRequestMessage(Method, uri);

        // body
        if (body is not null)
        {
            req.Content = body;
        }

        // headers
        foreach (var g in headers.GroupBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase))
        {
            // Content-* 헤더는 Content에 설정
            if (g.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase) && req.Content is not null)
            {
                foreach (var kv in g)
                    req.Content.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
            }
            else
            {
                req.Headers.TryAddWithoutValidation(g.Key, g.Select(kv => kv.Value));
            }
        }

        // cookies (단일 Cookie 헤더로 합치기)
        if (cookies.Count > 0)
        {
            var cookieHeader = string.Join("; ", cookies.Select(kv => $"{kv.Key}={kv.Value}"));
            req.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
        }

        return req;
    }

    /// <summary> 주어진 baseUrl, path, query를 합쳐 Uri를 생성합니다. </summary>
    private static Uri BuildUri(Uri baseUrl, string path, IEnumerable<KeyValuePair<string, string>> query)
    {
        var builder = new UriBuilder(new Uri(baseUrl, path));

        // 기존 쿼리 + 신규 쿼리 머지
        var pairs = new List<KeyValuePair<string, string>>();
        if (!string.IsNullOrWhiteSpace(builder.Query))
        {
            var existing = builder.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach (var e in existing)
            {
                var idx = e.IndexOf('=');
                if (idx > 0)
                {
                    var k = Uri.UnescapeDataString(e[..idx]);
                    var v = Uri.UnescapeDataString(e[(idx + 1)..]);
                    pairs.Add(new(k, v));
                }
                else
                {
                    // 키만 있고 값이 없는 경우도 보존
                    pairs.Add(new(Uri.UnescapeDataString(e), string.Empty));
                }
            }
        }
        pairs.AddRange(query);

        builder.Query = pairs.Count > 0
            ? string.Join("&", pairs.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"))
            : string.Empty;

        return builder.Uri;
    }
}
