namespace IronHive.Providers.OpenAI;

/// <summary>
/// 모델별 기능 플래그. 요청 빌더는 이 플래그만 참조하여 조건 분기를 수행합니다.
/// </summary>
internal record ModelCapabilities
{
    /// <summary>Responses API 사용 여부 (false이면 Chat Completion)</summary>
    public bool UseResponsesApi { get; init; }

    /// <summary>Responses API: instructions 파라미터 지원 여부</summary>
    public bool SupportsInstructions { get; init; } = true;

    /// <summary>Responses API: system prompt를 input 메시지로 삽입해야 하는지</summary>
    public bool SystemPromptAsMessage { get; init; }

    /// <summary>Chat Completion: developer message 사용 여부 (false이면 system message)</summary>
    public bool UseDeveloperMessage { get; init; }

    /// <summary>stop 파라미터 지원 여부</summary>
    public bool SupportsStop { get; init; } = true;

    /// <summary>temperature 파라미터 지원 여부</summary>
    public bool SupportsTemperature { get; init; } = true;

    /// <summary>top_p 파라미터 지원 여부</summary>
    public bool SupportsTopP { get; init; } = true;

    /// <summary>reasoning_effort 파라미터 지원 여부</summary>
    public bool SupportsReasoningEffort { get; init; } = true;

    /// <summary>Chat Completion: reasoning 지원 모델 여부 (developer message, temp/topP 비활성 등)</summary>
    public bool SupportsReasoning { get; init; }
}

/// <summary>
/// 모델명과 벤더 정보를 기반으로 <see cref="ModelCapabilities"/>를 결정합니다.
/// 모든 모델명 검사 로직이 이 클래스에만 존재합니다.
/// </summary>
internal static class ModelCapabilityResolver
{
    public static ModelCapabilities Resolve(string model, OpenAICompatibility compatibility)
    {
        var m = model.ToLowerInvariant();

        return compatibility switch
        {
            OpenAICompatibility.XAI => ResolveXAI(m),
            OpenAICompatibility.Azure => ResolveDefault(m),
            _ => ResolveDefault(m),
        };
    }

    private static ModelCapabilities ResolveDefault(string m)
    {
        // o-series reasoning models (o1, o3, o4 등)
        if (m.StartsWith("o1") || m.StartsWith("o3") || m.StartsWith("o4"))
        {
            return new ModelCapabilities
            {
                UseResponsesApi = true,
                SupportsInstructions = true,
                SystemPromptAsMessage = false,
                UseDeveloperMessage = true,
                SupportsStop = false,
                SupportsTemperature = false,
                SupportsTopP = false,
                SupportsReasoningEffort = true,
                SupportsReasoning = true,
            };
        }

        // gpt-5 이상
        if (m.StartsWith("gpt-5"))
        {
            return new ModelCapabilities
            {
                UseResponsesApi = true,
                SupportsInstructions = true,
                SystemPromptAsMessage = false,
                UseDeveloperMessage = true,
                SupportsStop = true,
                SupportsTemperature = true,
                SupportsTopP = true,
                SupportsReasoningEffort = true,
                SupportsReasoning = true,
            };
        }

        // Chat Completion: reasoning 지원 모델 (contains -o1, -o3 패턴)
        if (m.Contains("-o1") || m.Contains("-o3"))
        {
            return new ModelCapabilities
            {
                UseResponsesApi = false,
                SupportsInstructions = true,
                SystemPromptAsMessage = false,
                UseDeveloperMessage = false,
                SupportsStop = true,
                SupportsTemperature = true,
                SupportsTopP = true,
                SupportsReasoningEffort = true,
                SupportsReasoning = true,
            };
        }

        // 기타 (gpt-4o, gpt-4o-mini 등 표준 모델)
        return new ModelCapabilities
        {
            UseResponsesApi = false,
            SupportsInstructions = true,
            SystemPromptAsMessage = false,
            UseDeveloperMessage = false,
            SupportsStop = true,
            SupportsTemperature = true,
            SupportsTopP = true,
            SupportsReasoningEffort = true,
            SupportsReasoning = false,
        };
    }

    private static ModelCapabilities ResolveXAI(string m)
    {
        // xAI 공통: Chat Completion 사용, instructions 미지원, system을 메시지로
        var xaiBase = new ModelCapabilities
        {
            UseResponsesApi = false,
            SupportsInstructions = false,
            SystemPromptAsMessage = true,
            UseDeveloperMessage = false,
            SupportsStop = true,
            SupportsTemperature = true,
            SupportsTopP = true,
            SupportsReasoningEffort = true,
            SupportsReasoning = false,
        };

        // grok-4: reasoning_effort/stop/temp/topP 미지원
        if (m.StartsWith("grok-4"))
        {
            return xaiBase with
            {
                SupportsStop = false,
                SupportsTemperature = false,
                SupportsTopP = false,
                SupportsReasoningEffort = false,
            };
        }

        // grok-3-mini, grok-3, 기타 grok 모델: 기본값 유지
        return xaiBase;
    }
}
