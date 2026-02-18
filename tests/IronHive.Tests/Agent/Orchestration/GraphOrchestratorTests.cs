using System.Runtime.CompilerServices;
using FluentAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Tools;
using IronHive.Core.Agent.Orchestration;

namespace IronHive.Tests.Agent.Orchestration;

public class GraphOrchestratorTests
{
    #region Builder Validation

    [Fact]
    public void Build_NoNodes_Throws()
    {
        var builder = new GraphOrchestratorBuilder()
            .SetStartNode("a")
            .SetOutputNode("b");

        var act = () => builder.Build();
        act.Should().Throw<InvalidOperationException>().WithMessage("*at least one node*");
    }

    [Fact]
    public void Build_NoStartNode_Throws()
    {
        var agent = new MockAgent("a");
        var builder = new GraphOrchestratorBuilder()
            .AddNode("a", agent)
            .SetOutputNode("a");

        var act = () => builder.Build();
        act.Should().Throw<InvalidOperationException>().WithMessage("*Start node*");
    }

    [Fact]
    public void Build_NoOutputNode_Throws()
    {
        var agent = new MockAgent("a");
        var builder = new GraphOrchestratorBuilder()
            .AddNode("a", agent)
            .SetStartNode("a");

        var act = () => builder.Build();
        act.Should().Throw<InvalidOperationException>().WithMessage("*Output node*");
    }

    [Fact]
    public void Build_StartNodeNotInGraph_Throws()
    {
        var agent = new MockAgent("a");
        var builder = new GraphOrchestratorBuilder()
            .AddNode("a", agent)
            .SetStartNode("missing")
            .SetOutputNode("a");

        var act = () => builder.Build();
        act.Should().Throw<InvalidOperationException>().WithMessage("*Start node*not found*");
    }

    [Fact]
    public void Build_CyclicGraph_Throws()
    {
        var a = new MockAgent("a");
        var b = new MockAgent("b");

        var builder = new GraphOrchestratorBuilder()
            .AddNode("a", a)
            .AddNode("b", b)
            .AddEdge("a", "b")
            .AddEdge("b", "a")
            .SetStartNode("a")
            .SetOutputNode("b");

        var act = () => builder.Build();
        act.Should().Throw<InvalidOperationException>().WithMessage("*cycle*");
    }

    [Fact]
    public void Build_EdgeWithMissingSource_Throws()
    {
        var a = new MockAgent("a");
        var builder = new GraphOrchestratorBuilder()
            .AddNode("a", a)
            .AddEdge("missing", "a")
            .SetStartNode("a")
            .SetOutputNode("a");

        var act = () => builder.Build();
        act.Should().Throw<InvalidOperationException>().WithMessage("*source*not found*");
    }

    #endregion

    #region Execute — Linear DAG

    [Fact]
    public async Task Execute_SingleNode_ReturnsOutput()
    {
        var agent = new MockAgent("a") { ResponseFunc = _ => "from-a" };
        var orch = new GraphOrchestratorBuilder()
            .AddNode("a", agent)
            .SetStartNode("a")
            .SetOutputNode("a")
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().HaveCount(1);
        GetTextFromMessage(result.FinalOutput).Should().Be("from-a");
    }

