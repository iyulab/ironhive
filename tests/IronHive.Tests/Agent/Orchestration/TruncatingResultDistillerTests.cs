using FluentAssertions;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Tools;
using IronHive.Core.Agent.Orchestration;

namespace IronHive.Tests.Agent.Orchestration;

public class TruncatingResultDistillerTests
{
    private readonly TruncatingResultDistiller _distiller = new();

    #region Pass-Through Behavior

    [Fact]
    public async Task ShortResult_PassesThrough()
    {
        var response = MakeResponse("Short result text");

        var result = await _distiller.DistillAsync("agent1", response);

        result.Should().BeSameAs(response);
    }

    [Fact]
    public async Task BelowMinThreshold_PassesThrough()
    {
        var shortText = new string('A', 2999); // Just below 3000 default threshold
        var response = MakeResponse(shortText);

        var result = await _distiller.DistillAsync("agent1", response);

        result.Should().BeSameAs(response);
    }

    [Fact]
    public async Task EmptyContent_PassesThrough()
    {
        var response = new MessageResponse
        {
            Id = "test",
            DoneReason = MessageDoneReason.EndTurn,
            Message = new AssistantMessage { Content = [] }
        };

        var result = await _distiller.DistillAsync("agent1", response);

        result.Should().BeSameAs(response);
    }

    #endregion

    #region Text Truncation

    [Fact]
    public async Task LongTextResult_IsTruncated()
    {
        var longText = new string('X', 5000);
        var response = MakeResponse(longText);
        var options = new ResultDistillationOptions
        {
            MaxOutputChars = 1000,
            MinInputCharsForDistillation = 3000
        };

        var result = await _distiller.DistillAsync("agent1", response, options);

        var distilledText = GetText(result);
        distilledText.Length.Should().BeLessThan(longText.Length);
        distilledText.Should().Contain("chars omitted");
    }

    [Fact]
    public async Task Truncation_PreservesHeadAndTail()
    {
        // Create text with identifiable head and tail
        var head = "HEAD_MARKER_" + new string('H', 2000);
        var middle = new string('M', 3000);
        var tail = new string('T', 2000) + "_TAIL_MARKER";
        var longText = head + middle + tail;

        var response = MakeResponse(longText);
        var options = new ResultDistillationOptions
        {
            MaxOutputChars = 2000,
            MinInputCharsForDistillation = 3000
        };

        var result = await _distiller.DistillAsync("agent1", response, options);

        var distilledText = GetText(result);
        distilledText.Should().StartWith("HEAD_MARKER_");
        distilledText.Should().EndWith("_TAIL_MARKER");
    }

    [Fact]
    public async Task Truncation_IncludesOmittedCount()
    {
        var longText = new string('A', 10000);
        var response = MakeResponse(longText);
        var options = new ResultDistillationOptions
        {
            MaxOutputChars = 2000,
            MinInputCharsForDistillation = 3000
        };

        var result = await _distiller.DistillAsync("agent1", response, options);

        var distilledText = GetText(result);
        distilledText.Should().Contain("chars omitted");
    }

    #endregion

    #region Tool Content Handling

    [Fact]
    public async Task PreserveToolCalls_True_KeepsToolContent()
    {
        var response = MakeResponseWithTool(
            "analysis result " + new string('A', 4000),
            "read_file",
            """{"path": "src/Foo.cs"}""",
            new string('R', 4000)
        );

        var options = new ResultDistillationOptions
        {
            MaxOutputChars = 1000,
            MinInputCharsForDistillation = 3000,
            PreserveToolCalls = true
        };

        var result = await _distiller.DistillAsync("agent1", response, options);

        var assistant = result.Message as AssistantMessage;
        assistant.Should().NotBeNull();
        assistant!.Content.OfType<ToolMessageContent>().Should().NotBeEmpty();
    }

