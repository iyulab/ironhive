using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace IronHive.Core.Utilities;

/// <summary>
/// IronHive 프레임워크의 OpenTelemetry 계측을 제공합니다.
/// GenAI Semantic Conventions을 따릅니다.
/// </summary>
public static class HiveTelemetry
{
    /// <summary>
    /// 계측 소스 이름
    /// </summary>
    public const string SourceName = "IronHive";

    /// <summary>
    /// 버전
    /// </summary>
    public const string Version = "1.0.0";

    /// <summary>
    /// 추적을 위한 ActivitySource
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(SourceName, Version);

    /// <summary>
    /// 메트릭을 위한 Meter
    /// </summary>
    public static readonly Meter Meter = new(SourceName, Version);

    // GenAI Semantic Convention 속성 이름
    public static class Attributes
    {
        // 요청 속성
        /// <summary>The provider as the client sees it (<c>openai</c>, <c>anthropic</c>, …). Replaces the deprecated <c>gen_ai.system</c>.</summary>
        public const string GenAiProviderName = "gen_ai.provider.name";
        public const string GenAiRequestModel = "gen_ai.request.model";
        public const string GenAiRequestMaxTokens = "gen_ai.request.max_tokens";
        public const string GenAiRequestTemperature = "gen_ai.request.temperature";
        public const string GenAiRequestTopP = "gen_ai.request.top_p";
        public const string GenAiRequestStopSequences = "gen_ai.request.stop_sequences";

        // 응답 속성
        public const string GenAiResponseModel = "gen_ai.response.model";
        public const string GenAiResponseId = "gen_ai.response.id";
        public const string GenAiResponseFinishReasons = "gen_ai.response.finish_reasons";

        // 토큰 사용량
        public const string GenAiUsageInputTokens = "gen_ai.usage.input_tokens";
        public const string GenAiUsageOutputTokens = "gen_ai.usage.output_tokens";

        // 작업 유형
        public const string GenAiOperationName = "gen_ai.operation.name";

        // Agent 속성
        public const string GenAiAgentName = "gen_ai.agent.name";
        public const string GenAiAgentDescription = "gen_ai.agent.description";

        // Tool 속성
        public const string GenAiToolName = "gen_ai.tool.name";
        public const string GenAiToolCallId = "gen_ai.tool.call.id";

        // Orchestration 속성
        public const string OrchestrationName = "ironhive.orchestration.name";
        public const string OrchestrationId = "ironhive.orchestration.id";
        public const string OrchestrationStepIndex = "ironhive.orchestration.step_index";
        public const string OrchestrationPattern = "ironhive.orchestration.pattern";

        // 오류 속성
        /// <summary>The failure's class - the exception's full type name (<c>error.type</c>, OpenTelemetry general conventions).</summary>
        public const string ErrorType = "error.type";
    }

    // 작업 이름 상수
    public static class Operations
    {
        public const string Chat = "chat";
        public const string Embeddings = "embeddings";
        public const string ExecuteTool = "execute_tool";
        public const string AgentInvoke = "invoke_agent";
        /// <summary>An orchestration run (several agents as one workflow); its pattern and steps are the <c>ironhive.orchestration.*</c> attributes.</summary>
        public const string InvokeWorkflow = "invoke_workflow";
    }

    // 메트릭 정의
    private static readonly Counter<long> _tokenUsageCounter = Meter.CreateCounter<long>(
        "gen_ai.client.token.usage",
        "tokens",
        "Number of tokens used in GenAI operations");

    private static readonly Histogram<double> _operationDuration = Meter.CreateHistogram<double>(
        "gen_ai.client.operation.duration",
        "s",
        "Duration of GenAI operations");

    private static readonly Counter<long> _operationCounter = Meter.CreateCounter<long>(
        "gen_ai.client.operation.count",
        "operations",
        "Number of GenAI operations");

    /// <summary>
    /// 토큰 사용량을 기록합니다.
    /// </summary>
    public static void RecordTokenUsage(
        string providerName,
        string model,
        string operationName,
        long inputTokens,
        long outputTokens)
    {
        var inputTags = new TagList
        {
            { Attributes.GenAiProviderName, providerName },
            { Attributes.GenAiRequestModel, model },
            { Attributes.GenAiOperationName, operationName },
            { "token_type", "input" }
        };

        var outputTags = new TagList
        {
            { Attributes.GenAiProviderName, providerName },
            { Attributes.GenAiRequestModel, model },
            { Attributes.GenAiOperationName, operationName },
            { "token_type", "output" }
        };

        _tokenUsageCounter.Add(inputTokens, inputTags);
        _tokenUsageCounter.Add(outputTokens, outputTags);
    }

    /// <summary>
    /// 작업 지속 시간을 기록합니다.
    /// </summary>
    public static void RecordOperationDuration(
        string providerName,
        string model,
        string operationName,
        double durationSeconds,
        bool success)
    {
        var tags = new TagList
        {
            { Attributes.GenAiProviderName, providerName },
            { Attributes.GenAiRequestModel, model },
            { Attributes.GenAiOperationName, operationName },
            { "success", success }
        };

        _operationDuration.Record(durationSeconds, tags);
        _operationCounter.Add(1, tags);
    }

