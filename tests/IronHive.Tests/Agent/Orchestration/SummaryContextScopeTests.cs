using FluentAssertions;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Tools;
using IronHive.Core.Agent.Orchestration;

namespace IronHive.Tests.Agent.Orchestration;

public class SummaryContextScopeTests
{
    #region Basic Behavior

    [Fact]
    public void FewMessages_BelowThreshold_ReturnsAll()
    {
        var scope = new SummaryContextScope();
        var messages = MakeUserMessages("msg1", "msg2", "msg3");

        var result = scope.ScopeMessages(messages, "agent1");

        result.Should().HaveCount(3);
        result.Should().BeSameAs(messages); // Same reference, no copy
    }

    [Fact]
    public void ExactThreshold_ReturnsAll()
    {
        var scope = new SummaryContextScope(new SummaryContextScopeOptions { MinMessagesForSummary = 4 });
        var messages = MakeUserMessages("msg1", "msg2", "msg3", "msg4");

        var result = scope.ScopeMessages(messages, "agent1");

        result.Should().HaveCount(4); // Exactly at threshold, returns all
    }

    [Fact]
    public void AboveThreshold_CreatesSummaryPlusTask()
    {
        var scope = new SummaryContextScope(new SummaryContextScopeOptions { MinMessagesForSummary = 2 });
        var messages = MakeConversation(
            ("user", "Analyze the project structure"),
            ("assistant", "I'll scan the directory..."),
            ("user", "Now fix the bug in Foo.cs")
        );

        var result = scope.ScopeMessages(messages, "agent1");

        result.Should().HaveCount(2); // Summary + current task
        GetText(result[0]).Should().Contain("[Context Summary]");
        GetText(result[0]).Should().Contain("Analyze the project structure");
        GetText(result[1]).Should().Be("Now fix the bug in Foo.cs");
    }

    [Fact]
    public void EmptyMessages_ReturnsEmpty()
    {
        var scope = new SummaryContextScope(new SummaryContextScopeOptions { MinMessagesForSummary = 0 });
        var messages = new List<Message>();

        var result = scope.ScopeMessages(messages, "agent1");

        result.Should().BeEmpty();
    }

    [Fact]
    public void SingleUserMessage_ReturnsOriginal()
    {
        var scope = new SummaryContextScope(new SummaryContextScopeOptions { MinMessagesForSummary = 0 });
        var messages = MakeUserMessages("single task");

        var result = scope.ScopeMessages(messages, "agent1");

        // Single message: summary = goal, task = same message → just one message
        result.Should().HaveCount(1);
    }

    #endregion

    #region Goal Extraction

    [Fact]
    public void ExtractsGoalFromFirstUserMessage()
    {
        var scope = new SummaryContextScope(new SummaryContextScopeOptions { MinMessagesForSummary = 2 });
        var messages = MakeConversation(
            ("user", "Fix the authentication bug in login.cs"),
            ("assistant", "Looking into it..."),
            ("user", "What did you find?")
        );

        var result = scope.ScopeMessages(messages, "agent1");
        var summary = GetText(result[0]);

        summary.Should().Contain("Goal: Fix the authentication bug in login.cs");
    }

    [Fact]
    public void TruncatesLongGoal()
    {
        var longGoal = new string('A', 300);
        var scope = new SummaryContextScope(new SummaryContextScopeOptions
        {
            MinMessagesForSummary = 2,
            MaxGoalLength = 50
        });
        var messages = MakeConversation(
            ("user", longGoal),
            ("assistant", "OK"),
            ("user", "continue")
        );

        var result = scope.ScopeMessages(messages, "agent1");
        var summary = GetText(result[0]);

        summary.Should().Contain("Goal: ");
        summary.Should().Contain("...");
        // Goal should be truncated to ~50 chars
        var goalLine = summary.Split('\n').First(l => l.StartsWith("Goal:", StringComparison.Ordinal));
        goalLine.Length.Should().BeLessThan(70); // Goal: + truncated + ...
    }

    #endregion

    #region Tool Action Extraction

    [Fact]
    public void ExtractsToolActions()
    {
        var scope = new SummaryContextScope(new SummaryContextScopeOptions { MinMessagesForSummary = 2 });
        var messages = new List<Message>
        {
            new UserMessage { Content = [new TextMessageContent { Value = "Fix the bug" }] },
            MakeAssistantWithTools(
                ("read_file", """{"path": "src/Foo.cs"}"""),
                ("grep", """{"pattern": "TODO", "path": "src/"}""")
            ),
            new UserMessage { Content = [new TextMessageContent { Value = "Continue" }] }
        };

        var result = scope.ScopeMessages(messages, "agent1");
        var summary = GetText(result[0]);

        summary.Should().Contain("Actions:");
        summary.Should().Contain("read_file(src/Foo.cs)");
        summary.Should().Contain("grep(TODO)");
    }

