using IronHive.Abstractions.Json;
using IronHive.Abstractions.Tools;
using Json.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using System.Collections;
using System.Text;
using System.Text.Json;

namespace ConsoleTest;

public class OpenApiSecurity
{
    public required IOpenApiSecurityScheme Scheme { get; set; }

    public IOpenApiAuthSecret? Secret { get; set; }

    public IList<string>? Scopes { get; set; }
}

public class OpenApiProperties
{
    public IDictionary<string, IOpenApiParameter>? Query { get; set; }

    public IDictionary<string, IOpenApiParameter>? Path { get; set; }

    public IDictionary<string, IOpenApiParameter>? Header { get; set; }

    public IDictionary<string, IOpenApiParameter>? Cookie { get; set; }

    public IOpenApiRequestBody? Body { get; set; }

    public JsonSchema? ToJsonSchema()
    {
        var builder = new JsonSchemaBuilder().Type(SchemaValueType.Object);
        var properties = new Dictionary<string, JsonSchema>();
        var required = new List<string>();

        if (Query is { Count: > 0 })
        {
            properties["query"] = BuildParametersSchema(Query);
            if (Query.Values.Any(p => p.Required))
                required.Add("query");
        }
        if (Path is { Count: > 0 })
        {
            properties["path"] = BuildParametersSchema(Path);
            if (Path.Values.Any(p => p.Required))
                required.Add("path");
        }
        if (Header is { Count: > 0 })
        {
            properties["header"] = BuildParametersSchema(Header);
            if (Header.Values.Any(p => p.Required))
                required.Add("header");
        }
        if (Cookie is { Count: > 0 })
        {
            properties["cookie"] = BuildParametersSchema(Cookie);
            if (Cookie.Values.Any(p => p.Required))
                required.Add("cookie");
        }

        if (Body is not null && Body.Content is { Count: > 0 })
        {
            properties["body"] = BuildRequestBodySchema(Body);
            if (Body.Required)
                required.Add("body");
        }

        if (properties.Count > 0)
            builder = builder.Properties(properties);
        if (required.Count > 0)
            builder = builder.Required(required.ToArray());
        return builder.Build();
    }

    private static JsonSchema BuildParametersSchema(IDictionary<string, IOpenApiParameter> parameters)
    {
        return new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Properties(parameters.ToDictionary(kv => kv.Key, kv => kv.Value.Schema?.ToJsonSchema() ?? JsonSchema.Empty))
            .Required(parameters.Where(kv => kv.Value.Required).Select(kv => kv.Key).ToArray())
            .Build();
    }

    private static JsonSchema BuildRequestBodySchema(IOpenApiRequestBody body)
    {
        if (body.Content is null || body.Content.Count == 0)
            return JsonSchema.Empty;

        // 스키마 하나만 지원
        // 우선순위: application/json > text/plain > 기타
        var contentType = body.Content.ContainsKey("application/json") ? "application/json"
                        : body.Content.ContainsKey("text/plain") ? "text/plain"
                        : body.Content.Keys.First();
        var media = body.Content[contentType];

        if (media.Schema is not null)
            return media.Schema.ToJsonSchema();
        else
            return new JsonSchemaBuilder().Type(SchemaValueType.String).Build();
    }
}

