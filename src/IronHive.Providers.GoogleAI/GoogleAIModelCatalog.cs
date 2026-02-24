using Google.GenAI;
using IronHive.Abstractions.Catalog;

namespace IronHive.Providers.GoogleAI;

/// <inheritdoc />
public class GoogleAIModelCatalog : IModelCatalog
{
    private readonly Client _client;

    public GoogleAIModelCatalog(string apiKey)
        : this(new GoogleAIConfig { ApiKey = apiKey })
    { }

    public GoogleAIModelCatalog(GoogleAIConfig config)
    {
        _client = GoogleAIClientFactory.CreateClient(config);
    }

    public GoogleAIModelCatalog(VertexAIConfig config)
    {
        _client = GoogleAIClientFactory.CreateClient(config);
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
        var pager = await _client.Models.ListAsync(
            new Google.GenAI.Types.ListModelsConfig { PageSize = 1000 },
            cancellationToken);

        await foreach (var model in pager)
        {
            list.Add(ConvertToModelSpec(model));
        }

        return list;
    }

    /// <inheritdoc />
    public async Task<IModelSpec?> FindModelAsync(
        string modelId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var model = await _client.Models.GetAsync(modelId, cancellationToken: cancellationToken);
            return ConvertToModelSpec(model);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 모델을 규격에 맞게 변환합니다.
    /// </summary>
    private static IModelSpec ConvertToModelSpec(Google.GenAI.Types.Model model)
    {
        // 모델 ID를 정규화합니다.
        static string NormalizeName(string modelId) =>
            modelId.StartsWith("models/", StringComparison.Ordinal) ? modelId[7..] : modelId;

        var actions = model.SupportedActions ?? [];

        // LLM 채팅 모델인지 확인합니다.
        if (actions.Contains("generateContent") &&
            actions.Contains("countTokens"))
        {
            return new ChatModelSpec
            {
                ModelId = NormalizeName(model.Name ?? string.Empty),
                DisplayName = model.DisplayName,
                Description = model.Description,
                ContextWindow = model.InputTokenLimit,
                MaxOutputTokens = model.OutputTokenLimit,
            };
        }
        // 임베딩 모델인지 확인합니다.
        else if (actions.Contains("embedContent") ||
                 actions.Contains("embedText"))
        {
            return new EmbeddingModelSpec
            {
                ModelId = NormalizeName(model.Name ?? string.Empty),
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
                ModelId = NormalizeName(model.Name ?? string.Empty),
                DisplayName = model.DisplayName,
                Description = model.Description,
            };
        }
    }
}