    [Fact]
    public void TracksFileModifications()
    {
        var scope = new SummaryContextScope(new SummaryContextScopeOptions { MinMessagesForSummary = 2 });
        var messages = new List<Message>
        {
            new UserMessage { Content = [new TextMessageContent { Value = "Edit files" }] },
            MakeAssistantWithTools(
                ("write_file", """{"path": "src/New.cs"}"""),
                ("edit_file", """{"file_path": "src/Old.cs"}"""),
                ("read_file", """{"path": "src/ReadOnly.cs"}""")
            ),
            new UserMessage { Content = [new TextMessageContent { Value = "Done?" }] }
        };

        var result = scope.ScopeMessages(messages, "agent1");
        var summary = GetText(result[0]);

        summary.Should().Contain("Files modified:");
        var filesLine = summary.Split('\n').First(l => l.StartsWith("Files modified:", StringComparison.Ordinal));
        filesLine.Should().Contain("src/New.cs");
        filesLine.Should().Contain("src/Old.cs");
        filesLine.Should().NotContain("src/ReadOnly.cs"); // read_file is not file-modifying
    }

    [Fact]
    public void DeduplicatesToolActions()
    {
        var scope = new SummaryContextScope(new SummaryContextScopeOptions { MinMessagesForSummary = 2 });
        var messages = new List<Message>
        {
            new UserMessage { Content = [new TextMessageContent { Value = "Search" }] },
            MakeAssistantWithTools(
                ("grep", """{"pattern": "foo"}"""),
                ("grep", """{"pattern": "foo"}"""),
                ("grep", """{"pattern": "bar"}""")
            ),
            new UserMessage { Content = [new TextMessageContent { Value = "Next" }] }
        };

        var result = scope.ScopeMessages(messages, "agent1");
        var summary = GetText(result[0]);

        // "grep(foo)" should appear only once
        var actionsLine = summary.Split('\n').First(l => l.StartsWith("Actions:", StringComparison.Ordinal));
        actionsLine.Split("grep(foo)").Length.Should().Be(2); // 1 occurrence = 2 parts
    }

    [Fact]
    public void HandlesToolWithNoInput()
    {
        var scope = new SummaryContextScope(new SummaryContextScopeOptions { MinMessagesForSummary = 2 });
        var messages = new List<Message>
        {
            new UserMessage { Content = [new TextMessageContent { Value = "Check" }] },
            new AssistantMessage
            {
                Content =
                [
                    new ToolMessageContent { Id = "1", Name = "get_status", IsApproved = true }
                ]
            },
            new UserMessage { Content = [new TextMessageContent { Value = "Next" }] }
        };

        var result = scope.ScopeMessages(messages, "agent1");
        var summary = GetText(result[0]);

        summary.Should().Contain("get_status");
    }

    #endregion

    #region Error Code Extraction

    [Fact]
    public void ExtractsErrorCodes()
    {
        var scope = new SummaryContextScope(new SummaryContextScopeOptions { MinMessagesForSummary = 2 });
        var messages = MakeConversation(
            ("user", "Fix the build"),
            ("assistant", "Found errors: CS8600, CA1822, IDE0005"),
            ("user", "Fix them")
        );

        var result = scope.ScopeMessages(messages, "agent1");
        var summary = GetText(result[0]);

        summary.Should().Contain("Errors:");
        summary.Should().Contain("CA1822");
        summary.Should().Contain("CS8600");
        summary.Should().Contain("IDE0005");
    }

    [Fact]
    public void NoErrorCodes_OmitsErrorsSection()
    {
        var scope = new SummaryContextScope(new SummaryContextScopeOptions { MinMessagesForSummary = 2 });
        var messages = MakeConversation(
            ("user", "Hello"),
            ("assistant", "Hi there"),
            ("user", "Help me")
        );

        var result = scope.ScopeMessages(messages, "agent1");
        var summary = GetText(result[0]);

        summary.Should().NotContain("Errors:");
    }

    #endregion

    #region Agent Name Parameter

    [Fact]
    public void AgentName_DoesNotAffectScoping()
    {
        // Current implementation doesn't use agentName, but it should still work
        var scope = new SummaryContextScope(new SummaryContextScopeOptions { MinMessagesForSummary = 2 });
        var messages = MakeConversation(
            ("user", "Task"),
            ("assistant", "Response"),
            ("user", "Next task")
        );

        var result1 = scope.ScopeMessages(messages, "agent1");
        var result2 = scope.ScopeMessages(messages, "agent2");

        GetText(result1[0]).Should().Be(GetText(result2[0]));
    }

