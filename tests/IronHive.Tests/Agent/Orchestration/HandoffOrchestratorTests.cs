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

public class HandoffOrchestratorTests
{
    [Fact]
    public async Task Handoff_BasicFlow_TriageToSpecialistAndBack()
    {
        // Arrange
        var callSequence = new List<string>();

        var triage = new MockAgent("triage")
        {
            ResponseFunc = msgs =>
            {
                callSequence.Add("triage");
                if (callSequence.Count == 1)
                    return """{"handoff_to": "billing", "context": "Customer has billing issue"}""";
                return "Glad I could help!";
            }
        };

        var billing = new MockAgent("billing")
        {
            ResponseFunc = msgs =>
            {
                callSequence.Add("billing");
                return """{"handoff_to": "triage", "context": "Billing resolved"}""";
            }
        };

        var orch = new HandoffOrchestratorBuilder()
            .AddAgent(triage,
                new HandoffTarget { AgentName = "billing", Description = "Billing issues" })
            .AddAgent(billing,
                new HandoffTarget { AgentName = "triage", Description = "Back to triage" })
            .SetInitialAgent("triage")
            .Build();

        // Act
        var result = await orch.ExecuteAsync(MakeUserMessages("I have a billing problem"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        callSequence.Should().Equal("triage", "billing", "triage");
        result.Steps.Should().HaveCount(3);
        GetTextFromMessage(result.FinalOutput).Should().Contain("Glad I could help");
    }

    [Fact]
    public async Task Handoff_MaxTransitions_ShouldStopAtLimit()
    {
        // Arrange: agents that always handoff to each other
        var a = new MockAgent("a")
        {
            ResponseFunc = _ => """{"handoff_to": "b"}"""
        };
        var b = new MockAgent("b")
        {
            ResponseFunc = _ => """{"handoff_to": "a"}"""
        };

        var orch = new HandoffOrchestratorBuilder()
            .AddAgent(a, new HandoffTarget { AgentName = "b" })
            .AddAgent(b, new HandoffTarget { AgentName = "a" })
            .SetInitialAgent("a")
            .SetMaxTransitions(3)
            .Build();

        // Act
        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        // Assert: should stop after MaxTransitions + 1 total steps (initial + transitions)
        result.IsSuccess.Should().BeTrue();
        result.Steps.Count.Should().BeLessThanOrEqualTo(5);
    }

    [Fact]
    public void Builder_MissingInitialAgent_ShouldThrow()
    {
        var agent = new MockAgent("a");

        var act = () => new HandoffOrchestratorBuilder()
            .AddAgent(agent)
            .Build();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Initial agent*");
    }

    [Fact]
    public void Builder_InvalidHandoffTarget_ShouldThrow()
    {
        var agent = new MockAgent("a");

        var act = () => new HandoffOrchestratorBuilder()
            .AddAgent(agent, new HandoffTarget { AgentName = "nonexistent" })
            .SetInitialAgent("a")
            .Build();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*nonexistent*not a registered agent*");
    }

    [Fact]
    public void Builder_InitialAgentNotRegistered_ShouldThrow()
    {
        var agent = new MockAgent("a");

        var act = () => new HandoffOrchestratorBuilder()
            .AddAgent(agent)
            .SetInitialAgent("b")
            .Build();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Initial agent*not registered*");
    }

    [Fact]
    public async Task Handoff_NoHandoffHandler_ShouldBeCalled()
    {
        var handlerCalled = false;

        var agent = new MockAgent("a")
        {
            ResponseFunc = _ => "No handoff here"
        };

        var orch = new HandoffOrchestratorBuilder()
            .AddAgent(agent)
            .SetInitialAgent("a")
            .SetNoHandoffHandler((agentName, step) =>
            {
                handlerCalled = true;
                return Task.FromResult<Message?>(null); // terminate
            })
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("hello"));

        result.IsSuccess.Should().BeTrue();
        handlerCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handoff_Streaming_ShouldIncludeHandoffEvents()
    {
        var triage = new MockAgent("triage")
        {
            ResponseFunc = _ => """{"handoff_to": "specialist"}"""
        };
        var specialist = new MockAgent("specialist")
        {
            ResponseFunc = _ => "Done"
        };

        var orch = new HandoffOrchestratorBuilder()
            .AddAgent(triage, new HandoffTarget { AgentName = "specialist" })
            .AddAgent(specialist, new HandoffTarget { AgentName = "triage" })
            .SetInitialAgent("triage")
            .Build();

        var events = new List<OrchestrationStreamEvent>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("go")))
        {
            events.Add(evt);
        }

        events.Should().Contain(e => e.EventType == OrchestrationEventType.Started);
        events.Should().Contain(e => e.EventType == OrchestrationEventType.Handoff);
        events.Should().Contain(e => e.EventType == OrchestrationEventType.Completed);
    }

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
        public Func<IEnumerable<Message>, Task<string>>? ResponseFuncAsync { get; set; }

        public MockAgent(string name) { Name = name; }

        public async Task<MessageResponse> InvokeAsync(IEnumerable<Message> messages, CancellationToken ct = default)
        {
            string text;
            if (ResponseFuncAsync != null) text = await ResponseFuncAsync(messages);
            else if (ResponseFunc != null) text = ResponseFunc(messages);
            else text = $"MockAgent '{Name}'";

            return new MessageResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage
                {
                    Name = Name,
                    Content = [new TextMessageContent { Value = text }]
                },
                TokenUsage = new MessageTokenUsage { InputTokens = 10, OutputTokens = text.Length }
            };
        }

        public async IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
            IEnumerable<Message> messages,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            yield return new StreamingMessageBeginResponse { Id = Guid.NewGuid().ToString("N") };
            await Task.Yield();
            yield return new StreamingMessageDoneResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                DoneReason = MessageDoneReason.EndTurn,
                TokenUsage = new MessageTokenUsage { InputTokens = 10, OutputTokens = 0 },
                Model = "mock-model",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    #endregion
}
