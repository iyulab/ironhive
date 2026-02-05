namespace IronHive.Providers.OpenAI.Compatible;

public abstract class CompatibleConfig
{
    /// <summary>
    /// 인증용 API 키를 가져오거나 설정합니다.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Converts the current configuration to an equivalent <see cref="OpenAIConfig"/> instance.
    /// </summary>
    /// <returns>
    /// the OpenAI configuration derived from the current settings.
    /// </returns>
    public abstract OpenAIConfig ToOpenAI();
}