    #endregion

    #region Integration with Orchestrator

    [Fact]
    public async Task ParallelOrchestrator_WithSummaryScope_ReducesContext()
    {
        List<Message>? agent1Input = null;
        List<Message>? agent2Input = null;

        var agent1 = new MockAgent("researcher")
        {
            ResponseFunc = msgs =>
            {
                agent1Input = msgs.ToList();
                return "research complete";
            }
        };
        var agent2 = new MockAgent("writer")
        {
            ResponseFunc = msgs =>
            {
                agent2Input = msgs.ToList();
                return "writing complete";
            }
        };

        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            ContextScope = new SummaryContextScope(new SummaryContextScopeOptions { MinMessagesForSummary = 2 })
        });
        orch.AddAgents([agent1, agent2]);

        var messages = new List<Message>
        {
            new UserMessage { Content = [new TextMessageContent { Value = "Initial goal: analyze codebase" }] },
            new AssistantMessage { Content = [new TextMessageContent { Value = "I will analyze the project structure first..." }] },
            new UserMessage { Content = [new TextMessageContent { Value = "Summarize your findings" }] }
        };

        var result = await orch.ExecuteAsync(messages);

        result.IsSuccess.Should().BeTrue();

        // Both agents should receive summary + task (not full history)
        agent1Input.Should().HaveCount(2);
        agent2Input.Should().HaveCount(2);

        // First message should be context summary
        GetText(agent1Input![0]).Should().Contain("[Context Summary]");
        GetText(agent1Input[0]).Should().Contain("Initial goal: analyze codebase");

        // Second message should be the current task
        GetText(agent1Input[1]).Should().Be("Summarize your findings");
    }

    #endregion

    #region Helpers

    private static List<Message> MakeUserMessages(params string[] texts)
    {
        return texts.Select(t =>
            (Message)new UserMessage { Content = [new TextMessageContent { Value = t }] }
        ).ToList();
    }

    private static List<Message> MakeConversation(params (string role, string text)[] turns)
    {
        return turns.Select<(string role, string text), Message>(t => t.role switch
        {
            "user" => new UserMessage { Content = [new TextMessageContent { Value = t.text }] },
            "assistant" => new AssistantMessage { Content = [new TextMessageContent { Value = t.text }] },
            _ => throw new ArgumentException($"Unknown role: {t.role}")
        }).ToList();
    }

    private static AssistantMessage MakeAssistantWithTools(params (string name, string? input)[] tools)
    {
        var content = new List<MessageContent>();
        var id = 0;
        foreach (var (name, input) in tools)
        {
            content.Add(new ToolMessageContent
            {
                Id = $"tool_{id++}",
                Name = name,
                Input = input,
                IsApproved = true
            });
        }

        return new AssistantMessage { Content = content };
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

    // Reuse MockAgent from ContextScopeTests
    private sealed class MockAgent : IronHive.Abstractions.Agent.IAgent
    {
        public string Provider { get; set; } = "mock";
        public string Model { get; set; } = "mock-model";
        public string Name { get; set; }
        public string Description { get; set; } = "Mock";
        public string? Instructions { get; set; }
        public IEnumerable<IronHive.Abstractions.Tools.ToolItem>? Tools { get; set; }
        public IronHive.Abstractions.Messages.MessageGenerationParameters? Parameters { get; set; }
        public Func<IEnumerable<Message>, string>? ResponseFunc { get; set; }

        public MockAgent(string name) { Name = name; }

        public Task<IronHive.Abstractions.Messages.MessageResponse> InvokeAsync(
            IEnumerable<Message> messages, CancellationToken ct = default)
        {
            var text = ResponseFunc != null ? ResponseFunc(messages) : $"MockAgent '{Name}'";
            return Task.FromResult(new IronHive.Abstractions.Messages.MessageResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                DoneReason = IronHive.Abstractions.Messages.MessageDoneReason.EndTurn,
                Message = new AssistantMessage
                {
                    Name = Name,
                    Content = [new TextMessageContent { Value = text }]
                },
                TokenUsage = new IronHive.Abstractions.Messages.MessageTokenUsage
                {
                    InputTokens = 10,
                    OutputTokens = text.Length
                }
            });
        }

        public async IAsyncEnumerable<IronHive.Abstractions.Messages.StreamingMessageResponse> InvokeStreamingAsync(
            IEnumerable<Message> messages,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
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
