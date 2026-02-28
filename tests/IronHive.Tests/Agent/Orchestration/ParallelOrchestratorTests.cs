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

public class ParallelOrchestratorTests
{
    [Fact]
    public async Task Execute_NoAgents_ReturnsFailure()
    {
        var orch = new ParallelOrchestrator();

        var result = await orch.ExecuteAsync(MakeUserMessages("hello"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No agents");
    }

    [Fact]
    public async Task Execute_SingleAgent_ReturnsOutput()
    {
        var agent = new MockAgent("agent1") { ResponseFunc = _ => "Hello from agent1" };
        var orch = new ParallelOrchestrator();
        orch.AddAgent(agent);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().HaveCount(1);
        result.Steps[0].AgentName.Should().Be("agent1");
        GetTextFromMessage(result.FinalOutput).Should().Be("Hello from agent1");
    }

    [Fact]
    public async Task Execute_ThreeAgents_AllRunInParallel()
    {
        var callOrder = new List<string>();

        var a = new MockAgent("a") { ResponseFunc = _ => { lock (callOrder) callOrder.Add("a"); return "output-a"; } };
        var b = new MockAgent("b") { ResponseFunc = _ => { lock (callOrder) callOrder.Add("b"); return "output-b"; } };
        var c = new MockAgent("c") { ResponseFunc = _ => { lock (callOrder) callOrder.Add("c"); return "output-c"; } };

        var orch = new ParallelOrchestrator();
        orch.AddAgents([a, b, c]);

        var result = await orch.ExecuteAsync(MakeUserMessages("start"));

        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().HaveCount(3);
        callOrder.Should().HaveCount(3);
        callOrder.Should().Contain("a").And.Contain("b").And.Contain("c");
    }

    [Fact]
    public async Task Execute_AllAggregation_ReturnsLastSuccessOutput()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => "from-a" };
        var b = new MockAgent("b") { ResponseFunc = _ => "from-b" };

        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            ResultAggregation = ParallelResultAggregation.All
        });
        orch.AddAgents([a, b]);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().HaveCount(2);
        // All mode returns last successful response
        GetTextFromMessage(result.FinalOutput).Should().Be("from-b");
    }

    [Fact]
    public async Task Execute_FirstSuccessAggregation_ReturnsFirstSuccessfulOutput()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => throw new InvalidOperationException("fail") };
        var b = new MockAgent("b") { ResponseFunc = _ => "from-b" };
        var c = new MockAgent("c") { ResponseFunc = _ => "from-c" };

        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            ResultAggregation = ParallelResultAggregation.FirstSuccess,
            StopOnAgentFailure = false
        });
        orch.AddAgents([a, b, c]);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        // First successful should be "b" (a failed)
        GetTextFromMessage(result.FinalOutput).Should().Be("from-b");
    }

    [Fact]
    public async Task Execute_FastestAggregation_ReturnsFastestOutput()
    {
        // Fastest picks the step with shortest Duration
        var a = new MockAgent("a") { ResponseFunc = _ => { Thread.Sleep(50); return "slow-a"; } };
        var b = new MockAgent("b") { ResponseFunc = _ => "fast-b" };

        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            ResultAggregation = ParallelResultAggregation.Fastest
        });
        orch.AddAgents([a, b]);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        GetTextFromMessage(result.FinalOutput).Should().Be("fast-b");
    }

    [Fact]
    public async Task Execute_MergeAggregation_CombinesAllOutputs()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => "output-a" };
        var b = new MockAgent("b") { ResponseFunc = _ => "output-b" };

        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            ResultAggregation = ParallelResultAggregation.Merge
        });
        orch.AddAgents([a, b]);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        var text = GetTextFromMessage(result.FinalOutput);
        text.Should().Contain("[a]:");
        text.Should().Contain("output-a");
        text.Should().Contain("[b]:");
        text.Should().Contain("output-b");
    }

    [Fact]
    public async Task Execute_MaxConcurrency_LimitsConcurrentExecution()
    {
        var concurrentCount = 0;
        var maxConcurrent = 0;
        var lockObj = new object();

        MockAgent CreateAgent(string name) => new(name)
        {
            ResponseFunc = _ =>
            {
                lock (lockObj)
                {
                    concurrentCount++;
                    if (concurrentCount > maxConcurrent)
                        maxConcurrent = concurrentCount;
                }
                Thread.Sleep(50); // simulate work
                lock (lockObj) { concurrentCount--; }
                return $"from-{name}";
            }
        };

        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            MaxConcurrency = 2
        });
        orch.AddAgents([CreateAgent("a"), CreateAgent("b"), CreateAgent("c"), CreateAgent("d")]);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().HaveCount(4);
        maxConcurrent.Should().BeLessThanOrEqualTo(2, "concurrent execution should respect MaxConcurrency");
    }

    [Fact]
    public async Task Execute_AllAgentsFail_ReturnsFailure()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => throw new InvalidOperationException("fail-a") };
        var b = new MockAgent("b") { ResponseFunc = _ => throw new InvalidOperationException("fail-b") };

        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            StopOnAgentFailure = false
        });
        orch.AddAgents([a, b]);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("All agents failed");
        result.Steps.Should().HaveCount(2);
    }

    [Fact]
    public async Task Execute_SomeAgentsFail_SuccessWithPartialResults()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => throw new InvalidOperationException("fail") };
        var b = new MockAgent("b") { ResponseFunc = _ => "from-b" };

        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            StopOnAgentFailure = false
        });
        orch.AddAgents([a, b]);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().HaveCount(2);
        result.Steps.Where(s => s.IsSuccess).Should().HaveCount(1);
        result.Steps.Where(s => !s.IsSuccess).Should().HaveCount(1);
    }

    [Fact]
    public async Task Execute_TokenUsage_Aggregated()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => "output-a" };
        var b = new MockAgent("b") { ResponseFunc = _ => "output-b" };

        var orch = new ParallelOrchestrator();
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

        var orch = new ParallelOrchestrator();
        orch.AddAgent(a);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        result.TotalDuration.Should().BeGreaterThan(TimeSpan.Zero);
        result.Steps[0].Duration.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task ExecuteStreaming_NoAgents_YieldsFailedEvent()
    {
        var orch = new ParallelOrchestrator();

        var events = new List<OrchestrationStreamEvent>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("hello")))
        {
            events.Add(evt);
        }

        events.Should().Contain(e => e.EventType == OrchestrationEventType.Failed);
    }

    [Fact]
    public async Task ExecuteStreaming_TwoAgents_YieldsCorrectEventSequence()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => "from-a" };
        var b = new MockAgent("b") { ResponseFunc = _ => "from-b" };

        var orch = new ParallelOrchestrator();
        orch.AddAgents([a, b]);

        var events = new List<OrchestrationStreamEvent>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("input")))
        {
            events.Add(evt);
        }

        events.Should().Contain(e => e.EventType == OrchestrationEventType.Started);
        events.Should().Contain(e => e.EventType == OrchestrationEventType.AgentCompleted && e.AgentName == "a");
        events.Should().Contain(e => e.EventType == OrchestrationEventType.AgentCompleted && e.AgentName == "b");
        events.Should().Contain(e => e.EventType == OrchestrationEventType.Completed);
    }

    [Fact]
    public async Task ExecuteStreaming_AgentFailure_YieldsAgentFailedEvent()
    {
        var a = new MockAgent("a") { ResponseFunc = _ => throw new InvalidOperationException("boom") };
        var b = new MockAgent("b") { ResponseFunc = _ => "from-b" };

        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            StopOnAgentFailure = false
        });
        orch.AddAgents([a, b]);

        var events = new List<OrchestrationStreamEvent>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("input")))
        {
            events.Add(evt);
        }

        events.Should().Contain(e => e.EventType == OrchestrationEventType.AgentFailed && e.AgentName == "a");
        events.Should().Contain(e => e.EventType == OrchestrationEventType.AgentCompleted && e.AgentName == "b");
        events.Should().Contain(e => e.EventType == OrchestrationEventType.Completed);
    }

    [Fact]
    public async Task Execute_AllReceiveSameInput()
    {
        var receivedInputs = new List<string>();
        var lockObj = new object();

        var a = new MockAgent("a")
        {
            ResponseFunc = msgs =>
            {
                lock (lockObj) receivedInputs.Add(GetTextFromMessages(msgs));
                return "from-a";
            }
        };
        var b = new MockAgent("b")
        {
            ResponseFunc = msgs =>
            {
                lock (lockObj) receivedInputs.Add(GetTextFromMessages(msgs));
                return "from-b";
            }
        };

        var orch = new ParallelOrchestrator();
        orch.AddAgents([a, b]);

        await orch.ExecuteAsync(MakeUserMessages("shared-input"));

        receivedInputs.Should().HaveCount(2);
        receivedInputs.Should().AllBe("shared-input");
    }

    #region Checkpoint & Approval

    [Fact]
    public async Task Execute_WithCheckpoint_DeletesOnSuccess()
    {
        var store = new InMemoryCheckpointStore();
        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
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
        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            CheckpointStore = store,
            OrchestrationId = "ck-fail"
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

        // Pre-populate checkpoint with completed agent "a"
        var existingStep = new AgentStepResult
        {
            AgentName = "a",
            IsSuccess = true,
            Duration = TimeSpan.FromSeconds(1)
        };
        await store.SaveAsync(orchId, new OrchestrationCheckpoint
        {
            OrchestrationId = orchId,
            OrchestratorName = "ParallelOrchestrator",
            CompletedStepCount = 1,
            CompletedSteps = [existingStep],
            CurrentMessages = [new UserMessage { Content = [new TextMessageContent { Value = "go" }] }]
        });

        var executedAgents = new List<string>();
        var lockObj = new object();

        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            CheckpointStore = store,
            OrchestrationId = orchId
        });
        orch.AddAgent(new MockAgent("a") { ResponseFunc = _ => { lock (lockObj) executedAgents.Add("a"); return "from-a"; } });
        orch.AddAgent(new MockAgent("b") { ResponseFunc = _ => { lock (lockObj) executedAgents.Add("b"); return "from-b"; } });

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        result.IsSuccess.Should().BeTrue();
        executedAgents.Should().NotContain("a", "agent 'a' was already completed in checkpoint");
        executedAgents.Should().Contain("b", "agent 'b' should run");
        result.Steps.Should().HaveCount(2, "both checkpoint and new results should be merged");
    }

    [Fact]
    public async Task Execute_ApprovalDenied_SavesCheckpointAndFails()
    {
        var store = new InMemoryCheckpointStore();
        var approvalCalls = new List<string>();

        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            CheckpointStore = store,
            OrchestrationId = "ck-approval",
            ApprovalHandler = (agentName, _) =>
            {
                approvalCalls.Add(agentName);
                return Task.FromResult(agentName != "b"); // deny agent "b"
            }
        });
        orch.AddAgent(new MockAgent("a") { ResponseFunc = _ => "from-a" });
        orch.AddAgent(new MockAgent("b") { ResponseFunc = _ => "from-b" });

        var result = await orch.ExecuteAsync(MakeUserMessages("go"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Approval denied for agent 'b'");
        approvalCalls.Should().Contain("a").And.Contain("b");

        var ckpt = await store.LoadAsync("ck-approval");
        ckpt.Should().NotBeNull("checkpoint should be saved when approval is denied");
    }

    [Fact]
    public async Task Execute_ApprovalForSpecificAgents_OnlyChecksListed()
    {
        var approvalCalls = new List<string>();

        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            ApprovalHandler = (agentName, _) =>
            {
                approvalCalls.Add(agentName);
                return Task.FromResult(true);
            },
            RequireApprovalForAgents = ["b"] // only check agent "b"
        });
        orch.AddAgent(new MockAgent("a") { ResponseFunc = _ => "from-a" });
        orch.AddAgent(new MockAgent("b") { ResponseFunc = _ => "from-b" });

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
    public void SupportsRealTimeStreaming_ReturnsFalse()
    {
        var orch = new ParallelOrchestrator();
        orch.SupportsRealTimeStreaming.Should().BeFalse();
    }
}