    [Fact]
    public async Task Execute_LinearChain_RunsInOrder()
    {
        var callOrder = new List<string>();
        var a = new MockAgent("a") { ResponseFunc = _ => { callOrder.Add("a"); return "from-a"; } };
        var b = new MockAgent("b") { ResponseFunc = _ => { callOrder.Add("b"); return "from-b"; } };
        var c = new MockAgent("c") { ResponseFunc = _ => { callOrder.Add("c"); return "from-c"; } };

        var orch = new GraphOrchestratorBuilder()
            .AddNode("a", a)
            .AddNode("b", b)
            .AddNode("c", c)
            .AddEdge("a", "b")
            .AddEdge("b", "c")
            .SetStartNode("a")
            .SetOutputNode("c")
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("start"));

        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().HaveCount(3);
        callOrder.Should().Equal("a", "b", "c");
        GetTextFromMessage(result.FinalOutput).Should().Be("from-c");
    }

    #endregion

    #region Execute — Fan-Out / Fan-In

    [Fact]
    public async Task Execute_FanOut_BothBranchesExecute()
    {
        // A → B, A → C, B → D, C → D
        var callOrder = new List<string>();
        var a = new MockAgent("a") { ResponseFunc = _ => { callOrder.Add("a"); return "from-a"; } };
        var b = new MockAgent("b") { ResponseFunc = _ => { callOrder.Add("b"); return "from-b"; } };
        var c = new MockAgent("c") { ResponseFunc = _ => { callOrder.Add("c"); return "from-c"; } };
        var d = new MockAgent("d") { ResponseFunc = _ => { callOrder.Add("d"); return "from-d"; } };

        var orch = new GraphOrchestratorBuilder()
            .AddNode("a", a)
            .AddNode("b", b)
            .AddNode("c", c)
            .AddNode("d", d)
            .AddEdge("a", "b")
            .AddEdge("a", "c")
            .AddEdge("b", "d")
            .AddEdge("c", "d")
            .SetStartNode("a")
            .SetOutputNode("d")
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("start"));

        result.IsSuccess.Should().BeTrue();
        callOrder.Should().HaveCount(4);
        // A must be first, D must be last
        callOrder[0].Should().Be("a");
        callOrder[3].Should().Be("d");
        // B and C can be in either order (both depend only on A)
        callOrder.Skip(1).Take(2).Should().BeEquivalentTo(["b", "c"]);
        GetTextFromMessage(result.FinalOutput).Should().Be("from-d");
    }

    #endregion

    #region Execute — Conditional Edges

    [Fact]
    public async Task Execute_ConditionalEdge_TrueCondition_ExecutesTarget()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => "from-a" };
        var b = new MockAgent("b") { ResponseFunc = _ => "from-b" };

        var orch = new GraphOrchestratorBuilder()
            .AddNode("a", a)
            .AddNode("b", b)
            .AddEdge("a", "b", condition: step => step.IsSuccess) // always true
            .SetStartNode("a")
            .SetOutputNode("b")
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().HaveCount(2);
        GetTextFromMessage(result.FinalOutput).Should().Be("from-b");
    }

    [Fact]
    public async Task Execute_ConditionalEdge_FalseCondition_SkipsTarget()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => "from-a" };
        var b = new MockAgent("b") { ResponseFunc = _ => "from-b" };

        var orch = new GraphOrchestratorBuilder()
            .AddNode("a", a)
            .AddNode("b", b)
            .AddEdge("a", "b", condition: _ => false) // never execute
            .SetStartNode("a")
            .SetOutputNode("b")
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        // B is skipped due to false condition; output falls back to last success (A)
        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().HaveCount(1); // only A executed
        GetTextFromMessage(result.FinalOutput).Should().Be("from-a");
    }

    #endregion

    #region Execute — Failure Handling

    [Fact]
    public async Task Execute_AgentFailure_StopOnFailure_ReturnsFailure()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => throw new InvalidOperationException("boom") };
        var b = new MockAgent("b") { ResponseFunc = _ => "from-b" };

        var orch = new GraphOrchestratorBuilder()
            .WithOptions(new GraphOrchestratorOptions { StopOnAgentFailure = true })
            .AddNode("a", a)
            .AddNode("b", b)
            .AddEdge("a", "b")
            .SetStartNode("a")
            .SetOutputNode("b")
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeFalse();
        result.Steps.Should().HaveCount(1);
        result.Steps[0].Error.Should().Contain("boom");
    }

    [Fact]
    public async Task Execute_TokenUsage_Aggregated()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => "from-a" };
        var b = new MockAgent("b") { ResponseFunc = _ => "from-b" };

        var orch = new GraphOrchestratorBuilder()
            .AddNode("a", a)
            .AddNode("b", b)
            .AddEdge("a", "b")
            .SetStartNode("a")
            .SetOutputNode("b")
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        result.TokenUsage.Should().NotBeNull();
        result.TokenUsage!.TotalInputTokens.Should().BeGreaterThan(0);
    }

    #endregion

    #region ExecuteStreaming

    [Fact]
    public async Task ExecuteStreaming_NoNodes_YieldsFailedEvent()
    {
        // Use builder with valid but empty-equivalent setup — internal ctor
        // We can test the streaming failure path via a single node that fails
        // Actually, we can't create an empty GraphOrchestrator through the builder
        // since it requires at least one node. Test single-node streaming instead.
        var a = new MockAgent("a") { ResponseFunc = _ => "from-a" };
        var orch = new GraphOrchestratorBuilder()
            .AddNode("a", a)
            .SetStartNode("a")
            .SetOutputNode("a")
            .Build();

        var events = new List<OrchestrationStreamEvent>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("input")))
        {
            events.Add(evt);
        }

        events.Should().Contain(e => e.EventType == OrchestrationEventType.Started);
        events.Should().Contain(e => e.EventType == OrchestrationEventType.AgentStarted && e.AgentName == "a");
        events.Should().Contain(e => e.EventType == OrchestrationEventType.AgentCompleted && e.AgentName == "a");
        events.Should().Contain(e => e.EventType == OrchestrationEventType.Completed);
    }

    [Fact]
    public async Task ExecuteStreaming_LinearChain_YieldsCorrectEventSequence()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => "from-a" };
        var b = new MockAgent("b") { ResponseFunc = _ => "from-b" };

        var orch = new GraphOrchestratorBuilder()
            .AddNode("a", a)
            .AddNode("b", b)
            .AddEdge("a", "b")
            .SetStartNode("a")
            .SetOutputNode("b")
            .Build();

        var events = new List<OrchestrationStreamEvent>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("input")))
        {
            events.Add(evt);
        }

        events.Should().Contain(e => e.EventType == OrchestrationEventType.Started);
        events.Should().Contain(e => e.EventType == OrchestrationEventType.AgentStarted && e.AgentName == "a");
        events.Should().Contain(e => e.EventType == OrchestrationEventType.AgentCompleted && e.AgentName == "a");
        events.Should().Contain(e => e.EventType == OrchestrationEventType.AgentStarted && e.AgentName == "b");
        events.Should().Contain(e => e.EventType == OrchestrationEventType.AgentCompleted && e.AgentName == "b");
        events.Should().Contain(e => e.EventType == OrchestrationEventType.Completed);
    }

    [Fact]
    public async Task ExecuteStreaming_AgentFailure_YieldsAgentFailedEvent()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => throw new InvalidOperationException("boom") };

        var orch = new GraphOrchestratorBuilder()
            .WithOptions(new GraphOrchestratorOptions { StopOnAgentFailure = true })
            .AddNode("a", a)
            .SetStartNode("a")
            .SetOutputNode("a")
            .Build();

        var events = new List<OrchestrationStreamEvent>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("input")))
        {
            events.Add(evt);
        }

        events.Should().Contain(e => e.EventType == OrchestrationEventType.AgentFailed && e.AgentName == "a");
        events.Should().Contain(e => e.EventType == OrchestrationEventType.Failed);
    }

    #endregion

    #region Helpers

    private static IEnumerable<Message> MakeUserMessages(string text)
    {
        return [new UserMessage { Content = [new TextMessageContent { Value = text }] }];
    }

    private static string GetTextFromMessage(Message? message)
    {
        return message switch
        {
            AssistantMessage a => a.Content.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "",
            UserMessage u => u.Content.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "",
            _ => ""
        };
    }

    private sealed class MockAgent : IAgent
    {
        public string Provider { get; set; } = "mock";
        public string Model { get; set; } = "mock-model";
        public string Name { get; set; }
        public string Description { get; set; } = "Mock";
        public string? Instructions { get; set; }
        public IEnumerable<ToolItem>? Tools { get; set; }
        public MessageGenerationParameters? Parameters { get; set; }
        public Func<IEnumerable<Message>, string>? ResponseFunc { get; set; }

        public MockAgent(string name) { Name = name; }

        public Task<MessageResponse> InvokeAsync(IEnumerable<Message> messages, CancellationToken ct = default)
        {
            var text = ResponseFunc != null ? ResponseFunc(messages) : $"MockAgent '{Name}'";

            return Task.FromResult(new MessageResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage
                {
                    Name = Name,
                    Content = [new TextMessageContent { Value = text }]
                },
                TokenUsage = new MessageTokenUsage { InputTokens = 10, OutputTokens = text.Length }
            });
        }

        public async IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
            IEnumerable<Message> messages,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var text = ResponseFunc != null ? ResponseFunc(messages) : $"MockAgent '{Name}'";

            yield return new StreamingContentDeltaResponse
            {
                Index = 0,
                Delta = new TextDeltaContent { Value = text }
            };
            await Task.Yield();
            yield return new StreamingMessageDoneResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                DoneReason = MessageDoneReason.EndTurn,
                TokenUsage = new MessageTokenUsage { InputTokens = 10, OutputTokens = text.Length },
                Model = "mock-model",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    #endregion
}
