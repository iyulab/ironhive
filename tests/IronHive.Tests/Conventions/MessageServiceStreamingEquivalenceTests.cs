using System.Runtime.CompilerServices;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Core.Services;
using IronHive.Core.Tools;

namespace IronHive.Tests.Conventions;

// Equivalence test for IMessageService.GenerateMessage (ironhive-umbrella docs/CONVENTIONS.md
// section 5): given one generation described twice -- as a response and as a stream -- the two
// halves of MessageService must return the same thing. That is the whole of what a result says:
// every content part (tool outputs included), the done reason, usage, response id, suggestions,
// and context items; and the request each turn sends to the generator, because the tool loop
// feeds one turn's output into the next turn's input.
//
// Each case first asserts that the fixture actually took the path it claims (section 6): an
// equivalence test cannot see two halves that are wrong in the same way.
public class MessageServiceStreamingEquivalenceTests
{
    private const string Provider = "openai";
    private const string Model = "gpt-4o";

    [Fact]
    public async Task ToolLoop_WithSuggestionsAndMiddleware_BothHalvesCarryTheSameResult()
    {
        Turn[] turns =
        [
            new("r1", MessageDoneReason.ToolCall, 10, 4,
                [new Part.Thinking("plan the lookup"), new Part.Text("Let me check."), new Part.Tool("call-1", "echo", "{\"q\":1}")]),
            new("r2", MessageDoneReason.EndTurn, 25, 9,
                [new Part.Text("The answer is 1.\n<suggestion>\nQ: What next?\nA: Ask again\nA: Stop here\n</suggestion>")]),
        ];

        MessageRequest Request() => new()
        {
            Provider = Provider,
            Model = Model,
            Messages = [Message.User("look it up")],
            Tools = new ToolCollection([new EchoTool()]),
            Suggestions = new SuggestionOptions(),
        };

        var buffered = await RunBufferedAsync(turns, Request(), new CountingMiddleware());
        var streamed = await RunStreamingAsync(turns, Request(), new CountingMiddleware());

        buffered.Generator.Calls.Should().Be(2, "the fixture must run the tool loop for two turns");
        streamed.Generator.Calls.Should().Be(2, "the fixture must run the tool loop for two turns");
        buffered.Result.Message!.Content.OfType<ToolMessageContent>().Single().Output
            .Should().NotBeNull("the fixture must execute the tool between the turns");
        buffered.Result.Suggestions.Should().NotBeNull("the fixture must produce a suggestion block");
        buffered.Result.Items.Should().ContainKey(CountingMiddleware.Key, "the fixture's middleware must run");

        AssertSameResult(buffered, streamed);
    }

    [Fact]
    public async Task SuggestionBlockInAToolTurn_IsStrippedBeforeTheNextTurnOnBothHalves()
    {
        // A suggestion block in a turn that also calls a tool. The next turn sends that turn back to
        // the model as history -- the question is whether it sends the block or the stripped text.
        Turn[] turns =
        [
            new("r1", MessageDoneReason.ToolCall, 8, 3,
                [new Part.Text("Checking.\n<suggestion>\nQ: Early?\nA: Yes\nA: No\n</suggestion>"), new Part.Tool("call-1", "echo", "{}")]),
            new("r2", MessageDoneReason.EndTurn, 12, 2, [new Part.Text("Done.")]),
        ];

        MessageRequest Request() => new()
        {
            Provider = Provider,
            Model = Model,
            Messages = [Message.User("go")],
            Tools = new ToolCollection([new EchoTool()]),
            Suggestions = new SuggestionOptions(),
        };

        var buffered = await RunBufferedAsync(turns, Request());
        var streamed = await RunStreamingAsync(turns, Request());

        buffered.Generator.Calls.Should().Be(2, "the fixture must reach a second turn");
        streamed.Generator.Calls.Should().Be(2, "the fixture must reach a second turn");

        AssertSameResult(buffered, streamed);
    }

