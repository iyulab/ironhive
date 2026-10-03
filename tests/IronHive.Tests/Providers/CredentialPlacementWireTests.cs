using System.Net;
using System.Net.Sockets;
using AwesomeAssertions;
using IronHive.Abstractions.Http;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible;
using IronHive.Providers.OpenAI.Compatible.Embedding;
using IronHive.Providers.OpenAI.Compatible.GpuStack;

namespace IronHive.Tests.Providers;

/// <summary>
/// <c>ApiKeyPlacement</c> observed on the wire, on both request paths of the OpenAI wire: the vendor SDK (model listing)
/// and this package's own HTTP client (embeddings, the same client Chat Completions and rerank use). A gateway that
/// wants <c>Basic</c>, a bare token, or the key in its own header gets exactly that — and never the SDK's
/// <c>Authorization: Bearer</c> beside it.
/// </summary>
public sealed class CredentialPlacementWireTests : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly List<Dictionary<string, string?>> _requests = [];
    private readonly string _origin;

    public CredentialPlacementWireTests()
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
                    ["api-key"] = ctx.Request.Headers["api-key"],
                    ["X-Gateway"] = ctx.Request.Headers["X-Gateway"],
                });
            }

            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = "application/json";
            var body = "{\"object\":\"list\",\"data\":[]}"u8.ToArray();
            await ctx.Response.OutputStream.WriteAsync(body);
            ctx.Response.Close();
        }
    }

    private async Task<Dictionary<string, string?>> SentAsync(Func<Task> call)
    {
        int before;
        lock (_requests) before = _requests.Count;
        try { await call(); }
        catch { /* the stub body is not a real response; only the headers matter here */ }

        for (var i = 0; i < 100; i++)
        {
            lock (_requests)
            {
                if (_requests.Count > before)
                    return _requests[before];
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

    private Task<Dictionary<string, string?>> ViaSdkAsync(OpenAICompatibleConfig config)
    {
        config.BaseUrl = _origin;
        var finder = new OpenAICompatibleModelFinder(config);
        return SentAsync(() => finder.ListModelsAsync());
    }

    private Task<Dictionary<string, string?>> ViaOwnClientAsync(OpenAIConfig config)
    {
        config.BaseUrl = _origin + "/v1";
        var generator = new OpenAICompatibleEmbeddingGenerator(config);
        return SentAsync(() => generator.EmbedAsync("m", "text"));
    }

    public static TheoryData<string> Placements => ["basic", "bare", "api-key"];

    private static (CredentialPlacement Placement, string Header, string Value) Expected(string name) => name switch
    {
        "basic" => (CredentialPlacement.Authorization("Basic"), "Authorization", "Basic dXNlcjpwYXNz"),
        "bare" => (CredentialPlacement.Authorization(null), "Authorization", "dXNlcjpwYXNz"),
        "api-key" => (CredentialPlacement.InHeader("api-key"), "api-key", "dXNlcjpwYXNz"),
        _ => throw new ArgumentOutOfRangeException(nameof(name)),
    };

    [Theory]
    [MemberData(nameof(Placements))]
    public async Task SdkPath_SendsTheKeyAsPlaced(string name)
    {
        var (placement, header, value) = Expected(name);

        var sent = await ViaSdkAsync(new OpenAICompatibleConfig { ApiKey = "dXNlcjpwYXNz", ApiKeyPlacement = placement });

        sent[header].Should().Be(value);
        if (header != "Authorization")
            sent["Authorization"].Should().BeNull("the SDK's own bearer must not carry the key beside the configured header");
    }

    [Theory]
    [MemberData(nameof(Placements))]
    public async Task OwnClientPath_SendsTheKeyAsPlaced(string name)
    {
        var (placement, header, value) = Expected(name);

        var sent = await ViaOwnClientAsync(new OpenAIConfig { ApiKey = "dXNlcjpwYXNz", ApiKeyPlacement = placement });

        sent[header].Should().Be(value);
        if (header != "Authorization")
            sent["Authorization"].Should().BeNull();
    }

    [Fact]
    public async Task DefaultPlacement_IsBearer_OnBothPaths()
    {
        // Positive control: the capture sees the credential header at all, and nothing changed for existing configs.
        (await ViaSdkAsync(new OpenAICompatibleConfig { ApiKey = "k" }))["Authorization"].Should().Be("Bearer k");
        (await ViaOwnClientAsync(new OpenAIConfig { ApiKey = "k" }))["Authorization"].Should().Be("Bearer k");
    }

    [Fact]
    public async Task AResolvedKey_IsPlacedToo_AndRotatesPerRequest()
    {
        var key = "key-1";
        var config = new OpenAIConfig
        {
            BaseUrl = _origin,
            ApiKey = "static",
            ApiKeyResolver = () => key,
            ApiKeyPlacement = CredentialPlacement.InHeader("api-key"),
        };
        var finder = new OpenAIModelFinder(config);

        (await SentAsync(() => finder.ListModelsAsync()))["api-key"].Should().Be("key-1");
        key = "key-2";
        var second = await SentAsync(() => finder.ListModelsAsync());
        second["api-key"].Should().Be("key-2");
        second["Authorization"].Should().BeNull();
    }

    [Fact]
    public async Task NoKey_SendsNoCredentialHeader_InsteadOfAPlaceholder()
    {
        var sent = await ViaSdkAsync(new OpenAICompatibleConfig { ApiKeyPlacement = CredentialPlacement.InHeader("api-key") });

        sent["api-key"].Should().BeNull();
        sent["Authorization"].Should().BeNull();
    }

    [Fact]
    public async Task AnInjectedHttpClient_StillGetsThePlacement()
    {
        using var http = new HttpClient();
        var finder = new OpenAIModelFinder(new OpenAIConfig
        {
            BaseUrl = _origin,
            ApiKey = "k",
            ApiKeyPlacement = CredentialPlacement.Authorization("Basic"),
            HttpClient = http,
        });

        (await SentAsync(() => finder.ListModelsAsync()))["Authorization"].Should().Be("Basic k");
    }

    [Fact]
    public async Task GatewayHeaders_AreSentBesideThePlacedKey()
    {
        var sent = await ViaSdkAsync(new OpenAICompatibleConfig
        {
            ApiKey = "k",
            ApiKeyPlacement = CredentialPlacement.InHeader("api-key"),
            Headers = new Dictionary<string, string> { ["X-Gateway"] = "g" },
        });

        sent["api-key"].Should().Be("k");
        sent["X-Gateway"].Should().Be("g");
    }

    [Fact]
    public void Headers_RefuseThePlacementHeader_AndAuthorization()
    {
        var placement = CredentialPlacement.InHeader("api-key");

        var sdkPlacementHeader = () => OpenAIClientFactory.BuildOptions(new OpenAIConfig
        {
            ApiKeyPlacement = placement,
            Headers = new Dictionary<string, string> { ["API-KEY"] = "x" },
        });
        var sdkAuthorization = () => OpenAIClientFactory.BuildOptions(new OpenAIConfig
        {
            ApiKeyPlacement = placement,
            Headers = new Dictionary<string, string> { ["Authorization"] = "x" },
        });
        var ownClient = () => new OpenAICompatibleEmbeddingGenerator(new OpenAIConfig
        {
            BaseUrl = _origin,
            ApiKeyPlacement = placement,
            Headers = new Dictionary<string, string> { ["api-key"] = "x" },
        });

        sdkPlacementHeader.Should().Throw<ArgumentException>().WithMessage("*'API-KEY'*ApiKey*");
        sdkAuthorization.Should().Throw<ArgumentException>().WithMessage("*'Authorization'*");
        ownClient.Should().Throw<ArgumentException>().WithMessage("*'api-key'*");
    }

    [Fact]
    public void GpuStackAndCompatible_CarryThePlacement_ToEveryConversion()
    {
        var placement = CredentialPlacement.InHeader("api-key");
        var gpuStack = new GpuStackConfig { ApiKeyPlacement = placement };

        gpuStack.ToOpenAI().ApiKeyPlacement.Should().Be(placement);
        gpuStack.ToRerankConfig().ApiKeyPlacement.Should().Be(placement);
        gpuStack.ToOpenAICompatible().ApiKeyPlacement.Should().Be(placement);
        gpuStack.ToOpenAICompatible().ToOpenAI().ApiKeyPlacement.Should().Be(placement);
    }

    [Theory]
    [InlineData("", "Bearer")]
    [InlineData("api key", null)]
    [InlineData("api-key:", null)]
    [InlineData("Authorization", "")]
    [InlineData("Authorization", "Bearer token")]
    public void InvalidPlacements_AreRefusedAtConstruction(string header, string? scheme)
    {
        var create = () => new CredentialPlacement(header, scheme);

        create.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Placements_CompareTheHeaderCaseInsensitively()
    {
        new CredentialPlacement("authorization", "Bearer").Should().Be(CredentialPlacement.Bearer);
        CredentialPlacement.InHeader("API-KEY").Should().Be(CredentialPlacement.InHeader("api-key"));
        CredentialPlacement.Authorization("Basic").Should().NotBe(CredentialPlacement.Bearer);
    }
}
