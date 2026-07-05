using System.Text.Json.Nodes;
using FluentAssertions;
using IronHive.Abstractions.Exceptions;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using OpenAIMapper = IronHive.Providers.OpenAI.ContextWindowErrorMapper;
using AnthropicMapper = IronHive.Providers.Anthropic.ContextWindowErrorMapper;

namespace IronHive.Tests.Providers;

/// <summary>
/// Context-window overflow normalization: each provider maps its vendor-specific error
/// format to <see cref="ContextWindowExceededException"/> (vault-ai dogfooding, 2026-07-05).
/// </summary>
public class ContextWindowExceededMappingTests
{
    // ---- OpenAI.Compatible (llama.cpp / GPUStack / vLLM) ----

    [Fact]
    public void Compatible_LlamaCpp_ExceedContextSize_Maps_With_Tokens()
    {
        // GPUStack/llama.cpp: vault-ai live-reproduced error (32k model, 42k request)
        var message = "request (42259 tokens) exceeds the available context size (32768 tokens), try increasing it";
        var body = JsonNode.Parse(
            """{"error":{"code":400,"message":"request (42259 tokens) exceeds the available context size (32768 tokens), try increasing it","type":"exceed_context_size_error"}}""");

        var ex = ContextOverflowDetector.Detect(message, body);

        ex.Should().NotBeNull();
        ex!.PromptTokens.Should().Be(42259);
        ex.ContextWindow.Should().Be(32768);
        ex.IsPreflightRejection.Should().BeFalse();
        ex.Should().BeAssignableTo<HiveException>();
    }

    [Fact]
    public void Compatible_LlamaCpp_NumericFields_Preferred_Over_Message()
    {
        var body = JsonNode.Parse(
            """{"error":{"type":"exceed_context_size_error","message":"context overflow","n_prompt_tokens":45010,"n_ctx":32768}}""");

        var ex = ContextOverflowDetector.Detect("context overflow", body);

        ex.Should().NotBeNull();
        ex!.PromptTokens.Should().Be(45010);
        ex.ContextWindow.Should().Be(32768);
    }

    [Fact]
    public void Compatible_Vllm_ContextLengthExceeded_Maps()
    {
        var message = "This model's maximum context length is 32768 tokens. However, you requested 45010 tokens (44000 in the messages, 1010 in the completion). Please reduce the length of the messages or completion.";
        var body = new JsonObject
        {
            ["error"] = new JsonObject
            {
                ["message"] = message,
                ["type"] = "BadRequestError",
                ["code"] = "context_length_exceeded",
            },
        };

        var ex = ContextOverflowDetector.Detect(message, body);

        ex.Should().NotBeNull();
        ex!.ContextWindow.Should().Be(32768);
        ex.PromptTokens.Should().Be(45010);
    }

    [Fact]
    public void Compatible_MidStream_ErrorLine_Maps_Without_Body()
    {
        var ex = ContextOverflowDetector.Detect(
            "request (42259 tokens) exceeds the available context size (32768 tokens)");

        ex.Should().NotBeNull();
        ex!.PromptTokens.Should().Be(42259);
        ex.ContextWindow.Should().Be(32768);
    }

    [Fact]
    public void Compatible_Unrelated_Error_Returns_Null()
    {
        var body = JsonNode.Parse(
            """{"error":{"message":"Invalid API key provided","type":"invalid_request_error","code":"invalid_api_key"}}""");

        ContextOverflowDetector.Detect("Invalid API key provided", body).Should().BeNull();
    }

    // ---- OpenAI (Responses API, SDK ClientResultException) ----

    [Fact]
    public void OpenAI_MaximumContextLength_Maps_With_Tokens()
    {
        var message = "This model's maximum context length is 128000 tokens. However, your messages resulted in 130250 tokens. Please reduce the length of the messages. (context_length_exceeded)";

        var ex = OpenAIMapper.Detect(message);

        ex.Should().NotBeNull();
        ex!.ContextWindow.Should().Be(128000);
        ex.PromptTokens.Should().Be(130250);
    }

    [Fact]
    public void OpenAI_Unrelated_Error_Returns_Null()
    {
        OpenAIMapper.Detect("Rate limit reached for gpt-4o").Should().BeNull();
    }

    [Fact]
    public void OpenAI_TryMap_Ignores_NonSdk_Exceptions()
    {
        var ex = new InvalidOperationException("maximum context length is 128000 tokens");
        OpenAIMapper.TryMap(ex).Should().BeNull();
    }

    // ---- Anthropic ----

    [Fact]
    public void Anthropic_PromptTooLong_Maps_With_Tokens()
    {
        var message = "prompt is too long: 210145 tokens > 204698 maximum";

        var ex = AnthropicMapper.Detect(message);

        ex.Should().NotBeNull();
        ex!.PromptTokens.Should().Be(210145);
        ex.ContextWindow.Should().Be(204698);
    }

    [Fact]
    public void Anthropic_Unrelated_Error_Returns_Null()
    {
        AnthropicMapper.Detect("Your credit balance is too low").Should().BeNull();
    }

    // ---- Preflight flag contract ----

    [Fact]
    public void Preflight_Flag_Defaults_To_False_And_Is_Settable()
    {
        var providerSide = new ContextWindowExceededException("overflow");
        providerSide.IsPreflightRejection.Should().BeFalse();

        var preflight = new ContextWindowExceededException("overflow") { IsPreflightRejection = true };
        preflight.IsPreflightRejection.Should().BeTrue();
    }
}