    [Fact]
    public async Task Resume_FromACarriedInAssistantMessage_BothHalvesCarryTheSameResult()
    {
        // The last input message is an assistant message with an approved tool that has not run yet:
        // the service picks it up, runs the tool, then asks the model to continue. Its text is history
        // from an earlier call -- this call did not generate it.
        Message Carried() => new()
        {
            Role = MessageRole.Assistant,
            Content =
            [
                new TextMessageContent { Value = "Earlier answer.\n<suggestion>\nQ: Old question?\nA: x\nA: y\n</suggestion>" },
                new ToolMessageContent { Id = "call-0", Name = "echo", Input = "{}", IsApproved = true },
            ],
        };

        Turn[] turns = [new("r1", MessageDoneReason.EndTurn, 5, 2, [new Part.Text("Resumed.")])];

        var bufferedCarried = Carried();
        var streamedCarried = Carried();

        MessageRequest Request(Message carried) => new()
        {
            Provider = Provider,
            Model = Model,
            Messages = [Message.User("hi"), carried],
            Tools = new ToolCollection([new EchoTool()]),
            Suggestions = new SuggestionOptions(),
        };

        var buffered = await RunBufferedAsync(turns, Request(bufferedCarried));
        var streamed = await RunStreamingAsync(turns, Request(streamedCarried));

        buffered.Result.Message.Should().BeSameAs(bufferedCarried, "the fixture must resume the carried-in message");
        streamed.Done!.Message.Should().BeSameAs(streamedCarried, "the fixture must resume the carried-in message");
        bufferedCarried.Content.OfType<ToolMessageContent>().Single().Output
            .Should().NotBeNull("the fixture must run the carried-in tool");

        AssertSameResult(buffered, streamed);
    }

    [Fact]
    public async Task ErrorFrame_EndsTheStreamingCallTheWayTheBufferedCallThrows()
    {
        var generator = new FailingGenerator();
        var service = new MessageService(new Dictionary<string, IMessageGenerator> { [Provider] = generator });
        var request = new MessageRequest { Provider = Provider, Model = Model, Messages = [Message.User("hi")] };

        var bufferedCall = async () => await service.GenerateMessageAsync(request, TestContext.Current.CancellationToken);
        await bufferedCall.Should().ThrowAsync<InvalidOperationException>();
        generator.BufferedCalls.Should().Be(1, "the fixture's buffered generation must fail once and throw");

        var frames = new List<StreamingMessageResponse>();
        Exception? thrown = null;
        try
        {
            await foreach (var frame in service.GenerateStreamingMessageAsync(request, TestContext.Current.CancellationToken))
            {
                frames.Add(frame);
            }
        }
        catch (InvalidOperationException ex)
        {
            thrown = ex;
        }

        generator.StreamingCalls.Should().Be(1,
            "a failed generation carries no done reason, and the tool loop must not read that as "
            + "'continue' and send the same request again -- up to MaxTurns times");
        frames.OfType<StreamingMessageErrorResponse>().Should().ContainSingle(
            "a consumer reading frames must still see the provider's error");
        frames.OfType<StreamingMessageDoneResponse>().Should().BeEmpty(
            "a done frame after an error reads as a completed generation");
        thrown.Should().NotBeNull("the buffered call throws, so the streaming call must not end as if it succeeded");
        thrown!.Message.Should().Contain("server_error").And.Contain("boom");
    }

    [Fact]
    public async Task StreamEndingWithoutADoneFrame_EndsTheCallInsteadOfStartingAnotherTurn()
    {
        // Same root as the error frame: a turn with no done reason is not a turn that asked to continue.
        var generator = new DonelessGenerator();
        var service = new MessageService(new Dictionary<string, IMessageGenerator> { [Provider] = generator });
        var request = new MessageRequest { Provider = Provider, Model = Model, Messages = [Message.User("hi")] };

        var act = async () =>
        {
            await foreach (var _ in service.GenerateStreamingMessageAsync(request, TestContext.Current.CancellationToken))
            {
            }
        };

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*without a done frame*");
        generator.Calls.Should().Be(1, "the tool loop must not send the request again");
    }

