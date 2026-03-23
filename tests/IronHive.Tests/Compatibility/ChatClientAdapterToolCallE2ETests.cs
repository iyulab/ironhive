using System.Globalization;
using FluentAssertions;
using IronHive.Core.Compatibility;
using IronHive.Providers.OpenAI;
using Microsoft.Extensions.AI;

namespace IronHive.Tests.Compatibility;

/// <summary>
/// ChatClientAdapter를 통한 tool-call roundtrip E2E 테스트.
/// ChatClientAdapter가 tools를 OpenAI API에 올바르게 전달하고,
/// LLM이 tool_calls를 반환하는 전체 경로를 검증합니다.
/// </summary>
[Trait("Category", "RequiresApiKey")]
public class ChatClientAdapterToolCallE2ETests : IDisposable
{
    private readonly ChatClientAdapter? _adapter;
    private readonly string? _skipReason;

    public ChatClientAdapterToolCallE2ETests()
    {
        // GPUStack (OpenAI-compatible) 환경변수 우선
        var endpoint = Environment.GetEnvironmentVariable("GPUSTACK_ENDPOINT");
        var apiKey = Environment.GetEnvironmentVariable("GPUSTACK_API_KEY");
        var model = Environment.GetEnvironmentVariable("GPUSTACK_MODEL");

        // Fallback: OpenAI-compatible
        if (string.IsNullOrEmpty(endpoint))
        {
            endpoint = Environment.GetEnvironmentVariable("OPENAI_COMPATIBLE_ENDPOINT");
            apiKey = Environment.GetEnvironmentVariable("OPENAI_COMPATIBLE_API_KEY");
            model = Environment.GetEnvironmentVariable("OPENAI_COMPATIBLE_MODEL");
        }

        // Fallback: OpenAI
        if (string.IsNullOrEmpty(endpoint))
        {
            apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            model = "gpt-4o-mini";
        }

        if (string.IsNullOrEmpty(apiKey))
        {
            _skipReason = "No API key configured. Set GPUSTACK_*, OPENAI_COMPATIBLE_*, or OPENAI_API_KEY.";
            return;
        }

        // Ensure endpoint ends with /v1/ for OpenAI-compatible APIs
        if (!string.IsNullOrEmpty(endpoint) && !endpoint.Contains("/v1", StringComparison.OrdinalIgnoreCase))
        {
            endpoint = endpoint.TrimEnd('/') + "/v1";
        }

        var config = new OpenAIConfig
        {
            ApiKey = apiKey,
            BaseUrl = endpoint ?? string.Empty
        };

        var generator = new OpenAIChatMessageGenerator(config);
        _adapter = new ChatClientAdapter(generator, model ?? "gpt-4o-mini");
    }

    public void Dispose()
    {
        _adapter?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 단일 AIFunction tool을 등록하고 LLM이 tool_calls를 생성하는지 검증.
    /// FunctionInvokingChatClient 없이 ChatClientAdapter만으로 tool 선언 → LLM 응답 확인.
    /// </summary>
    [Fact]
    public async Task ToolCall_SingleFunction_LLMReturnsFunctionCallContent()
    {
        if (_skipReason is not null) return;

        var tool = AIFunctionFactory.Create(
            (string city) => "22°C, sunny",
            "get_current_weather",
            "Get the current weather for a given city");

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful assistant. Always use tools when available. Do not answer from your knowledge."),
            new(ChatRole.User, "What's the weather in Seoul?")
        };

        var options = new ChatOptions
        {
            Tools = [tool],
            Temperature = 0f
        };

        var response = await _adapter!.GetResponseAsync(messages, options);

        // LLM should return a tool call (FinishReason = ToolCalls)
        response.FinishReason.Should().Be(ChatFinishReason.ToolCalls,
            "LLM should respond with tool_calls when tools are provided and relevant");

        var functionCalls = response.Messages
            .SelectMany(m => m.Contents.OfType<FunctionCallContent>())
            .ToList();

        functionCalls.Should().NotBeEmpty("LLM should generate at least one function call");
        functionCalls.First().Name.Should().Be("get_current_weather");
    }

    /// <summary>
    /// 복수 tool 등록 후 LLM이 올바른 tool을 선택하는지 검증.
    /// </summary>
    [Fact]
    public async Task ToolCall_MultipleTools_CorrectOneSelected()
    {
        if (_skipReason is not null) return;

        var weatherTool = AIFunctionFactory.Create(
            (string city) => "25°C, clear sky",
            "get_weather",
            "Get current weather for a city");

        var calcTool = AIFunctionFactory.Create(
            (double a, double b) => (a + b).ToString(CultureInfo.InvariantCulture),
            "add_numbers",
            "Add two numbers together");

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful assistant. Always use tools when available."),
            new(ChatRole.User, "What is 42 plus 58?")
        };

        var options = new ChatOptions
        {
            Tools = [weatherTool, calcTool],
            Temperature = 0f
        };

