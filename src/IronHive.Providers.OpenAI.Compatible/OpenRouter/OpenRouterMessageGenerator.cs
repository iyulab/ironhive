using System.Text.Json;
using System.Text.Json.Nodes;
using IronHive.Abstractions.Messages;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;
using IronHive.Providers.OpenAI.Payloads.Responses;

namespace IronHive.Providers.OpenAI.Compatible.OpenRouter;

/// <summary>
/// OpenRouter 서비스를 위한 메시지 생성기입니다.
/// </summary>
public class OpenRouterMessageGenerator : OpenAIResponseMessageGenerator
{
    private readonly OpenRouterConfig _config;

    public OpenRouterMessageGenerator(OpenRouterConfig config) : base(config.ToOpenAI())
    {
        _config = config;
    }

    protected override ResponsesRequest OnBeforeGenerate(
        MessageGenerationRequest source,
        ResponsesRequest request)
    {
        // 미지원 필드 초기화
        request.Background = null;
        request.Conversation = null;
        request.TopLogProbs = null;

        request.ExtraBody ??= new JsonObject();

        // provider preferences 주입
        if (_config.ProviderPreferences != null)
        {
            var prefs = _config.ProviderPreferences;
            var providerObj = new JsonObject();

            if (prefs.AllowFallbacks.HasValue)
                providerObj["allow_fallbacks"] = prefs.AllowFallbacks.Value;
            if (prefs.RequireParameters.HasValue)
                providerObj["require_parameters"] = prefs.RequireParameters.Value;
            if (prefs.Order is { Count: > 0 })
                providerObj["order"] = JsonSerializer.SerializeToNode(prefs.Order);
            if (prefs.Ignore is { Count: > 0 })
                providerObj["ignore"] = JsonSerializer.SerializeToNode(prefs.Ignore);
            if (prefs.Quantizations is { Count: > 0 })
                providerObj["quantizations"] = JsonSerializer.SerializeToNode(prefs.Quantizations);
            if (!string.IsNullOrEmpty(prefs.DataCollection))
                providerObj["data_collection"] = prefs.DataCollection;
            if (!string.IsNullOrEmpty(prefs.Sort))
                providerObj["sort"] = prefs.Sort;

            if (providerObj.Count > 0)
                request.ExtraBody["provider"] = providerObj;
        }

        return request;
    }
}
