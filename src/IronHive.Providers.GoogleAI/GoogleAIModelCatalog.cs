using IronHive.Abstractions.Catalog;
using IronHive.Providers.GoogleAI.Catalog;
using IronHive.Providers.GoogleAI.Catalog.Models;
using IronHive.Providers.GoogleAI.Share;

namespace IronHive.Providers.GoogleAI;

/// <inheritdoc />
public class GoogleAIModelCatalog : IModelCatalog
{
    private readonly GoogleAIModelsClient _client;

    public GoogleAIModelCatalog(GoogleAIConfig config)
    {
        _client = new GoogleAIModelsClient(config);
    }

    public GoogleAIModelCatalog(string apiKey)
    {
        _client = new GoogleAIModelsClient(apiKey);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IModelSpec>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var list = new List<IModelSpec>();
        var next = string.Empty;
        do
        {
            var response = await _client.GetModelsAsync(new GoogleAIModelsRequest
            {
                PageSize = 1000,
                PageToken = next
            }, CancellationToken.None);
            foreach (var model in response.Models)
            {
                list.Add(ConvertToModelSpec(model));
            }
            next = response.NextPageToken;
        }
        while (string.IsNullOrEmpty(next) == false);

        return list;
    }

    /// <inheritdoc />
    public async Task<IModelSpec?> FindModelAsync(
        string modelId, 
        CancellationToken cancellationToken = default)
    {
        var model = await _client.GetModelAsync(modelId, cancellationToken);
        if (model is null)
            return null;

        return ConvertToModelSpec(model);
    }

    /// <summary> 
    /// 모델을 규격에 맞게 변환합니다. 
    /// </summary>
    private static IModelSpec ConvertToModelSpec(GoogleAIModel model)
    {
        // 모델 ID를 정규화합니다.
        static string NormalizeName(string modelId) =>
            modelId.StartsWith("models/") ? modelId[7..] : modelId;

        // LLM 채팅 모델인지 확인합니다.
        if (model.SupportedGenerationMethods.Contains("generateContent") &&
            model.SupportedGenerationMethods.Contains("countTokens"))
        {
            return new ChatModelSpec
            {
                ModelId = NormalizeName(model.Name),
                DisplayName = model.DisplayName,
                Description = model.Description,
                ContextWindow = model.InputTokenLimit,
                MaxOutputTokens = model.OutputTokenLimit,
            };
        }
        // 임베딩 모델인지 확인합니다.
        else if (model.SupportedGenerationMethods.Contains("embedContent") ||
                 model.SupportedGenerationMethods.Contains("embedText"))
        {
            return new EmbeddingModelSpec
            {
                ModelId = NormalizeName(model.Name),
                DisplayName = model.DisplayName,
                Description = model.Description,
                MaxInputTokens = model.InputTokenLimit,
            };
        }
        // 그 외의 모델은 일반 모델로 처리합니다.
        else
        {
            return new GenericModelSpec
            {
                ModelId = NormalizeName(model.Name),
                DisplayName = model.DisplayName,
                Description = model.Description,
            };
        }
    }
}
