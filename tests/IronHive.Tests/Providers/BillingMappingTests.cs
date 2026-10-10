using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;
using Anthropic.Exceptions;
using AwesomeAssertions;
using Google.GenAI;
using IronHive.Abstractions.Exceptions;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using OpenAIMapper = IronHive.Providers.OpenAI.OpenAIExceptionMapper;
using AnthropicMapper = IronHive.Providers.Anthropic.AnthropicExceptionMapper;
using GoogleAIMapper = IronHive.Providers.GoogleAI.GoogleAIExceptionMapper;

namespace IronHive.Tests.Providers;

/// <summary>
/// An account that cannot pay is a <see cref="BillingException"/> on every provider — never a
/// <see cref="RateLimitException"/>, whose contract is «wait and try again».
/// </summary>
public class BillingMappingTests
{
    // ---- OpenAI-compatible (own HTTP client) ----

    [Fact]
    public async Task Compatible_PaymentRequired_Is_Billing()
    {
        using var response = JsonResponse(HttpStatusCode.PaymentRequired,
            """{"error":{"message":"Insufficient Balance","type":"unknown_error"}}""");

        var ex = await ChatCompletionExceptionDetector.DetectAsync(response, TestContext.Current.CancellationToken);

        var billing = ex.Should().BeOfType<BillingException>().Subject;
        billing.Message.Should().Be("Insufficient Balance");
        billing.StatusCode.Should().Be(HttpStatusCode.PaymentRequired, "the status the provider answered with stays readable");
    }

