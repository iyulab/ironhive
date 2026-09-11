using System.Reflection;
using System.Runtime.CompilerServices;
using AwesomeAssertions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Core.Agent.Orchestration;

namespace IronHive.Tests.Conventions;

// The IAgentOrchestrator.Execute half of the equivalence work described in
// ironhive-umbrella docs/CONVENTIONS.md section 5.
//
// This pair has a property the others do not: SupportsRealTimeStreaming says, per
// orchestrator, whether the streaming path is a separate implementation or a wrapper over
// ExecuteAsync. Where it is false the two halves cannot disagree -- one is derived from the
// other -- so the risk is confined to the orchestrators that answer true, and only those are
// worth asserting against. Today that is SequentialOrchestrator and GraphOrchestrator.
//
// "The same result" means everything OrchestrationResult says about the run: whether it
// succeeded and why not, the final output's content, which steps ran in what order with what
// outcome, what each step was given and what it answered, and the token usage. Streaming-only
// events (AgentStarted, MessageDelta) and timings are not part of it. Content is compared as a
// fingerprint of every part -- a comparison of text alone passed while the streaming half
// dropped everything that was not text.
public class OrchestratorStreamingEquivalenceTests
{
    // --- GraphOrchestrator ---

    [Fact]
    public async Task Graph_FanOutFanInAndASkippedBranch_BothHalvesReachTheSameResult()
    {
        var (buffered, streamed) = await RunBoth(() => Diamond(new GraphOrchestratorOptions()));

        buffered.Steps.Select(s => s.AgentName).Should().Equal(["a", "b", "c", "d"],
            "the fixture must fan out, skip e on its false condition, and fan in at d");
        AssertSameResult(buffered, streamed);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Graph_ANodeFailsMidLevel_BothHalvesReachTheSameResult(bool stopOnAgentFailure)
    {
        // A graph treats a failed node as a normal outcome when told to continue, and as the end
        // of the run when told to stop. Either way the two halves must report the same run.
        var (buffered, streamed) = await RunBoth(() => Diamond(
            new GraphOrchestratorOptions { StopOnAgentFailure = stopOnAgentFailure }, failing: "b"));

        buffered.IsSuccess.Should().Be(!stopOnAgentFailure, "the fixture must exercise the option");
        AssertSameResult(buffered, streamed);
    }

    [Fact]
    public async Task Graph_ContextScopeAndResultDistiller_ApplyToBothHalves()
    {
        var (buffered, streamed) = await RunBoth(() => Diamond(new GraphOrchestratorOptions
        {
            ContextScope = new LastMessageOnly(),
            ResultDistiller = new NamingDistiller(),
        }));

        buffered.Steps.Should().OnlyContain(s => s.Input.Count == 1, "the scope must have run");
        TextOf(buffered.FinalOutput).Should().Be("distilled(d)", "the distiller must have run");
        AssertSameResult(buffered, streamed);
    }

    // --- SequentialOrchestrator ---

    [Fact]
    public async Task Sequential_StreamingAndBuffered_ReachTheSameResult()
    {
        var (buffered, streamed) = await RunBoth(() => Chain(new SequentialOrchestratorOptions()));

        AssertSameResult(buffered, streamed);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Sequential_AnAgentFailsMidChain_BothHalvesReachTheSameResult(bool stopOnAgentFailure)
    {
        var (buffered, streamed) = await RunBoth(() => Chain(
            new SequentialOrchestratorOptions { StopOnAgentFailure = stopOnAgentFailure }, failing: "second"));

        buffered.IsSuccess.Should().Be(!stopOnAgentFailure, "the fixture must exercise the option");
        AssertSameResult(buffered, streamed);
    }

    [Fact]
    public async Task Sequential_ContextScopeAndResultDistiller_ApplyToBothHalves()
    {
        var (buffered, streamed) = await RunBoth(() => Chain(new SequentialOrchestratorOptions
        {
            ContextScope = new LastMessageOnly(),
            ResultDistiller = new NamingDistiller(),
        }));

        TextOf(buffered.FinalOutput).Should().Be("distilled(third)", "the distiller must have run");
        AssertSameResult(buffered, streamed);
    }

    // --- timeouts: an agent that runs out of its own time fails as a step; an orchestration that
    // runs out of time ends the run without recording the interrupted agent as a step ---

    [Fact]
    public async Task Graph_AnAgentTimesOut_BothHalvesReachTheSameResult()
    {
        var (buffered, streamed) = await RunBoth(() => new GraphOrchestratorBuilder()
            .WithOptions(new GraphOrchestratorOptions { AgentTimeout = TimeSpan.FromMilliseconds(150) })
            .AddNode("a", Agent("a", failing: null))
            .AddNode("h", new HangingAgent("h"))
            .AddEdge("a", "h")
            .SetStartNode("a")
            .SetOutputNode("h")
            .Build());

        buffered.Steps.Should().Contain(s => s.AgentName == "h" && !s.IsSuccess, "the fixture must time the agent out");
        AssertSameResult(buffered, streamed);
    }

    [Fact]
    public async Task Graph_TheOrchestrationTimesOut_BothHalvesReachTheSameResult()
    {
        var (buffered, streamed) = await RunBoth(() => new GraphOrchestratorBuilder()
            .WithOptions(new GraphOrchestratorOptions { Timeout = TimeSpan.FromMilliseconds(200) })
            .AddNode("a", Agent("a", failing: null))
            .AddNode("h", new HangingAgent("h"))
            .AddEdge("a", "h")
            .SetStartNode("a")
            .SetOutputNode("h")
            .Build());

        buffered.Error.Should().StartWith("Orchestration timed out", "the fixture must time the run out");
        AssertSameResult(buffered, streamed);
    }

    [Fact]
    public async Task Sequential_AnAgentTimesOut_BothHalvesReachTheSameResult()
    {
        var (buffered, streamed) = await RunBoth(() =>
        {
            var orchestrator = new SequentialOrchestrator(
                new SequentialOrchestratorOptions { AgentTimeout = TimeSpan.FromMilliseconds(150) });
            orchestrator.AddAgents([Agent("first", failing: null), new HangingAgent("h")]);
            return orchestrator;
        });

        buffered.Steps.Should().Contain(s => s.AgentName == "h" && !s.IsSuccess, "the fixture must time the agent out");
        AssertSameResult(buffered, streamed);
    }

    [Fact]
    public async Task Sequential_TheOrchestrationTimesOut_BothHalvesReachTheSameResult()
    {
        var (buffered, streamed) = await RunBoth(() =>
        {
            var orchestrator = new SequentialOrchestrator(
                new SequentialOrchestratorOptions { Timeout = TimeSpan.FromMilliseconds(200) });
            orchestrator.AddAgents([Agent("first", failing: null), new HangingAgent("h")]);
            return orchestrator;
        });

        buffered.Error.Should().StartWith("Orchestration timed out", "the fixture must time the run out");
        AssertSameResult(buffered, streamed);
    }

    // SupportsRealTimeStreaming is what makes the assertions above worth writing, so the set of
    // orchestrators claiming it is pinned: a new one is a new place the two halves can disagree,
    // and it needs its own equivalence test rather than inheriting this one's assurance.
    [Fact]
    public void RealTimeStreamingOrchestrators_MatchKnownRoster()
    {
        // Read the declaration, not an instance: GraphOrchestrator has no parameterless
        // constructor, and a roster that can only see the types it happens to be able to
        // construct is a roster with a silent hole in it. Overriding the property at all is
        // the signal -- the interface's default is false, so an override is a claim.
        var claiming = typeof(SequentialOrchestrator).Assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsPublic: true } && typeof(IAgentOrchestrator).IsAssignableFrom(t))
            .Where(t => t.GetProperty(
                nameof(IAgentOrchestrator.SupportsRealTimeStreaming),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) is not null)
            .Select(t => t.Name)
            .ToList();

        claiming.Should().BeEquivalentTo(
            new[] { nameof(SequentialOrchestrator), nameof(GraphOrchestrator) },
            "an orchestrator that answers true implements streaming separately, so its two halves "
            + "can drift. Add an equivalence test for the new one, then add it here. "
            + "See docs/CONVENTIONS.md section 5.");
    }

