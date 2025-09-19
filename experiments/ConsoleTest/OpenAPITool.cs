using IronHive.Abstractions.Json;
using IronHive.Abstractions.Tools;
using Json.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using JsonSchema = Json.Schema.JsonSchema;

namespace ConsoleTest;

/// <summary>
/// OpenAPI 문서를 기반으로 HTTP 호출을 수행하는 ITool 구현체입니다.
/// </summary>
[JsonPolymorphicValue("openapi")]
public sealed class OpenApiTool : ITool
{
    private readonly OpenApiDocument _doc;
    private readonly OpenApiOperation _operation;
    private readonly string _path;
    private readonly OperationType _method;

    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// 고유 이름: openapi_{ApiName}_{OperationId}
    /// </summary>
    public string UniqueName => $"openapi_{ApiName}_{OperationId ?? $"{_method}_{_path.Trim('/').Replace('/', '_')}".ToLowerInvariant()}";

    /// <summary>
    /// API(서버) 별칭. 로깅/노출용.
    /// </summary>
    public required string ApiName { get; init; }

    /// <summary>
    /// OpenAPI 문서의 원본 위치(로깅/디버깅용).
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// 선택된 operationId (없으면 path+method 조합)
    /// </summary>
    public string? OperationId { get; }

    /// <inheritdoc />
    public string? Description { get; }

    /// <inheritdoc />
    public object? Parameters { get; }

    /// <inheritdoc />
    public required bool RequiresApproval { get; init; }

    /// <summary>
    /// 기본 요청 타임아웃(초). 기본 60초.
    /// </summary>
    public long TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// 서버 URL 강제 지정(선택). 미지정 시 OAS servers[0] 사용.
    /// </summary>
    public string? OverrideServerUrl { get; init; }

    /// <summary>
    /// 인증/전역 헤더 등 실행 옵션의 키 이름 기본값.
    /// </summary>
    public OpenApiToolOptionKeys OptionKeys { get; init; } = new();

    private OpenApiTool(
        OpenApiDocument doc,
        string path,
        OperationType method,
        string? apiName,
        string? source,
        JsonObject parametersSchema,
        string? description)
    {
        _doc = doc;
        _path = path;
        _method = method;
        _operation = doc.Paths[path].Operations[method];
        OperationId = _operation.OperationId;
        ApiName = apiName ?? "api";
        Source = source;
        Parameters = parametersSchema;
        Description = description;

        _jsonOptions = JsonDefaultOptions.Options; // 프로젝트 기본 옵션 활용
    }

    /// <summary>
    /// factory: operationId 로 선택
    /// </summary>
    public static OpenApiTool FromOperationId(
        string openApiContent,
        string operationId,
        string? apiName = null,
        string? source = null)
    {
        var doc = Parse(openApiContent, out _);
        var (path, method, op) = FindOperationById(doc, operationId)
            ?? throw new ArgumentException($"Operation '{operationId}' not found.");
        var schema = BuildInputSchema(doc, path, method, op);
        var desc = op.Summary ?? op.Description ?? $"Invoke {operationId}";
        return new OpenApiTool(doc, path, method, apiName, source, schema, desc)
        {
            ApiName = apiName ?? "api",
            RequiresApproval = true
        };
    }

    /// <summary>
    /// factory: path + method 로 선택
    /// </summary>
    public static OpenApiTool FromPathAndMethod(
        string openApiContent,
        string path,
        OperationType method,
        string? apiName = null,
        string? source = null)
    {
        var doc = Parse(openApiContent, out _);
        if (!doc.Paths.TryGetValue(path, out var item) || !item.Operations.TryGetValue(method, out var op))
            throw new ArgumentException($"Operation '{method} {path}' not found.");
        var schema = BuildInputSchema(doc, path, method, op);
        var desc = op.Summary ?? op.Description ?? $"Invoke {method} {path}";
        return new OpenApiTool(doc, path, method, apiName, source, schema, desc)
        {
            ApiName = apiName ?? "api",
            RequiresApproval = true
        };
    }

    /// <summary>
    /// OpenAPI 문서 파싱
    /// </summary>
    private static OpenApiDocument Parse(string content, out OpenApiDiagnostic diag)
    {
        var reader = new OpenApiStringReader(new OpenApiReaderSettings
        {
            //RuleSet = ValidationRuleSet.GetDefaultRuleSet()
        });
        var doc = reader.Read(content, out diag);
        if (doc is null)
            throw new ArgumentException("Failed to parse OpenAPI document.");
        return doc;
    }

