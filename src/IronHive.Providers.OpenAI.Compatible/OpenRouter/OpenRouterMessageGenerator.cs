using System.Text.Json;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;

namespace IronHive.Providers.OpenAI.Compatible.OpenRouter;

/// <summary>
/// OpenRouter 서비스를 위한 메시지 생성기입니다.
/// </summary>
internal class OpenRouterMessageGenerator : CompatibleChatMessageGenerator
{
    private readonly OpenRouterConfig _openRouterConfig;

    public OpenRouterMessageGenerator(OpenRouterConfig config) : base(config)
    {
        _openRouterConfig = config;
    }

    protected override T PostProcessRequest<T>(ChatCompletionRequest request)
    {
        request.AdditionalProperties ??= [];

        // transforms 파라미터 주입
        if (_openRouterConfig.Transforms is { Count: > 0 })
            request.AdditionalProperties["transforms"] =
                JsonSerializer.SerializeToElement(_openRouterConfig.Transforms);

        // route 파라미터 주입
        if (!string.IsNullOrEmpty(_openRouterConfig.Route))
            request.AdditionalProperties["route"] =
                JsonSerializer.SerializeToElement(_openRouterConfig.Route);

        // provider preferences 주입
        if (_openRouterConfig.ProviderPreferences != null)
        {
            var prefs = _openRouterConfig.ProviderPreferences;
            var providerObj = new Dictionary<string, object?>();

            if (prefs.AllowFallbacks.HasValue)
                providerObj["allow_fallbacks"] = prefs.AllowFallbacks.Value;
            if (prefs.RequireParameters.HasValue)
                providerObj["require_parameters"] = prefs.RequireParameters.Value;
            if (prefs.Order is { Count: > 0 })
                providerObj["order"] = prefs.Order;
            if (prefs.Ignore is { Count: > 0 })
                providerObj["ignore"] = prefs.Ignore;
            if (prefs.Quantizations is { Count: > 0 })
                providerObj["quantizations"] = prefs.Quantizations;
            if (!string.IsNullOrEmpty(prefs.DataCollection))
                providerObj["data_collection"] = prefs.DataCollection;
            if (!string.IsNullOrEmpty(prefs.Sort))
                providerObj["sort"] = prefs.Sort;

            if (providerObj.Count > 0)
                request.AdditionalProperties["provider"] =
                    JsonSerializer.SerializeToElement(providerObj);
        }

        return (T)request;
    }
}
