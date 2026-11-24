using System.Net;
using System.Text;
using AwesomeAssertions;
using IronHive.Abstractions.Exceptions;
using IronHive.Abstractions.Models;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible;

namespace IronHive.Tests.Providers;

/// <summary>
/// A single-model lookup answers <see langword="null"/> for «there is no such model» only. A lookup that failed — a refused
/// key, a server error, a server that cannot be reached — throws what the list lookup throws, so an expired key never reads as
/// a missing model.
/// </summary>
public class FindModelFailureTests
{
    public static TheoryData<string, HttpStatusCode> RefusedLookups()
    {
        var data = new TheoryData<string, HttpStatusCode>();
        foreach (var provider in new[] { "openai", "anthropic", "googleai", "compatible" })
        {
            data.Add(provider, HttpStatusCode.Unauthorized);
            data.Add(provider, HttpStatusCode.InternalServerError);
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(RefusedLookups))]
    public async Task FindModelAsync_AFailedLookup_ThrowsTheStatusTheServerSent(string provider, HttpStatusCode status)
    {
        using var finder = Finder(provider, new StatusHandler(status, ErrorBody(provider, status)));

        var find = () => finder.FindModelAsync("some-model", TestContext.Current.CancellationToken);

        (await find.Should().ThrowAsync<ProviderHttpException>()).Which.StatusCode.Should().Be(status);
    }

    [Theory]
    [InlineData("openai")]
    [InlineData("anthropic")]
    [InlineData("googleai")]
    public async Task FindModelAsync_TheServerKnowsNoSuchModel_IsNull(string provider)
    {
        using var finder = Finder(provider, new StatusHandler(HttpStatusCode.NotFound, ErrorBody(provider, HttpStatusCode.NotFound)));

        (await finder.FindModelAsync("some-model", TestContext.Current.CancellationToken)).Should().BeNull();
    }

    /// <summary>The compatible finder reads the list: a missing list route is a failed lookup, not a missing model.</summary>
    [Fact]
    public async Task FindModelAsync_ACompatibleServerWithoutAModelList_Throws()
    {
        using var finder = Finder("compatible", new StatusHandler(HttpStatusCode.NotFound, ErrorBody("compatible", HttpStatusCode.NotFound)));

        var find = () => finder.FindModelAsync("some-model", TestContext.Current.CancellationToken);

        (await find.Should().ThrowAsync<ProviderHttpException>()).Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("openai")]
    [InlineData("anthropic")]
    [InlineData("googleai")]
    [InlineData("compatible")]
    public async Task FindModelAsync_AServerThatCannotBeReached_Throws(string provider)
    {
        using var finder = Finder(provider, new UnreachableHandler());

        var find = () => finder.FindModelAsync("some-model", TestContext.Current.CancellationToken);

        await find.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task FindCapabilitiesAsync_ARefusedKey_Throws()
    {
        using var finder = (AnthropicModelFinder)Finder("anthropic",
            new StatusHandler(HttpStatusCode.Unauthorized, ErrorBody("anthropic", HttpStatusCode.Unauthorized)));

        var find = () => finder.FindCapabilitiesAsync("some-model", TestContext.Current.CancellationToken);

        (await find.Should().ThrowAsync<ProviderHttpException>()).Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static IModelFinder Finder(string provider, HttpMessageHandler handler)
    {
        var http = new HttpClient(handler);
        OpenAIConfig OpenAI() => new() { BaseUrl = "https://gateway.example/v1", ApiKey = "sk-test", HttpClient = http, MaxRetries = 0 };
        return provider switch
        {
            "openai" => new OpenAIModelFinder(OpenAI()),
            "compatible" => new OpenAICompatibleModelFinder(OpenAI()),
            "anthropic" => new AnthropicModelFinder(new AnthropicConfig { ApiKey = "sk-test", HttpClient = http, MaxRetries = 0 }),
            _ => new GoogleAIModelFinder(new GoogleAIConfig { ApiKey = "test", HttpClientFactory = () => http }),
        };
    }

    private static string ErrorBody(string provider, HttpStatusCode status) => provider switch
    {
        "anthropic" => """{"type":"error","error":{"type":"api_error","message":"refused"}}""",
        "googleai" => $$$"""{"error":{"code":{{{(int)status}}},"message":"refused","status":"UNKNOWN"}}""",
        _ => """{"error":{"message":"refused","type":"invalid_request_error"}}""",
    };

    private sealed class StatusHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
                RequestMessage = request,
            });
    }

    private sealed class UnreachableHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            throw new HttpRequestException("connection refused");
    }
}