    private static (string path, OperationType method, OpenApiOperation op)? FindOperationById(OpenApiDocument doc, string opId)
    {
        foreach (var (path, item) in doc.Paths)
        {
            foreach (var (method, op) in item.Operations!)
            {
                if (string.Equals(op.OperationId, opId, StringComparison.Ordinal))
                    return (path, method, op);
            }
        }
        return null;
    }

    /// <summary>
    /// LLM 입력을 위한 JSON 스키마 구성
    /// </summary>
    private static JsonObject BuildInputSchema(OpenApiDocument doc, string path, OperationType method, OpenApiOperation op)
    {
        // properties: path/query/header/cookie/body + extras
        var props = new JsonObject();

        // path/query/header/cookie parameters
        var paramGroups = op.Parameters
            .Concat(doc.Paths[path].Parameters ?? Enumerable.Empty<OpenApiParameter>())
            .GroupBy(p => p.In);

        foreach (var group in paramGroups)
        {
            var name = group.Key switch
            {
                ParameterLocation.Path => "path",
                ParameterLocation.Query => "query",
                ParameterLocation.Header => "headers",
                ParameterLocation.Cookie => "cookies",
                _ => "params"
            };
            var obj = new JsonObject();
            foreach (var p in group)
            {
                obj[p.Name] = OpenApiToJsonNode.Map(p.Schema);
            }
            if (obj.Count > 0) props[name] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = obj
            };
        }

        // requestBody (json / form)
        if (op.RequestBody is { } body && body.Content?.Any() == true)
        {
            if (TryPickBestMediaType(body.Content, out var mediaType, out var media) && media.Schema is not null)
            {
                props["body"] = OpenApiToJsonNode.Map(media.Schema);
                props["bodyContentType"] = JsonValue.Create(mediaType);
            }
        }

        // 추가 실행 옵션 (서버, 인증 등)
        // LLM이 선택적으로 채울 수 있도록 optional 로 두되, 스키마 힌트 제공
        props["__serverUrl"] = new JsonObject
        {
            ["type"] = "string",
            ["description"] = "Override server URL. If omitted, use servers[0] from OpenAPI."
        };
        props["__auth"] = new JsonObject
        {
            ["type"] = "object",
            ["description"] = "Auth data (apiKey, bearer, basic). If omitted, Options에서 주입.",
            ["properties"] = new JsonObject
            {
                ["type"] = JsonValue.Create("string"), // apiKey|bearer|basic
                ["token"] = JsonValue.Create(string.Empty),
                ["username"] = JsonValue.Create(string.Empty),
                ["password"] = JsonValue.Create(string.Empty),
                ["headerName"] = JsonValue.Create(string.Empty),
                ["queryName"] = JsonValue.Create(string.Empty)
            }
        };

        var required = new JsonArray();
        // path 파라미터 필수 표시
        foreach (var p in op.Parameters.Where(p => p.In == ParameterLocation.Path && p.Required))
        {
            if (!required.Any()) required.Add("path");
        }

