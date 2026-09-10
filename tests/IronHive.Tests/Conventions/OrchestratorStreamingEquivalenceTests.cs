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
public class OrchestratorStreamingEquivalenceTests
{
    [Fact]
    public async Task SequentialOrchestrator_StreamingAndBuffered_ReachTheSameResult()
    {
        var buffered = await Run(BuildOrchestrator(), streaming: false);
        var streamed = await Run(BuildOrchestrator(), streaming: true);

        streamed.Should().NotBeNull(
            "the streaming path must end with a Completed event carrying the result; without it "
            + "a consumer has no way to reconcile the two halves");

        streamed!.IsSuccess.Should().Be(buffered!.IsSuccess);

        TextOf(streamed.FinalOutput).Should().Be(
            TextOf(buffered.FinalOutput),
            "the orchestration's answer must not depend on which half a caller used");

        streamed.Steps.Select(s => s.AgentName).Should().Equal(
            buffered.Steps.Select(s => s.AgentName),
            "which agents ran, and in what order, is part of the result -- a streaming path that "
            + "records the steps differently is the same defect class as one that drops usage");

        streamed.Steps.Select(s => s.IsSuccess).Should().Equal(buffered.Steps.Select(s => s.IsSuccess));
    }

    // SupportsRealTimeStreaming is what makes the assertion above worth writing, so the set of
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
            + "See docs/CONVENTIONS.md section 5. (GraphOrchestrator's own equivalence test is "
            + "still outstanding -- it is listed because it claims real-time streaming, not "
            + "because it is covered.)");
    }

    private static SequentialOrchestrator BuildOrchestrator()
    {
        var orchestrator = new SequentialOrchestrator();
        orchestrator.AddAgents([
            new EchoAgent("first", "output-first"),
            new EchoAgent("second", "output-second"),
        ]);
        return orchestrator;
    }

    private static async Task<OrchestrationResult?> Run(SequentialOrchestrator orchestrator, bool streaming)
    {
        var input = new[] { Message.User("start") };

        if (!streaming)
        {
            return await orchestrator.ExecuteAsync(input, TestContext.Current.CancellationToken);
        }

        OrchestrationResult? result = null;
        await foreach (var e in orchestrator.ExecuteStreamingAsync(input, TestContext.Current.CancellationToken))
        {
            result = e.Result ?? result;
        }

        return result;
    }

    private static string TextOf(Message? message)
        => string.Concat((message?.Content ?? []).OfType<TextMessageContent>().Select(c => c.Value));

    // Both halves describe one answer, so a difference the assertions find belongs to the
    // orchestrator rather than to this double. That includes putting the accumulated message on
    // the done frame: a stream whose terminal frame omits it is describing something the buffered
    // call did not.
    private sealed class EchoAgent(string name, string reply) : IAgent
    {
        public string Provider { get; set; } = "mock";
        public string Model { get; set; } = "mock-model";
        public string Name { get; set; } = name;
        public string Description { get; set; } = "Echo";
        public string? Instructions { get; set; }
        public IToolCollection? Tools { get; set; }
        public int? MaxTokens { get; set; }

        private Message Reply => new()
        {
            Role = MessageRole.Assistant,
            Content = [new TextMessageContent { Value = reply }],
        };

        private static MessageTokenUsage Usage => new() { InputTokens = 10, OutputTokens = 3 };

        public Task<MessageResponse> InvokeAsync(
            IEnumerable<Message> messages,
            AgentInvokeOptions? options = null,
            CancellationToken ct = default)
            => Task.FromResult(new MessageResponse
            {
                DoneReason = MessageDoneReason.EndTurn,
                Message = Reply,
                TokenUsage = Usage,
            });

        public async IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
            IEnumerable<Message> messages,
            AgentInvokeOptions? options = null,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            yield return new StreamingContentDeltaResponse
            {
                Index = 0,
                Delta = new TextDeltaContent { Value = reply },
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
}
