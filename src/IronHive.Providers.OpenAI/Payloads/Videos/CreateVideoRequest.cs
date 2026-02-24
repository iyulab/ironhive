using IronHive.Abstractions.Images;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Videos;

/// <summary>
/// OpenAI 비디오 생성 요청 (POST /v1/videos)
/// </summary>
public class CreateVideoRequest : OpenAIPayloadBase
{
    /// <summary>
    /// 생성할 비디오를 설명하는 텍스트 프롬프트
    /// </summary>
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    /// <summary>
    /// 입력 참조 이미지 (Image-to-Video).
    /// multipart/form-data로 전송 시 별도 처리.
    /// </summary>
    [JsonIgnore]
    public GeneratedImage? Image { get; set; }

    /// <summary>
    /// 모델 ID (e.g. "sora-2", "sora-2-pro")
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// 비디오 길이(초). 
    /// <para>지원값: 4, 8, 12</para>
    /// </summary>
    [JsonPropertyName("seconds")]
    public int? Seconds { get; set; }

    /// <summary>
    /// 출력 해상도 
    /// <para>지원값: "720x1280", "1280x720", "1024x1792", "1792x1024"</para>
    /// </summary>
    [JsonPropertyName("size")]
    public string? Size { get; set; }
}
