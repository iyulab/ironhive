using System.Text.Json.Nodes;
using IronHive.Providers.OpenAI;

namespace IronHive.Providers.OpenAI.Compatible.Adapters;

/// <summary>
/// Together AI 제공자 어댑터입니다.
/// </summary>
/// <remarks>
/// Together AI는 OpenAI API와 대부분 호환됩니다.
/// Chat, Vision, Images, Embeddings, Speech 등 지원.
/// </remarks>
public class TogetherAIAdapter : BaseProviderAdapter
{
    /// <inheritdoc />
    public override CompatibleProvider Provider => CompatibleProvider.TogetherAI;

    /// <inheritdoc />
    protected override string DefaultBaseUrl => "https://api.together.xyz/v1";

    /// <inheritdoc />
    public override JsonObject TransformRequest(JsonObject request, CompatibleConfig config)
    {
        if (config is TogetherAIConfig togetherConfig)
        {
            // Together AI 특수 설정 (현재는 거의 OpenAI 호환)
        }

        return base.TransformRequest(request, config);
    }
}

/// <summary>
/// Together AI 특수 설정입니다.
/// </summary>
public class TogetherAIConfig : CompatibleConfig
{
    /// <summary>
    /// Together AI API를 위한 기본 설정으로 초기화합니다.
    /// </summary>
    public TogetherAIConfig()
    {
        Provider = CompatibleProvider.TogetherAI;
        BaseUrl = "https://api.together.xyz/v1";
    }

    /// <summary>
    /// Together AI의 Embeddings API 사용을 위해 OpenAIConfig로 변환합니다.
    /// </summary>
    public OpenAIConfig ToOpenAIConfig()
    {
        return new OpenAIConfig
        {
            ApiKey = ApiKey,
            BaseUrl = BaseUrl ?? "https://api.together.xyz/v1",
            DefaultHeaders = DefaultHeaders
        };
    }
}