    // Known limitation, pinned rather than hidden. IMessageMiddleware gives both methods a
    // pass-through default, so a middleware may implement only one; the other path then skips it
    // with nothing to say so. No implementation in this tree or its known consumers does this today.
    // Making both abstract would force an author to write both halves, but not to make them agree --
    // the library has no way to check that. Tracked as an issue draft; this test records the current
    // behaviour so that changing it is a decision, not an accident.
    [Fact]
    public async Task MiddlewareImplementingOnlyTheBufferedHalf_IsSkippedWhenStreaming()
    {
        Turn[] turns = [new("r1", MessageDoneReason.EndTurn, 1, 1, [new Part.Text("hi")])];
        MessageRequest Request() => new() { Provider = Provider, Model = Model, Messages = [Message.User("hi")] };

        var buffered = await RunBufferedAsync(turns, Request(), new BufferedOnlyMiddleware());
        var streamed = await RunStreamingAsync(turns, Request(), new BufferedOnlyMiddleware());

        buffered.Result.Items.Should().ContainKey(BufferedOnlyMiddleware.Key);
        streamed.Done!.Items.Should().NotContainKey(BufferedOnlyMiddleware.Key);
    }

    // ---- comparison ----

    private static void AssertSameResult(BufferedRun buffered, StreamedRun streamed)
    {
        streamed.Done.Should().NotBeNull("the streaming half must end with a done frame");
        var done = streamed.Done!;

        Fingerprint(done.Message).Should().Equal(Fingerprint(buffered.Result.Message), "every content part must match");
        done.DoneReason.Should().Be(buffered.Result.DoneReason);
        done.TokenUsage.Should().BeEquivalentTo(buffered.Result.TokenUsage);
        done.ResponseId.Should().Be(buffered.Result.ResponseId);
        SuggestionsOf(done.Suggestions).Should().Be(SuggestionsOf(buffered.Result.Suggestions));
        ItemsOf(done.Items).Should().Be(ItemsOf(buffered.Result.Items));
        streamed.Generator.Requests.Should().Equal(
            buffered.Generator.Requests,
            "each turn must send the model the same history on both halves");
    }

    private static List<string> Fingerprint(Message? message)
        => [.. (message?.Content ?? []).Select(ContentOf)];

    private static string ContentOf(MessageContent content) => content switch
    {
        TextMessageContent t => $"text({t.Value})",
        ThinkingMessageContent th => $"thinking({th.Value})",
        ToolMessageContent tool => $"tool({tool.Id},{tool.Name},{tool.Input},approved={tool.IsApproved},output={OutputOf(tool.Output)})",
        _ => content.GetType().Name,
    };

    private static string OutputOf(ToolOutput? output)
        => output is null
            ? "none"
            : $"{output.IsSuccess}:{string.Join("|", output.Content.OfType<TextMessageContent>().Select(c => c.Value))}";

    private static string SuggestionsOf(List<Suggestion>? suggestions)
        => suggestions is null
            ? "none"
            : string.Join(";", suggestions.Select(s => $"{s.Question}:{string.Join("/", s.Items)}"));

    private static string ItemsOf(MessageContextItems? items)
        => items is null
            ? "none"
            : string.Join(",", items.OrderBy(kv => kv.Key, StringComparer.Ordinal).Select(kv => $"{kv.Key}={kv.Value}"));

    private static string RequestOf(MessageGenerationRequest request)
        => $"system={request.System} | "
           + string.Join(" / ", request.Messages.Select(m => $"{m.Role}:[{string.Join(",", Fingerprint(m))}]"));

    // ---- runs ----

    private sealed record BufferedRun(MessageResponse Result, ScriptedGenerator Generator);

    private sealed record StreamedRun(StreamingMessageDoneResponse? Done, List<StreamingMessageResponse> Frames, ScriptedGenerator Generator);

