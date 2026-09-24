using System.Net;
using System.Net.Sockets;
using AwesomeAssertions;
using IronHive.Abstractions.Http;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;

namespace IronHive.Tests.Providers;

/// <summary>
/// <c>ApiKeyResolver</c> on the first-party provider configs, observed on the wire: every request carries the key
/// the resolver returns at that moment, in the provider's own credential header, so a key rotated or revoked in a
/// secret store takes effect on the next call. The static <c>ApiKey</c> is only the fallback for a blank answer.
/// </summary>
public sealed class ApiKeyResolverWireTests : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly List<Dictionary<string, string?>> _requests = [];
    private readonly string _origin;

    public ApiKeyResolverWireTests()
    {
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        _origin = $"http://localhost:{port}";
        _listener.Prefixes.Add(_origin + "/");
        _listener.Start();
        _ = Task.Run(ServeAsync);
    }

    private async Task ServeAsync()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext ctx;
            try { ctx = await _listener.GetContextAsync(); }
            catch { return; }

            lock (_requests)
            {
                _requests.Add(new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Authorization"] = ctx.Request.Headers["Authorization"],
                    ["x-api-key"] = ctx.Request.Headers["x-api-key"],
                    ["x-goog-api-key"] = ctx.Request.Headers["x-goog-api-key"],
                    ["query"] = ctx.Request.Url!.Query,
                });
            }

            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = "application/json";
            var body = "{\"object\":\"list\",\"data\":[],\"models\":[]}"u8.ToArray();
            await ctx.Response.OutputStream.WriteAsync(body);
            ctx.Response.Close();
        }
    }

    private async Task<string?> SentAsync(Func<Task> call, string header)
    {
        int before;
        lock (_requests) before = _requests.Count;
        try { await call(); }
        catch { /* the stub body is not a real model list; only the headers matter here */ }

        for (var i = 0; i < 100; i++)
        {
            lock (_requests)
            {
                if (_requests.Count > before)
                    return _requests[before][header];
            }
            await Task.Delay(20);
        }

        throw new InvalidOperationException("the client made no request");
    }

    public void Dispose()
    {
        try { _listener.Stop(); } catch (ObjectDisposedException) { }
        _listener.Close();
    }

    [Fact]
    public async Task OpenAI_SendsTheResolvedKey_OnEveryRequest()
    {
        var key = "key-1";
        var finder = new OpenAIModelFinder(new OpenAIConfig { BaseUrl = _origin, ApiKey = "static-key", ApiKeyResolver = () => key });

        (await SentAsync(() => finder.ListModelsAsync(), "Authorization")).Should().Be("Bearer key-1");
        key = "key-2";
        (await SentAsync(() => finder.ListModelsAsync(), "Authorization")).Should().Be("Bearer key-2", "a rotated key takes effect on the next call");
        key = "";
        (await SentAsync(() => finder.ListModelsAsync(), "Authorization")).Should().Be("Bearer static-key", "a blank answer falls back to ApiKey");
    }

    [Fact]
    public async Task OpenAI_WithoutAResolver_SendsTheStaticKey()
    {
        // Positive control: the wire capture sees the credential header at all.
        var finder = new OpenAIModelFinder(new OpenAIConfig { BaseUrl = _origin, ApiKey = "static-key" });

        (await SentAsync(() => finder.ListModelsAsync(), "Authorization")).Should().Be("Bearer static-key");
    }

    [Fact]
    public async Task Anthropic_SendsTheResolvedKey_OnEveryRequest()
    {
        var key = "key-1";
        var finder = new AnthropicModelFinder(new AnthropicConfig { BaseUrl = _origin, ApiKey = "static-key", ApiKeyResolver = () => key });

        (await SentAsync(() => finder.ListModelsAsync(), "x-api-key")).Should().Be("key-1");
        key = "key-2";
        (await SentAsync(() => finder.ListModelsAsync(), "x-api-key")).Should().Be("key-2");
    }

    [Fact]
    public async Task GoogleAI_SendsTheResolvedKey_OnEveryRequest()
    {
        var key = "key-1";
        var finder = new GoogleAIModelFinder(new GoogleAIConfig
        {
            ApiKey = "static-key",
            ApiKeyResolver = () => key,
            HttpOptions = new Google.GenAI.Types.HttpOptions { BaseUrl = _origin },
        });

        (await SentAsync(() => finder.ListModelsAsync(), "x-goog-api-key")).Should().Be("key-1");
        key = "key-2";
        (await SentAsync(() => finder.ListModelsAsync(), "x-goog-api-key")).Should().Be("key-2");
        (await SentAsync(() => finder.ListModelsAsync(), "query")).Should().NotContain("key=",
            "a key in the query string would carry the construction-time key past the resolver");
    }

    [Fact]
    public void AResolver_WithAConsumerSuppliedHttpClient_IsRefused_NotIgnored()
    {
        using var http = new HttpClient();

        var openAi = () => OpenAIClientFactory.BuildOptions(new OpenAIConfig { ApiKeyResolver = () => "k", HttpClient = http });
        var anthropic = () => AnthropicClientFactory.Create(new AnthropicConfig { ApiKeyResolver = () => "k", HttpClient = http });
        var google = () => GoogleAIClientFactory.ResolveHttpClientFactory(new GoogleAIConfig { ApiKeyResolver = () => "k", HttpClientFactory = () => http });

        openAi.Should().Throw<InvalidOperationException>().WithMessage("*OpenAIConfig.ApiKeyResolver*HttpClient*");
        anthropic.Should().Throw<InvalidOperationException>().WithMessage("*AnthropicConfig.ApiKeyResolver*HttpClient*");
        google.Should().Throw<InvalidOperationException>().WithMessage("*GoogleAIConfig.ApiKeyResolver*HttpClientFactory*");
    }

    [Fact]
    public void Validate_CountsAResolver_AsACredential()
    {
        new OpenAIConfig { ApiKeyResolver = () => "k" }.Validate().Should().BeTrue();
        new AnthropicConfig { ApiKeyResolver = () => "k" }.Validate().Should().BeTrue();
        new GoogleAIConfig { ApiKeyResolver = () => "k" }.Validate().Should().BeTrue();
        new OpenAIConfig().Validate().Should().BeFalse();
    }
}
