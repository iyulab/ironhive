namespace IronHive.Abstractions.Tools;

/// <summary>
/// 도구(툴)에 대한 메타데이터를 나타내는 클래스입니다.
/// 이름, 설명, 입력 파라미터 스키마, 실행 승인 여부 등의 정보를 포함합니다.
/// </summary>
public class ToolDescriptor
{
    /// <summary>
    /// 도구의 고유 이름입니다.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 도구의 설명입니다. 기능이나 용도에 대한 간략한 설명을 제공합니다.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 도구에 전달되는 파라미터의 JSON 스키마입니다.
    /// 입력 형식 및 필드 정의에 사용됩니다.
    /// </summary>
    public object? Parameters { get; set; }

    /// <summary>
    /// 도구 실행 전에 사용자의 승인이 필요한지를 나타냅니다.
    /// </summary>
    public required bool RequiresApproval { get; set; }
}
