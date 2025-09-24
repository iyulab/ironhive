using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace IronHive.Plugins.OpenAPI;

/// <summary>
/// OpenAPI 문서에 정의된 API 요청들을 툴로 변환하여 실행할 수 있는 클라이언트 클래스입니다.
/// </summary>
public sealed class OpenApiClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly OpenApiDocument _doc;
    private readonly OpenApiClientOptions? _options;

    private IEnumerable<OpenApiTool>? _tools;

    public OpenApiClient(OpenApiDocument doc, OpenApiClientOptions? options = null)
    {
        _doc = doc;
        _options = options;
        _http = CreateHttpClient(_options);
    }

    /// <summary>
    /// 클라이언트의 식별자 이름입니다.
    /// </summary>
    public required string ClientName { get; init; }

    /// <summary> OpenApi 문서의 제목입니다. </summary>
    public string? Title => _doc.Info.Title;

    /// <summary> OpenApi 문서의 설명입니다. </summary>
    public string? Description => _doc.Info.Description ?? _doc.Info.Summary;

    /// <summary> OpenApi 문서의 태그입니다. </summary>
    public IEnumerable<OpenApiTag>? Tags => _doc.Tags;

    /// <inheritdoc />
    public void Dispose()
    {
        _http.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// OpenApi Spec의 요청들에 대한 툴을 생성합니다.
    /// </summary>
    public async Task<IEnumerable<OpenApiTool>> ListToolsAsync(
        CancellationToken cancellationToken = default)
    {
        if (_tools is not null) return _tools;

        var tools = new List<OpenApiTool>();
        foreach (var (path, pathItem) in _doc.Paths)
        {
            if (pathItem.Operations is null || pathItem.Operations.Count == 0) continue;

            cancellationToken.ThrowIfCancellationRequested();
            foreach (var (method, op) in pathItem.Operations)
            {
                // ID 추출
                var oid = string.IsNullOrEmpty(op.OperationId)
                    ? CreateOperationId(method, path)
                    : op.OperationId;

                // 설명 추출
                var desc = op.Description ?? op.Summary;

                // 서버 목록 추출
                var servers = op.Servers?.Count > 0 ? op.Servers.ToList()
                    : pathItem.Servers?.Count > 0 ? pathItem.Servers.ToList()
                    : _doc.Servers?.ToList()
                    ?? throw new Exception("not have server");
                var baseUris = servers.SelectMany(s => s.ExtractServerUrls());

                // 보안 인증 추출
                var requires = op.Security?.Count > 0 ? op.Security
                    : _doc.Security?.Count > 0 ? _doc.Security
                    : null;

                // 프로퍼티 추출
                var props = new OpenApiProperties
                {
                    Body = op.RequestBody
                };

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

                tools.Add(new OpenApiTool(_http, _options?.Credentials)
                {
                    ClientName = ClientName,
                    OperationId = oid,
                    BaseUris = baseUris,
                    Method = method,
                    Path = path,
                    Properties = props,
                    Requirements = requires,
                    Description = desc,
                    Parameters = props.ToJsonSchema(),
                });
            }
        }
        _tools = tools;

        cancellationToken.ThrowIfCancellationRequested();
        await Task.CompletedTask;
        return _tools;
    }

    /// <summary> HttpClient를 설정에 따라 생성합니다. </summary>
    private static HttpClient CreateHttpClient(OpenApiClientOptions? options, IServiceProvider? services = null)
    {
        HttpClient? client = null;
        if (services is not null)
        {
            var factory = services.GetService<IHttpClientFactory>();
            if (factory is not null)
                client = factory.CreateClient($"IronHive_OpenApiTool");
            else
                client = services.GetService<HttpClient>();
        }
        client ??= new HttpClient();

        if (options is not null)
        {
            foreach (var (k, v) in options.DefaultHeaders)
                client.DefaultRequestHeaders.TryAddWithoutValidation(k, v);

            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        }

        return client;
    }

    /// <summary> 주어진 조건에 따라 OperationId를 임의로 생성합니다. </summary>
    private static string CreateOperationId(HttpMethod method, string path)
    {
        var id = $"{method}_{path}";
        id = new string(id.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
        return id.ToLowerInvariant();
    }
}