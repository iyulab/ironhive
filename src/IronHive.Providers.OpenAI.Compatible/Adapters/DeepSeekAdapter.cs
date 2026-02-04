using System.Text.Json.Nodes;

namespace IronHive.Providers.OpenAI.Compatible.Adapters;

/// <summary>
/// DeepSeek 제공자 어댑터입니다.
/// </summary>
/// <remarks>
/// DeepSeek 특수 기능:
/// - /beta URL prefix로 prefix completion 지원
/// - reasoning_content 별도 필드 (CoT)
/// - assistant message에 prefix: true 설정 가능
/// - thinking 모드에서 temperature, top_p 등 무시됨
/// </remarks>
public class DeepSeekAdapter : BaseProviderAdapter
{
    /// <inheritdoc />
    public override CompatibleProvider Provider => CompatibleProvider.DeepSeek;

    /// <inheritdoc />
    protected override string DefaultBaseUrl => "https://api.deepseek.com/v1";

    /// <inheritdoc />
    public override string GetBaseUrl(CompatibleConfig config)
    {
        if (config is DeepSeekConfig deepSeekConfig && deepSeekConfig.UseBetaApi)
        {
            return !string.IsNullOrEmpty(config.BaseUrl)
                ? config.BaseUrl.Replace("/v1", "/beta")
                : "https://api.deepseek.com/beta";
        }

        return base.GetBaseUrl(config);
    }

    /// <inheritdoc />
    public override JsonObject TransformRequest(JsonObject request, CompatibleConfig config)
    {
        if (config is DeepSeekConfig deepSeekConfig)
        {
            // Thinking 모드 활성화
            if (deepSeekConfig.EnableThinking)
            {
                // thinking 파라미터는 extra_body로 전달해야 함
                // 여기서는 직접 추가
                var thinking = new JsonObject
                {
                    ["type"] = "enabled"
                };
                request["thinking"] = thinking;

                // Thinking 모드에서는 이 파라미터들이 무시됨
                RemoveProperties(request,
                    "temperature",
                    "top_p",
                    "presence_penalty",
                    "frequency_penalty"
                );
            }

            // Prefix completion 설정
            if (deepSeekConfig.UseBetaApi && request["messages"] is JsonArray messages)
            {
                // 마지막 assistant 메시지에 prefix: true 설정이 필요한 경우
                // (사용자가 직접 설정해야 함 - 여기서는 안내만)
            }
        }

        return base.TransformRequest(request, config);
    }

    /// <inheritdoc />
    public override JsonObject TransformResponse(JsonObject response)
    {
        // DeepSeek의 reasoning_content를 OpenAI 형식으로 변환
        // reasoning_content는 content와 동일 레벨에 있음
        if (response["choices"] is JsonArray choices)
        {
            foreach (var choice in choices)
            {
                if (choice is JsonObject choiceObj && choiceObj["message"] is JsonObject message)
                {
                    // reasoning_content가 있으면 처리
                    if (message["reasoning_content"] is JsonNode reasoningContent)
                    {
                        // 필요시 별도 처리 (현재는 그대로 유지)
                        // OpenAI 형식에서는 이를 별도로 처리할 수 있음
                    }
                }
            }
        }

        return response;
    }

    /// <inheritdoc />
    public override JsonObject TransformStreamingChunk(JsonObject chunk)
    {
        // 스트리밍에서도 reasoning_content 처리
        if (chunk["choices"] is JsonArray choices)
        {
            foreach (var choice in choices)
            {
                if (choice is JsonObject choiceObj && choiceObj["delta"] is JsonObject delta)
                {
                    // reasoning_content delta가 있으면 처리
                    if (delta["reasoning_content"] is JsonNode reasoningDelta)
                    {
                        // 필요시 별도 처리
                    }
                }
            }
        }

        return chunk;
    }
}

/// <summary>
/// DeepSeek 특수 설정입니다.
/// </summary>
public class DeepSeekConfig : CompatibleConfig
{
    public DeepSeekConfig()
    {
        Provider = CompatibleProvider.DeepSeek;
    }

    /// <summary>
    /// Thinking 모드를 활성화합니다. (deepseek-reasoner 모델용)
    /// 활성화 시 temperature, top_p 등의 샘플링 파라미터는 무시됩니다.
    /// </summary>
    public bool EnableThinking { get; set; }

    /// <summary>
    /// Beta API를 사용합니다. (/beta endpoint)
    /// Prefix completion 등 베타 기능을 사용할 때 필요합니다.
    /// </summary>
    public bool UseBetaApi { get; set; }
}
