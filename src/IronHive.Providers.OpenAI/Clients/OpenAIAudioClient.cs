using IronHive.Providers.OpenAI.Payloads.Audio;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace IronHive.Providers.OpenAI.Clients;

public class OpenAIAudioClient : OpenAIClientBase
{
    public OpenAIAudioClient(string apiKey) : base(apiKey) { }

    public OpenAIAudioClient(OpenAIConfig config) : base(config) { }

    /// <summary>
    /// 텍스트를 음성으로 변환합니다. (TTS)
    /// </summary>
    public async Task<byte[]> PostCreateSpeechAsync(
        CreateSpeechRequest request,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        using var response = await Client.PostAsync(
            OpenAIConstants.PostAudioSpeechPath.RemovePrefix('/'),
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        var audio = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return audio;
    }

    /// <summary>
    /// 음성을 텍스트로 변환합니다. (STT)
    /// </summary>
    public async Task<CreateTranscriptionResponse> PostCreateTranscriptionAsync(
        CreateTranscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();

        // 오디오 파일
        if (request.Audio.Data != null && !string.IsNullOrEmpty(request.Audio.MimeType))
        {
            var audioContent = new ByteArrayContent(request.Audio.Data);
            audioContent.Headers.ContentType = new MediaTypeHeaderValue(request.Audio.MimeType);
            var extension = request.Audio.MimeType switch
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
            content.Add(audioContent, "file", $"audio.{extension}");
        }

        // 모델
        content.Add(new StringContent(request.Model), "model");

        // 선택적 파라미터
        if (request.ChunkingStrategy != null)
        {
            var chunkingJson = JsonSerializer.Serialize(request.ChunkingStrategy, JsonOptions);
            content.Add(new StringContent(chunkingJson, Encoding.UTF8, "application/json"), "chunking_strategy");
        }

        if (request.Include?.Length > 0)
        {
            foreach (var item in request.Include)
            {
                content.Add(new StringContent(item), "include");
            }
        }

        if (request.KnownSpeakerNames?.Length > 0)
        {
            foreach (var name in request.KnownSpeakerNames)
            {
                content.Add(new StringContent(name), "known_speaker_names");
            }
        }

        if (request.KnownSpeakerReferences?.Length > 0)
        {
            foreach (var reference in request.KnownSpeakerReferences)
            {
                content.Add(new StringContent(reference), "known_speaker_references");
            }
        }

        if (!string.IsNullOrEmpty(request.Language))
            content.Add(new StringContent(request.Language), "language");

        if (!string.IsNullOrEmpty(request.Prompt))
            content.Add(new StringContent(request.Prompt), "prompt");

        if (!string.IsNullOrEmpty(request.ResponseFormat))
            content.Add(new StringContent(request.ResponseFormat), "response_format");

        if (request.Stream.HasValue)
            content.Add(new StringContent(request.Stream.Value.ToString().ToLowerInvariant()), "stream");

        if (request.Temperature.HasValue)
            content.Add(new StringContent(request.Temperature.Value.ToString(CultureInfo.InvariantCulture)), "temperature");

        if (request.TimestampGranularities?.Length > 0)
        {
            foreach (var granularity in request.TimestampGranularities)
            {
                content.Add(new StringContent(granularity), "timestamp_granularities");
            }
        }

        using var response = await Client.PostAsync(
            OpenAIConstants.PostAudioTranscriptionsPath.RemovePrefix('/'),
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        var res = await response.Content.ReadFromJsonAsync<CreateTranscriptionResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");

        return res;
    }
}
