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
    public async Task Finding_one_model_reads_the_same_field()
    {
        using var finder = Finder("""{"id":"qwen3-8b","object":"model","created":1790000000,"owned_by":"vllm","max_model_len":32768}""", out var requests);

        var card = await finder.FindModelAsync("qwen3-8b", TestContext.Current.CancellationToken);

        card.Should().BeOfType<LanguageModelCard>().Which.ContextWindow.Should().Be(32768);
        requests.Should().ContainSingle().Which.Should().EndWith("/v1/models/qwen3-8b");
    }

    [Fact]
    public async Task An_unknown_model_is_null()
    {
        using var finder = Finder("""{"error":{"message":"not found"}}""", out _, HttpStatusCode.NotFound);

        (await finder.FindModelAsync("missing", TestContext.Current.CancellationToken)).Should().BeNull();
    }

    private static OpenAICompatibleModelFinder Finder(string body, out List<string> requests, HttpStatusCode status = HttpStatusCode.OK)
    {
        var seen = new List<string>();
        requests = seen;
        var http = new HttpClient(new StubHandler(seen, status, body));
        return new OpenAICompatibleModelFinder(new OpenAIConfig { BaseUrl = "http://server.test/v1", HttpClient = http });
    }

    private sealed class StubHandler(List<string> requests, HttpStatusCode status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            lock (requests)
                requests.Add(request.RequestUri!.AbsolutePath);
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
        }
    }
}
