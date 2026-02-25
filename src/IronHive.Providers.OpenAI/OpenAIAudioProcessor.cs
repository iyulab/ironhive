using IronHive.Abstractions.Audio;
using IronHive.Providers.OpenAI.Clients;
using IronHive.Providers.OpenAI.Payloads.Audio;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// OpenAI를 사용하여 오디오를 처리하는 클래스입니다.
/// <para>
/// TTS: gpt-4o-mini-tts, tts-1, tts-1-hd
/// </para>
/// <para>
/// STT: whisper-1, gpt-4o-transcribe, gpt-4o-mini-transcribe
/// </para>
/// </summary>
public class OpenAIAudioProcessor : IAudioProcessor
{
    private readonly OpenAIAudioClient _client;

    public OpenAIAudioProcessor(string apiKey)
        : this(new OpenAIConfig { ApiKey = apiKey })
    { }

    public OpenAIAudioProcessor(OpenAIConfig config)
    {
        _client = new OpenAIAudioClient(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public virtual async Task<TextToSpeechResponse> GenerateSpeechAsync(
        TextToSpeechRequest request,
        CancellationToken cancellationToken = default)
    {
        var payload = OnBeforeGenerateSpeech(request, new CreateSpeechRequest
        {
            Model = request.Model,
            Input = request.Text,
            Voice = request.Voice,
            ResponseFormat = "mp3",
        });

        var content = await _client.PostCreateSpeechAsync(payload, cancellationToken);

        return new TextToSpeechResponse
        {
            Audio = new GeneratedAudio
            {
                MimeType = "audio/mp3",
                Data = content,
            }
        };
    }
    
    /// <inheritdoc />
    public virtual async Task<SpeechToTextResponse> TranscribeAsync(
        SpeechToTextRequest request,
        CancellationToken cancellationToken = default)
    {
        var payload = OnBeforeTranscribe(request, new CreateTranscriptionRequest
        {
            Audio = request.Audio,
            Model = request.Model,
            ResponseFormat = request.Model.Contains("diarize") 
                ? "diarized_json" 
                : "json",
        });

        var response = await _client.PostCreateTranscriptionAsync(payload, cancellationToken);

        return OnAfterTranscribe(response, new SpeechToTextResponse
        {
            Text = response.Text ?? string.Empty,
            Segments = response.Segments != null && response.Segments.Any()
                ? response.Segments.Select(s => new Abstractions.Audio.TranscriptionSegment
                {
                    Speaker = s.Speaker,
                    Start = s.Start,
                    End = s.End,
                    Text = s.Text,
                }).ToList()
                : null
        });
    }

    /// <summary>
    /// TTS 요청을 보내기 전에 처리할 수 있는 가상 메서드입니다.
    /// </summary>
    protected virtual CreateSpeechRequest OnBeforeGenerateSpeech(
        TextToSpeechRequest source,
        CreateSpeechRequest request)
        => request;

    /// <summary>
    /// STT 요청을 보내기 전에 처리할 수 있는 가상 메서드입니다.
    /// </summary>
    protected virtual CreateTranscriptionRequest OnBeforeTranscribe(
        SpeechToTextRequest source,
        CreateTranscriptionRequest request)
        => request;

    /// <summary>
    /// STT 응답을 받은 후 처리할 수 있는 가상 메서드입니다.
    /// </summary>
    protected virtual SpeechToTextResponse OnAfterTranscribe(
        CreateTranscriptionResponse source,
        SpeechToTextResponse response)
        => response;
}
