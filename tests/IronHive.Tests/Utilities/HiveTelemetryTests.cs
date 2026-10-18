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
    public void A_failed_span_carries_the_error_type_and_status_but_no_exception_text_attributes()
    {
        using var activity = HiveTelemetry.StartAgentActivity("planner")!;

        activity.SetError(new InvalidOperationException("tool argument: secret-path"));

        activity.GetTagItem("error.type").Should().Be(typeof(InvalidOperationException).FullName);
        activity.Status.Should().Be(ActivityStatusCode.Error);
        activity.GetTagItem("exception.message").Should().BeNull();
        activity.GetTagItem("exception.stacktrace").Should().BeNull();
        activity.GetTagItem("exception.type").Should().BeNull();
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
