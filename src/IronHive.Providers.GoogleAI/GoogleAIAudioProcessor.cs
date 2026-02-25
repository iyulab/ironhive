using Google.GenAI;
using Google.GenAI.Types;
using IronHive.Abstractions.Audio;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace IronHive.Providers.GoogleAI;

/// <summary>
/// Google Gemini API를 사용하여 오디오를 처리하는 클래스입니다.
/// </summary>
public partial class GoogleAIAudioProcessor : IAudioProcessor
{
    private readonly Client _client;
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public GoogleAIAudioProcessor(string apiKey)
        : this(new GoogleAIConfig { ApiKey = apiKey })
    { }

    public GoogleAIAudioProcessor(GoogleAIConfig config)
    {
        _client = GoogleAIClientFactory.CreateClient(config);
    }

    public GoogleAIAudioProcessor(VertexAIConfig config)
    {
        _client = GoogleAIClientFactory.CreateClient(config);
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
        // Gemini API는 GenerateContent를 통해 오디오를 생성합니다
        var contents = new Content
        {
            Role = "user",
            Parts = [new Part { Text = request.Text }],
        };

        var config = new GenerateContentConfig
        {
            ResponseModalities = ["AUDIO"], // Modality.Audio를 문자열로
            SpeechConfig = new SpeechConfig
            {
                VoiceConfig = new VoiceConfig
                {
                    PrebuiltVoiceConfig = new PrebuiltVoiceConfig
                    {
                        VoiceName = request.Voice
                    }
                }
            }
        };

        var response = await _client.Models.GenerateContentAsync(
            request.Model,
            [contents],
            config,
            cancellationToken);

        var audioBlob = response.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.InlineData;

        if (audioBlob == null || audioBlob.Data == null || audioBlob.MimeType == null)
            throw new InvalidOperationException("No audio data in response.");

        // MIME 타입이 L16(Raw PCM)인 경우 WAV로 변환
        if (audioBlob.MimeType.StartsWith("audio/L16;codec=pcm;", StringComparison.OrdinalIgnoreCase))
        {
            var match = SampleRateRegex().Match(audioBlob.MimeType);
            int rate = match.Success ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) : 24000;
            return new TextToSpeechResponse
            {
                Audio = new GeneratedAudio 
                { 
                    MimeType = "audio/wav", 
                    Data = ConvertToWav(audioBlob.Data, rate) 
                }
            };
        }

        // 그 외의 경우는 원본 데이터를 그대로 반환
        return new TextToSpeechResponse
        {
            Audio = new GeneratedAudio 
            { 
                MimeType = audioBlob.MimeType, 
                Data = audioBlob.Data 
            }
        };
    }

    /// <inheritdoc />
    public virtual async Task<SpeechToTextResponse> TranscribeAsync(
        SpeechToTextRequest request,
        CancellationToken cancellationToken = default)
    {
        // Gemini API는 GenerateContent를 통해 오디오를 처리합니다.
        var contents = new Content
        {
            Role = "user",
            Parts =
            [
                new Part
                {
                    Text = """
                    Process the provided audio and generate a detailed transcription.

                    Requirements:
                    - Identify distinct speakers when possible (e.g., "Speaker 1", "Speaker 2").
                    - Provide accurate timestamps in seconds for each segment.
                    - Ensure segments are ordered chronologically.
                    - The full transcript should combine all segments into a single coherent text.
                    """
                },
                new Part
                {
                    InlineData = new Blob
                    {
                        MimeType = request.Audio.MimeType,
                        Data = request.Audio.Data
                    }
                }
            ]
        };

        // 요청 스키마는 text + audio blob 형태로 구성하고, 응답은 JSON으로 받도록 설정합니다.
        var config = new GenerateContentConfig
        {
            ResponseMimeType = "application/json",
            ResponseJsonSchema = new
            {
                type = "object",
                properties = new
                {
                    text = new
                    {
                        type = "string",
                        description = "The full transcript of the provided audio."
                    },
                    segments = new
                    {
                        type = "array",
                        description = "Array of transcript segments with timing and optional speaker.",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                speaker = new
                                {
                                    type = "string",
                                    description = "Speaker label, e.g., 'Speaker 1'."
                                },
                                text = new
                                {
                                    type = "string",
                                    description = "Segment transcript text."
                                },
                                start = new
                                {
                                    type = "number",
                                    description = "Segment start time in seconds."
                                },
                                end = new
                                {
                                    type = "number",
                                    description = "Segment end time in seconds."
                                }
                            },
                            required = new[] { "text", "start", "end" }
                        }
                    }
                },
                required = new[] { "text" }
            },
        };

        var response = await _client.Models.GenerateContentAsync(
            request.Model,
            [contents],
            config,
            cancellationToken);

        // JSON 파싱해서 반환
        var json = response.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text
            ?? throw new InvalidOperationException("No text data in response.");
        try
        {
            return JsonSerializer.Deserialize<SpeechToTextResponse>(json, s_jsonOptions)
                ?? throw new JsonException("Deserialized result was null.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse transcription JSON. Raw: {json}", ex);
        }
    }

    /// <summary>
    /// rate 파라미터를 추출하는 정규식입니다. 예: "audio/L16;rate=24000"에서 24000을 추출
    /// </summary>
    [GeneratedRegex(@"rate=(\d+)")]
    private static partial Regex SampleRateRegex();

    /// <summary>
    /// WAV 헤더를 생성하는 헬퍼 메서드
    /// </summary>
    /// <param name="pcmData">PCM 오디오 데이터</param>
    /// <param name="sampleRate">샘플링 레이트</param>
    /// <returns>WAV 형식의 오디오 데이터</returns>
    private static byte[] ConvertToWav(byte[] pcmData, int sampleRate)
    {
        // Gemini 쪽 출력은 little-endian PCM인 경우가 많아 그대로 WAV 헤더를 붙이는 방식으로 처리합니다.
        // 만약 노이즈면 pcmData를 역순으로 바꿔보는 것도 고려할 수 있습니다.
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true);

        const short channels = 1;
        const short bitsPerSample = 16;
        short blockAlign = (short)(channels * (bitsPerSample / 8));
        int byteRate = sampleRate * blockAlign;

        // RIFF header
        bw.Write(Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(36 + pcmData.Length);          // file size - 8
        bw.Write(Encoding.ASCII.GetBytes("WAVE"));

        // fmt chunk
        bw.Write(Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16);                           // PCM fmt chunk size
        bw.Write((short)1);                     // AudioFormat = 1 (PCM)
        bw.Write(channels);
        bw.Write(sampleRate);
        bw.Write(byteRate);
        bw.Write(blockAlign);
        bw.Write(bitsPerSample);

        // data chunk
        bw.Write(Encoding.ASCII.GetBytes("data"));
        bw.Write(pcmData.Length);
        bw.Write(pcmData);

        bw.Flush();
        return ms.ToArray();
    }
}
