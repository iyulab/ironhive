namespace IronHive.Abstractions.Message;

/// <summary>
/// 텍스트를 생성할 때 사용할 설정을 정의합니다.
/// </summary>
public class MessageGenerationParameters
{
    /// <summary>
    /// 생성할 최대 토큰 수. 생성할 텍스트의 길이를 제한합니다.
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// 온도 기반 토큰 샘플링 방식입니다. 이 값에 따라 확률 분포가 조정되어 LLM의 출력 무작위성을 제어합니다.
    /// 예: 0.1로 설정하면 확률 분포가 예측 가능하게 좁아져 안정적인 출력이 나오며, 2.0으로 설정하면 분포가 평탄해져 더 창의적이고 무작위한 출력이 됩니다.
    /// 주의: 모델 제공자에 따라 설정 범위가 다릅니다. (예: OpenAI에서 0.0 ~ 2.0, Anthropic에서 0.0 ~ 1.0)
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// 확률기반 토큰 샘플링 방식으로, 0.0에서 1.0 사이입니다.
    /// 예: 0.9로 설정하면 상위 90% 확률의 토큰 그룹만 사용됩니다.
    /// </summary>
    public float? TopP { get; set; }

    /// <summary>
    /// 갯수 기반 토큰 샘플링 방식으로, 0이상의 값을 사용합니다.
    /// 예: 10으로 설정하면 가장 가능성 높은 10개의 토큰만 사용됩니다.
    /// </summary>
    public int? TopK { get; set; }

    /// <summary>
    /// 텍스트 생성을 중지할 특정 시퀀스(문자열) 목록. 이 시퀀스를 만나면 생성을 바로 멈춥니다.
    /// 예: {"\n", "."}으로 설정하면 줄 바꿈이나 마침표가 나오면 생성이 끝납니다.
    /// </summary>
    public ICollection<string>? StopSequences { get; set; }
}
