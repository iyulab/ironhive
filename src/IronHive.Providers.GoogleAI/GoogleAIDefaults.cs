namespace IronHive.Providers.GoogleAI;

/// <summary>
/// Google AI 어댑터가 소비자 설정 없이 적용하는 기본값입니다.
/// </summary>
public static class GoogleAIDefaults
{
    /// <summary>
    /// 요청 타임아웃 기본값입니다. (10분)
    /// </summary>
    /// <remarks>
    /// 벤더 SDK는 타임아웃이 지정되지 않으면 <see cref="System.Net.Http.HttpClient"/>가 생성될 때의
    /// 기본값 100초를 그대로 사용한다. 그 값은 비스트리밍 호출에서 응답 전체를 제한하고, 스트리밍
    /// 호출에서는 첫 바이트 수신까지를 제한한다. 사고를 오래 하는 모델에서 100초는 흔히 부족하며,
    /// 그때 발생하는 취소는 어떤 설정에서 비롯됐는지 알려주지 않는다. 어댑터가 명시적 기본값을 두어
    /// 그 값이 결코 조용히 상속되지 않게 한다.
    /// </remarks>
    public static readonly TimeSpan Timeout = TimeSpan.FromMinutes(10);
}
