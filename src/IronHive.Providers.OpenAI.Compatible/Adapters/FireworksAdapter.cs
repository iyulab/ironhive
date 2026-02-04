using System.Text.Json.Nodes;
using IronHive.Providers.OpenAI;

namespace IronHive.Providers.OpenAI.Compatible.Adapters;

/// <summary>
/// Fireworks AI 제공자 어댑터입니다.
/// </summary>
/// <remarks>
/// Fireworks AI는 고성능 모델 서빙 플랫폼으로 OpenAI API와 호환됩니다.
/// </remarks>
public class FireworksAdapter : BaseProviderAdapter
{
    /// <inheritdoc />
    public override CompatibleProvider Provider => CompatibleProvider.Fireworks;

    /// <inheritdoc />
    protected override string DefaultBaseUrl => "https://api.fireworks.ai/inference/v1";

    /// <inheritdoc />
    public override JsonObject TransformRequest(JsonObject request, CompatibleConfig config)
    {
        if (config is FireworksConfig fireworksConfig)
        {
            // Fireworks 특수 설정
        }

        return base.TransformRequest(request, config);
    }
}

/// <summary>
/// Fireworks AI 특수 설정입니다.
/// </summary>
public class FireworksConfig : CompatibleConfig
{
    /// <summary>
    /// Fireworks AI API를 위한 기본 설정으로 초기화합니다.
    /// </summary>
    public FireworksConfig()
    {
        Provider = CompatibleProvider.Fireworks;
        BaseUrl = "https://api.fireworks.ai/inference/v1";
    }

    /// <summary>
    /// Fireworks의 Embeddings API 사용을 위해 OpenAIConfig로 변환합니다.
    /// </summary>
    public OpenAIConfig ToOpenAIConfig()
    {
        return new OpenAIConfig
        {
            ApiKey = ApiKey,
            BaseUrl = BaseUrl ?? "https://api.fireworks.ai/inference/v1",
            DefaultHeaders = DefaultHeaders
        };
    }
}