    // --- fixtures ---

    // a -> b, a -> c, b -> d, c -> d, and a -> e on a condition that is always false.
    private static GraphOrchestrator Diamond(GraphOrchestratorOptions options, string? failing = null)
        => new GraphOrchestratorBuilder()
            .WithOptions(options)
            .AddNode("a", Agent("a", failing))
            .AddNode("b", Agent("b", failing))
            .AddNode("c", Agent("c", failing))
            .AddNode("d", Agent("d", failing))
            .AddNode("e", Agent("e", failing))
            .AddEdge("a", "b")
            .AddEdge("a", "c")
            .AddEdge("b", "d")
            .AddEdge("c", "d")
            .AddEdge("a", "e", _ => false)
            .SetStartNode("a")
            .SetOutputNode("d")
            .Build();

    private static SequentialOrchestrator Chain(SequentialOrchestratorOptions options, string? failing = null)
    {
        var orchestrator = new SequentialOrchestrator(options);
        orchestrator.AddAgents([Agent("first", failing), Agent("second", failing), Agent("third", failing)]);
        return orchestrator;
    }

    private static ScriptedAgent Agent(string name, string? failing)
        => new(name) { Fails = name == failing };

    private static async Task<(OrchestrationResult Buffered, OrchestrationResult? Streamed)> RunBoth(
        Func<IAgentOrchestrator> build)
    {
        var input = new[] { Message.User("start") };
        var ct = TestContext.Current.CancellationToken;

        var buffered = await build().ExecuteAsync(input, ct);

        OrchestrationResult? streamed = null;
        await foreach (var e in build().ExecuteStreamingAsync(input, ct))
        {
            streamed = e.Result ?? streamed;
        }

        return (buffered, streamed);
    }

