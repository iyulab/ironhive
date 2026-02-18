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

public class OrchestratorAgentAdapterTests
{
    [Fact]
    public async Task AsAgent_ShouldWrapOrchestrator_AndReturnResult()
    {
        // Arrange: Sequential orchestrator with 2 mock agents
        var agent1 = new MockAgent("a1") { ResponseFunc = _ => "hello" };
        var agent2 = new MockAgent("a2") { ResponseFunc = _ => "world" };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            PassOutputAsInput = true
        });
        orch.AddAgents([agent1, agent2]);

        var adapter = orch.AsAgent("nested-orch", "Nested orchestrator");

        // Act
        var response = await adapter.InvokeAsync(MakeUserMessages("input"));

        // Assert
        adapter.Provider.Should().Be("orchestrator");
        adapter.Name.Should().Be("nested-orch");
        adapter.Description.Should().Be("Nested orchestrator");
        response.Should().NotBeNull();
        response.Message.Should().NotBeNull();
        GetText(response.Message).Should().Be("world");
    }

    [Fact]
    public async Task AsAgent_Nested_ShouldWorkInOuterOrchestrator()
    {
        // Arrange: inner orchestrator wrapped as agent, used in outer sequential
        var innerAgent = new MockAgent("inner") { ResponseFunc = _ => "inner-output" };
        var innerOrch = new SequentialOrchestrator();
        innerOrch.AddAgent(innerAgent);

        var outerAgent = new MockAgent("outer") { ResponseFunc = msgs => $"got: {GetLastAssistantText(msgs)}" };

        var outerOrch = new SequentialOrchestrator(new SequentialOrchestratorOptions { PassOutputAsInput = true });
        outerOrch.AddAgents([innerOrch.AsAgent(), outerAgent]);

        // Act
        var result = await outerOrch.ExecuteAsync(MakeUserMessages("start"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Steps.Should().HaveCount(2);
        GetTextFromMessage(result.FinalOutput).Should().Contain("got: inner-output");
    }

    [Fact]
    public async Task AsAgent_ShouldPropagateFailure()
    {
        // Arrange: orchestrator with failing agent
        var failAgent = new MockAgent("fail")
        {
            ResponseFuncAsync = _ => throw new InvalidOperationException("boom")
        };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions { StopOnAgentFailure = true });
        orch.AddAgent(failAgent);

        var adapter = orch.AsAgent();

        // Act & Assert
        var act = () => adapter.InvokeAsync(MakeUserMessages("go"));
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*failed*");
    }

    [Fact]
    public async Task AsAgent_Streaming_ShouldForwardMessageDeltaEvents()
    {
        // Arrange
        var agent = new MockAgent("streamer") { ResponseFunc = _ => "streamed-data" };
        var orch = new SequentialOrchestrator();
        orch.AddAgent(agent);

        var adapter = orch.AsAgent();

        // Act
        var events = new List<StreamingMessageResponse>();
        await foreach (var evt in adapter.InvokeStreamingAsync(MakeUserMessages("go")))
        {
            events.Add(evt);
        }

        // Assert: should have streaming events forwarded
        events.Should().NotBeEmpty();
    }

    #region Helpers

    private static IEnumerable<Message> MakeUserMessages(string text)
    {
        return [new UserMessage { Content = [new TextMessageContent { Value = text }] }];
    }

    private static string GetText(AssistantMessage msg)
    {
        return msg.Content.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "";
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

    private static string GetLastAssistantText(IEnumerable<Message> messages)
    {
        var last = messages.OfType<AssistantMessage>().LastOrDefault();
        return last?.Content.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "";
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
            var text = ResponseFunc != null ? ResponseFunc(messages) : "stream";
            yield return new StreamingMessageBeginResponse { Id = Guid.NewGuid().ToString("N") };
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
}