    [Fact]
    public async Task PreserveToolCalls_False_RemovesToolContent()
    {
        var response = MakeResponseWithTool(
            "analysis " + new string('A', 4000),
            "read_file",
            """{"path": "src/Foo.cs"}""",
            new string('R', 4000)
        );

        var options = new ResultDistillationOptions
        {
            MaxOutputChars = 1000,
            MinInputCharsForDistillation = 3000,
            PreserveToolCalls = false
        };

        var result = await _distiller.DistillAsync("agent1", response, options);

        var assistant = result.Message as AssistantMessage;
        assistant.Should().NotBeNull();
        assistant!.Content.OfType<ToolMessageContent>().Should().BeEmpty();
    }

    [Fact]
    public async Task ToolOutput_IsTruncated()
    {
        var response = MakeResponseWithTool(
            "short text",
            "grep",
            """{"pattern": "TODO"}""",
            new string('G', 10000) // Large grep result
        );

        var options = new ResultDistillationOptions
        {
            MaxOutputChars = 2000,
            MinInputCharsForDistillation = 3000,
            PreserveToolCalls = true
        };

        var result = await _distiller.DistillAsync("agent1", response, options);

        var assistant = result.Message as AssistantMessage;
        var tool = assistant!.Content.OfType<ToolMessageContent>().First();
        tool.Output!.Result!.Length.Should().BeLessThan(10000);
    }

    [Fact]
    public async Task ToolWithoutOutput_PreservedAsIs()
    {
        var response = new MessageResponse
        {
            Id = "test",
            DoneReason = MessageDoneReason.EndTurn,
            Message = new AssistantMessage
            {
                Content =
                [
                    new TextMessageContent { Value = new string('A', 4000) },
                    new ToolMessageContent
                    {
                        Id = "t1",
                        Name = "pending_tool",
                        Input = """{"key": "value"}""",
                        IsApproved = true
                        // No Output
                    }
                ]
            }
        };

        var options = new ResultDistillationOptions
        {
            MaxOutputChars = 1000,
            MinInputCharsForDistillation = 3000,
            PreserveToolCalls = true
        };

        var result = await _distiller.DistillAsync("agent1", response, options);

        var assistant = result.Message as AssistantMessage;
        var tool = assistant!.Content.OfType<ToolMessageContent>().First();
        tool.Name.Should().Be("pending_tool");
        tool.Output.Should().BeNull();
    }

    #endregion

    #region Response Metadata Preservation

    [Fact]
    public async Task PreservesResponseMetadata()
    {
        var response = new MessageResponse
        {
            Id = "resp-123",
            DoneReason = MessageDoneReason.EndTurn,
            Message = new AssistantMessage
            {
                Name = "researcher",
                Model = "gpt-4o",
                Content = [new TextMessageContent { Value = new string('A', 5000) }]
            },
            TokenUsage = new MessageTokenUsage { InputTokens = 100, OutputTokens = 200 }
        };

        var options = new ResultDistillationOptions
        {
            MaxOutputChars = 1000,
            MinInputCharsForDistillation = 3000
        };

        var result = await _distiller.DistillAsync("researcher", response, options);

        result.Id.Should().Be("resp-123");
        result.DoneReason.Should().Be(MessageDoneReason.EndTurn);
        result.TokenUsage!.InputTokens.Should().Be(100);

        var assistant = result.Message as AssistantMessage;
        assistant!.Name.Should().Be("researcher");
        assistant.Model.Should().Be("gpt-4o");
    }

    #endregion

    #region Custom Options

    [Fact]
    public async Task CustomThreshold_RespectsMinInput()
    {
        var text = new string('A', 500);
        var response = MakeResponse(text);

        var options = new ResultDistillationOptions
        {
            MaxOutputChars = 100,
            MinInputCharsForDistillation = 1000 // Input is 500 < 1000
        };

        var result = await _distiller.DistillAsync("agent1", response, options);

        result.Should().BeSameAs(response); // Below threshold, not distilled
    }

