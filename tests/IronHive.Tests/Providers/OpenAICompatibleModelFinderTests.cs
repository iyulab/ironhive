using System.Net;
using System.Text;
using AwesomeAssertions;
using IronHive.Abstractions.Models;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible;
using Xunit;

namespace IronHive.Tests.Providers;

/// <summary>
/// An OpenAI-compatible server run on a private network reports the context it accepts with each model; the consumer has
/// no catalogue that knows a locally named model. The OpenAI SDK's model type drops that field, so the context length
/// never reached <see cref="LanguageModelCard.ContextWindow"/>.
/// </summary>
public class OpenAICompatibleModelFinderTests
{
    private const string VllmList = """
        {"object":"list","data":[
          {"id":"qwen3-8b","object":"model","created":1790000000,"owned_by":"vllm","max_model_len":32768},
          {"id":"bge-m3","object":"model","created":1780000000,"owned_by":"vllm","max_model_len":8192}
        ]}
        """;

    private const string LlamaServerList = """
        {"object":"list","data":[
          {"id":"gemma.gguf","object":"model","created":1790000000,"owned_by":"llamacpp",
           "meta":{"vocab_type":1,"n_vocab":262144,"n_ctx_train":131072,"n_embd":2560}}
        ]}
        """;

    private const string PlainList = """
        {"object":"list","data":[{"id":"llama3","object":"model","created":1790000000,"owned_by":"library","meta":null}]}
        """;

    [Fact]
    public async Task A_vLLM_listing_carries_the_context_the_server_accepts()
    {
        using var finder = Finder(VllmList, out _);

        var cards = (await finder.ListModelsAsync(TestContext.Current.CancellationToken)).ToList();

        cards.Should().AllBeOfType<LanguageModelCard>();
        cards.Cast<LanguageModelCard>().Select(c => (c.ModelId, c.ContextWindow))
            .Should().Equal(("qwen3-8b", 32768), ("bge-m3", 8192));
        cards[0].OwnedBy.Should().Be("vllm");
        cards[0].CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1790000000).UtcDateTime);
    }

    [Fact]
    public async Task The_training_context_of_a_llama_server_model_is_not_reported_as_the_window()
    {
        // n_ctx_train is what the model was trained with; the server may run with less, and a window larger than
        // the server accepts is worse than none.
        using var finder = Finder(LlamaServerList, out _);

        var card = (await finder.ListModelsAsync(TestContext.Current.CancellationToken)).Single();

        card.Should().NotBeOfType<LanguageModelCard>();
        card.ModelId.Should().Be("gemma.gguf");
    }

    [Fact]
    public async Task A_server_that_reports_nothing_gives_plain_cards_as_before()
    {
        using var finder = Finder(PlainList, out _);

        var card = (await finder.ListModelsAsync(TestContext.Current.CancellationToken)).Single();

        card.Should().BeOfType<ModelCard>();
        card.ModelId.Should().Be("llama3");
    }

    [Fact]
    public async Task Finding_one_model_on_vLLM_returns_the_card_the_list_returns()
    {
        // vLLM serves only GET /v1/models; GET /v1/models/{id} is a 404 there (and on llama.cpp's server).
        using var finder = Finder(VllmList, out var requests, listOnly: true);

        var card = await finder.FindModelAsync("bge-m3", TestContext.Current.CancellationToken);

        card.Should().BeOfType<LanguageModelCard>().Which.ContextWindow.Should().Be(8192);
        requests.Should().Equal("/v1/models");
    }

    [Fact]
    public async Task A_model_the_server_does_not_list_is_null()
    {
        using var finder = Finder(VllmList, out _, listOnly: true);

        (await finder.FindModelAsync("missing", TestContext.Current.CancellationToken)).Should().BeNull();
        (await finder.FindModelAsync("QWEN3-8B", TestContext.Current.CancellationToken)).Should().BeNull();
    }

    [Fact]
    public async Task A_server_that_cannot_be_asked_is_null()
    {
        using var finder = Finder("""{"error":{"message":"unauthorized"}}""", out _, HttpStatusCode.Unauthorized);

        (await finder.FindModelAsync("qwen3-8b", TestContext.Current.CancellationToken)).Should().BeNull();
    }

    private static OpenAICompatibleModelFinder Finder(
        string body, out List<string> requests, HttpStatusCode status = HttpStatusCode.OK, bool listOnly = false)
    {
        var seen = new List<string>();
        requests = seen;
        var http = new HttpClient(new StubHandler(seen, status, body, listOnly));
        return new OpenAICompatibleModelFinder(new OpenAIConfig { BaseUrl = "http://server.test/v1", HttpClient = http });
    }

    /// <summary><paramref name="listOnly"/>: only <c>/v1/models</c> exists, as on vLLM — anything else is vLLM's 404.</summary>
    private sealed class StubHandler(List<string> requests, HttpStatusCode status, string body, bool listOnly) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;
            lock (requests)
                requests.Add(path);
            var routed = !listOnly || path == "/v1/models";
            return Task.FromResult(new HttpResponseMessage(routed ? status : HttpStatusCode.NotFound)
            {
                Content = new StringContent(routed ? body : """{"detail":"Not Found"}""", Encoding.UTF8, "application/json"),
            });
        }
    }
}