        var response = await _adapter!.GetResponseAsync(messages, options);

        response.FinishReason.Should().Be(ChatFinishReason.ToolCalls);

        var functionCalls = response.Messages
            .SelectMany(m => m.Contents.OfType<FunctionCallContent>())
            .ToList();

        functionCalls.Should().ContainSingle("LLM should call exactly one tool");
        functionCalls.First().Name.Should().Be("add_numbers",
            "LLM should select add_numbers for a math question, not get_weather");
    }

    /// <summary>
    /// tool 없이 호출하면 텍스트만 반환되는지 검증 (회귀 테스트).
    /// </summary>
    [Fact]
    public async Task NoTools_ReturnsTextOnly()
    {
        if (_skipReason is not null) return;

        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "Say 'hello' and nothing else.")
        };

        var response = await _adapter!.GetResponseAsync(messages);

        response.Messages.Should().NotBeEmpty();
        response.FinishReason.Should().Be(ChatFinishReason.Stop);

        var functionCalls = response.Messages
            .SelectMany(m => m.Contents.OfType<FunctionCallContent>())
            .ToList();
        functionCalls.Should().BeEmpty("no tools registered, so no tool calls expected");

        var text = response.Messages
            .SelectMany(m => m.Contents.OfType<TextContent>())
            .FirstOrDefault()?.Text ?? "";
        text.ToLowerInvariant().Should().Contain("hello");
    }

    /// <summary>
    /// tool의 parameter schema가 올바르게 전달되어 LLM이 arguments를 채우는지 검증.
    /// </summary>
    [Fact]
    public async Task ToolCall_ParametersPassedCorrectly()
    {
        if (_skipReason is not null) return;

        var tool = AIFunctionFactory.Create(
            (string recipient, string message) => $"Sent to {recipient}: {message}",
            "send_message",
            "Send a message to a person");

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful assistant. Always use tools when available."),
            new(ChatRole.User, "Send a message to Alice saying 'Meeting at 3pm'")
        };

        var options = new ChatOptions
        {
            Tools = [tool],
            Temperature = 0f
        };

        var response = await _adapter!.GetResponseAsync(messages, options);

        response.FinishReason.Should().Be(ChatFinishReason.ToolCalls);

        var fc = response.Messages
            .SelectMany(m => m.Contents.OfType<FunctionCallContent>())
            .First();

        fc.Name.Should().Be("send_message");
        // Arguments should be present (not empty)
        fc.Arguments.Should().NotBeNull("LLM should provide arguments for the tool call");
    }

    /// <summary>
    /// 스트리밍 모드에서 tool call이 올바르게 반환되는지 검증.
    /// </summary>
    [Fact]
    public async Task ToolCall_Streaming_ReturnsFunctionCallContent()
    {
        if (_skipReason is not null) return;

        var tool = AIFunctionFactory.Create(
            (string expression) => "42",
            "calculate",
            "Evaluate a math expression and return the result");

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful assistant. Always use the calculate tool for math questions."),
            new(ChatRole.User, "What is 7 times 6?")
        };

        var options = new ChatOptions
        {
            Tools = [tool],
            Temperature = 0f
        };

        var updates = new List<ChatResponseUpdate>();
        await foreach (var update in _adapter!.GetStreamingResponseAsync(messages, options))
        {
            updates.Add(update);
        }

        updates.Should().NotBeEmpty("should receive streaming updates");

        // In streaming mode, the adapter should return a done update with ToolCalls finish reason
        var doneUpdate = updates.LastOrDefault(u => u.FinishReason is not null);
        doneUpdate.Should().NotBeNull("should have a completion update");
        doneUpdate!.FinishReason.Should().Be(ChatFinishReason.ToolCalls,
            "LLM should indicate tool_calls finish reason in streaming mode");
    }

    /// <summary>
    /// tool이 등록되었지만 질문이 tool과 무관한 경우, LLM이 텍스트로 응답하는지 검증.
    /// </summary>
    [Fact]
    public async Task ToolRegistered_ButIrrelevantQuestion_ReturnsText()
    {
        if (_skipReason is not null) return;

        var tool = AIFunctionFactory.Create(
            (string city) => "22°C",
            "get_weather",
            "Get current weather for a city");

        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "Say 'hi' and nothing else.")
        };

        var options = new ChatOptions
        {
            Tools = [tool],
            Temperature = 0f
        };

        var response = await _adapter!.GetResponseAsync(messages, options);

        // LLM should NOT call the tool for an irrelevant question
        response.FinishReason.Should().Be(ChatFinishReason.Stop,
            "LLM should not call tools for irrelevant questions");

        var text = response.Messages
            .SelectMany(m => m.Contents.OfType<TextContent>())
            .FirstOrDefault()?.Text ?? "";
        text.ToLowerInvariant().Should().Contain("hi");
    }
}
