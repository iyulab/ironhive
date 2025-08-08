using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion;

public class ChatAudio
{
    /// <summary>
    /// The audio format for output audio content.
    /// </summary>
    [JsonPropertyName("format")]
    public required AudioFormat Format { get; set; }

    /// <summary>
    /// The voice to use for the audio content.
    /// </summary>
    [JsonPropertyName("voice")]
    public required AudioVoice Voice { get; set; }

    public enum AudioFormat
    {
        Wav, Mp3, Flac, Opus, Pcm16
    }

    public enum AudioVoice
    {
        Alloy, Ash, Ballad, Coral, Echo, Fable, Nova, Onyx, Sage, Shimmer
    }
}