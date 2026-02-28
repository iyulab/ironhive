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

public class GroupChatOrchestratorTests
{
    [Fact]
    public async Task RoundRobin_ShouldCycleAgents()
    {
        // Arrange: 3 agents, 3 rounds = 9 steps
        var order = new List<string>();
        var agents = new[]
        {
            CreateMockAgent("a", order),
            CreateMockAgent("b", order),
            CreateMockAgent("c", order)
        };

        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(agents[0])
            .AddAgent(agents[1])
            .AddAgent(agents[2])
            .WithRoundRobin()
            .TerminateAfterRounds(9)
            .Build();

        // Act
        var result = await orch.ExecuteAsync(MakeUserMessages("start"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().HaveCount(9);
        order.Should().Equal("a", "b", "c", "a", "b", "c", "a", "b", "c");
    }

    [Fact]
    public async Task MaxRounds_ShouldEnforceLimit()
    {
        var agent = CreateMockAgent("a", new List<string>());

        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(agent)
            .WithRoundRobin()
            .TerminateAfterRounds(100) // termination at 100
            .SetMaxRounds(5)           // safety limit at 5
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().HaveCount(5);
    }

    [Fact]
    public async Task KeywordTermination_ShouldStopOnKeyword()
    {
        var callCount = 0;
        var agent = new MockAgent("critic")
        {
            ResponseFunc = _ =>
            {
                callCount++;
                return callCount >= 3 ? "APPROVED" : "needs work";
            }
        };

        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(agent)
            .WithRoundRobin()
            .TerminateOnKeyword("APPROVED")
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("review"));

        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().HaveCount(3);
        GetTextFromMessage(result.FinalOutput).Should().Contain("APPROVED");
    }

    [Fact]
    public async Task LlmSpeakerSelector_ShouldUseMockManager()
    {
        var selectedAgents = new List<string>();
        var manager = new MockAgent("manager")
        {
            ResponseFunc = msgs =>
            {
                // Alternately select writer and editor
                return selectedAgents.Count % 2 == 0 ? "writer" : "editor";
            }
        };

        var writer = new MockAgent("writer")
        {
            ResponseFunc = _ => { selectedAgents.Add("writer"); return "Draft text"; }
        };
        var editor = new MockAgent("editor")
        {
            ResponseFunc = _ => { selectedAgents.Add("editor"); return "Edited text"; }
        };

        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(writer)
            .AddAgent(editor)
            .WithLlmManager(manager)
            .TerminateAfterRounds(4)
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("write an article"));

        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().HaveCount(4);
        selectedAgents.Should().Equal("writer", "editor", "writer", "editor");
    }

    [Fact]
    public void Builder_NoAgents_ShouldThrow()
    {
        var act = () => new GroupChatOrchestratorBuilder()
            .WithRoundRobin()
            .TerminateAfterRounds(5)
            .Build();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*at least one agent*");
    }

    [Fact]
    public void Builder_NoSpeakerSelector_ShouldThrow()
    {
        var agent = new MockAgent("a");

        var act = () => new GroupChatOrchestratorBuilder()
            .AddAgent(agent)
            .TerminateAfterRounds(5)
            .Build();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Speaker selector*");
    }

    [Fact]
    public void Builder_NoTerminationCondition_ShouldThrow()
    {
        var agent = new MockAgent("a");

        var act = () => new GroupChatOrchestratorBuilder()
            .AddAgent(agent)
            .WithRoundRobin()
            .Build();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Termination condition*");
    }