        var root = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = props
        };
        if (required.Count > 0) root["required"] = required;

        return root;
    }

    // using Microsoft.OpenApi.Models;
    // 반환: 성공 시 true, out 로 타입/미디어 반환
    private static bool TryPickBestMediaType(
        IDictionary<string, OpenApiMediaType>? content,
        out string mediaType,
        out OpenApiMediaType media)
    {
        mediaType = "";
        media = null!;
        if (content is null || content.Count == 0) return false;

        if (content.TryGetValue("application/json", out var json)) { mediaType = "application/json"; media = json; return true; }
        if (content.TryGetValue("multipart/form-data", out var mp)) { mediaType = "multipart/form-data"; media = mp; return true; }
        if (content.TryGetValue("application/x-www-form-urlencoded", out var form)) { mediaType = "application/x-www-form-urlencoded"; media = form; return true; }

        // 첫 항목
        var kv = content.First();
        mediaType = kv.Key;
        media = kv.Value;
        return true;
    }


    /// <inheritdoc />
    public async Task<ToolOutput> InvokeAsync(
        ToolInput input,
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(TimeoutSeconds));
        try
        {
            var baseUrl = ResolveServerUrl(input) ?? _doc.Servers.FirstOrDefault()?.Url
                          ?? throw new InvalidOperationException("No server URL found.");
            baseUrl = TrimTrailingSlash(baseUrl);

            var url = BuildUrl(baseUrl, _path, input);
            using var req = new HttpRequestMessage(ToHttpMethod(_method), url);

            // 헤더/쿠키/쿼리는 BuildUrl/ApplyHeaders 안에서 처리
            ApplyHeaders(req, input);
            ApplyAuth(req, input);

            // Body
            await ApplyBodyAsync(req, input, timeoutCts.Token).ConfigureAwait(false);

            // HttpClient 준비
            var http = GetHttpClient(input.Services);
            http.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);

            var resp = await http.SendAsync(req, HttpCompletionOption.ResponseContentRead, timeoutCts.Token)
                                 .ConfigureAwait(false);

            var payload = await resp.Content.ReadAsStringAsync(timeoutCts.Token).ConfigureAwait(false);

            // 성공/실패 판정: 2xx
            if ((int)resp.StatusCode >= 200 && (int)resp.StatusCode < 300)
            {
                // 그대로 반환 (너무 큰 경우 제한)
                return payload.Length > 30_000
                    ? ToolOutput.Failure("The result is too large to return. Please try again with different parameters.")
                    : ToolOutput.Success(payload);
            }

            // 오류: 본문 함께 반환
            var reason = $"{(int)resp.StatusCode} {resp.ReasonPhrase}";
            return ToolOutput.Failure(string.IsNullOrWhiteSpace(payload) ? reason : $"{reason}\n{payload}");
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested) throw;
            return ToolOutput.Failure($"The tool execution was cancelled due to timeout ({TimeoutSeconds} seconds).");
        }
        catch (Exception ex)
        {
            return ToolOutput.Failure(ex.Message);
        }
    }

    private static HttpClient GetHttpClient(IServiceProvider? sp)
        => sp?.GetService<IHttpClientFactory>()?.CreateClient("OpenApiTool")
           ?? sp?.GetService<HttpClient>()
           ?? new HttpClient();

    private string? ResolveServerUrl(ToolInput input)
    {
        // 우선순위: input.__serverUrl > this.OverrideServerUrl > doc.servers[0]
        if (input.TryGetValue("__serverUrl", out var v) && v is string s && !string.IsNullOrWhiteSpace(s)) return s;
        if (!string.IsNullOrWhiteSpace(OverrideServerUrl)) return OverrideServerUrl;
        return null;
    }

    private static string TrimTrailingSlash(string url)
        => url.EndsWith("/") ? url[..^1] : url;

    private static HttpMethod ToHttpMethod(OperationType t) => t switch
    {
        OperationType.Get => HttpMethod.Get,
        OperationType.Put => HttpMethod.Put,
        OperationType.Post => HttpMethod.Post,
        OperationType.Delete => HttpMethod.Delete,
        OperationType.Options => HttpMethod.Options,
        OperationType.Head => HttpMethod.Head,
        OperationType.Patch => HttpMethod.Patch,
        OperationType.Trace => HttpMethod.Trace,
        _ => HttpMethod.Get
    };

    private string BuildUrl(string baseUrl, string pathTemplate, ToolInput input)
    {
        // path params
        var path = pathTemplate;
        if (input.TryGetValue("path", out var pObj) && pObj is IReadOnlyDictionary<string, object?> pathDict)
        {
            foreach (var kv in pathDict)
            {
                path = path.Replace($"{{{kv.Key}}}", Uri.EscapeDataString(kv.Value?.ToString() ?? string.Empty));
            }
        }

        var ub = new UriBuilder($"{baseUrl}{(path.StartsWith("/") ? "" : "/")}{path}");

        // query params
        if (input.TryGetValue("query", out var qObj) && qObj is IReadOnlyDictionary<string, object?> qDict)
        {
            var query = System.Web.HttpUtility.ParseQueryString(ub.Query);
            foreach (var (k, v) in qDict)
            {
                if (v is null) continue;
                if (v is IEnumerable<object?> arr && v is not string)
                {
                    foreach (var e in arr)
                        query.Add(k, e?.ToString());
                }
                else
                {
                    query[k] = v.ToString();
                }
            }
            ub.Query = query.ToString();
        }

        return ub.Uri.ToString();
    }

    private void ApplyHeaders(HttpRequestMessage req, ToolInput input)
    {
        if (input.TryGetValue("headers", out var hObj) && hObj is IReadOnlyDictionary<string, object?> headers)
        {
            foreach (var (k, v) in headers)
            {
                if (string.IsNullOrWhiteSpace(k) || v is null) continue;
                if (!req.Headers.TryAddWithoutValidation(k, v.ToString()))
                {
                    // content header 로 재시도
                    req.Content ??= new StringContent(string.Empty);
                    req.Content.Headers.TryAddWithoutValidation(k, v.ToString());
                }
            }
        }

        // cookies
        if (input.TryGetValue("cookies", out var cObj) && cObj is IReadOnlyDictionary<string, object?> cookies && cookies.Count > 0)
        {
            var cookiePairs = string.Join("; ", cookies.Where(kv => kv.Value is not null)
                                                       .Select(kv => $"{kv.Key}={kv.Value}"));
            if (!string.IsNullOrEmpty(cookiePairs))
            {
                req.Headers.TryAddWithoutValidation("Cookie", cookiePairs);
            }
        }
    }

    private void ApplyAuth(HttpRequestMessage req, ToolInput input)
    {
        // 1) input.__auth
        if (TryGetAuthFromInput(input, out var auth))
        {
            ApplyAuthCore(req, auth);
            return;
        }

        // 2) 옵션 객체에서 가져오기 (ToolInput.Options)
        if (input.Options is not null)
        {
            // 키 이름은 OptionKeys 로 유연하게
            var opt = input.Options.ConvertTo<Dictionary<string, object?>>() ?? new();
            var a = new OpenApiAuth();

            if (opt.TryGetValue(OptionKeys.AuthType, out var t) && t is string ts) a.Type = ts;
            if (opt.TryGetValue(OptionKeys.AuthToken, out var token) && token is string tok) a.Token = tok;
            if (opt.TryGetValue(OptionKeys.Username, out var u) && u is string us) a.Username = us;
            if (opt.TryGetValue(OptionKeys.Password, out var pw) && pw is string ps) a.Password = ps;
            if (opt.TryGetValue(OptionKeys.HeaderName, out var hn) && hn is string hs) a.HeaderName = hs;
            if (opt.TryGetValue(OptionKeys.QueryName, out var qn) && qn is string qs) a.QueryName = qs;

            ApplyAuthCore(req, a);
        }
    }

    private static bool TryGetAuthFromInput(ToolInput input, out OpenApiAuth auth)
    {
        auth = new OpenApiAuth();
        if (!input.TryGetValue("__auth", out var aObj) || aObj is null) return false;

        var dict = aObj.ConvertTo<Dictionary<string, object?>>();
        if (dict is null) return false;

        dict.TryGetValue("type", out var t);
        dict.TryGetValue("token", out var token);
        dict.TryGetValue("username", out var u);
        dict.TryGetValue("password", out var p);
        dict.TryGetValue("headerName", out var h);
        dict.TryGetValue("queryName", out var q);

        auth.Type = t?.ToString();
        auth.Token = token?.ToString();
        auth.Username = u?.ToString();
        auth.Password = p?.ToString();
        auth.HeaderName = h?.ToString();
        auth.QueryName = q?.ToString();
        return true;
    }

    private static void ApplyAuthCore(HttpRequestMessage req, OpenApiAuth auth)
    {
        switch (auth.Type?.ToLowerInvariant())
        {
            case "bearer":
                if (!string.IsNullOrWhiteSpace(auth.Token))
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
                break;

            case "basic":
                if (!string.IsNullOrWhiteSpace(auth.Username))
                {
                    var raw = $"{auth.Username}:{auth.Password ?? ""}";
                    var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
                    req.Headers.Authorization = new AuthenticationHeaderValue("Basic", b64);
                }
                break;

            case "apikey":
            case "api-key":
                // 기본은 X-API-Key 헤더. headerName/queryName 로 변경 가능
                var key = auth.Token ?? string.Empty;
                var headerName = string.IsNullOrWhiteSpace(auth.HeaderName) ? "X-API-Key" : auth.HeaderName;
                if (!string.IsNullOrWhiteSpace(headerName))
                {
                    req.Headers.TryAddWithoutValidation(headerName, key);
                }
                else if (!string.IsNullOrWhiteSpace(auth.QueryName))
                {
                    // URL에 쿼리스트링으로 부착
                    var ub = new UriBuilder(req.RequestUri!);
                    var query = System.Web.HttpUtility.ParseQueryString(ub.Query);
                    query.Add(auth.QueryName, key);
                    ub.Query = query.ToString();
                    req.RequestUri = ub.Uri;
                }
                break;

            default:
                // no-op
                break;
        }
    }

    private async Task ApplyBodyAsync(HttpRequestMessage req, ToolInput input, CancellationToken ct)
    {
        if (!_operation.RequestBody?.Content?.Any() ?? true) return;

        // 스키마 구성 시 bodyContentType 를 넣어두었음
        string contentType = "application/json";
        if (input.TryGetValue("bodyContentType", out var ctObj) && ctObj is string s && !string.IsNullOrWhiteSpace(s))
            contentType = s;

        if (!input.TryGetValue("body", out var bodyObj))
        {
            // body 필요하지만 없는 경우: 일부 API는 optional
            return;
        }

        switch (contentType)
        {
            case "application/json":
                {
                    var json = bodyObj is string str && IsJsonLike(str)
                        ? str
                        : JsonSerializer.Serialize(bodyObj, _jsonOptions);

                    req.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    break;
                }

            case "application/x-www-form-urlencoded":
                {
                    var dict = bodyObj.ConvertTo<Dictionary<string, object?>>() ?? new();
                    var kvs = new List<KeyValuePair<string, string>>();
                    foreach (var (k, v) in dict)
                    {
                        if (v is null) continue;
                        if (v is IEnumerable<object?> arr && v is not string)
                            kvs.AddRange(arr.Select(e => new KeyValuePair<string, string>(k, e?.ToString() ?? "")));
                        else
                            kvs.Add(new KeyValuePair<string, string>(k, v.ToString() ?? ""));
                    }
                    req.Content = new FormUrlEncodedContent(kvs);
                    break;
                }

            case "multipart/form-data":
                {
                    var dict = bodyObj.ConvertTo<Dictionary<string, object?>>() ?? new();
                    var mp = new MultipartFormDataContent();
                    foreach (var (k, v) in dict)
                    {
                        if (v is null) continue;

                        // 파일(바이트) 지원: { bytes, fileName, contentType } 관례
                        if (v is IReadOnlyDictionary<string, object?> f &&
                            f.TryGetValue("bytes", out var bytesObj) &&
                            bytesObj is byte[] bytes)
                        {
                            var fileName = f.TryGetValue("fileName", out var fn) ? fn?.ToString() ?? "file" : "file";
                            var ctype = f.TryGetValue("contentType", out var ct2) ? ct2?.ToString() ?? "application/octet-stream" : "application/octet-stream";
                            var content = new ByteArrayContent(bytes);
                            content.Headers.ContentType = new MediaTypeHeaderValue(ctype);
                            mp.Add(content, k, fileName);
                        }
                        else
                        {
                            mp.Add(new StringContent(v.ToString() ?? ""), k);
                        }
                    }
                    req.Content = mp;
                    break;
                }

            default:
                // 기타 미디어타입은 문자열로 시도
                var raw = bodyObj is string sraw ? sraw : JsonSerializer.Serialize(bodyObj, _jsonOptions);
                req.Content = new StringContent(raw, Encoding.UTF8, contentType);
                break;
        }

        await Task.CompletedTask;
    }

    private static bool IsJsonLike(string s)
    {
        s = s.Trim();
        return s.StartsWith("{") && s.EndsWith("}") || s.StartsWith("[") && s.EndsWith("]");
    }
}

