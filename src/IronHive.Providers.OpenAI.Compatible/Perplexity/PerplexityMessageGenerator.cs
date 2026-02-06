using System.Text.Json;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;

namespace IronHive.Providers.OpenAI.Compatible.Perplexity;

/// <summary>
/// Perplexity 서비스를 위한 메시지 생성기입니다.
/// </summary>
internal class PerplexityMessageGenerator : CompatibleChatMessageGenerator
{
    private readonly PerplexityConfig _perplexityConfig;

    public PerplexityMessageGenerator(PerplexityConfig config) : base(config)
    {
        _perplexityConfig = config;
    }

    protected override T PostProcessRequest<T>(ChatCompletionRequest request)
    {
        // n=1 강제 (Perplexity는 1만 지원)
        request.N = 1;

        // 미지원 파라미터 제거
        request.Tools = null;
        request.ToolChoice = null;
        request.ResponseFormat = null;
        request.LogitBias = null;
        request.LogProbs = null;
        request.TopLogProbs = null;
        request.WebSearchOptions = null;

        // Perplexity 전용 검색 파라미터 주입
        request.AdditionalProperties ??= [];

        if (_perplexityConfig.SearchDomainFilter is { Count: > 0 })
            request.AdditionalProperties["search_domain_filter"] =
                JsonSerializer.SerializeToElement(_perplexityConfig.SearchDomainFilter);

        if (_perplexityConfig.ReturnImages.HasValue)
            request.AdditionalProperties["return_images"] =
                JsonSerializer.SerializeToElement(_perplexityConfig.ReturnImages.Value);

        if (_perplexityConfig.ReturnRelatedQuestions.HasValue)
            request.AdditionalProperties["return_related_questions"] =
                JsonSerializer.SerializeToElement(_perplexityConfig.ReturnRelatedQuestions.Value);

        if (!string.IsNullOrEmpty(_perplexityConfig.SearchRecencyFilter))
            request.AdditionalProperties["search_recency_filter"] =
                JsonSerializer.SerializeToElement(_perplexityConfig.SearchRecencyFilter);

        if (_perplexityConfig.TopK.HasValue)
            request.AdditionalProperties["top_k"] =
                JsonSerializer.SerializeToElement(_perplexityConfig.TopK.Value);

        return (T)request;
    }
}
