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

public class SequentialOrchestratorTests
{
    [Fact]
    public async Task Execute_NoAgents_ReturnsFailure()
    {
        var orch = new SequentialOrchestrator();

        var result = await orch.ExecuteAsync(MakeUserMessages("hello"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No agents");
    }

    [Fact]
    public async Task Execute_SingleAgent_ReturnsOutput()
    {
        var agent = new MockAgent("agent1") { ResponseFunc = _ => "Hello from agent1" };
        var orch = new SequentialOrchestrator();
        orch.AddAgent(agent);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().HaveCount(1);
        result.Steps[0].AgentName.Should().Be("agent1");
        GetTextFromMessage(result.FinalOutput).Should().Be("Hello from agent1");
    }

    [Fact]
    public async Task Execute_ThreeAgents_RunsInSequence()
    {
        var callOrder = new List<string>();

        var a = new MockAgent("a") { ResponseFunc = _ => { callOrder.Add("a"); return "output-a"; } };
        var b = new MockAgent("b") { ResponseFunc = _ => { callOrder.Add("b"); return "output-b"; } };
        var c = new MockAgent("c") { ResponseFunc = _ => { callOrder.Add("c"); return "output-c"; } };

        var orch = new SequentialOrchestrator();
        orch.AddAgents([a, b, c]);

        var result = await orch.ExecuteAsync(MakeUserMessages("start"));

        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().HaveCount(3);
        callOrder.Should().Equal("a", "b", "c");
        GetTextFromMessage(result.FinalOutput).Should().Be("output-c");
    }

    [Fact]
    public async Task Execute_PassOutputAsInput_ChainsOutput()
    {
        var receivedInputs = new List<string>();

        var a = new MockAgent("a")
        {
            ResponseFunc = msgs =>
            {
                receivedInputs.Add(GetTextFromMessages(msgs));
                return "from-a";
            }
        };
        var b = new MockAgent("b")
        {
            ResponseFunc = msgs =>
            {
                receivedInputs.Add(GetTextFromMessages(msgs));
                return "from-b";
            }
        };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            PassOutputAsInput = true,
            AccumulateHistory = false
        });
        orch.AddAgents([a, b]);

        await orch.ExecuteAsync(MakeUserMessages("initial"));

        receivedInputs[0].Should().Contain("initial");
        receivedInputs[1].Should().Contain("from-a");
    }