    private static async Task<BufferedRun> RunBufferedAsync(Turn[] turns, MessageRequest request, params IMessageMiddleware[] middlewares)
    {
        var generator = new ScriptedGenerator(turns);
        var service = new MessageService(new Dictionary<string, IMessageGenerator> { [Provider] = generator }, middlewares);
        var result = await service.GenerateMessageAsync(request, TestContext.Current.CancellationToken);
        return new BufferedRun(result, generator);
    }

    private static async Task<StreamedRun> RunStreamingAsync(Turn[] turns, MessageRequest request, params IMessageMiddleware[] middlewares)
    {
        var generator = new ScriptedGenerator(turns);
        var service = new MessageService(new Dictionary<string, IMessageGenerator> { [Provider] = generator }, middlewares);
        var frames = new List<StreamingMessageResponse>();
        await foreach (var frame in service.GenerateStreamingMessageAsync(request, TestContext.Current.CancellationToken))
        {
            frames.Add(frame);
        }

        return new StreamedRun(frames.OfType<StreamingMessageDoneResponse>().LastOrDefault(), frames, generator);
    }

    // ---- fixture ----

    private abstract record Part
    {
        public sealed record Text(string Value) : Part;

        public sealed record Thinking(string Value) : Part;

        public sealed record Tool(string Id, string Name, string Input) : Part;
    }

    private sealed record Turn(string ResponseId, MessageDoneReason Reason, int InputTokens, int OutputTokens, Part[] Parts);

    // One scripted generation per turn, served either as a response or as a stream. Content objects
    // are created fresh on every call because the service merges into and edits what it receives.
    private sealed class ScriptedGenerator(Turn[] turns) : IMessageGenerator
    {
        public int Calls { get; private set; }

        public List<string> Requests { get; } = [];

        public Task<MessageResponse> GenerateMessageAsync(MessageGenerationRequest request, CancellationToken cancellationToken = default)
        {
            var turn = Next(request);
            return Task.FromResult(new MessageResponse
            {
                ResponseId = turn.ResponseId,
                DoneReason = turn.Reason,
                Message = new Message { Role = MessageRole.Assistant, Content = [.. turn.Parts.Select(Materialize)] },
                TokenUsage = new MessageTokenUsage { InputTokens = turn.InputTokens, OutputTokens = turn.OutputTokens },
            });
        }

        public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
            MessageGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var turn = Next(request);
            yield return new StreamingMessageBeginResponse();

            for (var i = 0; i < turn.Parts.Length; i++)
            {
                switch (turn.Parts[i])
                {
                    case Part.Text text:
                        yield return new StreamingContentAddedResponse { Index = i, Content = new TextMessageContent() };
                        foreach (var chunk in Chunks(text.Value))
                        {
                            yield return new StreamingContentDeltaResponse { Index = i, Delta = new TextDeltaContent { Value = chunk } };
                        }

                        break;
                    case Part.Thinking thinking:
                        yield return new StreamingContentAddedResponse { Index = i, Content = new ThinkingMessageContent() };
                        yield return new StreamingContentDeltaResponse { Index = i, Delta = new ThinkingDeltaContent { Data = thinking.Value } };
                        break;
                    case Part.Tool tool:
                        yield return new StreamingContentAddedResponse
                        {
                            Index = i,
                            Content = new ToolMessageContent { Id = tool.Id, Name = tool.Name, Input = string.Empty, IsApproved = true },
                        };
                        yield return new StreamingContentDeltaResponse { Index = i, Delta = new ToolDeltaContent { Input = tool.Input } };
                        break;
                }

                yield return new StreamingContentCompletedResponse { Index = i };
                await Task.Yield();
            }

            yield return new StreamingMessageDoneResponse
            {
                ResponseId = turn.ResponseId,
                DoneReason = turn.Reason,
                TokenUsage = new MessageTokenUsage { InputTokens = turn.InputTokens, OutputTokens = turn.OutputTokens },
            };
        }

