using System.Net;
using System.Text;
using AwesomeAssertions;
using IronHive.Abstractions.Embedding;
using IronHive.Core.Microsoft;
using IronHive.Providers.OpenAI;
using NSubstitute;

namespace IronHive.Tests.Providers;

/// <summary>
/// The provider reports how many input tokens an embedding call consumed and which model served it; until 0.39.0
/// <see cref="IEmbeddingGenerator.EmbedBatchAsync(string, IEnumerable{string}, CancellationToken)"/> returned the vectors only, so a consumer that prices or audits
/// embedding calls re-implemented the HTTP call to read them. Unreported stays null — never a guess.
/// </summary>
public class EmbeddingUsageTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private sealed class StubHandler(string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });
    }

    private static OpenAIEmbeddingGenerator OpenAIWith(string json) => new(new OpenAIConfig
    {
        ApiKey = "k",
        BaseUrl = "http://embed.invalid/v1",
        HttpClient = new HttpClient(new StubHandler(json)),
    });

    [Fact]
    public async Task OpenAI_ReportsTheServersInputTokens_AndTheModelThatAnswered()
    {
        using var generator = OpenAIWith("""
            {"object":"list","model":"qwen3-embedding-0.6b-q8","usage":{"prompt_tokens":7,"total_tokens":7},
             "data":[{"object":"embedding","index":0,"embedding":[0.1,0.2]},{"object":"embedding","index":1,"embedding":[0.3,0.4]}]}
            """);

        var response = await generator.EmbedBatchAsync("qwen-embed", ["alpha", "beta"], Ct);

        response.InputTokens.Should().Be(7);
        response.Model.Should().Be("qwen3-embedding-0.6b-q8", "the model the server says answered, not the id that was asked for");
        response.Results.Select(r => r.Index).Should().Equal(0, 1);
        response.Results[1].Embedding.Should().Equal(0.3f, 0.4f);
    }

    [Fact]
    public async Task OpenAI_WithoutUsageInTheResponse_ReportsNull_NotZero()
    {
        using var generator = OpenAIWith("""
            {"object":"list","model":"m","data":[{"object":"embedding","index":0,"embedding":[0.1]}]}
            """);

        var response = await generator.EmbedBatchAsync("m", ["alpha"], Ct);

        response.InputTokens.Should().BeNull("a server that reports nothing must not read as zero tokens");
    }

    [Fact]
    public async Task MicrosoftAdapter_CarriesUsageAndModel()
    {
        var inner = Substitute.For<IEmbeddingGenerator>();
        inner.EmbedBatchAsync("m", Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new EmbeddingResponse
            {
                Results = [new EmbeddingResult { Index = 0, Embedding = [0.1f] }],
                InputTokens = 5,
                Model = "served-m",
            });
        using var adapter = new EmbeddingGeneratorAdapter(inner, "m");

        var generated = await adapter.GenerateAsync(["alpha"], cancellationToken: Ct);

        generated.Usage!.InputTokenCount.Should().Be(5);
        generated.Single().ModelId.Should().Be("served-m");
    }

    [Fact]
    public async Task MicrosoftAdapter_WithoutReportedUsage_LeavesUsageUnset()
    {
        var inner = Substitute.For<IEmbeddingGenerator>();
        inner.EmbedBatchAsync("m", Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new EmbeddingResponse { Results = [new EmbeddingResult { Index = 0, Embedding = [0.1f] }] });
        using var adapter = new EmbeddingGeneratorAdapter(inner, "m");

        var generated = await adapter.GenerateAsync(["alpha"], cancellationToken: Ct);

        generated.Usage.Should().BeNull();
    }
}
