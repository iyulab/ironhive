namespace IronHive.Abstractions.Audio;

/// <summary>
/// 오디오 처리 서비스를 제공하는 애플리케이션 인터페이스입니다.
/// <para>
/// 이 인터페이스는 <b>애플리케이션 개발자</b>가 사용합니다.
/// 등록된 프로바이더를 통해 오디오 처리 작업(TTS, STT)을 수행합니다.
/// </para>
/// <para>
/// 프로바이더를 구현하는 경우에는 <see cref="IAudioProcessor"/>를 구현하세요.
/// </para>
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// 텍스트를 음성으로 변환합니다. (TTS)
    /// </summary>
    /// <param name="provider">사용할 프로바이더 이름</param>
    /// <param name="request">TTS 요청</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>생성된 오디오 응답</returns>
    Task<TextToSpeechResponse> GenerateSpeechAsync(
        string provider,
        TextToSpeechRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 음성을 텍스트로 변환합니다. (STT)
    /// </summary>
    /// <param name="provider">사용할 프로바이더 이름</param>
    /// <param name="request">STT 요청</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>변환된 텍스트 응답</returns>
    Task<SpeechToTextResponse> TranscribeAsync(
        string provider,
        SpeechToTextRequest request,
        CancellationToken cancellationToken = default);
}