        public Task<int> CountTokensAsync(MessageGenerationRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public void Dispose()
        {
        }

        private Turn Next(MessageGenerationRequest request)
        {
            Requests.Add(RequestOf(request));
            var turn = turns[Math.Min(Calls, turns.Length - 1)];
            Calls++;
            return turn;
        }

        private static MessageContent Materialize(Part part) => part switch
        {
            Part.Text text => new TextMessageContent { Value = text.Value },
            Part.Thinking thinking => new ThinkingMessageContent { Value = thinking.Value },
            Part.Tool tool => new ToolMessageContent { Id = tool.Id, Name = tool.Name, Input = tool.Input, IsApproved = true },
            _ => throw new ArgumentOutOfRangeException(nameof(part)),
        };

        // Split text so that a suggestion tag straddles two deltas -- the collector has to hold a
        // partial tag across frames, which is the streaming half's hardest case.
        private static string[] Chunks(string value)
        {
            var tag = value.IndexOf('<', StringComparison.Ordinal);
            var cut = tag >= 0 ? Math.Min(tag + 4, value.Length) : value.Length / 2;
            return cut <= 0 || cut >= value.Length ? [value] : [value[..cut], value[cut..]];
        }
    }

    private sealed class FailingGenerator : IMessageGenerator
    {
        public int BufferedCalls { get; private set; }

        public int StreamingCalls { get; private set; }

        public Task<MessageResponse> GenerateMessageAsync(MessageGenerationRequest request, CancellationToken cancellationToken = default)
        {
            BufferedCalls++;
            throw new InvalidOperationException("OpenAI API Error: server_error - boom");
        }

        public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
            MessageGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            StreamingCalls++;
            yield return new StreamingMessageBeginResponse();
            await Task.Yield();
            yield return new StreamingMessageErrorResponse { Code = "server_error", Message = "boom" };
        }

        public Task<int> CountTokensAsync(MessageGenerationRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public void Dispose()
        {
        }
    }

    private sealed class DonelessGenerator : IMessageGenerator
    {
        public int Calls { get; private set; }

        public Task<MessageResponse> GenerateMessageAsync(MessageGenerationRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
            MessageGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Calls++;
            yield return new StreamingMessageBeginResponse();
            await Task.Yield();
            yield return new StreamingContentAddedResponse { Index = 0, Content = new TextMessageContent { Value = "partial" } };
        }

        public Task<int> CountTokensAsync(MessageGenerationRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public void Dispose()
        {
        }
    }

    private sealed class EchoTool : ITool
    {
        public string UniqueName => "echo";

        public string? Description => null;

        public object? Parameters => null;

        public bool RequiresApproval => false;

        public Task<ToolOutput> InvokeAsync(ToolInput input, CancellationToken cancellationToken = default)
            => Task.FromResult(ToolOutput.Success("echoed"));
    }

    private sealed class CountingMiddleware : IMessageMiddleware
    {
        public const string Key = "middleware-turns";

        public Task<MessageResponse> GenerateAsync(
            MessageContext context,
            Func<MessageContext, Task<MessageResponse>> next,
            CancellationToken cancellationToken = default)
        {
            Count(context);
            return next(context);
        }

        public IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingAsync(
            MessageContext context,
            Func<MessageContext, IAsyncEnumerable<StreamingMessageResponse>> next,
            CancellationToken cancellationToken = default)
        {
            Count(context);
            return next(context);
        }

        private static void Count(MessageContext context)
            => context.Items[Key] = context.Items.Get<int>(Key) + 1;
    }

    private sealed class BufferedOnlyMiddleware : IMessageMiddleware
    {
        public const string Key = "buffered-only";

        public Task<MessageResponse> GenerateAsync(
            MessageContext context,
            Func<MessageContext, Task<MessageResponse>> next,
            CancellationToken cancellationToken = default)
        {
            context.Items[Key] = true;
            return next(context);
        }
    }
}
