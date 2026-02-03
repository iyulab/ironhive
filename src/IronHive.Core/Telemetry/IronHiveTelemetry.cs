using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace IronHive.Core.Telemetry;

/// <summary>
/// IronHive 프레임워크의 OpenTelemetry 계측을 제공합니다.
/// GenAI Semantic Conventions을 따릅니다.
/// </summary>
public static class IronHiveTelemetry
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
        public const string GenAiSystem = "gen_ai.system";
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
        public const string GenAiToolCallId = "gen_ai.tool.call_id";
    }

    // 작업 이름 상수
    public static class Operations
    {
        public const string Chat = "chat";
        public const string Embedding = "embedding";
        public const string ToolCall = "tool_call";
        public const string AgentInvoke = "invoke_agent";
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
        string system,
        string model,
        string operationName,
        long inputTokens,
        long outputTokens)
    {
        var inputTags = new TagList
        {
            { Attributes.GenAiSystem, system },
            { Attributes.GenAiRequestModel, model },
            { Attributes.GenAiOperationName, operationName },
            { "token_type", "input" }
        };

        var outputTags = new TagList
        {
            { Attributes.GenAiSystem, system },
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
        string system,
        string model,
        string operationName,
        double durationSeconds,
        bool success)
    {
        var tags = new TagList
        {
            { Attributes.GenAiSystem, system },
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
        string system,
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
            activity.SetTag(Attributes.GenAiSystem, system);
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
    public static Activity? StartEmbeddingActivity(string system, string model, int inputCount)
    {
        var activity = ActivitySource.StartActivity(
            $"{Operations.Embedding} {model}",
            ActivityKind.Client);

        if (activity != null)
        {
            activity.SetTag(Attributes.GenAiSystem, system);
            activity.SetTag(Attributes.GenAiOperationName, Operations.Embedding);
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
            $"{Operations.ToolCall} {toolName}",
            ActivityKind.Internal);

        if (activity != null)
        {
            activity.SetTag(Attributes.GenAiOperationName, Operations.ToolCall);
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
    /// 에러 정보로 Activity를 업데이트합니다.
    /// </summary>
    public static void SetError(this Activity? activity, Exception exception)
    {
        if (activity == null) return;

        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        // OpenTelemetry Semantic Convention에 따라 예외 정보 기록
        activity.SetTag("exception.type", exception.GetType().FullName);
        activity.SetTag("exception.message", exception.Message);
        activity.SetTag("exception.stacktrace", exception.StackTrace);
    }
}
