using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.GenerateContent;

internal sealed class SpeechConfig
{
    /// <summary>사전 정의된 보이스 설정.</summary>
    [JsonPropertyName("voiceConfig")]
    public VoiceConfig? VoiceConfig { get; set; }

    /// <summary>멀티 스피커 구성.</summary>
    [JsonPropertyName("multiSpeakerVoiceConfig")]
    public MultiSpeakerVoiceConfig? MultiVoiceConfig { get; set; }

    /// <summary> 
    /// de-DE, en-AU, en-GB, en-IN, en-US, es-US, 
    /// fr-FR, hi-IN, pt-BR, ar-XA, es-ES, fr-CA, id-ID, it-IT, ja-JP, 
    /// tr-TR, vi-VN, bn-IN, gu-IN, kn-IN, ml-IN, mr-IN, 
    /// ta-IN, te-IN, nl-NL, ko-KR, cmn-CN, pl-PL, ru-RU, and th-TH.
    /// </summary>
    [JsonPropertyName("languageCode")]
    public string? LanguageCode { get; set; }
}

internal sealed class VoiceConfig
{
    /// <summary>사전 정의된 보이스 설정.</summary>
    [JsonPropertyName("prebuiltVoiceConfig")]
    public Config? PrebuiltConfig { get; set; }

    internal sealed class Config
    {
        /// <summary>사전 정의 보이스 이름.</summary>
        [JsonPropertyName("voiceName")]
        public string? VoiceName { get; set; }
    }
}

internal sealed class MultiSpeakerVoiceConfig
{
    /// <summary>스피커별 보이스 설정.</summary>
    [JsonPropertyName("speakerVoiceConfigs")]
    public ICollection<Config> VoiceConfigs { get; set; } = [];

    internal sealed class Config
    {
        /// <summary>스피커 식별자(프롬프트에 등장하는 이름과 매칭).</summary>
        [JsonPropertyName("speaker")]
        public required string Speaker { get; set; }

        /// <summary>보이스 설정.</summary>
        [JsonPropertyName("voiceConfig")]
        public required VoiceConfig VoiceConfig { get; set; }
    }
}