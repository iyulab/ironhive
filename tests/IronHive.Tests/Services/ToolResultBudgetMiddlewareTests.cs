using System.Runtime.CompilerServices;
using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Core.Services;
using IronHive.Core.Tools;

namespace IronHive.Tests.Services;

// A tool loop whose results each fit a per-result cap can still overflow a small context window,
// because the results add up across rounds. These tests describe the request the generator
// receives on every turn -- the only thing the context window ever sees.
public class ToolResultBudgetMiddlewareTests
{
    private const string Provider = "openai";
    private const int ResultChars = 6_000;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ResultsAcrossRounds_NeverExceedTheBudget_AndToolsStopOnceItIsSpent(bool streaming)
    {
        // One tool call per round, three rounds of 6,000 chars, then the answer.
        string[][] script = [["big"], ["big"], ["big"], []];

        var turns = await RunAsync(script, new ToolResultBudgetMiddleware(10_000), streaming);

        turns.Should().HaveCount(4, "the fixture must run three tool rounds and an answer");
        turns[1].ResultChars.Should().Be(ResultChars, "the first result fits whole");
        turns[2].ResultChars.Should().Be(10_000, "the second result is cut to what is left");
        turns[3].ResultChars.Should().Be(10_000, "the third result gets nothing");
        turns[3].Notices.Should().Be(1, "a result with no budget left is replaced by the notice");
        turns[1].ToolChoice.Should().BeNull("tools stay available while budget remains");
        turns[2].ToolChoice.Should().Be(ToolChoice.None, "once the budget is spent the model should answer");
        turns[3].ToolChoice.Should().Be(ToolChoice.None);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ParallelResults_AreAllocatedInCallOrder(bool streaming)
    {
        string[][] script = [["big", "big", "big"], []];

        var turns = await RunAsync(script, new ToolResultBudgetMiddleware(10_000), streaming, maxParallel: 3);

        turns[1].PerResult.Should().Equal(
            [ResultChars, 10_000 - ResultChars, DefaultNoticeLength()],
            "results are shared out in the order the model called them, whichever finished first");
        turns[1].Notices.Should().Be(1);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ResultsWithinBudget_AreSentUnchanged(bool streaming)
    {
        string[][] script = [["big"], ["big"], []];

        var turns = await RunAsync(script, new ToolResultBudgetMiddleware(100_000), streaming);

        turns[2].PerResult.Should().Equal([ResultChars, ResultChars]);
        turns[2].Texts.Should().AllSatisfy(t => t.Should().Be(BigTool.Output));
        turns.Should().AllSatisfy(t => t.ToolChoice.Should().BeNull());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WithoutTheMiddleware_TheSameLoopSendsEveryResultWhole(bool streaming)
    {
        // The defect the middleware exists for, pinned: nothing else bounds the sum.
        string[][] script = [["big"], ["big"], ["big"], []];

        var turns = await RunAsync(script, middleware: null, streaming);

        turns[3].ResultChars.Should().Be(3 * ResultChars);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_RejectsANonPositiveBudget(int maxTotalChars)
    {
        var act = () => new ToolResultBudgetMiddleware(maxTotalChars);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private static int DefaultNoticeLength() => ToolResultBudgetMiddleware.DefaultExhaustedNotice.Length;

    private static async Task<List<TurnRequest>> RunAsync(
        string[][] script,
        IMessageMiddleware? middleware,
        bool streaming,
        int maxParallel = 3)
    {
        var generator = new ScriptedToolGenerator(script);
        var service = new MessageService(
            new Dictionary<string, IMessageGenerator> { [Provider] = generator },
            middleware is null ? [] : [middleware]);

        var request = new MessageRequest
        {
            Provider = Provider,
            Model = "local",
            Messages = [Message.User("look it up")],
            Tools = new ToolCollection([new BigTool()]),
            ToolOptions = new ToolOptions { MaxParallel = maxParallel },
        };

        if (streaming)
        {
            await foreach (var _ in service.GenerateStreamingMessageAsync(request, TestContext.Current.CancellationToken))
            {
            }
        }
        else
        {
            await service.GenerateMessageAsync(request, TestContext.Current.CancellationToken);
        }

        return generator.Requests;
    }

    private sealed record TurnRequest(IReadOnlyList<string> Texts, ToolChoice? ToolChoice)
    {
        public int[] PerResult => [.. Texts.Select(t => t.Length)];

        public int Notices => Texts.Count(t => t == ToolResultBudgetMiddleware.DefaultExhaustedNotice);

        public int ResultChars => Texts.Where(t => t != ToolResultBudgetMiddleware.DefaultExhaustedNotice).Sum(t => t.Length);
    }

    // Each turn calls the tools named in its script row; an empty row ends with an answer. Every
    // request is snapshotted as it arrives, because the service keeps editing the same objects.
    private sealed class ScriptedToolGenerator(string[][] script) : IMessageGenerator
    {
        private int _calls;

        public List<TurnRequest> Requests { get; } = [];

        public Task<MessageResponse> GenerateMessageAsync(MessageGenerationRequest request, CancellationToken cancellationToken = default)
        {
            var (tools, reason) = Next(request);
            return Task.FromResult(new MessageResponse
            {
                ResponseId = $"r{_calls}",
                DoneReason = reason,
                Message = new Message
                {
                    Role = MessageRole.Assistant,
                    Content = tools.Length == 0
                        ? [new TextMessageContent { Value = "done" }]
                        : [.. tools.Select((name, i) => (MessageContent)Call(name, i))],
                },
            });
        }

        public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
            MessageGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var (tools, reason) = Next(request);
            yield return new StreamingMessageBeginResponse();

            if (tools.Length == 0)
            {
                yield return new StreamingContentAddedResponse { Index = 0, Content = new TextMessageContent() };
                yield return new StreamingContentDeltaResponse { Index = 0, Delta = new TextDeltaContent { Value = "done" } };
                yield return new StreamingContentCompletedResponse { Index = 0 };
            }

            for (var i = 0; i < tools.Length; i++)
            {
                var call = Call(tools[i], i);
                call.Input = string.Empty;
                yield return new StreamingContentAddedResponse { Index = i, Content = call };
                yield return new StreamingContentDeltaResponse { Index = i, Delta = new ToolDeltaContent { Input = "{}" } };
                yield return new StreamingContentCompletedResponse { Index = i };
                await Task.Yield();
            }

            yield return new StreamingMessageDoneResponse { ResponseId = $"r{_calls}", DoneReason = reason };
        }

        public Task<int> CountTokensAsync(MessageGenerationRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public void Dispose()
        {
        }

        private (string[] Tools, MessageDoneReason Reason) Next(MessageGenerationRequest request)
        {
            var texts = request.Messages
                .SelectMany(m => m.Content.OfType<ToolMessageContent>())
                .Where(t => t.Output is not null)
                .Select(t => string.Concat(t.Output!.Content.OfType<TextMessageContent>().Select(c => c.Value)))
                .ToList();
            Requests.Add(new TurnRequest(texts, request.ToolChoice));

            var tools = script[Math.Min(_calls, script.Length - 1)];
            _calls++;
            return (tools, tools.Length == 0 ? MessageDoneReason.EndTurn : MessageDoneReason.ToolCall);
        }

        private ToolMessageContent Call(string name, int index) => new()
        {
            Id = $"call-{_calls}-{index}",
            Name = name,
            Input = "{}",
            IsApproved = true,
        };
    }

    // Lines, so a line-based compactor would be tempted to keep head and tail; the budget cuts by chars.
    private sealed class BigTool : ITool
    {
        private const string Line = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-=+*/\n";

        public static readonly string Output = string.Concat(Enumerable.Repeat(Line, ResultChars / Line.Length + 1))[..ResultChars];

        public string UniqueName => "big";

        public string? Description => null;

        public object? Parameters => null;

        public bool RequiresApproval => false;

        public Task<ToolOutput> InvokeAsync(ToolInput input, CancellationToken cancellationToken = default)
            => Task.FromResult(ToolOutput.Success(Output));
    }
}
