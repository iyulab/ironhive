namespace IronHive.Abstractions.Catalog;

/// <summary>
/// 대화형(챗) 모델 전용 스펙입니다.
/// </summary>
public sealed record ChatModelSpec : ModelSpec
{
    /// <summary>
    /// 문맥 창 크기(토큰). 한 번의 대화 턴에서 모델이 고려할 수 있는 총 토큰 수(입력+출력).
    /// </summary>
    public int? ContextWindow { get; init; }

    /// <summary>
    /// 단일 응답에서 생성 가능한 최대 토큰 수.
    /// </summary>
    public int? MaxOutputTokens { get; init; }

    /// <summary>
    /// 모델이 제공하는 기능(예: tool-calling, structured output, web-search 등).
    /// </summary>
    public IReadOnlyCollection<string>? Features { get; init; }

    /// <summary>
    /// 허용 입력 모달리티(예: "text", "image", "audio").
    /// </summary>
    public IReadOnlyCollection<string>? InputModalities { get; init; }

    /// <summary>
    /// 생성 가능한 출력 모달리티(예: "text", "image", "audio").
    /// </summary>
    public IReadOnlyCollection<string>? OutputModalities { get; init; }
}
