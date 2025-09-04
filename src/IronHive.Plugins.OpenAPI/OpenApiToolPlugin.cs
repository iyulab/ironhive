using IronHive.Abstractions.Tools;

namespace IronHive.Plugins.OpenAPI;

/// <summary>
/// 최신 OpenAPI.NET(2.x) 기반의 범용 OpenAPI 호출 도구
/// 필수 입력: operationId
/// 선택 입력: path/query/header 파라미터명 그대로, body는 Json 직렬화 가능한 객체 또는 JsonElement/JsonNode
/// </summary>
public sealed class OpenApiTool : ITool
{
    public string UniqueName => throw new NotImplementedException();

    public string Name => throw new NotImplementedException();

    public string Description => throw new NotImplementedException();

    public object? Parameters => throw new NotImplementedException();

    public bool RequiresApproval { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Task<IToolOutput> InvokeAsync(ToolInput input, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