    [Fact]
    public async Task CustomThreshold_DistillsAboveMin()
    {
        var text = new string('A', 1500);
        var response = MakeResponse(text);

        var options = new ResultDistillationOptions
        {
            MaxOutputChars = 500,
            MinInputCharsForDistillation = 1000 // Input is 1500 > 1000
        };

        var result = await _distiller.DistillAsync("agent1", response, options);

        result.Should().NotBeSameAs(response);
        GetText(result).Length.Should().BeLessThan(1500);
    }

    #endregion

    #region Integration with Orchestrator

    [Fact]
    public async Task ParallelOrchestrator_WithDistiller_DistillsResults()
    {
        var agentText = new string('X', 5000);
        var agent = new ContextScopeTests_MockAgent("verbose-agent")
        {
            ResponseFunc = _ => agentText
        };

        var distiller = new TruncatingResultDistiller();
        var orch = new ParallelOrchestrator(new ParallelOrchestratorOptions
        {
            ResultDistiller = distiller,
            ResultDistillationOptions = new ResultDistillationOptions
            {
                MaxOutputChars = 1000,
                MinInputCharsForDistillation = 3000
            }
        });
        orch.AddAgent(agent);

        var messages = new List<Message>
        {
            new UserMessage { Content = [new TextMessageContent { Value = "Analyze" }] }
        };

        var result = await orch.ExecuteAsync(messages);

        result.IsSuccess.Should().BeTrue();

        // The step result should have distilled content
        var stepText = result.Steps[0].Response?.Message switch
        {
            AssistantMessage a => a.Content.OfType<TextMessageContent>().FirstOrDefault()?.Value,
            _ => null
        };

        stepText.Should().NotBeNull();
        stepText!.Length.Should().BeLessThan(5000);
        stepText.Should().Contain("chars omitted");
    }

    #endregion

    #region Helpers

    private static MessageResponse MakeResponse(string text)
    {
        return new MessageResponse
        {
            Id = Guid.NewGuid().ToString("N"),
            DoneReason = MessageDoneReason.EndTurn,
            Message = new AssistantMessage
            {
                Content = [new TextMessageContent { Value = text }]
            },
            TokenUsage = new MessageTokenUsage { InputTokens = 10, OutputTokens = text.Length / 4 }
        };
    }

    private static MessageResponse MakeResponseWithTool(
        string textContent,
        string toolName,
        string toolInput,
        string toolOutput)
    {
        return new MessageResponse
        {
            Id = Guid.NewGuid().ToString("N"),
            DoneReason = MessageDoneReason.EndTurn,
            Message = new AssistantMessage
            {
                Content =
                [
                    new TextMessageContent { Value = textContent },
                    new ToolMessageContent
                    {
                        Id = "tool_1",
                        Name = toolName,
                        Input = toolInput,
                        IsApproved = true,
                        Output = ToolOutput.Success(toolOutput)
                    }
                ]
            }
        };
    }

    private static string GetText(MessageResponse response)
    {
        return response.Message switch
        {
            AssistantMessage a => a.Content.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "",
            _ => ""
        };
    }

    // Same MockAgent as ContextScopeTests but renamed to avoid conflicts
    private sealed class ContextScopeTests_MockAgent : IronHive.Abstractions.Agent.IAgent
    {
        public string Provider { get; set; } = "mock";
        public string Model { get; set; } = "mock-model";
        public string Name { get; set; }
        public string Description { get; set; } = "Mock";
        public string? Instructions { get; set; }
        public IEnumerable<IronHive.Abstractions.Tools.ToolItem>? Tools { get; set; }
        public MessageGenerationParameters? Parameters { get; set; }
        public Func<IEnumerable<Message>, string>? ResponseFunc { get; set; }

        public ContextScopeTests_MockAgent(string name) { Name = name; }

        public Task<MessageResponse> InvokeAsync(
            IEnumerable<Message> messages, CancellationToken ct = default)
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
