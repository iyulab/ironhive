using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;
using Anthropic.Exceptions;
using AwesomeAssertions;
using Google.GenAI;
using IronHive.Abstractions.Exceptions;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using AnthropicMapper = IronHive.Providers.Anthropic.AnthropicExceptionMapper;
using GoogleAIMapper = IronHive.Providers.GoogleAI.GoogleAIExceptionMapper;
using OpenAIMapper = IronHive.Providers.OpenAI.OpenAIExceptionMapper;

namespace IronHive.Tests.Providers;

/// <summary>
/// A provider failure says the provider's sentence (<c>error.message</c>), not its SDK's formatting of the raw answer, and
/// keeps the raw answer in <c>ResponseBody</c> for the fields beyond the sentence. An HTTP error without a more specific
/// IronHive type is a <see cref="ProviderHttpException"/> on every provider, never the SDK's own exception.
/// </summary>
public class ProviderErrorMessageTests
{
    // ---- Anthropic ----

    [Fact]
    public void Anthropic_Billing_SaysTheSentence_AndKeepsTheBody()
    {
        // A metering gateway's 402 in Anthropic's error shape, with a field of its own.
        var body = """{"type":"error","error":{"type":"hold_rejected","message":"Not enough credits for this request's output limit.","affordable_max_tokens":4000}}""";
        var sdk = AnthropicExceptionFactory.CreateApiException(HttpStatusCode.PaymentRequired, body);

        var billing = AnthropicMapper.Map(sdk, TestContext.Current.CancellationToken).Should().BeOfType<BillingException>().Subject;

        billing.Message.Should().Be("Not enough credits for this request's output limit.");
        billing.ResponseBody.Should().Be(body);
        billing.ErrorCode.Should().Be("hold_rejected");
        billing.StatusCode.Should().Be(HttpStatusCode.PaymentRequired);
    }

    [Fact]
    public void Anthropic_CreditBalance_SaysTheSentence()
    {
        var body = """{"type":"error","error":{"type":"invalid_request_error","message":"Your credit balance is too low to access the Anthropic API."}}""";
        var sdk = AnthropicExceptionFactory.CreateApiException(HttpStatusCode.BadRequest, body);

        AnthropicMapper.Map(sdk, TestContext.Current.CancellationToken).Should().BeOfType<BillingException>()
            .Which.Message.Should().Be("Your credit balance is too low to access the Anthropic API.");
    }

