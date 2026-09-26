using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using IronHive.Abstractions;
using NSubstitute;
using IronHive.Abstractions.Embedding;
using IronHive.Core.Services;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible;
using IronHive.Providers.OpenAI.Compatible.Embedding;

namespace IronHive.Tests.Providers;

/// <summary>
/// An embedding call can carry provider-specific request fields — the embedding counterpart of
/// <c>MessageGenerationRequest.ExtraBody</c>. Compatible embeddings went through the OpenAI SDK's client, whose request
/// model has no slot for them, so a consumer that configures server fields per endpoint could not move onto IronHive
/// without those fields silently dropping. The compatible provider now sends them; a provider that cannot says so.
/// </summary>
public class EmbeddingExtraBodyTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private const string TwoVectors = """
        {"object":"list","model":"served-model","usage":{"prompt_tokens":5,"total_tokens":5},
         "data":[{"object":"embedding","index":1,"embedding":[0.3,0.4]},{"object":"embedding","index":0,"embedding":[0.1,0.2]}]}
        """;

    private sealed class RecordingHandler(string json) : HttpMessageHandler
    {
        public List<string> Bodies { get; } = [];
        public List<string?> Authorizations { get; } = [];
        public List<Uri?> Uris { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Uris.Add(request.RequestUri);
            Authorizations.Add(request.Headers.Authorization?.ToString());
            Bodies.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        }
    }

    private static (OpenAICompatibleEmbeddingGenerator Generator, RecordingHandler Handler) Compatible(string json = TwoVectors)
    {
        var handler = new RecordingHandler(json);
        var generator = new OpenAICompatibleEmbeddingGenerator(new OpenAIConfig
        {
            BaseUrl = "http://embed.invalid/v1",
            HttpClient = new HttpClient(handler),
        });
        return (generator, handler);
    }

    [Fact]
    public async Task Compatible_sends_the_callers_extra_fields_and_still_reports_usage_and_model()
    {
        var (generator, handler) = Compatible();
        using var _ = generator;

        var response = await generator.EmbedBatchAsync("embed-model", ["alpha", "beta"],
            new EmbeddingRequestOptions { ExtraBody = new JsonObject { ["truncate"] = true, ["pooling"] = new JsonObject { ["type"] = "mean" } } }, Ct);

        var body = JsonNode.Parse(handler.Bodies.Single())!.AsObject();
        body["model"]!.GetValue<string>().Should().Be("embed-model");
        body["input"]!.AsArray().Select(n => n!.GetValue<string>()).Should().Equal("alpha", "beta");
        body["truncate"]!.GetValue<bool>().Should().BeTrue();
        body["pooling"]!["type"]!.GetValue<string>().Should().Be("mean");
        handler.Uris.Single()!.AbsolutePath.Should().Be("/v1/embeddings");

        response.InputTokens.Should().Be(5, "the server's usage, not the estimate");
        response.Model.Should().Be("served-model");
        response.Results.Select(r => r.Index).Should().Equal(0, 1);
        // data[].index maps back to the caller's positions (the server answered out of order).
        response.Results[0].Embedding.Should().Equal(0.1f, 0.2f);
    }

    [Fact]
    public async Task Compatible_without_options_sends_only_the_typed_fields()
    {
        // Control: nothing leaks into the body when the caller sent nothing.
        var (generator, handler) = Compatible();
        using var _ = generator;

        await generator.EmbedBatchAsync("embed-model", ["alpha", "beta"], Ct);

        JsonNode.Parse(handler.Bodies.Single())!.AsObject().Select(p => p.Key).Should().BeEquivalentTo(["model", "input"]);
    }

    [Fact]
    public void Extra_fields_replace_a_typed_field_like_the_message_path()
    {
        var body = OpenAICompatibleEmbeddingGenerator.BuildRequestBody("m", ["x"], new JsonObject { ["model"] = "override" });

        body["model"]!.GetValue<string>().Should().Be("override");
    }

    [Fact]
    public async Task Compatible_without_usage_reports_null_not_the_estimate()
    {
        var (generator, _) = Compatible("""{"model":"m","data":[{"index":0,"embedding":[0.5]}]}""");
        using var __ = generator;

        var response = await generator.EmbedBatchAsync("m", ["alpha"], Ct);

        response.InputTokens.Should().BeNull();
        response.Results.Single().Embedding.Should().Equal(0.5f);
    }

    [Fact]
    public async Task Compatible_reads_the_api_key_resolver_on_every_request()
    {
        // OpenAIConfig.ApiKeyResolver is honoured per call on the SDK path; the raw-HTTP clients took ApiKey only.
        var handler = new RecordingHandler(TwoVectors);
        var key = "first";
        using var generator = new OpenAICompatibleEmbeddingGenerator(new OpenAIConfig
        {
            BaseUrl = "http://embed.invalid/v1",
            ApiKey = "static",
            ApiKeyResolver = () => key,
            HttpClient = new HttpClient(handler),
        });

        await generator.EmbedBatchAsync("m", ["alpha", "beta"], Ct);
        key = "rotated";
        await generator.EmbedBatchAsync("m", ["alpha", "beta"], Ct);
        key = "";
        await generator.EmbedBatchAsync("m", ["alpha", "beta"], Ct);

        handler.Authorizations.Should().Equal("Bearer first", "Bearer rotated", "Bearer static");
    }

    [Fact]
    public async Task OpenAI_refuses_extra_fields_instead_of_dropping_them()
    {
        using var generator = new OpenAIEmbeddingGenerator(new OpenAIConfig
        {
            ApiKey = "k",
            BaseUrl = "http://embed.invalid/v1",
            HttpClient = new HttpClient(new RecordingHandler(TwoVectors)),
        });

        var call = () => generator.EmbedBatchAsync("m", ["alpha"], new EmbeddingRequestOptions { ExtraBody = new JsonObject { ["x"] = 1 } }, Ct);

        await call.Should().ThrowAsync<NotSupportedException>().WithMessage("*ExtraBody*OpenAI-compatible*");
    }

    [Fact]
    public async Task EmbeddingService_passes_the_options_to_the_provider()
    {
        var (generator, handler) = Compatible();
        var service = new EmbeddingService(new Dictionary<string, IEmbeddingGenerator> { ["local"] = generator });

        await service.EmbedBatchAsync("local", "embed-model", ["alpha", "beta"],
            new EmbeddingRequestOptions { ExtraBody = new JsonObject { ["truncate"] = true } }, Ct);

        JsonNode.Parse(handler.Bodies.Single())!["truncate"]!.GetValue<bool>().Should().BeTrue();
    }

    [Fact]
    public void Compatible_and_GpuStack_register_the_raw_http_embedding_generator()
    {
        var generators = new List<IEmbeddingGenerator>();
        var builder = NSubstitute.Substitute.For<IronHive.Abstractions.IHiveServiceBuilder>();
        builder.AddEmbeddingGenerator(NSubstitute.Arg.Any<string>(), NSubstitute.Arg.Do<IEmbeddingGenerator>(generators.Add)).Returns(builder);

        builder.AddOpenAICompatibleProviders("c", new OpenAICompatibleConfig { BaseUrl = "http://c.invalid/v1" }, OpenAICompatibleServiceType.Embeddings);
        builder.AddGpuStackProviders("g", new IronHive.Providers.OpenAI.Compatible.GpuStack.GpuStackConfig { BaseUrl = "http://g.invalid" }, IronHive.Providers.OpenAI.Compatible.GpuStack.GpuStackServiceType.Embeddings);

        generators.Should().HaveCount(2).And.AllBeOfType<OpenAICompatibleEmbeddingGenerator>();
    }
}