internal static class OpenApiUtil
{
    internal static bool IsUrlString(string str)
    {
        return Uri.TryCreate(str, UriKind.Absolute, out var u) &&
            (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
    }

    internal static bool IsRawString(string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return false;
        var t = str.TrimStart();
        return t.StartsWith("openapi:") || t.StartsWith("swagger:") ||  // YAML
               t.StartsWith('{') || t.StartsWith('[');                  // JSON
    }

    internal static string BuildOperationId(HttpMethod method, string path)
    {
        var id = $"{method}_{path}";
        id = new string(id.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
        return id.ToLowerInvariant();
    }


    internal static string CombineUrl(string baseUrl, string relativePath)
    {
        if (string.IsNullOrEmpty(baseUrl)) return relativePath;
        if (string.IsNullOrEmpty(relativePath)) return baseUrl;
        if (baseUrl.EndsWith('/')) baseUrl = baseUrl[..^1];
        return relativePath.StartsWith('/') ? baseUrl + relativePath : baseUrl + "/" + relativePath;
    }

    internal static bool IsSequenceButNotString(object? value) =>
        value is IEnumerable && value is not string && value is not byte[] && value is not JsonElement;

    internal static void AppendQueryParam(List<string> query, string name, object value, string style = "form", bool explode = true)
    {
        // 최소 구현: form + explode / non-explode, spaceDelimited, pipeDelimited, deepObject
        if (!IsSequenceButNotString(value))
        {
            query.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value.ToString()!)}");
            return;
        }

        var seq = ((IEnumerable)value).Cast<object?>().ToArray();
        if (style == "spaceDelimited")
        {
            query.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(string.Join(" ", seq))}");
        }
        else if (style == "pipeDelimited")
        {
            query.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(string.Join("|", seq))}");
        }
        else // form
        {
            if (explode)
            {
                foreach (var item in seq)
                    query.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(item?.ToString() ?? "")}");
            }
            else
            {
                query.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(string.Join(",", seq))}");
            }
        }
    }
}

public sealed class OpenApiClient
{
    private IEnumerable<OpenApiTool>? _tools;

    private OpenApiClient(OpenApiDocument doc)
    {
        Document = doc;
    }

    public OpenApiDocument Document { get; }

    public required OpenApiClientConfig Config { get; init; }

    public string ClientName => Config.ClientName;

    public string? Title => Document.Info.Title;

    public string? Description => Document.Info.Description ?? Document.Info.Summary;

    public IEnumerable<OpenApiTag>? Tags => Document.Tags;

    public static async Task<OpenApiClient> CreateAsync(OpenApiClientConfig config, CancellationToken ct = default)
    {
        OpenApiDocument? doc;
        OpenApiDiagnostic? dialog;

        if (OpenApiUtil.IsUrlString(config.Document))
        {
            (doc, dialog) = await OpenApiDocument.LoadAsync(url: config.Document, token: ct);
        }
        else if (OpenApiUtil.IsRawString(config.Document))
        {
            var settings = new OpenApiReaderSettings();
            settings.AddYamlReader();
            (doc, dialog) = OpenApiDocument.Parse(config.Document, settings: settings);
        }
        else
        {
            throw new Exception("unknown openapi document format");
        }

        if (dialog is not null && dialog.Errors.Count > 0)
            throw new Exception("Failed to parse OpenAPI document: " + string.Join("; ", dialog.Errors.Select(e => e.Message)));
        if (doc is null)
            throw new Exception("Failed to load OpenAPI document.");

        return new OpenApiClient(doc)
        {
            Config = config
        };
    }

