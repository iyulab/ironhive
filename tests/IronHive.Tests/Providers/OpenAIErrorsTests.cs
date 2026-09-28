using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;
using System.Text;
using AwesomeAssertions;
using IronHive.Abstractions.Exceptions;
using IronHive.Providers.OpenAI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;

namespace IronHive.Tests.Providers;

/// <summary>
/// <see cref="OpenAIErrors"/> is for a caller that talks to an OpenAI-compatible server through its own OpenAI SDK
/// client. These go through a real SDK <see cref="ChatClient"/> over a canned HTTP response, so the exception under
/// test is the one the SDK actually throws — message format, buffered body and all.
/// </summary>
public class OpenAIErrorsTests
{
    private const string LlamaCppBody =
        """{"error":{"code":400,"message":"request (40408 tokens) exceeds the available context size (32768 tokens), try increasing it","type":"exceed_context_size_error","n_prompt_tokens":40408,"n_ctx":32768}}""";

    [Fact]
    public async Task LlamaCpp_Overflow_From_Sdk_Client_Maps_With_Window_And_Request_Tokens()
    {
        var sdkException = await ThrownBySdk(HttpStatusCode.BadRequest, LlamaCppBody);

        var overflow = OpenAIErrors.TryMapContextOverflow(sdkException);

        overflow.Should().NotBeNull();
        overflow!.ContextWindow.Should().Be(32768);
        overflow.RequestTokens.Should().Be(40408);
        overflow.InnerException.Should().BeSameAs(sdkException);
    }

    [Fact]
    public async Task Overflow_Through_Extensions_AI_ChatClient_Maps()
    {
        // The consumer shape this exists for: the SDK client wrapped as an IChatClient by Microsoft.Extensions.AI.OpenAI.
        var chatClient = SdkClient(HttpStatusCode.BadRequest, LlamaCppBody).AsIChatClient();

        var thrown = await Record.ExceptionAsync(() =>
            chatClient.GetResponseAsync("hi", cancellationToken: TestContext.Current.CancellationToken));

        var overflow = OpenAIErrors.TryMapContextOverflow(thrown!);
        overflow!.ContextWindow.Should().Be(32768);
        overflow.RequestTokens.Should().Be(40408);
    }

    [Fact]
    public async Task Overflow_Through_Extensions_AI_Streaming_Maps()
    {
        var chatClient = SdkClient(HttpStatusCode.BadRequest, LlamaCppBody).AsIChatClient();

        var thrown = await Record.ExceptionAsync(async () =>
        {
            await foreach (var _ in chatClient.GetStreamingResponseAsync("hi", cancellationToken: TestContext.Current.CancellationToken))
            { }
        });

        OpenAIErrors.TryMapContextOverflow(thrown!)!.ContextWindow.Should().Be(32768);
    }

    [Fact]
    public async Task LlamaCpp_Overflow_Without_Numeric_Fields_Reads_The_Message()
    {
        var sdkException = await ThrownBySdk(HttpStatusCode.BadRequest,
            """{"error":{"code":400,"message":"request (40408 tokens) exceeds the available context size (32768 tokens), try increasing it","type":"exceed_context_size_error"}}""");

        var overflow = OpenAIErrors.TryMapContextOverflow(sdkException);

        overflow!.ContextWindow.Should().Be(32768);
        overflow.RequestTokens.Should().Be(40408);
    }

    [Fact]
    public async Task OpenAI_ContextLengthExceeded_From_Sdk_Client_Maps()
    {
        var sdkException = await ThrownBySdk(HttpStatusCode.BadRequest,
            """{"error":{"message":"This model's maximum context length is 128000 tokens. However, you requested 130512 tokens (129000 in the messages, 1512 in the completion). Please reduce the length of the messages or completion.","type":"invalid_request_error","param":"messages","code":"context_length_exceeded"}}""");

        var overflow = OpenAIErrors.TryMapContextOverflow(sdkException);

        overflow!.ContextWindow.Should().Be(128000);
        overflow.RequestTokens.Should().Be(130512);
    }

