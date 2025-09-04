using IronHive.Providers.OpenAI;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// OpenAI의 모든 서비스들을 지정된 이름으로 설정합니다.
    /// </summary>
    public static IHiveServiceBuilder AddOpenAIProviders(
        this IHiveServiceBuilder builder, 
        string providerName,
        OpenAIConfig config)
    {
        builder.AddModelCatalog(providerName, new OpenAIModelCatalog(config));
        builder.AddMessageGenerator(providerName, new OpenAIMessageGenerator(config));
        builder.AddEmbeddingGenerator(providerName, new OpenAIEmbeddingGenerator(config));
        return builder;
    }
}
