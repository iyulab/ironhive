using FluentAssertions;
using IronHive.Abstractions.Exceptions;
using IronHive.Abstractions.Messages;
using IronHive.Core.Services;
using NSubstitute;

namespace IronHive.Tests.Services;

/// <summary>
/// Context budget enforcement (MessageRequest.ContextPolicy): preflight detection before
/// any provider call, compaction hook, fallback estimation, and tool-loop re-enforcement.
/// </summary>
public class MessageServiceContextPolicyTests
{
    private readonly Dictionary<string, IMessageGenerator> _generators = [];
    private readonly MessageService _service;
    private readonly IMessageGenerator _generator = Substitute.For<IMessageGenerator>();

    public MessageServiceContextPolicyTests()
    {
        _service = new MessageService(_generators);
        _generators["test"] = _generator;
        _generator
            .GenerateMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MessageResponse
            {
                DoneReason = MessageDoneReason.EndTurn,
                Message = Message.Assistant("ok"),
            });
    }

    private static MessageRequest NewRequest(ContextPolicy? policy) => new()
    {
        Provider = "test",
        Model = "test-model",
        Messages = [Message.User("Hello")],
        ContextPolicy = policy,
    };

    [Fact]
    public async Task NoPolicy_Preserves_Existing_Behavior_And_Never_Counts()
    {
        var response = await _service.GenerateMessageAsync(NewRequest(policy: null));

        response.DoneReason.Should().Be(MessageDoneReason.EndTurn);
        await _generator.DidNotReceive().CountTokensAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Fail_Policy_Throws_Preflight_Without_Network_Call()
    {
        _generator.CountTokensAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(45_010);

        var act = () => _service.GenerateMessageAsync(NewRequest(new ContextPolicy { MaxInputTokens = 32_768 }));

        var ex = (await act.Should().ThrowAsync<ContextWindowExceededException>()).Which;
        ex.PromptTokens.Should().Be(45_010);
        ex.IsPreflightRejection.Should().BeTrue();
        await _generator.DidNotReceive().GenerateMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Under_Budget_Proceeds_Normally()
    {
        _generator.CountTokensAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(1_000);

        var response = await _service.GenerateMessageAsync(NewRequest(new ContextPolicy { MaxInputTokens = 32_768 }));

        response.DoneReason.Should().Be(MessageDoneReason.EndTurn);
        await _generator.Received(1).GenerateMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Compact_Policy_Invokes_Compactor_Then_Proceeds_When_Under_Budget()
    {
        // Over budget on first estimate, under budget after compaction.
        _generator.CountTokensAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(45_010, 10_000);

        var compactor = Substitute.For<IMessageCompactor>();
        compactor.CompactAsync(Arg.Any<MessageCompactionContext>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult<IList<Message>>([Message.User("summarized")]));

        // The pipeline mutates the request after the call, so capture the state at call time.
        var messageCountAtCall = -1;
        _generator
            .When(g => g.GenerateMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>()))
            .Do(ci => messageCountAtCall = ci.Arg<MessageGenerationRequest>().Messages.Count);

        var response = await _service.GenerateMessageAsync(NewRequest(new ContextPolicy
        {
            MaxInputTokens = 32_768,
            OnOverflow = ContextOverflowBehavior.Compact,
            Compactor = compactor,
        }));

        response.DoneReason.Should().Be(MessageDoneReason.EndTurn);
        await compactor.Received(1).CompactAsync(
            Arg.Is<MessageCompactionContext>(c => c.EstimatedTokens == 45_010 && c.BudgetTokens == 32_768),
            Arg.Any<CancellationToken>());
        messageCountAtCall.Should().Be(1, "the compacted message list should be sent to the provider");
    }

    [Fact]
    public async Task Compact_Policy_Throws_When_Still_Over_Budget_After_Compaction()
    {
        _generator.CountTokensAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(45_010, 40_000);

        var compactor = Substitute.For<IMessageCompactor>();
        compactor.CompactAsync(Arg.Any<MessageCompactionContext>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult<IList<Message>>([Message.User("still huge")]));

        var act = () => _service.GenerateMessageAsync(NewRequest(new ContextPolicy
        {
            MaxInputTokens = 32_768,
            OnOverflow = ContextOverflowBehavior.Compact,
            Compactor = compactor,
        }));

        var ex = (await act.Should().ThrowAsync<ContextWindowExceededException>()).Which;
        ex.PromptTokens.Should().Be(40_000);
        ex.IsPreflightRejection.Should().BeTrue();
    }

    [Fact]
    public async Task Compact_Without_Compactor_Falls_Back_To_Fail()
    {
        _generator.CountTokensAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(45_010);

        var act = () => _service.GenerateMessageAsync(NewRequest(new ContextPolicy
        {
            MaxInputTokens = 32_768,
            OnOverflow = ContextOverflowBehavior.Compact,
        }));

        await act.Should().ThrowAsync<ContextWindowExceededException>();
    }

    [Fact]
    public async Task FallbackEstimator_Used_When_Provider_Counting_Unsupported()
    {
        _generator.CountTokensAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<int>>(_ => throw new NotSupportedException());

        var act = () => _service.GenerateMessageAsync(NewRequest(new ContextPolicy
        {
            MaxInputTokens = 32_768,
            FallbackEstimator = _ => 50_000,
        }));

        var ex = (await act.Should().ThrowAsync<ContextWindowExceededException>()).Which;
        ex.PromptTokens.Should().Be(50_000);
    }

    [Fact]
    public async Task Active_Policy_Without_Any_Estimation_Path_Is_A_Config_Error()
    {
        _generator.CountTokensAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<int>>(_ => throw new NotSupportedException());

        var act = () => _service.GenerateMessageAsync(NewRequest(new ContextPolicy { MaxInputTokens = 32_768 }));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*FallbackEstimator*");
    }

    [Fact]
    public async Task Tool_Loop_Reenforces_Policy_On_Every_Iteration()
    {
        // First iteration under budget; after the tool result grows the request,
        // the second iteration exceeds the budget and must be blocked.
        _generator.CountTokensAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(1_000, 45_010);

        var toolContent = new IronHive.Abstractions.Messages.Content.ToolMessageContent
        {
            Id = "call-1",
            Name = "missing-tool",
            IsApproved = true,
        };
        var firstResponse = new MessageResponse
        {
            DoneReason = MessageDoneReason.ToolCall,
            Message = new Message { Role = MessageRole.Assistant, Content = [toolContent] },
        };
        _generator
            .GenerateMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(firstResponse);

        var act = () => _service.GenerateMessageAsync(NewRequest(new ContextPolicy { MaxInputTokens = 32_768 }));

        await act.Should().ThrowAsync<ContextWindowExceededException>();
        await _generator.Received(1).GenerateMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>());
        await _generator.Received(2).CountTokensAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task First_Compaction_Context_Has_Original_Count_And_Null_Previous()
    {
        _generator.CountTokensAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(45_010, 10_000);

        MessageCompactionContext? captured = null;
        var compactor = Substitute.For<IMessageCompactor>();
        compactor.CompactAsync(Arg.Any<MessageCompactionContext>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                captured = ci.Arg<MessageCompactionContext>();
                return Task.FromResult<IList<Message>>([Message.User("summarized")]);
            });

        await _service.GenerateMessageAsync(NewRequest(new ContextPolicy
        {
            MaxInputTokens = 32_768,
            OnOverflow = ContextOverflowBehavior.Compact,
            Compactor = compactor,
        }));

        captured.Should().NotBeNull();
        captured!.OriginalMessageCount.Should().Be(1, "the request started with a single user message");
        captured.PreviousCompactedMessages.Should().BeNull("no compaction preceded this one in the request");
    }

    [Fact]
    public async Task Tool_Loop_Compaction_Distinguishes_Original_From_Loop_Added_Messages()
    {
        // Iteration 1 under budget; the tool loop appends an assistant message, and
        // iteration 2 exceeds the budget — the compactor must still see the original count.
        _generator.CountTokensAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(1_000, 45_010, 10_000);

        var toolContent = new IronHive.Abstractions.Messages.Content.ToolMessageContent
        {
            Id = "call-1",
            Name = "missing-tool",
            IsApproved = true,
        };
        _generator
            .GenerateMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                new MessageResponse
                {
                    DoneReason = MessageDoneReason.ToolCall,
                    Message = new Message { Role = MessageRole.Assistant, Content = [toolContent] },
                },
                new MessageResponse
                {
                    DoneReason = MessageDoneReason.EndTurn,
                    Message = Message.Assistant("done"),
                });

        MessageCompactionContext? captured = null;
        var compactor = Substitute.For<IMessageCompactor>();
        compactor.CompactAsync(Arg.Any<MessageCompactionContext>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                captured = ci.Arg<MessageCompactionContext>();
                return Task.FromResult<IList<Message>>([Message.User("summarized")]);
            });

        await _service.GenerateMessageAsync(NewRequest(new ContextPolicy
        {
            MaxInputTokens = 32_768,
            OnOverflow = ContextOverflowBehavior.Compact,
            Compactor = compactor,
        }));

        captured.Should().NotBeNull();
        captured!.Messages.Count.Should().Be(2, "the tool loop appended one assistant message");
        captured.OriginalMessageCount.Should().Be(1, "only one message existed at request start");
    }

    [Fact]
    public async Task Second_Compaction_In_Same_Request_Receives_Previous_Result()
    {
        // Both iterations exceed the budget: compaction fires twice in one request,
        // and the second invocation must see the first invocation's returned messages.
        _generator.CountTokensAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(45_010, 10_000, 45_010, 10_000);

        var toolContent = new IronHive.Abstractions.Messages.Content.ToolMessageContent
        {
            Id = "call-1",
            Name = "missing-tool",
            IsApproved = true,
        };
        _generator
            .GenerateMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                new MessageResponse
                {
                    DoneReason = MessageDoneReason.ToolCall,
                    Message = new Message { Role = MessageRole.Assistant, Content = [toolContent] },
                },
                new MessageResponse
                {
                    DoneReason = MessageDoneReason.EndTurn,
                    Message = Message.Assistant("done"),
                });

        var firstSummary = Message.User("summary-1");
        var contexts = new List<MessageCompactionContext>();
        var compactor = Substitute.For<IMessageCompactor>();
        compactor.CompactAsync(Arg.Any<MessageCompactionContext>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                contexts.Add(ci.Arg<MessageCompactionContext>());
                return Task.FromResult<IList<Message>>(contexts.Count == 1
                    ? [firstSummary]
                    : [Message.User("summary-2")]);
            });

        await _service.GenerateMessageAsync(NewRequest(new ContextPolicy
        {
            MaxInputTokens = 32_768,
            OnOverflow = ContextOverflowBehavior.Compact,
            Compactor = compactor,
        }));

        contexts.Should().HaveCount(2, "the budget was exceeded on both loop iterations");
        contexts[0].PreviousCompactedMessages.Should().BeNull();
        contexts[1].PreviousCompactedMessages.Should().NotBeNull();
        contexts[1].PreviousCompactedMessages.Should().ContainSingle()
            .Which.Should().BeSameAs(firstSummary, "the second compaction must see the first compaction's returned messages");
        contexts[1].OriginalMessageCount.Should().Be(1, "the original count is a request-start fact, unaffected by compaction");
    }

    [Fact]
    public async Task Streaming_Compaction_Context_Carries_Pipeline_State()
    {
        _generator.CountTokensAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(45_010, 10_000);
        _generator
            .GenerateStreamingMessageAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(ToAsyncEnumerable<StreamingMessageResponse>(
                new StreamingMessageDoneResponse { DoneReason = MessageDoneReason.EndTurn }));

        MessageCompactionContext? captured = null;
        var compactor = Substitute.For<IMessageCompactor>();
        compactor.CompactAsync(Arg.Any<MessageCompactionContext>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                captured = ci.Arg<MessageCompactionContext>();
                return Task.FromResult<IList<Message>>([Message.User("summarized")]);
            });

        await foreach (var _ in _service.GenerateStreamingMessageAsync(NewRequest(new ContextPolicy
        {
            MaxInputTokens = 32_768,
            OnOverflow = ContextOverflowBehavior.Compact,
            Compactor = compactor,
        })))
        { }

        captured.Should().NotBeNull();
        captured!.OriginalMessageCount.Should().Be(1);
        captured.PreviousCompactedMessages.Should().BeNull();
    }

    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(params T[] items)
    {
        foreach (var item in items)
        {
            yield return item;
        }
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Streaming_Fail_Policy_Throws_Preflight()
    {
        _generator.CountTokensAsync(Arg.Any<MessageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(45_010);

        var act = async () =>
        {
            await foreach (var _ in _service.GenerateStreamingMessageAsync(
                NewRequest(new ContextPolicy { MaxInputTokens = 32_768 })))
            { }
        };

        await act.Should().ThrowAsync<ContextWindowExceededException>();
    }
}
