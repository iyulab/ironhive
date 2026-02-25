using IronHive.Abstractions.Registries;

namespace IronHive.Abstractions.Audio;

/// <summary>
/// 오디오 처리 기능(TTS, STT)을 제공하는 프로바이더 인터페이스입니다.
/// <para>
/// 이 인터페이스는 <b>프로바이더 구현자</b>가 사용합니다.
/// OpenAI, Google 등 특정 오디오 처리 제공자에 대한 저수준 어댑터 역할을 합니다.
/// </para>
/// <para>
/// 애플리케이션 코드에서는 <see cref="IAudioService"/>를 사용하세요.
/// </para>
/// </summary>
public interface IAudioProcessor : IProviderItem
{
    /// <summary>
    /// 텍스트를 음성으로 변환합니다. (TTS)
    /// </summary>
    Task<TextToSpeechResponse> GenerateSpeechAsync(
        TextToSpeechRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 음성을 텍스트로 변환합니다. (STT)
    /// </summary>
    Task<SpeechToTextResponse> TranscribeAsync(
        SpeechToTextRequest request,
        CancellationToken cancellationToken = default);
}