/// <summary>
/// 인증 및 부가 옵션 키 이름(호출자 옵션 객체의 키 커스터마이징)
/// </summary>
public sealed class OpenApiToolOptionKeys
{
    public string AuthType { get; init; } = "auth.type";
    public string AuthToken { get; init; } = "auth.token";
    public string Username { get; init; } = "auth.username";
    public string Password { get; init; } = "auth.password";
    public string HeaderName { get; init; } = "auth.headerName";
    public string QueryName { get; init; } = "auth.queryName";
}

/// <summary>
/// 간단 인증 구조체
/// </summary>
public sealed class OpenApiAuth
{
    public string? Type { get; set; }         // bearer | basic | apikey
    public string? Token { get; set; }        // bearer/apikey token
    public string? Username { get; set; }     // basic
    public string? Password { get; set; }     // basic
    public string? HeaderName { get; set; }   // apikey in header
    public string? QueryName { get; set; }    // apikey in query
}

internal static class OpenApiToJsonNode
{
    public static JsonNode Map(OpenApiSchema? s)
    {
        if (s is null) return new JsonObject { ["type"] = "null" };

        // $ref 간단 처리 (깊은 deref는 생략)
        if (s.Reference?.Id is string refId && !string.IsNullOrWhiteSpace(refId))
            return new JsonObject { ["$ref"] = $"#/components/schemas/{refId}" };

        var o = new JsonObject();

        // type/nullable
        var type = s.Type;
        if (s.Nullable && !string.IsNullOrWhiteSpace(type))
            o["type"] = new JsonArray(type, "null");
        else
            o["type"] = string.IsNullOrWhiteSpace(type) ? "object" : type;

        // 형식/설명/제약
        if (!string.IsNullOrWhiteSpace(s.Format)) o["format"] = s.Format;
        if (!string.IsNullOrWhiteSpace(s.Description)) o["description"] = s.Description;

        // enum
        if (s.Enum is { Count: > 0 })
        {
            var arr = new JsonArray();
            foreach (var e in s.Enum) arr.Add(OpenApiAnyToJson(e));
            o["enum"] = arr;
        }

        // object
        if (o["type"]?.ToString() == "object" || s.Properties?.Count > 0)
        {
            var props = new JsonObject();
            foreach (var kv in s.Properties)
                props[kv.Key] = Map(kv.Value);
            if (props.Count > 0) o["properties"] = props;

            if (s.Required is { Count: > 0 })
            {
                var r = new JsonArray();
                foreach (var name in s.Required) r.Add(name);
                o["required"] = r;
            }

            o["additionalProperties"] = s.AdditionalPropertiesAllowed;
            if (s.AdditionalProperties is not null)
                o["additionalProperties"] = Map(s.AdditionalProperties);
        }

        // array
        if (o["type"]?.ToString() == "array" && s.Items is not null)
            o["items"] = Map(s.Items);

        // number/string 길이/범위 제약
        if (s.Maximum.HasValue) o["maximum"] = s.Maximum.Value;
        if (s.Minimum.HasValue) o["minimum"] = s.Minimum.Value;
        if (s.ExclusiveMaximum == true) o["exclusiveMaximum"] = true;
        if (s.ExclusiveMinimum == true) o["exclusiveMinimum"] = true;
        if (s.MaxLength.HasValue) o["maxLength"] = s.MaxLength.Value;
        if (s.MinLength.HasValue) o["minLength"] = s.MinLength.Value;
        if (s.MaxItems.HasValue) o["maxItems"] = s.MaxItems.Value;
        if (s.MinItems.HasValue) o["minItems"] = s.MinItems.Value;

        return o;
    }

    private static JsonNode OpenApiAnyToJson(IOpenApiAny any) =>
        any switch
        {
            OpenApiString v => JsonValue.Create(v.Value)!,
            OpenApiInteger v => JsonValue.Create(v.Value)!,
            OpenApiLong v => JsonValue.Create(v.Value)!,
            OpenApiFloat v => JsonValue.Create(v.Value)!,
            OpenApiDouble v => JsonValue.Create(v.Value)!,
            OpenApiBoolean v => JsonValue.Create(v.Value)!,
            OpenApiDate v => JsonValue.Create(v.Value.ToString("yyyy-MM-dd"))!,
            OpenApiDateTime v => JsonValue.Create(v.Value.ToString("o"))!,
            OpenApiArray arr => new JsonArray(arr.Select(OpenApiAnyToJson).ToArray()),
            OpenApiObject obj => new JsonObject(obj.ToDictionary(k => k.Key, v => OpenApiAnyToJson(v.Value))),
            _ => JsonValue.Create(any?.ToString())!
        };
}
