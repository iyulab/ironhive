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

public class ContextScopeTests
{
    #region LastNMessagesScope

    [Fact]
    public void LastNMessagesScope_FewMessages_ReturnsAll()
    {
        var scope = new LastNMessagesScope(5);
        var messages = MakeMessages("msg1", "msg2", "msg3");

        var result = scope.ScopeMessages(messages, "agent1");

        result.Should().HaveCount(3);
    }

    [Fact]
    public void LastNMessagesScope_KeepsFirstAndLastN()
    {
        var scope = new LastNMessagesScope(2);
        var messages = MakeMessages("first", "second", "third", "fourth", "fifth");

        var result = scope.ScopeMessages(messages, "agent1");

        result.Should().HaveCount(3); // first + last 2
        GetText(result[0]).Should().Be("first");
        GetText(result[1]).Should().Be("fourth");
        GetText(result[2]).Should().Be("fifth");
    }

    [Fact]
    public void LastNMessagesScope_ExactBoundary_ReturnsAll()
    {
        var scope = new LastNMessagesScope(4);
        var messages = MakeMessages("msg1", "msg2", "msg3", "msg4", "msg5");

        var result = scope.ScopeMessages(messages, "agent1");

        // 5 messages, maxMessages=4, boundary is 4+1=5, so returns all
        result.Should().HaveCount(5);
    }

    [Fact]
    public void LastNMessagesScope_MaxMessages1_KeepsFirstAndLast()
    {
        var scope = new LastNMessagesScope(1);
        var messages = MakeMessages("first", "middle", "last");

        var result = scope.ScopeMessages(messages, "agent1");

        result.Should().HaveCount(2);
        GetText(result[0]).Should().Be("first");
        GetText(result[1]).Should().Be("last");
    }

    [Fact]
    public void LastNMessagesScope_InvalidMaxMessages_Throws()
    {
        var act = () => new LastNMessagesScope(0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region TaskOnlyScope

    [Fact]
    public void TaskOnlyScope_ReturnsLastUserMessage()
    {
        var scope = new TaskOnlyScope();
        var messages = new List<Message>
        {
            new UserMessage { Content = [new TextMessageContent { Value = "first task" }] },
            new AssistantMessage { Content = [new TextMessageContent { Value = "response" }] },
            new UserMessage { Content = [new TextMessageContent { Value = "second task" }] }
        };

        var result = scope.ScopeMessages(messages, "agent1");

        result.Should().HaveCount(1);
        GetText(result[0]).Should().Be("second task");
    }

    [Fact]
    public void TaskOnlyScope_NoUserMessage_ReturnsAll()
    {
        var scope = new TaskOnlyScope();
        var messages = new List<Message>
        {
            new AssistantMessage { Content = [new TextMessageContent { Value = "response" }] }
        };

        var result = scope.ScopeMessages(messages, "agent1");

        result.Should().HaveCount(1);
    }

    [Fact]
    public void TaskOnlyScope_SingleUserMessage_ReturnsThat()
    {
        var scope = new TaskOnlyScope();
        var messages = new List<Message>
        {
            new UserMessage { Content = [new TextMessageContent { Value = "only task" }] }
        };

        var result = scope.ScopeMessages(messages, "agent1");

        result.Should().HaveCount(1);
        GetText(result[0]).Should().Be("only task");
    }

    #endregion

    #region Integration with ParallelOrchestrator

    [Fact]
    public async Task ParallelOrchestrator_WithTaskOnlyScope_IsolatesAgents()
    {
        // Capture what each agent receives
        List<Message>? agent1Messages = null;
        List<Message>? agent2Messages = null;

        var agent1 = new MockAgent("agent1")
        {
            ResponseFunc = msgs =>
            {
                agent1Messages = msgs.ToList();
                return "result1";
            }
        };
        var agent2 = new MockAgent("agent2")
        {
            ResponseFunc = msgs =>
            {
                agent2Messages = msgs.ToList();
                return "result2";
            }
        };

        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            ContextScope = new TaskOnlyScope()
        });
        orch.AddAgents([agent1, agent2]);

        // Send multiple messages, but agents should only see the last user message
        var messages = new List<Message>
        {
            new UserMessage { Content = [new TextMessageContent { Value = "previous context" }] },
            new AssistantMessage { Content = [new TextMessageContent { Value = "some response" }] },
            new UserMessage { Content = [new TextMessageContent { Value = "current task" }] }
        };

        var result = await orch.ExecuteAsync(messages);

        result.IsSuccess.Should().BeTrue();

        // Each agent should only see the task message
        agent1Messages.Should().HaveCount(1);
        GetText(agent1Messages![0]).Should().Be("current task");

        agent2Messages.Should().HaveCount(1);
        GetText(agent2Messages![0]).Should().Be("current task");
    }

