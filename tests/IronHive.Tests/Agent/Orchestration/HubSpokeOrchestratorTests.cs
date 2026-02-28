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

public class HubSpokeOrchestratorTests
{
    [Fact]
    public async Task Execute_NoHubAgent_ReturnsFailure()
    {
        var orch = new HubSpokeOrchestrator();
        orch.AddSpokeAgent(new MockAgent("spoke1") { ResponseFunc = _ => "ok" });

        var result = await orch.ExecuteAsync(MakeUserMessages("hello"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Hub agent is not set");
    }

    [Fact]
    public async Task Execute_NoSpokeAgents_ReturnsFailure()
    {
        var orch = new HubSpokeOrchestrator();
        orch.SetHubAgent(new MockAgent("hub") { ResponseFunc = _ => "{\"complete\": true}" });

        var result = await orch.ExecuteAsync(MakeUserMessages("hello"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No spoke agents");
    }

    [Fact]
    public async Task Execute_HubReturnsComplete_ImmediateSuccess()
    {
        var hub = new MockAgent("hub") { ResponseFunc = _ => "{\"complete\": true}" };
        var spoke = new MockAgent("spoke1") { ResponseFunc = _ => "spoke-output" };

        var orch = new HubSpokeOrchestrator();
        orch.SetHubAgent(hub);
        orch.AddSpokeAgent(spoke);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        // Only hub step, no spoke execution
        result.Steps.Should().HaveCount(1);
        result.Steps[0].AgentName.Should().Be("hub");
    }

    [Fact]
    public async Task Execute_HubDelegatesThenCompletes_RunsSpokeAndReturns()
    {
        var hubCallCount = 0;
        var hub = new MockAgent("hub")
        {
            ResponseFunc = _ =>
            {
                hubCallCount++;
                if (hubCallCount == 1)
                {
                    // First round: delegate to spoke
                    return "{\"complete\": false, \"tasks\": [{\"agent\": \"worker\", \"instruction\": \"do work\"}]}";
                }
                // Second round: complete
                return "{\"complete\": true}";
            }
        };

        var spokeExecuted = false;
        var worker = new MockAgent("worker")
        {
            Description = "A worker agent",
            ResponseFunc = _ => { spokeExecuted = true; return "work-done"; }
        };

        var orch = new HubSpokeOrchestrator();
        orch.SetHubAgent(hub);
        orch.AddSpokeAgent(worker);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        spokeExecuted.Should().BeTrue();
        // Steps: hub(round1) + spoke(worker) + hub(round2)
        result.Steps.Should().HaveCount(3);
        result.Steps[0].AgentName.Should().Be("hub");
        result.Steps[1].AgentName.Should().Be("worker");
        result.Steps[2].AgentName.Should().Be("hub");
    }

    [Fact]
    public async Task Execute_HubDelegatesToMultipleSpokes_AllExecute()
    {
        var hub = new MockAgent("hub")
        {
            ResponseFunc = _ =>
                "{\"complete\": false, \"tasks\": [" +
                "{\"agent\": \"analyst\", \"instruction\": \"analyze\"}," +
                "{\"agent\": \"writer\", \"instruction\": \"write\"}" +
                "]}"
        };

        var executedAgents = new List<string>();

        // Override hub to return complete on second call
        var hubCallCount = 0;
        hub.ResponseFunc = _ =>
        {
            hubCallCount++;
            if (hubCallCount == 1)
            {
                return "{\"complete\": false, \"tasks\": [" +
                    "{\"agent\": \"analyst\", \"instruction\": \"analyze\"}," +
                    "{\"agent\": \"writer\", \"instruction\": \"write\"}" +
                    "]}";
            }
            return "{\"complete\": true}";
        };

        var analyst = new MockAgent("analyst")
        {
            Description = "Analyst",
            ResponseFunc = _ => { executedAgents.Add("analyst"); return "analysis-done"; }
        };
        var writer = new MockAgent("writer")
        {
            Description = "Writer",
            ResponseFunc = _ => { executedAgents.Add("writer"); return "writing-done"; }
        };

        var orch = new HubSpokeOrchestrator();
        orch.SetHubAgent(hub);
        orch.AddSpokeAgent(analyst);
        orch.AddSpokeAgent(writer);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        executedAgents.Should().Contain("analyst");
        executedAgents.Should().Contain("writer");
    }

    [Fact]
    public async Task Execute_SpokeAgentNotFound_ReturnsFailureStep()
    {
        var hubCallCount = 0;
        var hub = new MockAgent("hub")
        {
            ResponseFunc = _ =>
            {
                hubCallCount++;
                if (hubCallCount == 1)
                    return "{\"complete\": false, \"tasks\": [{\"agent\": \"nonexistent\", \"instruction\": \"do work\"}]}";
                return "{\"complete\": true}";
            }
        };
        var spoke = new MockAgent("worker")
        {
            Description = "Worker",
            ResponseFunc = _ => "ok"
        };

        var orch = new HubSpokeOrchestrator(new HubSpokeOrchestratorOptions
        {
            StopOnAgentFailure = false
        });
        orch.SetHubAgent(hub);
        orch.AddSpokeAgent(spoke);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().Contain(s => s.AgentName == "nonexistent" && !s.IsSuccess);
    }

    [Fact]
    public async Task Execute_MaxRoundsReached_ReturnsLastHubOutput()
    {
        // Hub never says complete
        var hub = new MockAgent("hub")
        {
            ResponseFunc = _ => "{\"complete\": false, \"tasks\": [{\"agent\": \"worker\", \"instruction\": \"work\"}]}"
        };
        var worker = new MockAgent("worker")
        {
            Description = "Worker",
            ResponseFunc = _ => "working"
        };

        var orch = new HubSpokeOrchestrator(new HubSpokeOrchestratorOptions
        {
            MaxRounds = 2
        });
        orch.SetHubAgent(hub);
        orch.AddSpokeAgent(worker);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        // 2 rounds: hub + spoke per round = 4 steps
        result.Steps.Should().HaveCount(4);
    }

    [Fact]
    public async Task Execute_HubFailure_ReturnsFailure()
    {
        var hub = new MockAgent("hub")
        {
            ResponseFunc = _ => throw new InvalidOperationException("hub crashed")
        };
        var spoke = new MockAgent("worker")
        {
            Description = "Worker",
            ResponseFunc = _ => "ok"
        };

        var orch = new HubSpokeOrchestrator();
        orch.SetHubAgent(hub);
        orch.AddSpokeAgent(spoke);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeFalse();
        result.Steps.Should().HaveCount(1);
        result.Steps[0].AgentName.Should().Be("hub");
        result.Steps[0].Error.Should().Contain("hub crashed");
    }

    [Fact]
    public async Task Execute_HubReturnsInvalidJson_TreatedAsFailure()
    {
        var hub = new MockAgent("hub")
        {
            ResponseFunc = _ => "This is not JSON at all"
        };
        var spoke = new MockAgent("worker")
        {
            Description = "Worker",
            ResponseFunc = _ => "ok"
        };

        var orch = new HubSpokeOrchestrator();
        orch.SetHubAgent(hub);
        orch.AddSpokeAgent(spoke);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        // Invalid JSON is treated as a parse failure
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("does not contain valid JSON");
        result.Steps.Should().HaveCount(1); // only hub
    }

    [Fact]
    public async Task Execute_HubReturnsJsonInCodeFence_ParsedCorrectly()
    {
        var hubCallCount = 0;
        var hub = new MockAgent("hub")
        {
            ResponseFunc = _ =>
            {
                hubCallCount++;
                if (hubCallCount == 1)
                    return "Here is my plan:\n```json\n{\"complete\": false, \"tasks\": [{\"agent\": \"worker\", \"instruction\": \"do work\"}]}\n```";
                return "```json\n{\"complete\": true}\n```";
            }
        };
        var worker = new MockAgent("worker")
        {
            Description = "Worker",
            ResponseFunc = _ => "done"
        };

        var orch = new HubSpokeOrchestrator();
        orch.SetHubAgent(hub);
        orch.AddSpokeAgent(worker);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        // Steps: hub(round1) + spoke(worker) + hub(round2)
        result.Steps.Should().HaveCount(3);
    }

    [Fact]
    public async Task Execute_HubReturnsMalformedJson_ReturnsFailureWithError()
    {
        var hub = new MockAgent("hub")
        {
            ResponseFunc = _ => "{\"complete\": invalid_value}"
        };
        var spoke = new MockAgent("worker")
        {
            Description = "Worker",
            ResponseFunc = _ => "ok"
        };

        var orch = new HubSpokeOrchestrator();
        orch.SetHubAgent(hub);
        orch.AddSpokeAgent(spoke);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to parse hub response JSON");
        result.Steps.Should().HaveCount(1); // only hub
    }

    [Fact]
    public async Task Execute_TokenUsage_Aggregated()
    {
        var hubCallCount = 0;
        var hub = new MockAgent("hub")
        {
            ResponseFunc = _ =>
            {
                hubCallCount++;
                if (hubCallCount == 1)
                    return "{\"complete\": false, \"tasks\": [{\"agent\": \"worker\", \"instruction\": \"work\"}]}";
                return "{\"complete\": true}";
            }
        };
        var worker = new MockAgent("worker")
        {
            Description = "Worker",
            ResponseFunc = _ => "done"
        };

        var orch = new HubSpokeOrchestrator();
        orch.SetHubAgent(hub);
        orch.AddSpokeAgent(worker);

        var result = await orch.ExecuteAsync(MakeUserMessages("input"));

        result.IsSuccess.Should().BeTrue();
        result.TokenUsage.Should().NotBeNull();
        result.TokenUsage!.TotalInputTokens.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExecuteStreaming_SingleRound_YieldsCorrectEvents()
    {
        var hub = new MockAgent("hub")
        {
            ResponseFunc = _ => "{\"complete\": true}"
        };
        var spoke = new MockAgent("worker")
        {
            Description = "Worker",
            ResponseFunc = _ => "ok"
        };

        var orch = new HubSpokeOrchestrator();
        orch.SetHubAgent(hub);
        orch.AddSpokeAgent(spoke);

        var events = new List<OrchestrationStreamEvent>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("input")))
        {
            events.Add(evt);
        }

        events.Should().Contain(e => e.EventType == OrchestrationEventType.Started);
        events.Should().Contain(e => e.EventType == OrchestrationEventType.AgentCompleted && e.AgentName == "hub");
        events.Should().Contain(e => e.EventType == OrchestrationEventType.Completed);
    }

    [Fact]
    public async Task ExecuteStreaming_HubFailure_YieldsFailedEvent()
    {
        var hub = new MockAgent("hub")
        {
            ResponseFunc = _ => throw new InvalidOperationException("boom")
        };
        var spoke = new MockAgent("worker")
        {
            Description = "Worker",
            ResponseFunc = _ => "ok"
        };

        var orch = new HubSpokeOrchestrator();
        orch.SetHubAgent(hub);
        orch.AddSpokeAgent(spoke);

        var events = new List<OrchestrationStreamEvent>();
        await foreach (var evt in orch.ExecuteStreamingAsync(MakeUserMessages("input")))
        {
            events.Add(evt);
        }

        events.Should().Contain(e => e.EventType == OrchestrationEventType.AgentFailed && e.AgentName == "hub");
        events.Should().Contain(e => e.EventType == OrchestrationEventType.Failed);
    }

    #region Checkpoint & Approval

    [Fact]
    public async Task Execute_WithCheckpoint_DeletesOnSuccess()
    {
        var store = new InMemoryCheckpointStore();
        var hub = new MockAgent("hub")
        {
            ResponseFunc = _ => "{\"complete\": true}"
        };
        var spoke = new MockAgent("spoke");

        var orch = new HubSpokeOrchestrator(new HubSpokeOrchestratorOptions
        {
            CheckpointStore = store,
            OrchestrationId = "hs-chk-1"
        });
        orch.SetHubAgent(hub);
        orch.AddSpokeAgent(spoke);

        var result = await orch.ExecuteAsync(MakeUserMessages("test"));

        result.IsSuccess.Should().BeTrue();

        var checkpoint = await store.LoadAsync("hs-chk-1");
        checkpoint.Should().BeNull();
    }

    [Fact]
    public async Task Execute_WithCheckpoint_SavesOnHubFailure()
    {
        var store = new InMemoryCheckpointStore();
        var hub = new MockAgent("hub")
        {
            ResponseFunc = _ => throw new InvalidOperationException("hub fail!")
        };
        var spoke = new MockAgent("spoke");

        var orch = new HubSpokeOrchestrator(new HubSpokeOrchestratorOptions
        {
            CheckpointStore = store,
            OrchestrationId = "hs-chk-2"
        });
        orch.SetHubAgent(hub);
        orch.AddSpokeAgent(spoke);

        var result = await orch.ExecuteAsync(MakeUserMessages("test"));

        result.IsSuccess.Should().BeFalse();

        var checkpoint = await store.LoadAsync("hs-chk-2");
        checkpoint.Should().NotBeNull();
    }

    [Fact]
    public async Task Execute_ResumesFromCheckpoint_SkipsCompletedRounds()
    {
        var store = new InMemoryCheckpointStore();
        var hubCallCount = 0;

        // Round 0: hub delegates to spoke, Round 1: hub completes
        var hub = new MockAgent("hub")
        {
            ResponseFunc = _ =>
            {
                hubCallCount++;
                // Always complete on the first call (which is round 1 after resume)
                return "{\"complete\": true}";
            }
        };
        var spoke = new MockAgent("spoke");

        // Round 0 이미 완료된 체크포인트
        var existingCheckpoint = new OrchestrationCheckpoint
        {
            OrchestrationId = "hs-resume-1",
            OrchestratorName = "test",
            CompletedStepCount = 2,
            CompletedSteps =
            [
                new AgentStepResult
                {
                    AgentName = "hub",
                    Input = MakeUserMessages("test").ToList(),
                    Response = new MessageResponse
                    {
                        Id = "prev-hub",
                        DoneReason = MessageDoneReason.EndTurn,
                        Message = new AssistantMessage
                        {
                            Name = "hub",
                            Content = [new TextMessageContent { Value = "{\"complete\": false, \"tasks\": [{\"agent\": \"spoke\", \"instruction\": \"do work\"}]}" }]
                        }
                    },
                    IsSuccess = true,
                    Duration = TimeSpan.FromMilliseconds(50)
                },
                new AgentStepResult
                {
                    AgentName = "spoke",
                    Input = [],
                    Response = new MessageResponse
                    {
                        Id = "prev-spoke",
                        DoneReason = MessageDoneReason.EndTurn,
                        Message = new AssistantMessage
                        {
                            Name = "spoke",
                            Content = [new TextMessageContent { Value = "spoke result" }]
                        }
                    },
                    IsSuccess = true,
                    Duration = TimeSpan.FromMilliseconds(30)
                }
            ],
            CurrentMessages = MakeUserMessages("test").ToList()
        };
        await store.SaveAsync("hs-resume-1", existingCheckpoint);

        var orch = new HubSpokeOrchestrator(new HubSpokeOrchestratorOptions
        {
            CheckpointStore = store,
            OrchestrationId = "hs-resume-1"
        });
        orch.SetHubAgent(hub);
        orch.AddSpokeAgent(spoke);

        var result = await orch.ExecuteAsync(MakeUserMessages("test"));

        result.IsSuccess.Should().BeTrue();
        // Hub는 round 1부터 시작하므로 1번만 호출
        hubCallCount.Should().Be(1);
        // 전체 steps: 2 (체크포인트) + 1 (hub round 1)
        result.Steps.Should().HaveCount(3);
    }

    [Fact]
    public async Task Execute_ApprovalDenied_ForSpokeAgent_SavesCheckpoint()
    {
        var store = new InMemoryCheckpointStore();
        var hub = new MockAgent("hub")
        {
            ResponseFunc = _ => "{\"complete\": false, \"tasks\": [{\"agent\": \"spoke\", \"instruction\": \"do work\"}]}"
        };
        var spoke = new MockAgent("spoke");

        var orch = new HubSpokeOrchestrator(new HubSpokeOrchestratorOptions
        {
            CheckpointStore = store,
            OrchestrationId = "hs-approval-1",
            ApprovalHandler = (agentName, _) => Task.FromResult(agentName != "spoke"),
            RequireApprovalForAgents = ["spoke"]
        });
        orch.SetHubAgent(hub);
        orch.AddSpokeAgent(spoke);

        var result = await orch.ExecuteAsync(MakeUserMessages("test"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Approval denied");
        result.Error.Should().Contain("spoke");

        var checkpoint = await store.LoadAsync("hs-approval-1");
        checkpoint.Should().NotBeNull();
    }

    #endregion

    #region Helpers

    private static IEnumerable<Message> MakeUserMessages(string text)
    {
        return [new UserMessage { Content = [new TextMessageContent { Value = text }] }];
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
        var orch = new HubSpokeOrchestrator();
        orch.SupportsRealTimeStreaming.Should().BeFalse();
    }
}