    [Fact]
    public async Task Compatible_InsufficientQuota_429_Is_Billing_Not_RateLimit()
    {
        using var response = JsonResponse(HttpStatusCode.TooManyRequests,
            """{"error":{"message":"You exceeded your current quota, please check your plan and billing details.","type":"insufficient_quota","code":"insufficient_quota"}}""");
        response.Headers.Add("Retry-After", "20");

        var ex = await ChatCompletionExceptionDetector.DetectAsync(response, TestContext.Current.CancellationToken);

        ex.Should().BeOfType<BillingException>().Which.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Compatible_Plain_RateLimit_Stays_RateLimit()
    {
        using var response = JsonResponse(HttpStatusCode.TooManyRequests,
            """{"error":{"message":"Rate limit reached for requests","type":"requests","code":"rate_limit_exceeded"}}""");

        var ex = await ChatCompletionExceptionDetector.DetectAsync(response, TestContext.Current.CancellationToken);

        ex.Should().BeOfType<RateLimitException>();
    }

    [Fact]
    public void Compatible_MidStream_InsufficientQuota_Is_Billing()
    {
        ChatCompletionExceptionDetector.Detect("insufficient_quota: You exceeded your current quota")
            .Should().BeOfType<BillingException>().Which.StatusCode.Should().BeNull("an error line inside a stream has no status");
    }

    // ---- OpenAI SDK (ClientResultException) ----

    [Fact]
    public void OpenAI_InsufficientQuota_429_Is_Billing()
    {
        var sdk = new ClientResultException(new FakeResponse(429,
            """{"error":{"message":"You exceeded your current quota","type":"insufficient_quota","code":"insufficient_quota"}}"""));

        var billing = OpenAIMapper.Map(sdk, TestContext.Current.CancellationToken).Should().BeOfType<BillingException>().Subject;
        billing.InnerException.Should().BeSameAs(sdk);
        billing.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        OpenAIErrors.TryMapRateLimit(sdk).Should().BeNull("a quota that waiting does not restore is not a rate limit");
    }

    [Fact]
    public void OpenAI_PaymentRequired_Is_Billing()
    {
        var sdk = new ClientResultException(new FakeResponse(402, """{"error":{"message":"Payment required"}}"""));

        OpenAIMapper.Map(sdk, TestContext.Current.CancellationToken).Should().BeOfType<BillingException>()
            .Which.StatusCode.Should().Be(HttpStatusCode.PaymentRequired);
    }

    [Fact]
    public void OpenAI_Plain_429_Stays_RateLimit()
    {
        var sdk = new ClientResultException(new FakeResponse(429,
            """{"error":{"message":"Rate limit reached","type":"requests","code":"rate_limit_exceeded"}}"""));

        OpenAIMapper.Map(sdk, TestContext.Current.CancellationToken).Should().BeOfType<RateLimitException>();
    }

    // ---- Anthropic ----

    [Fact]
    public void Anthropic_BillingError_Is_Billing()
    {
        var body = """{"type":"error","error":{"type":"billing_error","message":"Your account has a billing issue."}}""";
        var sdk = AnthropicExceptionFactory.CreateApiException(HttpStatusCode.PaymentRequired, body);

        AnthropicMapper.Map(sdk, TestContext.Current.CancellationToken).Should().BeOfType<BillingException>()
            .Which.StatusCode.Should().Be(HttpStatusCode.PaymentRequired);
    }

    [Fact]
    public void Anthropic_CreditBalanceTooLow_Is_Billing()
    {
        var body = """{"type":"error","error":{"type":"invalid_request_error","message":"Your credit balance is too low to access the Anthropic API."}}""";
        var sdk = AnthropicExceptionFactory.CreateApiException(HttpStatusCode.BadRequest, body);

        AnthropicMapper.Map(sdk, TestContext.Current.CancellationToken).Should().BeOfType<BillingException>()
            .Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ---- Google GenAI ----

    [Fact]
    public void Google_PaymentRequired_Is_Billing()
    {
        var clientError = new ClientError("Payment required", 402, "PAYMENT_REQUIRED");

        GoogleAIMapper.Map(clientError, TestContext.Current.CancellationToken).Should().BeOfType<BillingException>()
            .Which.StatusCode.Should().Be(HttpStatusCode.PaymentRequired);
    }

    // ---- the vendor's own code travels with the type (#1016: «top up» vs «raise the limit» are different codes) ----

    [Fact]
    public async Task Compatible_Billing_Carries_The_Body_Code()
    {
        using var response = JsonResponse(HttpStatusCode.PaymentRequired,
            """{"error":{"message":"Spending limit reached for this key","type":"billing_error","code":"key_budget_exceeded"}}""");

        var ex = await ChatCompletionExceptionDetector.DetectAsync(response, TestContext.Current.CancellationToken);

        ex.Should().BeOfType<BillingException>().Which.ErrorCode.Should().Be("key_budget_exceeded", "code wins over type");
    }

    [Fact]
    public async Task Compatible_Unclassified_Error_Carries_The_Body_Code()
    {
        using var response = JsonResponse(HttpStatusCode.ServiceUnavailable,
            """{"error":{"message":"Upstream unavailable","type":"upstream_error"}}""");

        var ex = await ChatCompletionExceptionDetector.DetectAsync(response, TestContext.Current.CancellationToken);

        ex.Should().BeOfType<ProviderHttpException>().Which.ErrorCode.Should().Be("upstream_error");
    }

    [Fact]
    public void OpenAI_Billing_Carries_The_Code()
    {
        var sdk = new ClientResultException(new FakeResponse(429,
            """{"error":{"message":"You exceeded your current quota","type":"insufficient_quota","code":"insufficient_quota"}}"""));

        OpenAIMapper.Map(sdk, TestContext.Current.CancellationToken).Should().BeOfType<BillingException>()
            .Which.ErrorCode.Should().Be("insufficient_quota");
    }

    [Fact]
    public void OpenAI_HardLimitReached_Is_Billing()
    {
        // The account's own spending limit: raising the limit clears it, waiting does not.
        var sdk = new ClientResultException(new FakeResponse(400,
            """{"error":{"message":"Billing hard limit has been reached","type":"invalid_request_error","code":"billing_hard_limit_reached"}}"""));

        OpenAIMapper.Map(sdk, TestContext.Current.CancellationToken).Should().BeOfType<BillingException>()
            .Which.ErrorCode.Should().Be("billing_hard_limit_reached");
    }

    [Fact]
    public void Anthropic_PaymentRequired_Without_Anthropic_Body_Is_Billing()
    {
        // A gateway in front of the API answers 402 in its own shape; the status alone says the account cannot pay.
        var body = """{"error":{"message":"Insufficient credits"}}""";
        var sdk = AnthropicExceptionFactory.CreateApiException(HttpStatusCode.PaymentRequired, body);

        AnthropicMapper.Map(sdk, TestContext.Current.CancellationToken).Should().BeOfType<BillingException>()
            .Which.StatusCode.Should().Be(HttpStatusCode.PaymentRequired);
    }

    [Fact]
    public void Anthropic_Billing_Carries_The_Error_Type()
    {
        var body = """{"type":"error","error":{"type":"billing_error","message":"Your account has a billing issue."}}""";
        var sdk = AnthropicExceptionFactory.CreateApiException(HttpStatusCode.PaymentRequired, body);

        AnthropicMapper.Map(sdk, TestContext.Current.CancellationToken).Should().BeOfType<BillingException>()
            .Which.ErrorCode.Should().Be("billing_error");
    }

    [Fact]
    public void Google_PaymentRequired_Carries_The_Status()
    {
        GoogleAIMapper.Map(new ClientError("Payment required", 402, "PAYMENT_REQUIRED"), TestContext.Current.CancellationToken)
            .Should().BeOfType<BillingException>().Which.ErrorCode.Should().Be("PAYMENT_REQUIRED");
    }

    [Fact]
    public void Google_InsufficientQuota_429_Is_Billing_Not_RateLimit()
    {
        var clientError = new ClientError("You exceeded your current quota (insufficient_quota)", 429, "Too Many Requests");

        var billing = GoogleAIMapper.Map(clientError, TestContext.Current.CancellationToken).Should().BeOfType<BillingException>().Subject;
        billing.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        billing.ErrorCode.Should().Be("insufficient_quota");
    }

    [Fact]
    public void Google_429_Without_ResourceExhausted_Is_RateLimit()
    {
        // A gateway in front of Gemini answers 429 without Gemini's status; the status alone is the signal.
        var clientError = new ClientError("Too many requests", 429, "Too Many Requests");

        GoogleAIMapper.Map(clientError, TestContext.Current.CancellationToken).Should().BeOfType<RateLimitException>();
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json)
        => new(statusCode) { Content = new StringContent(json) };

    private sealed class FakeResponse(int status, string json) : PipelineResponse
    {
        public override int Status => status;
        public override string ReasonPhrase => "fake";
        public override Stream? ContentStream { get; set; }
        public override BinaryData Content => BinaryData.FromString(json);
        protected override PipelineResponseHeaders HeadersCore { get; } = new NoHeaders();
        public override BinaryData BufferContent(CancellationToken cancellationToken = default) => Content;
        public override ValueTask<BinaryData> BufferContentAsync(CancellationToken cancellationToken = default) => new(Content);
        public override void Dispose() { }
    }

    private sealed class NoHeaders : PipelineResponseHeaders
    {
        public override bool TryGetValue(string name, out string? value)
        {
            value = null;
            return false;
        }

        public override bool TryGetValues(string name, out IEnumerable<string>? values)
        {
            values = null;
            return false;
        }

        public override IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            => Enumerable.Empty<KeyValuePair<string, string>>().GetEnumerator();
    }
}
