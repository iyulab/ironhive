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

    #region Checkpoint & Approval

    [Fact]
    public async Task Execute_WithCheckpoint_DeletesOnSuccess()
    {
        var store = new InMemoryCheckpointStore();

        var orch = new HandoffOrchestratorBuilder()
            .AddAgent(new MockAgent("a") { ResponseFunc = _ => "done" })
            .SetInitialAgent("a")
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

        var orch = new HandoffOrchestratorBuilder()
            .AddAgent(new MockAgent("a")
            {
                ResponseFunc = _ => throw new InvalidOperationException("boom")
            })
            .SetInitialAgent("a")
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
    public async Task Execute_ResumesFromCheckpoint_SkipsCompletedTransitions()
    {
        var store = new InMemoryCheckpointStore();
        var orchId = "ck-resume";

        // Pre-populate checkpoint: triage already completed with handoff to billing
        var triageStep = new AgentStepResult
        {
            AgentName = "triage",
            IsSuccess = true,
            Duration = TimeSpan.FromSeconds(1),
            Response = new MessageResponse
            {
                Id = "step-1",
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage
                {
                    Name = "triage",
                    Content = [new TextMessageContent { Value = """{"handoff_to": "billing", "context": "issue"}""" }]
                }
            }
        };

        await store.SaveAsync(orchId, new OrchestrationCheckpoint
        {
            OrchestrationId = orchId,
            OrchestratorName = "HandoffOrchestrator",
            CompletedStepCount = 1,
            CompletedSteps = [triageStep],
            CurrentMessages =
            [
                new UserMessage { Content = [new TextMessageContent { Value = "go" }] },
                new AssistantMessage { Name = "triage", Content = [new TextMessageContent { Value = """{"handoff_to": "billing", "context": "issue"}""" }] },
                new UserMessage { Content = [new TextMessageContent { Value = "issue" }] }
            ]
        });

        var executedAgents = new List<string>();

        var orch = new HandoffOrchestratorBuilder()
            .AddAgent(new MockAgent("triage")
            {
                ResponseFunc = _ => { executedAgents.Add("triage"); return "from triage"; }
            })
            .AddAgent(new MockAgent("billing")
            {
                ResponseFunc = _ => { executedAgents.Add("billing"); return "billing resolved"; }
            },
                new HandoffTarget { AgentName = "triage", Description = "Back to triage" })
            .SetInitialAgent("triage")
            .SetCheckpointStore(store)
            .SetOrchestrationId(orchId)
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        result.IsSuccess.Should().BeTrue();
        executedAgents.Should().NotContain("triage", "triage was already completed in checkpoint");
        executedAgents.Should().Contain("billing", "billing should run as next agent");
        result.Steps.Should().HaveCount(2, "checkpoint step + new billing step");
    }

    [Fact]
    public async Task Execute_ApprovalDenied_SavesCheckpointAndFails()
    {
        var store = new InMemoryCheckpointStore();
        var approvalCalls = new List<string>();

        var orch = new HandoffOrchestratorBuilder()
            .AddAgent(new MockAgent("triage")
            {
                ResponseFunc = _ => """{"handoff_to": "billing"}"""
            },
                new HandoffTarget { AgentName = "billing", Description = "Billing" })
            .AddAgent(new MockAgent("billing")
            {
                ResponseFunc = _ => "resolved"
            },
                new HandoffTarget { AgentName = "triage", Description = "Triage" })
            .SetInitialAgent("triage")
            .SetCheckpointStore(store)
            .SetOrchestrationId("ck-approval")
            .SetApprovalHandler((agentName, _) =>
            {
                approvalCalls.Add(agentName);
                return Task.FromResult(agentName != "billing"); // deny billing
            })
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Approval denied for agent 'billing'");
        approvalCalls.Should().Contain("triage").And.Contain("billing");

        var ckpt = await store.LoadAsync("ck-approval");
        ckpt.Should().NotBeNull("checkpoint should be saved when approval is denied");
        ckpt!.CompletedSteps.Should().HaveCount(1, "triage step completed before billing was denied");
    }

    [Fact]
    public async Task Execute_ApprovalForSpecificAgents_OnlyChecksListed()
    {
        var approvalCalls = new List<string>();

        var orch = new HandoffOrchestratorBuilder()
            .AddAgent(new MockAgent("triage")
            {
                ResponseFunc = _ => """{"handoff_to": "billing"}"""
            },
                new HandoffTarget { AgentName = "billing", Description = "Billing" })
            .AddAgent(new MockAgent("billing")
            {
                ResponseFunc = _ => "resolved"
            },
                new HandoffTarget { AgentName = "triage", Description = "Triage" })
            .SetInitialAgent("triage")
            .SetApprovalHandler((agentName, _) =>
            {
                approvalCalls.Add(agentName);
                return Task.FromResult(true);
            })
            .SetRequireApprovalForAgents("billing")
            .Build();

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        result.IsSuccess.Should().BeTrue();
        approvalCalls.Should().NotContain("triage", "triage is not in RequireApprovalForAgents");
        approvalCalls.Should().Contain("billing");
    }

    #endregion

    #region Helpers


    [Fact]
    public async Task Execute_DoesNotMutateAgentInstructions()
    {
        // Arrange: 에이전트의 원본 Instructions가 변경되지 않아야 함
        const string originalInstructions = "You are a helpful triage agent.";
        var triage = new MockAgent("triage")
        {
            Instructions = originalInstructions,
            ResponseFunc = _ => "I can help you directly!"
        };

        var billing = new MockAgent("billing") { Instructions = "Billing agent" };

        var orch = new HandoffOrchestratorBuilder()
            .AddAgent(triage,
                new HandoffTarget { AgentName = "billing", Description = "Billing issues" })
            .AddAgent(billing,
                new HandoffTarget { AgentName = "triage", Description = "Triage" })
            .SetInitialAgent("triage")
            .Build();

        // Act
        var result = await orch.ExecuteAsync(MakeUserMessages("test"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        triage.Instructions.Should().Be(originalInstructions,
            "에이전트의 Instructions는 오케스트레이션 후에도 변경되면 안 됩니다");
        billing.Instructions.Should().Be("Billing agent");
    }

    [Fact]
    public async Task Execute_ConcurrentExecution_InstructionsNotCorrupted()
    {
        // Arrange: 동일 에이전트 인스턴스를 여러 오케스트레이터에서 동시 실행
        const string originalInstructions = "Shared agent instructions";
        var callCount = 0;

        var sharedAgent = new MockAgent("agent")
        {
            Instructions = originalInstructions,
            ResponseFuncAsync = async msgs =>
            {
                Interlocked.Increment(ref callCount);
                await Task.Delay(50); // 동시 실행 시 overlap 유도
                return "Done";
            }
        };

        var orch1 = new HandoffOrchestratorBuilder()
            .AddAgent(sharedAgent, new HandoffTarget { AgentName = "other", Description = "Other" })
            .AddAgent(new MockAgent("other"), new HandoffTarget { AgentName = "agent", Description = "Agent" })
            .SetInitialAgent("agent")
            .Build();

        var orch2 = new HandoffOrchestratorBuilder()
            .AddAgent(sharedAgent, new HandoffTarget { AgentName = "other2", Description = "Other2" })
            .AddAgent(new MockAgent("other2"), new HandoffTarget { AgentName = "agent", Description = "Agent" })
            .SetInitialAgent("agent")
            .Build();

        // Act: 동시 실행
        var tasks = new[]
        {
            orch1.ExecuteAsync(MakeUserMessages("request 1")),
            orch2.ExecuteAsync(MakeUserMessages("request 2"))
        };
        var results = await Task.WhenAll(tasks);

        // Assert: 모든 실행 성공하고 Instructions 불변
        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
        sharedAgent.Instructions.Should().Be(originalInstructions,
            "동시 실행 후에도 Instructions는 원래 값이어야 합니다");
        callCount.Should().Be(2);
    }

    [Fact]
    public async Task Execute_HandoffInstructionsPassedViaMessages()
    {
        // Arrange: handoff 지시사항이 메시지로 전달되는지 검증
        List<Message>? capturedMessages = null;

        var triage = new MockAgent("triage")
        {
            ResponseFunc = msgs =>
            {
                capturedMessages = msgs.ToList();
                return """{"handoff_to": "billing", "context": "billing question"}""";
            }
        };

        var billing = new MockAgent("billing")
        {
            ResponseFunc = _ => "Billing resolved"
        };

        var orch = new HandoffOrchestratorBuilder()
            .AddAgent(triage,
                new HandoffTarget { AgentName = "billing", Description = "Billing issues" })
            .AddAgent(billing,
                new HandoffTarget { AgentName = "triage", Description = "Triage" })
            .SetInitialAgent("triage")
            .Build();

        // Act
        await orch.ExecuteAsync(MakeUserMessages("I have a billing question"));

        // Assert: triage 에이전트가 받은 메시지에 handoff 지시사항이 포함되어야 함
        capturedMessages.Should().NotBeNull();
        capturedMessages!.Count.Should().BeGreaterThan(1,
            "handoff 지시사항 메시지가 앞에 추가되어야 합니다");
        var firstMessage = capturedMessages[0] as UserMessage;
        firstMessage.Should().NotBeNull();
        var textContent = firstMessage!.Content.OfType<TextMessageContent>().First();
        textContent.Value.Should().Contain("HANDOFF INSTRUCTIONS");
        textContent.Value.Should().Contain("billing");
    }

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

    [Fact]
    public void SupportsRealTimeStreaming_ReturnsFalse()
    {
        var agent = new MockAgent("a") { ResponseFunc = _ => "ok" };
        var orch = new HandoffOrchestratorBuilder()
            .AddAgent(agent)
            .SetInitialAgent("a")
            .Build();

        orch.SupportsRealTimeStreaming.Should().BeFalse();
    }
}
