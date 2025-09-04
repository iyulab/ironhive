using IronHive.Abstractions.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Tools;

/// <summary>
/// 툴 실행 결과를 나타내는 인터페이스입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ToolSuccessOutput), "success")]
[JsonDerivedType(typeof(ToolFailureOutput), "failure")]
[JsonDerivedType(typeof(ToolDeniedOutput), "denied")]
[JsonDerivedType(typeof(ToolTimeoutOutput), "timeout")]
[JsonDerivedType(typeof(ToolExcessiveOutput), "excessive")]
public interface IToolOutput
{
    /// <summary>
    /// 퉅 실행이 성공했는지를 나타냅니다.
    /// </summary>
    bool IsSuccess => this is ToolSuccessOutput;

    /// <summary>
    /// LLM에게 반환할 결과에 대한 메시지입니다.
    /// </summary>
    string Result { get; set; }
}

public sealed record ToolSuccessOutput : IToolOutput
{
    public ToolSuccessOutput(object? result = null)
    {
        var str = JsonSerializer.Serialize(result, JsonDefaultOptions.Options);
        Result = string.IsNullOrWhiteSpace(str) ? "null" : str;
    }

    /// <inheritdoc />
    public string Result { get; set; }
}

public sealed record ToolFailureOutput : IToolOutput
{
    public ToolFailureOutput(Exception? ex = null)
    {
        Result = ex?.Message ?? "Tool execution failed.";
    }

    /// <inheritdoc />
    public string Result { get; set; }
}

public sealed record ToolDeniedOutput : IToolOutput
{
    /// <inheritdoc />
    public string Result { get; set; } = "Tool execution was denied.";
}

public sealed record ToolTimeoutOutput : IToolOutput
{
    /// <inheritdoc />
    public string Result { get; set; } = "Tool execution timed out.";
}

public sealed record ToolExcessiveOutput : IToolOutput
{
    /// <inheritdoc />
    public string Result { get; set; } = "Tool result is too large.";
}