    private static void AssertSameResult(OrchestrationResult buffered, OrchestrationResult? streamed)
    {
        streamed.Should().NotBeNull(
            "the streaming path must end with a terminal event carrying the result; without it "
            + "a consumer has no way to reconcile the two halves");

        streamed!.IsSuccess.Should().Be(buffered.IsSuccess,
            "a run that failed must not be reported as a success because it was streamed");
        streamed.Error.Should().Be(buffered.Error);

        Fingerprint(streamed.FinalOutput).Should().Be(Fingerprint(buffered.FinalOutput),
            "the orchestration's answer must not depend on which half a caller used");

        StepShapes(streamed).Should().Equal(StepShapes(buffered),
            "which agents ran, in what order, with what input and outcome, is part of the result");

        (streamed.TokenUsage?.TotalInputTokens, streamed.TokenUsage?.TotalOutputTokens)
            .Should().Be((buffered.TokenUsage?.TotalInputTokens, buffered.TokenUsage?.TotalOutputTokens));
    }

    private static List<string> StepShapes(OrchestrationResult result)
        => result.Steps.Select(s =>
            $"{s.AgentName} ok={s.IsSuccess} err={s.Error ?? "-"} "
            + $"done={s.Response?.DoneReason?.ToString() ?? "-"} "
            + $"usage={s.Response?.TokenUsage?.InputTokens}/{s.Response?.TokenUsage?.OutputTokens} "
            + $"in=[{string.Join(" | ", s.Input.Select(Fingerprint))}] "
            + $"out={Fingerprint(s.Response?.Message)}").ToList();

    // One line per message: its role, then each content part in order.
    private static string Fingerprint(Message? message)
        => message is null
            ? "<none>"
            : $"{message.Role}: " + string.Join(", ", (message.Content ?? []).Select(c => c switch
            {
                TextMessageContent t => $"text({t.Value})",
                ThinkingMessageContent t => $"thinking({t.Value})",
                _ => c.GetType().Name,
            }));

    private static string TextOf(Message? message)
        => string.Concat((message?.Content ?? []).OfType<TextMessageContent>().Select(c => c.Value));