    [Fact]
    public async Task Responses_Style_Overflow_Without_Numbers_Maps_With_Nulls()
    {
        var sdkException = await ThrownBySdk(HttpStatusCode.BadRequest,
            """{"error":{"message":"Your input exceeds the context window of this model. Please adjust your input and try again.","type":"invalid_request_error","param":"input","code":"context_length_exceeded"}}""");

        var overflow = OpenAIErrors.TryMapContextOverflow(sdkException);

        overflow.Should().NotBeNull();
        overflow!.ContextWindow.Should().BeNull();
        overflow.RequestTokens.Should().BeNull();
    }

    [Fact]
    public async Task Wrapped_Sdk_Exception_Is_Found()
    {
        // A consumer's own IChatClient wrapper may rethrow with the SDK exception inside.
        var sdkException = await ThrownBySdk(HttpStatusCode.BadRequest, LlamaCppBody);
        var wrapped = new InvalidOperationException("chat call failed", sdkException);

        OpenAIErrors.TryMapContextOverflow(wrapped)!.ContextWindow.Should().Be(32768);
    }

    [Fact]
    public async Task Unrelated_Sdk_Error_Is_Not_An_Overflow()
    {
        var sdkException = await ThrownBySdk(HttpStatusCode.Unauthorized,
            """{"error":{"message":"Incorrect API key provided","type":"invalid_request_error","code":"invalid_api_key"}}""");

        OpenAIErrors.TryMapContextOverflow(sdkException).Should().BeNull();
        OpenAIErrors.TryMapRateLimit(sdkException).Should().BeNull();
    }

    [Fact]
    public void Non_Sdk_Exception_With_Overflow_Text_Is_Not_Mapped()
    {
        // The exception overload recognizes SDK errors only; free text goes through the string overload.
        OpenAIErrors.TryMapContextOverflow(new InvalidOperationException("maximum context length is 128000 tokens"))
            .Should().BeNull();
    }

    [Fact]
    public void Bare_Message_Maps()
    {
        var overflow = OpenAIErrors.TryMapContextOverflow(
            "request (42259 tokens) exceeds the available context size (32768 tokens)");

        overflow!.ContextWindow.Should().Be(32768);
        overflow.RequestTokens.Should().Be(42259);
        OpenAIErrors.TryMapContextOverflow("Invalid API key provided").Should().BeNull();
    }

    [Fact]
    public async Task RateLimit_From_Sdk_Client_Maps_With_RetryAfter()
    {
        var sdkException = await ThrownBySdk(HttpStatusCode.TooManyRequests,
            """{"error":{"message":"Rate limit reached","type":"requests","code":"rate_limit_exceeded"}}""",
            retryAfterSeconds: "7");

        var rateLimit = OpenAIErrors.TryMapRateLimit(sdkException);

        rateLimit!.RetryAfter.Should().Be(TimeSpan.FromSeconds(7));
        OpenAIErrors.TryMapContextOverflow(sdkException).Should().BeNull();
    }

    private static async Task<ClientResultException> ThrownBySdk(
        HttpStatusCode status, string body, string? retryAfterSeconds = null)
    {
        var client = SdkClient(status, body, retryAfterSeconds);

        try
        {
            await client.CompleteChatAsync([new UserChatMessage("hi")], cancellationToken: TestContext.Current.CancellationToken);
        }
        catch (ClientResultException ex)
        {
            return ex;
        }
        throw new InvalidOperationException("The SDK did not throw.");
    }

    private static ChatClient SdkClient(HttpStatusCode status, string body, string? retryAfterSeconds = null)
    {
        var handler = new CannedHandler(status, body, retryAfterSeconds);
        return new ChatClient("any-model", new ApiKeyCredential("test"), new OpenAIClientOptions
        {
            Endpoint = new Uri("http://localhost:1/v1"),
            Transport = new HttpClientPipelineTransport(new HttpClient(handler)),
            RetryPolicy = new ClientRetryPolicy(maxRetries: 0),
        });
    }

    private sealed class CannedHandler(HttpStatusCode status, string body, string? retryAfterSeconds) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            };
            if (retryAfterSeconds is not null)
                response.Headers.TryAddWithoutValidation("retry-after", retryAfterSeconds);
            return Task.FromResult(response);
        }
    }
}
