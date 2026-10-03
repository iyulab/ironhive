using System.Diagnostics;
using AwesomeAssertions;
using IronHive.Core.Utilities;

namespace IronHive.Tests.Utilities;

/// <summary>
/// The span names and attributes IronHive emits follow the OpenTelemetry GenAI semantic conventions, so a backend that
/// knows them (dashboards keyed on <c>gen_ai.operation.name</c>, provider breakdowns) reads IronHive spans without a
/// mapping. Pinned here because nothing else observes them.
/// </summary>
public sealed class HiveTelemetryTests : IDisposable
{
    private readonly ActivityListener _listener = new()
    {
        ShouldListenTo = source => source.Name == HiveTelemetry.SourceName,
        Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
    };

    public HiveTelemetryTests() => ActivitySource.AddActivityListener(_listener);

    public void Dispose() => _listener.Dispose();

    [Fact]
    public void A_chat_span_names_the_provider_with_gen_ai_provider_name()
    {
        using var activity = HiveTelemetry.StartChatActivity("openai", "gpt-5")!;

        activity.DisplayName.Should().Be("chat gpt-5");
        activity.GetTagItem("gen_ai.operation.name").Should().Be("chat");
        activity.GetTagItem("gen_ai.provider.name").Should().Be("openai");
        activity.GetTagItem("gen_ai.system").Should().BeNull();
    }

    [Fact]
    public void An_embeddings_span_uses_the_embeddings_operation()
    {
        using var activity = HiveTelemetry.StartEmbeddingActivity("openai", "text-embedding-3-small", 2)!;

        activity.DisplayName.Should().Be("embeddings text-embedding-3-small");
        activity.GetTagItem("gen_ai.operation.name").Should().Be("embeddings");
        activity.GetTagItem("gen_ai.provider.name").Should().Be("openai");
    }

    [Fact]
    public void A_tool_span_is_execute_tool_with_the_call_id()
    {
        using var activity = HiveTelemetry.StartToolActivity("read_file", "call_1")!;

        activity.DisplayName.Should().Be("execute_tool read_file");
        activity.GetTagItem("gen_ai.operation.name").Should().Be("execute_tool");
        activity.GetTagItem("gen_ai.tool.name").Should().Be("read_file");
        activity.GetTagItem("gen_ai.tool.call.id").Should().Be("call_1");
    }

    [Fact]
    public void An_orchestration_span_is_invoke_workflow_and_keeps_its_pattern()
    {
        using var activity = HiveTelemetry.StartOrchestrationActivity("review", "sequential", "run-1")!;

        activity.DisplayName.Should().Be("invoke_workflow review");
        activity.GetTagItem("gen_ai.operation.name").Should().Be("invoke_workflow");
        activity.GetTagItem("ironhive.orchestration.pattern").Should().Be("sequential");
    }

    [Fact]
    public void An_agent_span_is_invoke_agent()
    {
        using var activity = HiveTelemetry.StartAgentActivity("writer")!;

        activity.DisplayName.Should().Be("invoke_agent writer");
        activity.GetTagItem("gen_ai.operation.name").Should().Be("invoke_agent");
        activity.GetTagItem("gen_ai.agent.name").Should().Be("writer");
    }
}
