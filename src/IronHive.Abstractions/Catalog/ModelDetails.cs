namespace IronHive.Abstractions.Catalog;

/// <summary>
/// AI 모델의 상세 정보를 포함하는 클래스입니다.
/// </summary>
public class ModelDetails : ModelSummary
{
    /// <summary>
    /// 모델이 제공하는 기능의 목록입니다.
    /// </summary>
    public IEnumerable<string>? Capabilities { get; set; }

    /// <summary>
    /// 모델이 지원하는 입력 양식의 목록입니다.
    /// </summary>
    public IEnumerable<string>? InputModalities { get; set; }

    /// <summary>
    /// 모델이 지원하는 출력 양식의 목록입니다.
    /// </summary>
    public IEnumerable<string>? OutputModalities { get; set; }

    /// <summary>
    /// 모델의 소유자 또는 개발 기관의 이름입니다.
    /// </summary>
    public string? OwnedBy { get; set; }

    /// <summary>
    /// 모델이 생성된 날짜 및 시간입니다 (UTC 기준).
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 모델이 수정된 날짜 및 시간입니다 (UTC 기준).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 모델의 메타데이터입니다. 추가적인 정보나 속성을 포함할 수 있습니다.
    /// </summary>
    public IDictionary<string, object>? Metadata { get; set; }
}