    [Fact]
    public async Task Execute_AccumulateHistory_AllMessagesAccumulated()
    {
        var receivedCounts = new List<int>();

        var a = new MockAgent("a")
        {
            ResponseFunc = msgs =>
            {
                receivedCounts.Add(msgs.Count());
                return "from-a";
            }
        };
        var b = new MockAgent("b")
        {
            ResponseFunc = msgs =>
            {
                receivedCounts.Add(msgs.Count());
                return "from-b";
            }
        };
        var c = new MockAgent("c")
        {
            ResponseFunc = msgs =>
            {
                receivedCounts.Add(msgs.Count());
                return "from-c";
            }
        };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            PassOutputAsInput = true,
            AccumulateHistory = true
        });
        orch.AddAgents([a, b, c]);

        await orch.ExecuteAsync(MakeUserMessages("start"));

        // Agent a: 1 message (initial)
        // Agent b: 2 messages (initial + from-a)
        // Agent c: 3 messages (initial + from-a + from-b)
        receivedCounts.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task Execute_AgentFailure_StopOnFailure_ReturnsFailure()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => throw new InvalidOperationException("boom") };
        var b = new MockAgent("b") { ResponseFunc = _ => "from-b" };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            StopOnAgentFailure = true
        });
        orch.AddAgents([a, b]);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeFalse();
        result.Steps.Should().HaveCount(1);
        result.Steps[0].IsSuccess.Should().BeFalse();
        result.Steps[0].Error.Should().Contain("boom");
    }

    [Fact]
    public async Task Execute_AgentFailure_ContinueOnFailure_SkipsToNext()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => throw new InvalidOperationException("boom") };
        var b = new MockAgent("b") { ResponseFunc = _ => "from-b" };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            StopOnAgentFailure = false
        });
        orch.AddAgents([a, b]);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().HaveCount(2);
        result.Steps[0].IsSuccess.Should().BeFalse();
        result.Steps[1].IsSuccess.Should().BeTrue();
        GetTextFromMessage(result.FinalOutput).Should().Be("from-b");
    }

    [Fact]
    public async Task Execute_AllAgentsFail_ReturnsNoSuccessfulOutput()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => throw new InvalidOperationException("fail-a") };
        var b = new MockAgent("b") { ResponseFunc = _ => throw new InvalidOperationException("fail-b") };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            StopOnAgentFailure = false
        });
        orch.AddAgents([a, b]);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No successful agent output");
        result.Steps.Should().HaveCount(2);
    }

    [Fact]
    public async Task Execute_ApprovalDenied_ReturnsFailure()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => "from-a" };
        var b = new MockAgent("b") { ResponseFunc = _ => "from-b" };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            ApprovalHandler = (agentName, _) =>
                Task.FromResult(agentName != "b") // deny agent b
        });
        orch.AddAgents([a, b]);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Approval denied");
        result.Steps.Should().HaveCount(1); // only agent a ran
    }

    [Fact]
    public async Task Execute_TokenUsage_Aggregated()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => "output-a" };
        var b = new MockAgent("b") { ResponseFunc = _ => "output-b" };

        var orch = new SequentialOrchestrator();
        orch.AddAgents([a, b]);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        result.TokenUsage.Should().NotBeNull();
        result.TokenUsage!.TotalInputTokens.Should().BeGreaterThan(0);
        result.TokenUsage.TotalOutputTokens.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Execute_Duration_IsRecorded()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => "output" };

        var orch = new SequentialOrchestrator();
        orch.AddAgent(a);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        result.TotalDuration.Should().BeGreaterThan(TimeSpan.Zero);
        result.Steps[0].Duration.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task ExecuteStreaming_NoAgents_YieldsFailedEvent()
    {
        var orch = new SequentialOrchestrator();

        var events = new List<OrchestrationStreamEvent>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("hello")))
        {
            events.Add(evt);
        }

        events.Should().ContainSingle(e => e.EventType == OrchestrationEventType.Failed);
    }

    [Fact]
    public async Task ExecuteStreaming_TwoAgents_YieldsCorrectEventSequence()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => "from-a" };
        var b = new MockAgent("b") { ResponseFunc = _ => "from-b" };

        var orch = new SequentialOrchestrator();
        orch.AddAgents([a, b]);

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

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            StopOnAgentFailure = true
        });
        orch.AddAgent(a);

        var events = new List<OrchestrationStreamEvent>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("input")))
        {
            events.Add(evt);
        }

        events.Should().Contain(e => e.EventType == OrchestrationEventType.AgentFailed && e.AgentName == "a");
        events.Should().Contain(e => e.EventType == OrchestrationEventType.Failed);
    }

    #region Checkpoint & Approval

    [Fact]
    public async Task Execute_WithCheckpoint_DeletesOnSuccess()
    {
        var store = new InMemoryCheckpointStore();
        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            CheckpointStore = store,
            OrchestrationId = "ck-success"
        });
        orch.AddAgent(new MockAgent("a") { ResponseFunc = _ => "ok" });

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        result.IsSuccess.Should().BeTrue();
        var ckpt = await store.LoadAsync("ck-success");
        ckpt.Should().BeNull("checkpoint should be deleted on success");
    }

    [Fact]
    public async Task Execute_WithCheckpoint_SavesOnFailure()
    {
        var store = new InMemoryCheckpointStore();
        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            CheckpointStore = store,
            OrchestrationId = "ck-fail",
            StopOnAgentFailure = true
        });
        orch.AddAgent(new MockAgent("a") { ResponseFunc = _ => throw new InvalidOperationException("boom") });

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        result.IsSuccess.Should().BeFalse();
        var ckpt = await store.LoadAsync("ck-fail");
        ckpt.Should().NotBeNull("checkpoint should be saved on failure");
        ckpt!.CompletedSteps.Should().HaveCount(1);
    }

    [Fact]
    public async Task Execute_ResumesFromCheckpoint_SkipsCompletedAgents()
    {
        var store = new InMemoryCheckpointStore();
        var orchId = "ck-resume";

        // Pre-populate checkpoint: agent "a" already completed
        var existingStep = new AgentStepResult
        {
            AgentName = "a",
            IsSuccess = true,
            Duration = TimeSpan.FromSeconds(1),
            Response = new MessageResponse
            {
                Id = "step-1",
                DoneReason = MessageDoneReason.EndTurn,
                Message = new AssistantMessage
                {
                    Name = "a",
                    Content = [new TextMessageContent { Value = "from-a" }]
                }
            }
        };
        await store.SaveAsync(orchId, new OrchestrationCheckpoint
        {
            OrchestrationId = orchId,
            OrchestratorName = "SequentialOrchestrator",
            CompletedStepCount = 1,
            CompletedSteps = [existingStep],
            CurrentMessages = [new AssistantMessage { Name = "a", Content = [new TextMessageContent { Value = "from-a" }] }]
        });

        var executedAgents = new List<string>();
        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            CheckpointStore = store,
            OrchestrationId = orchId
        });
        orch.AddAgent(new MockAgent("a") { ResponseFunc = _ => { executedAgents.Add("a"); return "from-a"; } });
        orch.AddAgent(new MockAgent("b") { ResponseFunc = _ => { executedAgents.Add("b"); return "from-b"; } });

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        result.IsSuccess.Should().BeTrue();
        executedAgents.Should().NotContain("a", "agent 'a' was already completed in checkpoint");
        executedAgents.Should().Contain("b", "agent 'b' should run");
        result.Steps.Should().HaveCount(2, "both checkpoint and new results should be merged");
    }

    [Fact]
    public async Task Execute_ApprovalDenied_SavesCheckpoint()
    {
        var store = new InMemoryCheckpointStore();
        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            CheckpointStore = store,
            OrchestrationId = "ck-approval",
            ApprovalHandler = (agentName, _) =>
                Task.FromResult(agentName != "b")
        });
        orch.AddAgent(new MockAgent("a") { ResponseFunc = _ => "from-a" });
        orch.AddAgent(new MockAgent("b") { ResponseFunc = _ => "from-b" });

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Approval denied for agent 'b'");

        var ckpt = await store.LoadAsync("ck-approval");
        ckpt.Should().NotBeNull("checkpoint should be saved when approval is denied");
        ckpt!.CompletedSteps.Should().HaveCount(1, "only agent 'a' completed");
    }

    [Fact]
    public async Task Execute_ApprovalForSpecificAgents_OnlyChecksListed()
    {
        var approvalCalls = new List<string>();

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            ApprovalHandler = (agentName, _) =>
            {
                approvalCalls.Add(agentName);
                return Task.FromResult(true);
            },
            RequireApprovalForAgents = ["b"]
        });
        orch.AddAgent(new MockAgent("a") { ResponseFunc = _ => "from-a" });
        orch.AddAgent(new MockAgent("b") { ResponseFunc = _ => "from-b" });

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        result.IsSuccess.Should().BeTrue();
        approvalCalls.Should().NotContain("a", "agent 'a' is not in RequireApprovalForAgents");
        approvalCalls.Should().Contain("b");
    }

    [Fact]
    public async Task ExecuteStreaming_WithCheckpoint_DeletesOnSuccess()
    {
        var store = new InMemoryCheckpointStore();
        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            CheckpointStore = store,
            OrchestrationId = "stream-ck-del"
        });
        orch.AddAgent(new MockAgent("a") { ResponseFunc = _ => "from-a" });
        orch.AddAgent(new MockAgent("b") { ResponseFunc = _ => "from-b" });

        var events = new List<OrchestrationStreamEvent>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("go")))
        {
            events.Add(evt);
        }

        events.Should().Contain(e => e.EventType == OrchestrationEventType.Completed);
        var ckpt = await store.LoadAsync("stream-ck-del");
        ckpt.Should().BeNull("checkpoint should be deleted on streaming success");
    }

    [Fact]
    public async Task ExecuteStreaming_ApprovalDenied_YieldsFailedEvent()
    {
        var store = new InMemoryCheckpointStore();
        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            CheckpointStore = store,
            OrchestrationId = "stream-ck-approval",
            ApprovalHandler = (agentName, _) => Task.FromResult(agentName != "b"),
            RequireApprovalForAgents = ["b"]
        });
        orch.AddAgent(new MockAgent("a") { ResponseFunc = _ => "from-a" });
        orch.AddAgent(new MockAgent("b") { ResponseFunc = _ => "from-b" });

        var events = new List<OrchestrationStreamEvent>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("go")))
        {
            events.Add(evt);
        }

        events.Should().Contain(e => e.EventType == OrchestrationEventType.ApprovalDenied);
        events.Should().Contain(e => e.EventType == OrchestrationEventType.Failed);

        var ckpt = await store.LoadAsync("stream-ck-approval");
        ckpt.Should().NotBeNull("checkpoint should be saved when streaming approval is denied");
        ckpt!.CompletedSteps.Should().HaveCount(1, "only agent 'a' completed");
    }

    [Fact]
    public async Task ExecuteStreaming_ResumesFromCheckpoint_SkipsCompletedAgents()
    {
        var store = new InMemoryCheckpointStore();
        var orchId = "stream-ck-resume";

        // Pre-populate checkpoint: agent "a" already completed
        await store.SaveAsync(orchId, new OrchestrationCheckpoint
        {
            OrchestrationId = orchId,
            OrchestratorName = "SequentialOrchestrator",
            CompletedStepCount = 1,
            CompletedSteps =
            [
                new AgentStepResult
                {
                    AgentName = "a",
                    IsSuccess = true,
                    Duration = TimeSpan.FromSeconds(1),
                    Response = new MessageResponse
                    {
                        Id = "step-1",
                        DoneReason = MessageDoneReason.EndTurn,
                        Message = new AssistantMessage
                        {
                            Name = "a",
                            Content = [new TextMessageContent { Value = "from-a" }]
                        }
                    }
                }
            ],
            CurrentMessages = [new AssistantMessage { Name = "a", Content = [new TextMessageContent { Value = "from-a" }] }]
        });

        var executedAgents = new List<string>();
        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            CheckpointStore = store,
            OrchestrationId = orchId
        });
        orch.AddAgent(new MockAgent("a") { ResponseFunc = _ => { executedAgents.Add("a"); return "from-a"; } });
        orch.AddAgent(new MockAgent("b") { ResponseFunc = _ => { executedAgents.Add("b"); return "from-b"; } });

        var events = new List<OrchestrationStreamEvent>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("go")))
        {
            events.Add(evt);
        }

        events.Should().Contain(e => e.EventType == OrchestrationEventType.Completed);
        executedAgents.Should().NotContain("a", "agent 'a' was already completed in checkpoint");
        executedAgents.Should().Contain("b");
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

    private static string GetTextFromMessages(IEnumerable<Message> messages)
    {
        return string.Join(" ", messages.Select(m => GetTextFromMessage(m)));
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

    [Fact]
    public void SupportsRealTimeStreaming_ReturnsTrue()
    {
        var orch = new SequentialOrchestrator();
        orch.SupportsRealTimeStreaming.Should().BeTrue();
    }
}
