using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Images;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(GeneratedImagePresetSize), "preset")]
[JsonDerivedType(typeof(GeneratedImageCustomSize), "custom")]
public abstract class GeneratedImageSize
{ }

public class GeneratedImagePresetSize : GeneratedImageSize
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

public class GeneratedImageCustomSize : GeneratedImageSize
{
    /// <summary>
    /// 서비스 제공자가 정의하는 사전 정의된 크기
    /// <para>예: "256x256", "512x512", "1024x1024"</para>
    /// </summary>
    public string? Value { get; set; }
}
