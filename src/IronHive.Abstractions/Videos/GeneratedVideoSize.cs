using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Videos;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(GeneratedVideoPresetSize), "preset")]
[JsonDerivedType(typeof(GeneratedVideoCustomSize), "custom")]
public abstract class GeneratedVideoSize
{ }

public class GeneratedVideoPresetSize : GeneratedVideoSize
{
    /// <summary>
    /// 서비스 제공자가 정의하는 해상도 크기
    /// <para>예: "720p", "1080p", "4k"</para>
    /// </summary>
    public string? Resolution { get; set; }

    /// <summary>
    /// 서비스 제공자가 정의하는 가로 세로 비율
    /// <para>예: "16:9", "9:16"</para>
    /// </summary>
    public string? AspectRatio { get; set; }
}

public class GeneratedVideoCustomSize : GeneratedVideoSize
{
    /// <summary>
    /// 서비스 제공자가 정의하는 사용자 지정 크기
    /// <para>예: "1280x720", "720x1280"</para>
    /// </summary>
    public string? Value { get; set; }
}