    [Fact]
    public void Anthropic_RateLimit_And_Overflow_SayTheSentence()
    {
        var rateBody = """{"type":"error","error":{"type":"rate_limit_error","message":"Number of request tokens has exceeded your rate limit."}}""";
        var overflowBody = """{"type":"error","error":{"type":"invalid_request_error","message":"prompt is too long: 210000 tokens > 200000 maximum"}}""";

        var rate = AnthropicMapper.Map(AnthropicExceptionFactory.CreateApiException(HttpStatusCode.TooManyRequests, rateBody), TestContext.Current.CancellationToken);
        var overflow = AnthropicMapper.Map(AnthropicExceptionFactory.CreateApiException(HttpStatusCode.BadRequest, overflowBody), TestContext.Current.CancellationToken);

        rate.Should().BeOfType<RateLimitException>().Which.Message.Should().Be("Number of request tokens has exceeded your rate limit.");
        overflow.Should().BeOfType<ContextOverflowException>().Which.ResponseBody.Should().Be(overflowBody);
        overflow!.Message.Should().Be("prompt is too long: 210000 tokens > 200000 maximum");
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "authentication_error", "invalid x-api-key")]
    [InlineData(HttpStatusCode.NotFound, "not_found_error", "model: claude-nope")]
    [InlineData(HttpStatusCode.InternalServerError, "api_error", "Internal server error")]
    public void Anthropic_OtherHttpError_IsProviderHttpException(HttpStatusCode status, string type, string message)
    {
        var body = $$$"""{"type":"error","error":{"type":"{{{type}}}","message":"{{{message}}}"}}""";
        var sdk = AnthropicExceptionFactory.CreateApiException(status, body);

        var http = AnthropicMapper.Map(sdk, TestContext.Current.CancellationToken).Should().BeOfType<ProviderHttpException>().Subject;

        http.StatusCode.Should().Be(status);
        http.Message.Should().Be(message);
        http.ErrorCode.Should().Be(type);
        http.ResponseBody.Should().Be(body);
        http.InnerException.Should().BeSameAs(sdk, "the SDK's exception stays reachable");
    }

    [Fact]
    public void Anthropic_PlainTextBody_IsTheMessage()
    {
        // A gateway in front of the API that does not answer in Anthropic's shape: the body is all there is to say.
        var sdk = AnthropicExceptionFactory.CreateApiException(HttpStatusCode.PaymentRequired, "Payment required");

        AnthropicMapper.Map(sdk, TestContext.Current.CancellationToken).Should().BeOfType<BillingException>()
            .Which.Message.Should().Be("Payment required");
    }

    // ---- OpenAI SDK ----

    [Fact]
    public void OpenAI_Billing_SaysTheSentence_AndKeepsTheBody()
    {
        var body = """{"error":{"message":"You exceeded your current quota.","type":"insufficient_quota","code":"insufficient_quota"}}""";
        var sdk = new ClientResultException(new FakeResponse(429, body));

        var billing = OpenAIMapper.Map(sdk, TestContext.Current.CancellationToken).Should().BeOfType<BillingException>().Subject;

        billing.Message.Should().Be("You exceeded your current quota.");
        billing.ResponseBody.Should().Be(body);
    }

    [Fact]
    public void OpenAI_OtherHttpError_IsProviderHttpException_WithRetryHint()
    {
        var body = """{"error":{"message":"The server is overloaded.","type":"server_error","code":"overloaded"}}""";
        var sdk = new ClientResultException(new FakeResponse(503, body, ("retry-after", "12")));

        var http = OpenAIMapper.Map(sdk, TestContext.Current.CancellationToken).Should().BeOfType<ProviderHttpException>().Subject;

        http.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        http.Message.Should().Be("The server is overloaded.");
        http.ErrorCode.Should().Be("overloaded");
        http.RetryAfter.Should().Be(TimeSpan.FromSeconds(12));
        http.ResponseBody.Should().Be(body);
    }

    [Fact]
    public void OpenAI_WithoutBody_DropsTheSdkHeadline()
    {
        var sdk = new ClientResultException("HTTP 401 (invalid_request_error: invalid_api_key)\n\nIncorrect API key provided.", new FakeResponse(401, ""));

        var http = OpenAIErrors.TryMapHttp(sdk).Should().BeOfType<ProviderHttpException>().Subject;

        http.Message.Should().Be("Incorrect API key provided.");
        http.ResponseBody.Should().BeNull();
    }

    // ---- Google GenAI ----

    [Fact]
    public void Google_RateLimit_DropsTheSdkDetails_AndReadsTheRetryDelay()
    {
        var clientError = new ClientError(
            "Resource has been exhausted (e.g. check quota).\nDetails: {\"@type\":\"type.googleapis.com/google.rpc.RetryInfo\",\"retryDelay\":\"7s\"}",
            429, "RESOURCE_EXHAUSTED");

        var rate = GoogleAIMapper.Map(clientError, TestContext.Current.CancellationToken).Should().BeOfType<RateLimitException>().Subject;

        rate.Message.Should().Be("Resource has been exhausted (e.g. check quota).");
        rate.RetryAfter.Should().Be(TimeSpan.FromSeconds(7));
    }

    [Fact]
    public void Google_ServerError_IsProviderHttpException()
    {
        var serverError = new ServerError("The model is overloaded. Please try again later.", 503, "UNAVAILABLE");

        var http = GoogleAIMapper.Map(serverError, TestContext.Current.CancellationToken).Should().BeOfType<ProviderHttpException>().Subject;

        http.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        http.ErrorCode.Should().Be("UNAVAILABLE");
        http.Message.Should().Be("The model is overloaded. Please try again later.");
    }

    // ---- OpenAI-compatible (own HTTP client) ----

    [Fact]
    public async Task Compatible_KeepsTheBody()
    {
        var body = """{"error":{"message":"Authentication Fails","type":"authentication_error","code":"invalid_api_key"}}""";
        using var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent(body) };

        var ex = await ChatCompletionExceptionDetector.DetectAsync(response, TestContext.Current.CancellationToken);

        var http = ex.Should().BeOfType<ProviderHttpException>().Subject;
        http.Message.Should().Be("Authentication Fails");
        http.ResponseBody.Should().Be(body);
    }

    private sealed class FakeResponse(int status, string json, params (string Name, string Value)[] headers) : PipelineResponse
    {
        public override int Status => status;
        public override string ReasonPhrase => "fake";
        public override Stream? ContentStream { get; set; }
        public override BinaryData Content => BinaryData.FromString(json);
        protected override PipelineResponseHeaders HeadersCore { get; } = new Headers(headers);
        public override BinaryData BufferContent(CancellationToken cancellationToken = default) => Content;
        public override ValueTask<BinaryData> BufferContentAsync(CancellationToken cancellationToken = default) => new(Content);
        public override void Dispose() { }
    }

    private sealed class Headers((string Name, string Value)[] headers) : PipelineResponseHeaders
    {
        public override bool TryGetValue(string name, out string? value)
        {
            value = headers.FirstOrDefault(h => string.Equals(h.Name, name, StringComparison.OrdinalIgnoreCase)).Value;
            return value is not null;
        }

        public override bool TryGetValues(string name, out IEnumerable<string>? values)
        {
            values = TryGetValue(name, out var value) ? [value!] : null;
            return values is not null;
        }

        public override IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            => headers.Select(h => new KeyValuePair<string, string>(h.Name, h.Value)).GetEnumerator();
    }
}
