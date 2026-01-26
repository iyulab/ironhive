using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.GenerateContent;

internal sealed class ImageConfig
{
    /// <summary>
    /// 생성할 이미지의 가로세로 비율.
    /// 1:1, 2:3, 3:2, 3:4, 4:3, 4:5, 5:4, 9:16, 16:9, 21:9 중 하나.
    /// </summary>
    [JsonPropertyName("aspectRatio")]
    public string? AspectRatio { get; set; }

    /// <summary>
    /// 이미지의 크기.
    /// 1K, 2K, 4K 중 하나.
    /// </summary>
    [JsonPropertyName("imageSize")]
    public string? ImageSize { get; set; }
}