    /// <summary>
    /// 채팅 완료 작업을 위한 Activity를 시작합니다.
    /// </summary>
    public static Activity? StartChatActivity(
        string providerName,
        string model,
        int? maxTokens = null,
        float? temperature = null,
        float? topP = null)
    {
        var activity = ActivitySource.StartActivity(
            $"{Operations.Chat} {model}",
            ActivityKind.Client);

        if (activity != null)
        {
            activity.SetTag(Attributes.GenAiProviderName, providerName);
            activity.SetTag(Attributes.GenAiOperationName, Operations.Chat);
            activity.SetTag(Attributes.GenAiRequestModel, model);

            if (maxTokens.HasValue)
                activity.SetTag(Attributes.GenAiRequestMaxTokens, maxTokens.Value);
            if (temperature.HasValue)
                activity.SetTag(Attributes.GenAiRequestTemperature, temperature.Value);
            if (topP.HasValue)
                activity.SetTag(Attributes.GenAiRequestTopP, topP.Value);
        }

        return activity;
    }

    /// <summary>
    /// 임베딩 생성 작업을 위한 Activity를 시작합니다.
    /// </summary>
    public static Activity? StartEmbeddingActivity(string providerName, string model, int inputCount)
    {
        var activity = ActivitySource.StartActivity(
            $"{Operations.Embeddings} {model}",
            ActivityKind.Client);

        if (activity != null)
        {
            activity.SetTag(Attributes.GenAiProviderName, providerName);
            activity.SetTag(Attributes.GenAiOperationName, Operations.Embeddings);
            activity.SetTag(Attributes.GenAiRequestModel, model);
            activity.SetTag("gen_ai.embedding.input_count", inputCount);
        }

        return activity;
    }

    /// <summary>
    /// Tool 호출을 위한 Activity를 시작합니다.
    /// </summary>
    public static Activity? StartToolActivity(string toolName, string? callId = null)
    {
        var activity = ActivitySource.StartActivity(
            $"{Operations.ExecuteTool} {toolName}",
            ActivityKind.Internal);

        if (activity != null)
        {
            activity.SetTag(Attributes.GenAiOperationName, Operations.ExecuteTool);
            activity.SetTag(Attributes.GenAiToolName, toolName);
            if (callId != null)
                activity.SetTag(Attributes.GenAiToolCallId, callId);
        }

        return activity;
    }

    /// <summary>
    /// Agent 호출을 위한 Activity를 시작합니다.
    /// </summary>
    public static Activity? StartAgentActivity(string agentName, string? description = null)
    {
        var activity = ActivitySource.StartActivity(
            $"{Operations.AgentInvoke} {agentName}",
            ActivityKind.Internal);

        if (activity != null)
        {
            activity.SetTag(Attributes.GenAiOperationName, Operations.AgentInvoke);
            activity.SetTag(Attributes.GenAiAgentName, agentName);
            if (description != null)
                activity.SetTag(Attributes.GenAiAgentDescription, description);
        }

        return activity;
    }

    /// <summary>
    /// 오케스트레이션을 위한 Activity를 시작합니다.
    /// </summary>
    public static Activity? StartOrchestrationActivity(string name, string pattern, string? orchestrationId = null)
    {
        var activity = ActivitySource.StartActivity(
            $"{Operations.InvokeWorkflow} {name}",
            ActivityKind.Internal);

        if (activity != null)
        {
            activity.SetTag(Attributes.GenAiOperationName, Operations.InvokeWorkflow);
            activity.SetTag(Attributes.OrchestrationName, name);
            activity.SetTag(Attributes.OrchestrationPattern, pattern);
            if (orchestrationId != null)
                activity.SetTag(Attributes.OrchestrationId, orchestrationId);
        }

        return activity;
    }

    /// <summary>
    /// 응답 정보로 Activity를 업데이트합니다.
    /// </summary>
    public static void SetResponseInfo(
        this Activity? activity,
        string? responseId,
        string? model,
        string? finishReason,
        int? inputTokens,
        int? outputTokens)
    {
        if (activity == null) return;

        if (responseId != null)
            activity.SetTag(Attributes.GenAiResponseId, responseId);
        if (model != null)
            activity.SetTag(Attributes.GenAiResponseModel, model);
        if (finishReason != null)
            activity.SetTag(Attributes.GenAiResponseFinishReasons, finishReason);
        if (inputTokens.HasValue)
            activity.SetTag(Attributes.GenAiUsageInputTokens, inputTokens.Value);
        if (outputTokens.HasValue)
            activity.SetTag(Attributes.GenAiUsageOutputTokens, outputTokens.Value);
    }

    /// <summary>
    /// 에러 정보로 Activity를 업데이트합니다 — <c>error.type</c>(예외 형식 이름)과 Error 상태.
    /// </summary>
    /// <remarks>
    /// Microsoft.Extensions.AI 의 GenAI span 과 같은 형태다(<c>error.type</c> + 상태 설명 = 예외 메시지). 예외 메시지·스택
    /// 트레이스를 span 속성으로 따로 싣지 않는다 — 스택 트레이스는 span 마다 수 KB 를 더하고, 메시지는 상태 설명에
    /// 이미 있다. 상태 설명까지 빼야 하는 호스트는 span 처리기 한 곳에서 모든 계층(이 span 과 M.E.AI span)을 함께 거른다.
    /// </remarks>
    public static void SetError(this Activity? activity, Exception exception)
    {
        if (activity == null) return;

        activity.SetTag(Attributes.ErrorType, exception.GetType().FullName);
        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
    }
}