    public async Task<IEnumerable<OpenApiTool>> ListToolsAsync(CancellationToken cancellationToken = default)
    {
        if (_tools is not null) return _tools;

        var tools = new List<OpenApiTool>();
        foreach (var (path, pathItem) in Document.Paths)
        {
            if (pathItem.Operations is null || pathItem.Operations.Count == 0) continue;

            foreach (var (method, op) in pathItem.Operations)
            {
                // ID 추출
                var oid = string.IsNullOrEmpty(op.OperationId)
                    ? OpenApiUtil.BuildOperationId(method, path)
                    : op.OperationId;

                // 설명 추출
                var desc = op.Description ?? op.Summary;

                // 서버 목록 추출
                var servers = op.Servers?.Count > 0 ? op.Servers.ToList()
                    : pathItem.Servers?.Count > 0 ? pathItem.Servers.ToList()
                    : Document.Servers?.ToList()
                    ?? throw new Exception("not have server");
                
                var baseUris = new List<Uri>();
                var baseUriStrs = new HashSet<string>();
                foreach (var server in servers)
                {
                    if (string.IsNullOrWhiteSpace(server.Url)) continue;

                    var url = server.Url;
                    if (server.Variables is { Count: > 0 })
                    {
                        foreach (var (key, value) in server.Variables)
                        {
                            if (!string.IsNullOrWhiteSpace(value.Default))
                                baseUriStrs.Add(url.Replace($"{{{key}}}", value.Default));

                            if (value.Enum is { Count: > 0 })
                            {
                                foreach (var v in value.Enum)
                                {
                                    if (!string.IsNullOrWhiteSpace(v))
                                        baseUriStrs.Add(url.Replace($"{{{key}}}", v));
                                }
                            }
                        }
                    }
                    else
                    {
                        baseUriStrs.Add(url);
                    }
                }
                foreach (var u in baseUriStrs)
                {
                    if (Uri.TryCreate(u, UriKind.Absolute, out var uri))
                        baseUris.Add(uri);
                }
                if (baseUris.Count == 0)
                    throw new Exception("not have valid server url");

                // 보안 인증 추출
                var requires = op.Security?.Count > 0 ? op.Security
                    : Document.Security?.Count > 0 ? Document.Security
                    : new List<OpenApiSecurityRequirement>();

                var securities = new List<OpenApiSecurity>();
                foreach (var req in requires)
                {
                    foreach (var (sref, scope) in req)
                    {
                        var name = sref.Reference?.Id;
                        var scheme = sref.Target;
                        if (scheme == null || string.IsNullOrWhiteSpace(name))
                            continue;

                        if (Config.Secrets != null && Config.Secrets.TryGetValue(name, out var secret))
                        {
                            securities.Add(new OpenApiSecurity
                            {
                                Scheme = scheme,
                                Scopes = scope,
                                Secret = secret
                            });
                        }
                    }
                }

                // 프로퍼티 추출
                var props = new OpenApiProperties();
                props.Body = op.RequestBody;

                var parameters = new List<IOpenApiParameter>();
                if (pathItem.Parameters != null)
                    parameters.AddRange(pathItem.Parameters);
                if (op.Parameters != null)
                    parameters.AddRange(op.Parameters);

                foreach (var param in parameters)
                {
                    switch (param.In)
                    {
                        case ParameterLocation.Query:
                            props.Query ??= new Dictionary<string, IOpenApiParameter>(StringComparer.Ordinal);
                            if (!string.IsNullOrWhiteSpace(param.Name) && !props.Query.ContainsKey(param.Name))
                                props.Query[param.Name] = param;
                            break;
                        case ParameterLocation.Path:
                            props.Path ??= new Dictionary<string, IOpenApiParameter>(StringComparer.Ordinal);
                            if (!string.IsNullOrWhiteSpace(param.Name) && !props.Path.ContainsKey(param.Name))
                                props.Path[param.Name] = param;
                            break;
                        case ParameterLocation.Header:
                            props.Header ??= new Dictionary<string, IOpenApiParameter>(StringComparer.OrdinalIgnoreCase);
                            if (!string.IsNullOrWhiteSpace(param.Name) && !props.Header.ContainsKey(param.Name))
                                props.Header[param.Name] = param;
                            break;
                        case ParameterLocation.Cookie:
                            props.Cookie ??= new Dictionary<string, IOpenApiParameter>(StringComparer.Ordinal);
                            if (!string.IsNullOrWhiteSpace(param.Name) && !props.Cookie.ContainsKey(param.Name))
                                props.Cookie[param.Name] = param;
                            break;
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
                tools.Add(new OpenApiTool
                {
                    ClientName = ClientName,
                    Description = desc,
                    OperationId = oid,
                    Method = method,
                    Path = path,
                    BaseUris = baseUris,
                    Securities = securities,
                    Properties = props,
                    Parameters = props.ToJsonSchema(),
                    DefaultHeaders = Config.DefaultHeaders,
                    TimeoutSeconds = TimeSpan.FromSeconds(Config.TimeoutSeconds),
                });
            }
        }

        await Task.CompletedTask;
        _tools = tools;
        return tools.ToArray();
    }
}

[JsonPolymorphicValue("openapi")]
public sealed class OpenApiTool : ITool
{
    public required string ClientName { get; init; }

    public required string OperationId { get; init; }

    public required HttpMethod Method { get; init; }

    public required string Path { get; init; }

    public required IEnumerable<Uri> BaseUris { get; init; }

    public required IList<OpenApiSecurity> Securities { get; init; }

    public required OpenApiProperties Properties { get; init; }

    public string UniqueName => $"openapi_{ClientName}_{OperationId}";

    public string? Description { get; init; }

    public object? Parameters { get; init; }

    public bool RequiresApproval { get; set; } = true;

    public IDictionary<string, string>? DefaultHeaders { get; set; }

    public TimeSpan TimeoutSeconds { get; init; }

    public async Task<ToolOutput> InvokeAsync(ToolInput input, CancellationToken cancellationToken = default)
    {
        using var client = CreateHttpClient(input.Services);
        try
        {
            var queue = new Queue<Uri>(BaseUris);
            var errors = new List<Exception>();
            while (queue.TryDequeue(out var baseUri))
            {
                try
                {
                    var req = BuildHttpRequest(baseUri, input);
                    using var res = await client.SendAsync(req!, cancellationToken);
                    var text = await res.Content.ReadAsStringAsync(cancellationToken);
                    if (!res.IsSuccessStatusCode)
                        throw new Exception($"HTTP {(int)res.StatusCode} {res.ReasonPhrase} {text}");

                    var payload = text.Length > 120_000 ? text[..120_000] : text; // JSON도 텍스트 그대로
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

            if (errors.Count > 0)
                return ToolOutput.Failure("Request failed on all servers: " + string.Join(" | ", errors.Select(e => e.Message)));
            else
                return ToolOutput.Failure("Request failed on all servers.");
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

    private HttpClient CreateHttpClient(IServiceProvider? services)
    {
        HttpClient? client = null;
        if (services is not null)
        {
            var factory = services.GetService<IHttpClientFactory>();
            if (factory is not null)
                client = factory.CreateClient($"OpenApiTool:{ClientName}");
            else
                client = services.GetService<HttpClient>();
        }
        client ??= new HttpClient();

        if (DefaultHeaders is not null)
        {
            foreach (var (k, v) in DefaultHeaders)
                client.DefaultRequestHeaders.TryAddWithoutValidation(k, v);
        }
        client.Timeout = TimeoutSeconds;
        return client;
    }

    private HttpRequestMessage BuildHttpRequest(Uri baseUrl, ToolInput input)
    {
        // 경로 치환
        string path = Path;
        if (Properties.Path is not null)
        {
            if (input.TryGetValue<Dictionary<string, string>>("path", out var items))
            {
                foreach (var (name, _) in Properties.Path)
                {
                    if (items.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value))
                        path = path.Replace($"{{{name}}}", Uri.EscapeDataString(value));
                }
            }
        }

        // 쿼리 문자열 구성
        var query = new List<string>();
        if (Properties.Query is not null)
        {
            if (input.TryGetValue<Dictionary<string, object>>("query", out var items))
            {
                foreach (var (name, _) in Properties.Query)
                {
                    if (items.TryGetValue(name, out var value) && value is not null)
                    {
                        if (value is IEnumerable seq)
                        {
                            foreach (var item in seq)
                            {
                                query.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(item.ToString()!)}");
                            }
                        }
                        else
                        {
                            query.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value.ToString()!)}");
                        }
                    }
                }
            }
        }

        var headers = new Dictionary<string, string>();
        // 헤더 구성
        if (Properties.Header is not null)
        {
            if (input.TryGetValue<Dictionary<string, object?>>("header", out var items))
            {
                foreach (var (name, _) in Properties.Header)
                {
                    if (items.TryGetValue(name, out var value) && value is not null)
                    {
                        headers.TryAdd(name, value.ToString()!);
                    }
                }
            }
        }

        // 쿠키 구성
        if (Properties.Cookie is not null)
        {
            if (input.TryGetValue<Dictionary<string, object?>>("cookie", out var items))
            {
                var cookies = new List<string>();
                foreach (var (name, _) in Properties.Cookie)
                {
                    if (items.TryGetValue(name, out var value) && value is not null)
                    {
                        cookies.Add($"{name}={value}");
                    }
                }

                if (cookies.Count > 0)
                    headers.TryAdd("Cookie", string.Join("; ", cookies));
            }
        }

        // 바디 구성
        HttpContent? content = null;
        if (Properties.Body?.Content is not null)
        {
            var contentType = Properties.Body.Content.ContainsKey("application/json") ? "application/json"
                            : Properties.Body.Content.ContainsKey("text/plain") ? "text/plain"
                            : Properties.Body.Content.Keys.First();

            if (input.TryGetValue<object>("body", out var item))
            {
                if (contentType == "application/json")
                {
                    var json = JsonSerializer.Serialize(item);
                    content = new StringContent(json, Encoding.UTF8, contentType);
                }
                else
                {
                    content = new StringContent(item?.ToString() ?? string.Empty, Encoding.UTF8, contentType);
                }
            }
        }

        foreach (var sec in Securities)
        {
            if (sec.Scheme.Type == SecuritySchemeType.ApiKey && sec.Secret is ApiKeyAuthSecret aas)
            {
                if (sec.Scheme.In == ParameterLocation.Path)
                {
                    path = path.Replace($"{{{sec.Scheme.Name}}}", Uri.EscapeDataString(aas.Value));
                }
                else if (sec.Scheme.In == ParameterLocation.Query)
                {
                    query.Add($"{Uri.EscapeDataString(sec.Scheme.Name!)}={Uri.EscapeDataString(aas.Value)}");
                }
                else if (sec.Scheme.In == ParameterLocation.Header)
                {
                    headers.TryAdd(sec.Scheme.Name!, aas.Value);
                }
                else if (sec.Scheme.In == ParameterLocation.Cookie)
                {
                    if (headers.TryGetValue("Cookie", out var exist))
                        headers["Cookie"] = exist + "; " + $"{sec.Scheme.Name}={aas.Value}";
                    else
                        headers["Cookie"] = $"{sec.Scheme.Name}={aas.Value}";
                }
            }
            else if ((sec.Scheme.Type == SecuritySchemeType.Http))
            {
                if (sec.Secret is HttpBasicAuthSecret bas)
                {
                    var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{bas.Username}:{bas.Password}"));
                    headers["Authorization"] = $"Basic {auth}";
                }
                else if (sec.Secret is HttpBearerAuthSecret bss)
                {
                    headers["Authorization"] = $"Bearer {bss.Token}";
                }
            }
            else if (sec.Scheme.Type == SecuritySchemeType.OAuth2 || 
                sec.Scheme.Type == SecuritySchemeType.OpenIdConnect)
            {
                if (sec.Secret is OAuth2AuthSecret oas)
                {
                    headers["Authorization"] = $"Bearer {oas.AccessToken}";
                }
                else if (sec.Secret is OpenIdConnectAuthSecret ois)
                {
                    headers["Authorization"] = $"Bearer {ois.AccessToken}";
                }
            }
        }

        var uriBuilder = new UriBuilder(OpenApiUtil.CombineUrl(baseUrl.ToString(), path));
        if (query.Count > 0)
            uriBuilder.Query = string.Join("&", query);
        var req = new HttpRequestMessage(Method, uriBuilder.Uri);
        
        foreach (var (k, v) in headers)
            req.Headers.TryAddWithoutValidation(k, v);

        if (content is not null)
            req.Content = content;

        return req;
    }
}