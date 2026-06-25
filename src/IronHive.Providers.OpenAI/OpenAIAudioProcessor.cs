using IronHive.Abstractions.Audio;
using OpenAI;
using OpenAI.Audio;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// OpenAI를 사용하여 오디오를 처리하는 클래스입니다.
/// </summary>
public class OpenAIAudioProcessor : IAudioProcessor
{
    private readonly OpenAIClient _openai;

    public OpenAIAudioProcessor(string apiKey)
        : this(new OpenAIConfig { ApiKey = apiKey })
    { }

    public OpenAIAudioProcessor(OpenAIConfig config)
    {
        _openai = OpenAIClientFactory.Create(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public virtual async Task<TextToSpeechResponse> GenerateSpeechAsync(
        TextToSpeechRequest request,
        CancellationToken cancellationToken = default)
    {
        var client = _openai.GetAudioClient(request.Model);

        var result = await client.GenerateSpeechAsync(
            request.Text,
            new GeneratedSpeechVoice(request.Voice),
            new SpeechGenerationOptions
            {
                ResponseFormat = GeneratedSpeechFormat.Mp3
            },
            cancellationToken);

        return new TextToSpeechResponse
        {
            Audio = new GeneratedAudio
            {
                MimeType = "audio/mp3",
                Data = result.Value.ToArray(),
            }
        };
    }

    /// <inheritdoc />
    public virtual async Task<SpeechToTextResponse> TranscribeAsync(
        SpeechToTextRequest request,
        CancellationToken cancellationToken = default)
    {
        var client = _openai.GetAudioClient(request.Model);

        using var stream = new MemoryStream(request.Audio.Data);
        var filename = "audio." + request.Audio.MimeType switch
        {
            "audio/flac" => "flac",
            "audio/mp3" or "audio/mpeg" => "mp3",
            "audio/mp4" or "audio/m4a" => "m4a",
            "audio/mpga" => "mpga",
            "audio/ogg" => "ogg",
            "audio/wav" or "audio/x-wav" => "wav",
            "audio/webm" => "webm",
            _ => "bin"
        };

        if (request.Model.Contains("diarize"))
        {
            var result = await client.TranscribeAudioDiarizedAsync(
                stream,
                filename,
                new AudioTranscriptionOptions
                {
                    ResponseFormat = AudioTranscriptionFormat.Diarized,
                    ChunkingStrategy = AudioTranscriptionDefaultChunkingStrategy.Auto,
                },
                cancellationToken);

            var transcription = result.Value;
            return new SpeechToTextResponse
            {
                Text = transcription.Text ?? string.Empty,
                Segments = transcription.Segments?.Count > 0
                    ? transcription.Segments.Select(s => new TranscriptionSegment
                    {
                        Speaker = s.SpeakerLabel,
                        Start = (float)s.StartTime.TotalSeconds,
                        End = (float)s.EndTime.TotalSeconds,
                        Text = s.Text,
                    }).ToList()
                    : null
            };
        }
        else
        {
            var result = await client.TranscribeAudioAsync(
                stream,
                filename,
                cancellationToken: cancellationToken);

            var transcription = result.Value;
            return new SpeechToTextResponse
            {
                Text = transcription.Text ?? string.Empty,
            };
        }
    }
}
