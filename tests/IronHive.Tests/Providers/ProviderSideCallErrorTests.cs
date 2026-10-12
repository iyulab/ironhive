using System.Net;
using System.Text;
using AwesomeAssertions;
using IronHive.Abstractions.Exceptions;
using IronHive.Abstractions.Images;
using IronHive.Abstractions.Models;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;

namespace IronHive.Tests.Providers;

/// <summary>
/// Listing models and generating images fail the way a message call with the same key does: a
/// <see cref="ProviderHttpException"/> with the status and the provider's sentence, never the SDK's own exception.
/// </summary>
public class ProviderSideCallErrorTests
{
    public static TheoryData<string> Finders => new() { "openai", "anthropic", "googleai" };

    [Theory]
    [MemberData(nameof(Finders))]
    public async Task ListModels_WithAnInvalidKey_IsProviderHttpException401(string provider)
    {
        var (body, sentence) = provider switch
        {
            "openai" => ("""{"error":{"message":"Incorrect API key provided.","type":"invalid_request_error","code":"invalid_api_key"}}""", "Incorrect API key provided."),
            "anthropic" => ("""{"type":"error","error":{"type":"authentication_error","message":"invalid x-api-key"}}""", "invalid x-api-key"),
            _ => ("""{"error":{"code":401,"message":"API key not valid. Please pass a valid API key.","status":"UNAUTHENTICATED"}}""", "API key not valid. Please pass a valid API key."),
        };
        using var http = new HttpClient(new FixedHandler(HttpStatusCode.Unauthorized, body));
        using IModelFinder finder = provider switch
        {
            "openai" => new OpenAIModelFinder(new OpenAIConfig { BaseUrl = "https://gateway.example/v1", ApiKey = "sk-test", HttpClient = http, MaxRetries = 0 }),
            "anthropic" => new AnthropicModelFinder(new AnthropicConfig { ApiKey = "sk-test", HttpClient = http, MaxRetries = 0 }),
            _ => new GoogleAIModelFinder(new GoogleAIConfig { ApiKey = "test", HttpClientFactory = () => http }),
        };

        var list = () => finder.ListModelsAsync(TestContext.Current.CancellationToken);

        var thrown = await list.Should().ThrowAsync<ProviderHttpException>();
        thrown.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        thrown.Which.Message.Should().Be(sentence);
    }

    [Fact]
    public async Task OpenAIImage_WithAnInvalidKey_IsProviderHttpException401()
    {
        var body = """{"error":{"message":"Incorrect API key provided.","type":"invalid_request_error","code":"invalid_api_key"}}""";
        using var http = new HttpClient(new FixedHandler(HttpStatusCode.Unauthorized, body));
        var generator = new OpenAIImageGenerator(new OpenAIConfig { BaseUrl = "https://gateway.example/v1", ApiKey = "sk-test", HttpClient = http, MaxRetries = 0 });

        var generate = () => generator.GenerateImageAsync(new ImageGenerationRequest { Model = "gpt-image-1", Prompt = "a cat" }, TestContext.Current.CancellationToken);

        var thrown = await generate.Should().ThrowAsync<ProviderHttpException>();
        thrown.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        thrown.Which.Message.Should().Be("Incorrect API key provided.");
        thrown.Which.ResponseBody.Should().Be(body);
    }

    [Fact]
    public async Task OpenAI_PlainTextBody_IsTheMessage()
    {
        // A proxy in front of an OpenAI-compatible server that answers in plain text: System.ClientModel's own message
        // would be «Service request failed.» and a response dump.
        using var http = new HttpClient(new FixedHandler(HttpStatusCode.Forbidden, "Access denied by proxy policy", "text/plain"));
        using var finder = new OpenAIModelFinder(new OpenAIConfig { BaseUrl = "https://gateway.example/v1", ApiKey = "sk-test", HttpClient = http, MaxRetries = 0 });

        var list = () => finder.ListModelsAsync(TestContext.Current.CancellationToken);

        (await list.Should().ThrowAsync<ProviderHttpException>()).Which.Message.Should().Be("Access denied by proxy policy");
    }

    private sealed class FixedHandler(HttpStatusCode status, string body, string mediaType = "application/json") : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, mediaType) });
    }
}