    [Fact]
    public async Task Streaming_ShouldIncludeSpeakerSelectedEvents()
    {
        var agent1 = new MockAgent("a") { ResponseFunc = _ => "hello" };
        var agent2 = new MockAgent("b") { ResponseFunc = _ => "world" };

        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(agent1)
            .AddAgent(agent2)
            .WithRoundRobin()
            .TerminateAfterRounds(2)
            .Build();

        var events = new List<OrchestrationStreamEvent>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("go")))
        {
            events.Add(evt);
        }

        events.Should().Contain(e => e.EventType == OrchestrationEventType.Started);
        events.Should().Contain(e => e.EventType == OrchestrationEventType.SpeakerSelected);
        events.Should().Contain(e => e.EventType == OrchestrationEventType.Completed);
        events.Count(e => e.EventType == OrchestrationEventType.SpeakerSelected).Should().Be(2);
    }

    [Fact]
    public async Task CompositeTermination_Any_ShouldStopOnFirstMatch()
    {
        var callCount = 0;
        var agent = new MockAgent("a")
        {
            ResponseFunc = _ => { callCount++; return callCount >= 3 ? "DONE" : "working"; }
        };

        var composite = new CompositeTermination(
            requireAll: false,
            new KeywordTermination("DONE"),
            new MaxRoundsTermination(10));

        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(agent)
            .WithRoundRobin()
            .WithTerminationCondition(composite)
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().HaveCount(3); // stopped at keyword, not at 10
    }

    #region Checkpoint & Approval

    [Fact]
    public async Task Execute_WithCheckpoint_DeletesOnSuccess()
    {
        var store = new InMemoryCheckpointStore();

        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(new MockAgent("a") { ResponseFunc = _ => "done" })
            .WithRoundRobin()
            .TerminateAfterRounds(1)
            .SetCheckpointStore(store)
            .SetOrchestrationId("ck-success")
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        result.IsSuccess.Should().BeTrue();
        var ckpt = await store.LoadAsync("ck-success");
        ckpt.Should().BeNull("checkpoint should be deleted on success");
    }

    [Fact]
    public async Task Execute_WithCheckpoint_SavesOnFailure()
    {
        var store = new InMemoryCheckpointStore();

        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(new MockAgent("a")
            {
                ResponseFunc = _ => throw new InvalidOperationException("boom")
            })
            .WithRoundRobin()
            .TerminateAfterRounds(3)
            .SetCheckpointStore(store)
            .SetOrchestrationId("ck-fail")
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        result.IsSuccess.Should().BeFalse();
        var ckpt = await store.LoadAsync("ck-fail");
        ckpt.Should().NotBeNull("checkpoint should be saved on failure");
        ckpt!.CompletedSteps.Should().HaveCount(1);
    }

    [Fact]
    public async Task Execute_ResumesFromCheckpoint_SkipsCompletedRounds()
    {
        var store = new InMemoryCheckpointStore();
        var orchId = "ck-resume";

        // Pre-populate checkpoint: rounds 0 and 1 already completed
        var step0 = new AgentStepResult
        {
            AgentName = "a",
            IsSuccess = true,
            Duration = TimeSpan.FromSeconds(1),
            Response = new MessageResponse
            {
                Id = "step-0",
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage
                {
                    Name = "a",
                    Content = [new TextMessageContent { Value = "response-a" }]
                }
            }
        };
        var step1 = new AgentStepResult
        {
            AgentName = "b",
            IsSuccess = true,
            Duration = TimeSpan.FromSeconds(1),
            Response = new MessageResponse
            {
                Id = "step-1",
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage
                {
                    Name = "b",
                    Content = [new TextMessageContent { Value = "response-b" }]
                }
            }
        };

        await store.SaveAsync(orchId, new OrchestrationCheckpoint
        {
            OrchestrationId = orchId,
            OrchestratorName = "GroupChatOrchestrator",
            CompletedStepCount = 2,
            CompletedSteps = [step0, step1],
            CurrentMessages =
            [
                new UserMessage { Content = [new TextMessageContent { Value = "go" }] },
                new AssistantMessage { Name = "a", Content = [new TextMessageContent { Value = "response-a" }] },
                new AssistantMessage { Name = "b", Content = [new TextMessageContent { Value = "response-b" }] }
            ]
        });

        var executedAgents = new List<string>();

        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(new MockAgent("a") { ResponseFunc = _ => { executedAgents.Add("a"); return "from-a"; } })
            .AddAgent(new MockAgent("b") { ResponseFunc = _ => { executedAgents.Add("b"); return "from-b"; } })
            .WithRoundRobin()
            .TerminateAfterRounds(4)
            .SetCheckpointStore(store)
            .SetOrchestrationId(orchId)
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        result.IsSuccess.Should().BeTrue();
        // RoundRobin continues from round 2: a, b (rounds 2 and 3)
        executedAgents.Should().HaveCount(2, "only rounds 2-3 should execute");
        result.Steps.Should().HaveCount(4, "2 checkpoint steps + 2 new steps");
    }

    [Fact]
    public async Task Execute_ApprovalDenied_SavesCheckpointAndFails()
    {
        var store = new InMemoryCheckpointStore();
        var approvalCalls = new List<string>();

        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(new MockAgent("a") { ResponseFunc = _ => "from-a" })
            .AddAgent(new MockAgent("b") { ResponseFunc = _ => "from-b" })
            .WithRoundRobin()
            .TerminateAfterRounds(4)
            .SetCheckpointStore(store)
            .SetOrchestrationId("ck-approval")
            .SetApprovalHandler((agentName, _) =>
            {
                approvalCalls.Add(agentName);
                return Task.FromResult(agentName != "b"); // deny agent "b"
            })
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Approval denied for agent 'b'");
        approvalCalls.Should().Contain("a").And.Contain("b");

        var ckpt = await store.LoadAsync("ck-approval");
        ckpt.Should().NotBeNull("checkpoint should be saved when approval is denied");
        ckpt!.CompletedSteps.Should().HaveCount(1, "only agent 'a' completed before 'b' was denied");
    }

    [Fact]
    public async Task Execute_ApprovalForSpecificAgents_OnlyChecksListed()
    {
        var approvalCalls = new List<string>();

        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(new MockAgent("a") { ResponseFunc = _ => "from-a" })
            .AddAgent(new MockAgent("b") { ResponseFunc = _ => "from-b" })
            .WithRoundRobin()
            .TerminateAfterRounds(2)
            .SetApprovalHandler((agentName, _) =>
            {
                approvalCalls.Add(agentName);
                return Task.FromResult(true);
            })
            .SetRequireApprovalForAgents("b")
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        result.IsSuccess.Should().BeTrue();
        approvalCalls.Should().NotContain("a", "agent 'a' is not in RequireApprovalForAgents");
        approvalCalls.Should().Contain("b");
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
            _ => ""
        };
    }

    private static MockAgent CreateMockAgent(string name, List<string> order)
    {
        var agent = new MockAgent(name);
        agent.ResponseFunc = _ => { order.Add(name); return $"response-{name}"; };
        return agent;
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

    [Fact]
    public void SupportsRealTimeStreaming_ReturnsFalse()
    {
        var agent = new MockAgent("a") { ResponseFunc = _ => "ok" };
        var orch = new GroupChatOrchestratorBuilder()
            .AddAgent(agent)
            .WithRoundRobin()
            .TerminateAfterRounds(1)
            .Build();

        orch.SupportsRealTimeStreaming.Should().BeFalse();
    }
}
