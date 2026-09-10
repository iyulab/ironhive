using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Core.Agent;
using NSubstitute;

namespace IronHive.Tests.Conventions;

// The equivalence half of ironhive-umbrella docs/CONVENTIONS.md section 5, and the reference
// the roster test points at.
//
// StreamingPairRosterTests pins WHICH operations are exposed twice. It cannot assert that the
// two halves agree, because "agree" means something different for each operation. This is what
// the roster's failure message asks an author to write, shown once, for IAgent.Invoke.
//
// The question it asks is the one that went unasked while three separate defects shipped green:
// given a generation, does the streaming path carry through everything the buffered path does?
// Those defects were a dropped tool result, an unaggregated usage total, and a tool result
// missing from history -- each invisible to tests that only ever drove one half.
public class StreamingBufferedEquivalenceTests
{
    private const string Provider = "openai";
    private const string Model = "gpt-4o";

    [Fact]
    public async Task IAgentInvoke_StreamingAndBuffered_CarryTheSameResult()
    {
        var expectedMessage = new Message
        {
            Role = MessageRole.Assistant,
            Content = [new TextMessageContent { Value = "Hello world" }],
        };

        var expectedUsage = new MessageTokenUsage { InputTokens = 11, OutputTokens = 7 };
        var timestamp = new DateTime(2026, 9, 11, 0, 0, 0, DateTimeKind.Utc);

        var service = Substitute.For<IMessageService>();

        service
            .GenerateMessageAsync(Arg.Any<MessageRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                ResponseId = "msg-1",
                DoneReason = MessageDoneReason.EndTurn,
                Message = expectedMessage,
                TokenUsage = expectedUsage,
                Model = Model,
                Timestamp = timestamp,
            });

        // The same generation, described as a stream: the deltas are how it arrives, and the
        // done frame is what it amounts to. Both halves therefore describe one outcome, so any
        // difference the assertions find is the agent losing something on the streaming path.
        service
            .GenerateStreamingMessageAsync(Arg.Any<MessageRequest>(), Arg.Any<CancellationToken>())
            .Returns(ToStream(
                new StreamingMessageBeginResponse(),
                new StreamingContentAddedResponse
                {
                    Index = 0,
                    Content = new TextMessageContent { Value = string.Empty },
                },
                new StreamingContentDeltaResponse
                {
                    Index = 0,
                    Delta = new TextDeltaContent { Value = "Hello " },
                },
                new StreamingContentDeltaResponse
                {
                    Index = 0,
                    Delta = new TextDeltaContent { Value = "world" },
                },
                new StreamingMessageDoneResponse
                {
                    ResponseId = "msg-1",
                    DoneReason = MessageDoneReason.EndTurn,
                    Message = expectedMessage,
                    TokenUsage = expectedUsage,
                    Model = Model,
                    Timestamp = timestamp,
                }));

        var agent = new BasicAgent(service) { Provider = Provider, Model = Model, Description = string.Empty };
        var input = new[] { Message.User("Hi") };

        var buffered = await agent.InvokeAsync(input, cancellationToken: TestContext.Current.CancellationToken);

        var streamed = new List<StreamingMessageResponse>();
        await foreach (var frame in agent.InvokeStreamingAsync(
            input, cancellationToken: TestContext.Current.CancellationToken))
        {
            streamed.Add(frame);
        }

        var done = streamed.OfType<StreamingMessageDoneResponse>().LastOrDefault();
        done.Should().NotBeNull(
            "the streaming half must terminate with a done frame; without one there is nothing "
            + "for a consumer to reconcile against the buffered result");

        done!.DoneReason.Should().Be(buffered.DoneReason);
        done.TokenUsage.Should().BeEquivalentTo(
            buffered.TokenUsage,
            "usage aggregated on one path and dropped on the other is one of the three defects "
            + "this convention exists for");

        TextOf(done.Message).Should().Be(
            TextOf(buffered.Message),
            "the streamed deltas must add up to what the buffered call returns");
    }

    private static string TextOf(Message? message)
        => string.Concat(
            (message?.Content ?? []).OfType<TextMessageContent>().Select(c => c.Value));

    private static async IAsyncEnumerable<StreamingMessageResponse> ToStream(
        params StreamingMessageResponse[] frames)
    {
        foreach (var frame in frames)
        {
            yield return frame;
            await Task.Yield();
        }
    }
}