    // Answers with a reasoning part and a text part, the shape a reasoning model produces. The
    // streaming half streams both deltas and, like MessageService, puts the accumulated message
    // on the done frame -- so both halves describe one answer, and a difference the assertions
    // find belongs to the orchestrator rather than to this double.
    private sealed class ScriptedAgent(string name) : IAgent
    {
        public string Provider { get; set; } = "mock";
        public string Model { get; set; } = "mock-model";
        public string Name { get; set; } = name;
        public string Description { get; set; } = "Scripted";
        public string? Instructions { get; set; }
        public IToolCollection? Tools { get; set; }
        public int? MaxTokens { get; set; }
        public bool Fails { get; init; }

        private Message Reply => new()
        {
            Role = MessageRole.Assistant,
            Content =
            [
                new ThinkingMessageContent { Value = $"why-{Name}" },
                new TextMessageContent { Value = $"from-{Name}" },
            ],
        };

        private MessageTokenUsage Usage => new() { InputTokens = 10, OutputTokens = Name.Length };

        public Task<MessageResponse> InvokeAsync(
            IEnumerable<Message> messages,
            AgentInvokeOptions? options = null,
            CancellationToken ct = default)
        {
            if (Fails)
            {
                throw new InvalidOperationException($"{Name} broke");
            }

            return Task.FromResult(new MessageResponse
            {
                DoneReason = MessageDoneReason.EndTurn,
                Message = Reply,
                TokenUsage = Usage,
            });
        }

        public async IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
            IEnumerable<Message> messages,
            AgentInvokeOptions? options = null,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            if (Fails)
            {
                throw new InvalidOperationException($"{Name} broke");
            }

            yield return new StreamingContentDeltaResponse
            {
                Index = 0,
                Delta = new ThinkingDeltaContent { Data = $"why-{Name}" },
            };
            yield return new StreamingContentDeltaResponse
            {
                Index = 1,
                Delta = new TextDeltaContent { Value = $"from-{Name}" },
            };

            await Task.Yield();

            yield return new StreamingMessageDoneResponse
            {
                DoneReason = MessageDoneReason.EndTurn,
                Message = Reply,
                TokenUsage = Usage,
                Model = "mock-model",
                Timestamp = DateTime.UtcNow,
            };
        }
    }

    // Never answers: waits until whichever token it was given is cancelled.
    private sealed class HangingAgent(string name) : IAgent
    {
        public string Provider { get; set; } = "mock";
        public string Model { get; set; } = "mock-model";
        public string Name { get; set; } = name;
        public string Description { get; set; } = "Hanging";
        public string? Instructions { get; set; }
        public IToolCollection? Tools { get; set; }
        public int? MaxTokens { get; set; }

        public async Task<MessageResponse> InvokeAsync(
            IEnumerable<Message> messages,
            AgentInvokeOptions? options = null,
            CancellationToken ct = default)
        {
            await Task.Delay(Timeout.Infinite, ct);
            throw new InvalidOperationException("unreachable");
        }

        public async IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
            IEnumerable<Message> messages,
            AgentInvokeOptions? options = null,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            await Task.Delay(Timeout.Infinite, ct);
            yield break;
        }
    }

    private sealed class LastMessageOnly : IContextScope
    {
        public IReadOnlyList<Message> ScopeMessages(IReadOnlyList<Message> messages, string agentName)
            => messages.Count == 0 ? messages : [messages[^1]];
    }

    private sealed class NamingDistiller : IResultDistiller
    {
        public Task<MessageResponse> DistillAsync(
            string agentName,
            MessageResponse response,
            ResultDistillationOptions? options = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new MessageResponse
            {
                ResponseId = response.ResponseId,
                DoneReason = response.DoneReason,
                TokenUsage = response.TokenUsage,
                Model = response.Model,
                Timestamp = response.Timestamp,
                Message = new Message
                {
                    Role = MessageRole.Assistant,
                    Content = [new TextMessageContent { Value = $"distilled({agentName})" }],
                },
            });
    }
}
