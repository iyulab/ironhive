using System.Net;
using System.Net.Sockets;
using System.Text;
using AwesomeAssertions;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.Reranking;

namespace IronHive.Tests.Providers;

/// <summary>
/// <see cref="CohereDocumentReranker"/> talks raw HTTP/JSON against a Cohere-shaped
/// <c>POST /rerank</c> endpoint, taking an already-resolved <see cref="OpenAIConfig"/>. These
/// tests exercise it against a real loopback listener rather than a fake handler, keeping the
/// request/response wire contract under test without depending on an internal seam.
/// </summary>
public class CohereDocumentRerankerTests
{
    private static (HttpListener Listener, string Prefix) StartListener()
    {
        var port = GetFreeTcpPort();
        var prefix = $"http://127.0.0.1:{port}/";
        var listener = new HttpListener();
        listener.Prefixes.Add(prefix);
        listener.Start();
        return (listener, prefix);
    }

    private static int GetFreeTcpPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    [Fact]
    public async Task RerankAsync_SendsModelQueryDocumentsTopNAndAuthorization_ToTheRerankPath()
    {
        var (listener, prefix) = StartListener();
        using var _ = listener;

        string? capturedPath = null;
        string? capturedMethod = null;
        string? capturedBody = null;
        string? capturedAuth = null;

        var serverTask = Task.Run(async () =>
        {
            var ctx = await listener.GetContextAsync().WaitAsync(TestContext.Current.CancellationToken);
            capturedPath = ctx.Request.Url!.AbsolutePath;
            capturedMethod = ctx.Request.HttpMethod;
            capturedAuth = ctx.Request.Headers["Authorization"];
            using var reader = new StreamReader(ctx.Request.InputStream);
            capturedBody = await reader.ReadToEndAsync();

            var responseBytes = Encoding.UTF8.GetBytes("""{"results":[]}""");
            ctx.Response.ContentType = "application/json";
            await ctx.Response.OutputStream.WriteAsync(responseBytes);
            ctx.Response.OutputStream.Close();
        }, TestContext.Current.CancellationToken);

        var config = new OpenAIConfig { BaseUrl = prefix.TrimEnd('/'), ApiKey = "test-key" };
        using var reranker = new CohereDocumentReranker(config);

        await reranker.RerankAsync(
            "rerank-v1",
            "what is the capital of france?",
            ["paris is the capital", "berlin is the capital of germany"],
            topN: 1,
            cancellationToken: TestContext.Current.CancellationToken);

        await serverTask;

        capturedMethod.Should().Be("POST");
        capturedPath.Should().Be("/rerank");
        capturedAuth.Should().Be("Bearer test-key");
        capturedBody.Should().Contain("\"model\":\"rerank-v1\"");
        capturedBody.Should().Contain("\"query\":\"what is the capital of france?\"");
        capturedBody.Should().Contain("\"documents\":[\"paris is the capital\",\"berlin is the capital of germany\"]");
        capturedBody.Should().Contain("\"top_n\":1");
    }

    [Fact]
    public async Task RerankAsync_ParsesIndexAndRelevanceScoreFromResponse()
    {
        var (listener, prefix) = StartListener();
        using var _ = listener;

        var serverTask = Task.Run(async () =>
        {
            var ctx = await listener.GetContextAsync().WaitAsync(TestContext.Current.CancellationToken);
            var body = """
            {
              "results": [
                { "index": 1, "relevance_score": 0.987 },
                { "index": 0, "relevance_score": 0.123 }
              ]
            }
            """;
            var bytes = Encoding.UTF8.GetBytes(body);
            ctx.Response.ContentType = "application/json";
            await ctx.Response.OutputStream.WriteAsync(bytes);
            ctx.Response.OutputStream.Close();
        }, TestContext.Current.CancellationToken);

        var config = new OpenAIConfig { BaseUrl = prefix.TrimEnd('/') };
        using var reranker = new CohereDocumentReranker(config);

        var results = (await reranker.RerankAsync(
            "rerank-v1",
            "query",
            ["doc-a", "doc-b"],
            cancellationToken: TestContext.Current.CancellationToken)).ToList();

        await serverTask;

        results.Should().HaveCount(2);
        results[0].Index.Should().Be(1);
        results[0].Score.Should().BeApproximately(0.987f, 0.0001f);
        results[1].Index.Should().Be(0);
        results[1].Score.Should().BeApproximately(0.123f, 0.0001f);
    }

    [Fact]
    public async Task RerankAsync_NonSuccessStatus_ThrowsWithResponseBody()
    {
        var (listener, prefix) = StartListener();
        using var _ = listener;

        var serverTask = Task.Run(async () =>
        {
            var ctx = await listener.GetContextAsync().WaitAsync(TestContext.Current.CancellationToken);
            ctx.Response.StatusCode = 400;
            var bytes = Encoding.UTF8.GetBytes("""{"error":"invalid model"}""");
            await ctx.Response.OutputStream.WriteAsync(bytes);
            ctx.Response.OutputStream.Close();
        }, TestContext.Current.CancellationToken);

        var config = new OpenAIConfig { BaseUrl = prefix.TrimEnd('/') };
        using var reranker = new CohereDocumentReranker(config);

        var act = async () => await reranker.RerankAsync(
            "bad-model",
            "query",
            ["doc-a"],
            cancellationToken: TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*invalid model*");

        await serverTask;
    }
}
