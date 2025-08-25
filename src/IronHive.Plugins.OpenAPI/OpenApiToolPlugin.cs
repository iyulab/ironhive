using IronHive.Abstractions.Json;
using IronHive.Abstractions.Tools;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System.Text;
using System.Text.Json;

namespace IronHive.Plugins.OpenAPI;

[Obsolete("이 도구는 완성되지 않았으며, 향후 버전에서 개선될 예정입니다.")]
public class OpenApiTool : ITool
{
    private readonly OpenApiDocument _document;
    private readonly Dictionary<string, (OpenApiOperation operation, string path, OperationType method)> _operations;

    public OpenApiTool(Stream openApiStream)
    {
        var reader = new OpenApiStreamReader();
        _document = reader.Read(openApiStream, out var diagnostic);

        _operations = new Dictionary<string, (OpenApiOperation, string, OperationType)>();
        foreach (var path in _document.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                var operationId = operation.Value.OperationId ?? $"{operation.Key}_{path.Key}";
                _operations[operationId] = (operation.Value, path.Key, operation.Key);
            }
        }
    }

    public string Name { get; init; }

    public string Description { get; init; }

    public object? Parameters { get; init; }

    public bool RequiresApproval { get; set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async Task<ToolOutput> InvokeAsync(
        ToolInput input,
        CancellationToken cancellationToken = default)
    {
        if (!_operations.TryGetValue(string.Empty, out var operationInfo))
        {
            return ToolOutput.Failure("OpenAPI 문서에서 유효한 operationId를 찾을 수 없습니다.");
        }

        var (operation, path, method) = operationInfo;

        // 기본 서버 URL 설정
        var serverUrl = _document.Servers.FirstOrDefault()?.Url?.TrimEnd('/');
        if (string.IsNullOrEmpty(serverUrl))
        {
            return ToolOutput.Failure("서버 URL이 OpenAPI 문서에 정의되어 있지 않습니다.");
        }

        // 경로 파라미터 치환
        var finalPath = path;
        foreach (var parameter in operation.Parameters.Where(p => p.In == ParameterLocation.Path))
        {
            if (!input.TryGetValue(parameter.Name, out var value))
            {
                return ToolOutput.Failure($"경로 파라미터 '{parameter.Name}'가 필요합니다.");
            }
            finalPath = finalPath.Replace($"{{{parameter.Name}}}", Uri.EscapeDataString(value?.ToString()!));
        }

        // 쿼리 파라미터 구성
        var queryParams = new List<string>();
        foreach (var parameter in operation.Parameters.Where(p => p.In == ParameterLocation.Query))
        {
            if (input.TryGetValue(parameter.Name, out var value))
            {
                queryParams.Add($"{Uri.EscapeDataString(parameter.Name)}={Uri.EscapeDataString(value?.ToString()!)}");
            }
        }
        var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : string.Empty;

        // 최종 URL 구성
        var url = $"{serverUrl}{finalPath}{queryString}";

        // HttpRequestMessage 생성
        var request = new HttpRequestMessage(new HttpMethod(method.ToString()), url);

        // 요청 본문 설정 (필요한 경우)
        if (operation.RequestBody != null)
        {
            var contentType = operation.RequestBody.Content.Keys.FirstOrDefault();
            if (string.IsNullOrEmpty(contentType))
            {
                return ToolOutput.Failure("지원되는 Content-Type이 없습니다.");
            }

            if (!operation.RequestBody.Content.TryGetValue(contentType, out var mediaType))
            {
                return ToolOutput.Failure($"Content-Type '{contentType}'에 대한 정의를 찾을 수 없습니다.");
            }

            // 요청 본문 스키마에 따라 JSON 생성
            var schema = mediaType.Schema;
            var jsonBody = new Dictionary<string, object>();
            foreach (var property in schema.Properties)
            {
                if (input.TryGetValue(property.Key, out var value))
                {
                    jsonBody[property.Key] = value!;
                }
                else if (schema.Required.Contains(property.Key))
                {
                    return ToolOutput.Failure($"요청 본문 속성 '{property.Key}'가 필요합니다.");
                }
            }

            var jsonString = JsonSerializer.Serialize(jsonBody);
            request.Content = new StringContent(jsonString, Encoding.UTF8, contentType);
        }

        // HttpClient를 통해 요청 실행
        using var httpClient = new HttpClient();
        try
        {
            var response = await httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ToolOutput.Failure($"API 호출 실패: {response.StatusCode}\n{responseContent}");
            }

            return ToolOutput.Success(responseContent);
        }
        catch (Exception ex)
        {
            return ToolOutput.Failure($"예외 발생: {ex.Message}");
        }
    }

}