    [Fact]
    public async Task ParallelOrchestrator_WithLastNScope_LimitsContext()
    {
        List<Message>? capturedMessages = null;

        var agent = new MockAgent("agent1")
        {
            ResponseFunc = msgs =>
            {
                capturedMessages = msgs.ToList();
                return "result";
            }
        };

        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            ContextScope = new LastNMessagesScope(1)
        });
        orch.AddAgent(agent);

        var messages = MakeMessages("first", "second", "third");

        var result = await orch.ExecuteAsync(messages);

        result.IsSuccess.Should().BeTrue();

        // Should keep first message + last 1 message
        capturedMessages.Should().HaveCount(2);
        GetText(capturedMessages![0]).Should().Be("first");
        GetText(capturedMessages[1]).Should().Be("third");
    }

    [Fact]
    public async Task ParallelOrchestrator_NoScope_PassesFullContext()
    {
        List<Message>? capturedMessages = null;

        var agent = new MockAgent("agent1")
        {
            ResponseFunc = msgs =>
            {
                capturedMessages = msgs.ToList();
                return "result";
            }
        };

        var orch = new ParallelOrchestrator(); // No scope
        orch.AddAgent(agent);

        var messages = MakeMessages("first", "second", "third");

        var result = await orch.ExecuteAsync(messages);

        result.IsSuccess.Should().BeTrue();
        capturedMessages.Should().HaveCount(3);
    }

    [Fact]
    public async Task SequentialOrchestrator_WithLastNScope_LimitsContext()
    {
        var capturedMessages = new List<List<Message>>();

        var agent1 = new MockAgent("agent1")
        {
            ResponseFunc = msgs =>
            {
                capturedMessages.Add(msgs.ToList());
                return "output1";
            }
        };
        var agent2 = new MockAgent("agent2")
        {
            ResponseFunc = msgs =>
            {
                capturedMessages.Add(msgs.ToList());
                return "output2";
            }
        };

        var orch = new SequentialOrchestrator(new SequentialOrchestratorOptions
        {
            AccumulateHistory = true,
            ContextScope = new LastNMessagesScope(2)
        });
        orch.AddAgents([agent1, agent2]);

        var messages = MakeMessages("task1", "task2", "task3", "task4");

        await orch.ExecuteAsync(messages);

        // Agent2 should get scoped messages from accumulated history
        // Full history would be: task1, task2, task3, task4, output1
        // Scoped: first (task1) + last 2 (task4, output1)
        capturedMessages.Should().HaveCount(2);
    }

    #endregion

    #region Helpers

    private static List<Message> MakeMessages(params string[] texts)
    {
        return texts.Select(t =>
            (Message)new UserMessage { Content = [new TextMessageContent { Value = t }] }
        ).ToList();
    }

    private static string GetText(Message message)
    {
        return message switch
        {
            UserMessage u => u.Content.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "",
            AssistantMessage a => a.Content.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "",
            _ => ""
        };
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
        }
    }

    #endregion
}
