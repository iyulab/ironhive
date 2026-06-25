using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Images;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(GeneratedImageScaleSize), "scale")]
[JsonDerivedType(typeof(GeneratedImagePixelSize), "pixel")]
public abstract class GeneratedImageSize
{ }

public class GeneratedImageScaleSize : GeneratedImageSize
{
    /// <summary>
    /// 서비스 제공자가 정의하는 해상도 크기
    /// <para>예: "1k", "2k", "4k"</para>
    /// </summary>
    public string? Resolution { get; set; }

    /// <summary>
    /// 서비스 제공자가 정의하는 가로 세로 비율
    /// <para>예: "1:1", "4:3", "16:9"</para>
    /// </summary>
    public string? AspectRatio { get; set; }
}

/// <summary>
/// 정확한 픽셀 단위로 이미지 크기를 지정합니다.
/// </summary>
public class GeneratedImagePixelSize : GeneratedImageSize
{
    /// <summary>
    /// 이미지의 가로 크기 (px)
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// 이미지의 세로 크기 (px)
    /// </summary>
    public int Height { get; set; }
}
