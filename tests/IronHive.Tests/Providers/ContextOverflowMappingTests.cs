using System.Net;
using Anthropic.Exceptions;
using AwesomeAssertions;
using Google.GenAI;
using IronHive.Abstractions.Exceptions;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using OpenAIMapper = IronHive.Providers.OpenAI.OpenAIExceptionMapper;
using AnthropicMapper = IronHive.Providers.Anthropic.AnthropicExceptionMapper;
using GoogleAIMapper = IronHive.Providers.GoogleAI.GoogleAIExceptionMapper;

namespace IronHive.Tests.Providers;

/// <summary>
/// Context-window overflow normalization: each provider maps its vendor-specific error
/// format to <see cref="ContextOverflowException"/> (vault-ai dogfooding, 2026-07-05).
/// </summary>
public class ContextOverflowMappingTests
{
    // ---- OpenAI.Compatible (llama.cpp / GPUStack / vLLM) ----
    // DetectAsync reads the error body directly off the HttpResponseMessage, so these build a
    // real response rather than a hand-rolled JsonNode.

    [Fact]
    public async Task Compatible_LlamaCpp_ExceedContextSize_Maps_With_ContextWindow()
    {
        // GPUStack/llama.cpp: vault-ai live-reproduced error (32k model, 42k request)
        using var response = JsonResponse(HttpStatusCode.BadRequest,
            """{"error":{"code":400,"message":"request (42259 tokens) exceeds the available context size (32768 tokens), try increasing it","type":"exceed_context_size_error"}}""");

        var ex = await ChatCompletionExceptionDetector.DetectAsync(response, TestContext.Current.CancellationToken);

        ex.Should().BeOfType<ContextOverflowException>().Which.ContextWindow.Should().Be(32768);
    }

    [Fact]
    public async Task Compatible_LlamaCpp_NumericFields_Preferred_Over_Message()
    {
        using var response = JsonResponse(HttpStatusCode.BadRequest,
            """{"error":{"type":"exceed_context_size_error","message":"context overflow","n_ctx":32768}}""");

        var ex = await ChatCompletionExceptionDetector.DetectAsync(response, TestContext.Current.CancellationToken);

        ex.Should().BeOfType<ContextOverflowException>().Which.ContextWindow.Should().Be(32768);
    }

    [Fact]
    public async Task Compatible_Vllm_ContextLengthExceeded_Maps()
    {
        var message = "This model's maximum context length is 32768 tokens. However, you requested 45010 tokens (44000 in the messages, 1010 in the completion). Please reduce the length of the messages or completion.";
        var json = """{"error":{"message":"__MESSAGE__","type":"BadRequestError","code":"context_length_exceeded"}}"""
            .Replace("__MESSAGE__", message);
        using var response = JsonResponse(HttpStatusCode.BadRequest, json);

        var ex = await ChatCompletionExceptionDetector.DetectAsync(response, TestContext.Current.CancellationToken);

        ex.Should().BeOfType<ContextOverflowException>().Which.ContextWindow.Should().Be(32768);
    }

    [Fact]
    public void Compatible_MidStream_ErrorLine_Maps_Without_Body()
    {
        var ex = ChatCompletionExceptionDetector.Detect(
            "request (42259 tokens) exceeds the available context size (32768 tokens)");

        ex.Should().BeOfType<ContextOverflowException>().Which.ContextWindow.Should().Be(32768);
    }

    [Fact]
    public async Task Compatible_Unrelated_Error_Falls_Back_To_HttpRequestException()
    {
        using var response = JsonResponse(HttpStatusCode.BadRequest,
            """{"error":{"message":"Invalid API key provided","type":"invalid_request_error","code":"invalid_api_key"}}""");

        var ex = await ChatCompletionExceptionDetector.DetectAsync(response, TestContext.Current.CancellationToken);

        ex.Should().BeOfType<HttpRequestException>().Which.Message.Should().Be("Invalid API key provided");
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json)
        => new(statusCode) { Content = new StringContent(json) };

    // ---- OpenAI (Responses API, SDK ClientResultException) ----
    // ClientResultException has no public constructor that doesn't require a PipelineResponse
    // (abstract, no public fake available), so only the type-gate (negative) path is unit
    // testable here — same accepted-gap category as ChatCompletionHttpClient's SDK boundary.

    [Fact]
    public void OpenAI_Map_Ignores_NonSdk_Exceptions()
    {
        var ex = new InvalidOperationException("maximum context length is 128000 tokens");
        OpenAIMapper.Map(ex).Should().BeNull();
    }

    // ---- Anthropic ----
    // Map() has no separate string-only seam (merged into one method), so these go through the
    // SDK's own exception factory rather than a hand-rolled fake.

    [Fact]
    public void Anthropic_Map_Maps_Real_SDK_Exception()
    {
        var body = """{"type":"error","error":{"type":"invalid_request_error","message":"prompt is too long: 210145 tokens > 204698 maximum"}}""";
        var sdkException = AnthropicExceptionFactory.CreateApiException(HttpStatusCode.BadRequest, body);

        var mapped = AnthropicMapper.Map(sdkException);

        mapped.Should().BeOfType<ContextOverflowException>().Which.ContextWindow.Should().Be(204698);
    }

    [Fact]
    public void Anthropic_Map_Ignores_Unrelated_Message()
    {
        var body = """{"type":"error","error":{"type":"invalid_request_error","message":"Your credit balance is too low"}}""";
        var sdkException = AnthropicExceptionFactory.CreateApiException(HttpStatusCode.BadRequest, body);

        AnthropicMapper.Map(sdkException).Should().BeNull();
    }

    [Fact]
    public void Anthropic_Map_Ignores_Other_ErrorTypes()
    {
        // A rate_limit_error should never be reinterpreted as a context overflow, even if it
        // happened to contain overflow-shaped text — it maps to RateLimitException instead.
        var body = """{"type":"error","error":{"type":"rate_limit_error","message":"prompt is too long: 1 tokens > 1 maximum"}}""";
        var sdkException = AnthropicExceptionFactory.CreateApiException(HttpStatusCode.TooManyRequests, body);

        AnthropicMapper.Map(sdkException).Should().BeOfType<RateLimitException>();
    }

    // ---- Google GenAI (Gemini) ----
    // Same story as Anthropic — Map() is one method now, so these construct a real ClientError
    // (its constructor is public, so this is no more ceremony than a string-based test would be).

    [Fact]
    public void Google_Map_Maps_Real_ClientError()
    {
        var message = "The input token count (185586) exceeds the maximum number of tokens allowed (131072).";
        var clientError = new ClientError(message, 400, "INVALID_ARGUMENT");

        var mapped = GoogleAIMapper.Map(clientError);

        mapped.Should().BeOfType<ContextOverflowException>().Which.ContextWindow.Should().Be(131072);
    }

    [Fact]
    public void Google_Map_Ignores_Unrelated_Message()
    {
        var clientError = new ClientError("API key not valid. Please pass a valid API key.", 400, "INVALID_ARGUMENT");

        GoogleAIMapper.Map(clientError).Should().BeNull();
    }

    [Fact]
    public void Google_Map_Ignores_Other_Status()
    {
        var clientError = new ClientError("API key not valid.", 400, "UNAUTHENTICATED");

        GoogleAIMapper.Map(clientError).Should().BeNull();
    }
}
