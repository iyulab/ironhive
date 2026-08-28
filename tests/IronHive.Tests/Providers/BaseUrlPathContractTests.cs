using System.Net;
using System.Net.Sockets;
using AwesomeAssertions;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible;

namespace IronHive.Tests.Providers;

/// <summary>
/// Two configurations in the same provider family expose a property called <c>BaseUrl</c> with opposite
/// contracts: <see cref="OpenAIConfig"/> takes the complete endpoint including the version segment,
/// while <see cref="OpenAICompatibleConfig"/> takes the server address and appends <c>Path</c>. Moving
/// the same value between them produces a 404 on one side, and neither the type system nor an error
/// message says so — the difference is only visible on the wire.
/// <para>
/// These tests pin it there. The adapter deliberately does not synthesise a version segment for the
/// plain configuration: the rule differs per compatible service (GPUStack serves <c>/v1-openai</c>), so
/// appending one would corrupt the paths the sibling configurations already build correctly.
/// </para>
/// </summary>
public sealed class BaseUrlPathContractTests : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly List<string> _requestedPaths = [];
    private readonly string _origin;

    public BaseUrlPathContractTests()
    {
        var port = FreeLoopbackPort();
        _origin = $"http://localhost:{port}";
        _listener.Prefixes.Add(_origin + "/");
        _listener.Start();
        _ = Task.Run(ServeAsync);
    }

    private static int FreeLoopbackPort()
    {
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        return port;
    }

    private async Task ServeAsync()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext ctx;
            try { ctx = await _listener.GetContextAsync(); }
            catch { return; }

            lock (_requestedPaths)
                _requestedPaths.Add(ctx.Request.Url!.AbsolutePath);

            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = "application/json";
            var body = "{\"object\":\"list\",\"data\":[]}"u8.ToArray();
            await ctx.Response.OutputStream.WriteAsync(body);
            ctx.Response.Close();
        }
    }

    private async Task<string> RequestedPathAsync(OpenAIConfig config)
    {
        var finder = new OpenAIModelFinder(config);
        try { await finder.ListModelsAsync(); }
        catch { /* the stub body is not a real model list; only the path matters here */ }

        for (var i = 0; i < 50; i++)
        {
            lock (_requestedPaths)
                if (_requestedPaths.Count > 0) return _requestedPaths[^1];
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
    public async Task PlainConfig_TakesTheBaseUrlVerbatim_AndAddsNoVersionSegment()
    {
        var path = await RequestedPathAsync(new OpenAIConfig { BaseUrl = _origin, ApiKey = "sk-test" });

        path.Should().Be("/models",
            "OpenAIConfig.BaseUrl is the complete endpoint — a caller who omits /v1 gets a 404 from the server");
    }

    [Fact]
    public async Task PlainConfig_WithTheVersionSegment_ReachesTheVersionedPath()
    {
        var path = await RequestedPathAsync(new OpenAIConfig { BaseUrl = _origin + "/v1", ApiKey = "sk-test" });

        path.Should().Be("/v1/models");
    }

    [Fact]
    public async Task CompatibleConfig_AppendsItsOwnPath_FromABareServerAddress()
    {
        var path = await RequestedPathAsync(new OpenAICompatibleConfig { BaseUrl = _origin }.ToOpenAI());

        path.Should().Be("/v1/models",
            "OpenAICompatibleConfig.BaseUrl is the server address and Path is appended — the opposite contract");
    }

    [Fact]
    public async Task CompatibleConfig_WithACustomPath_UsesIt_AndTheAdapterDoesNotAddAVersionOnTop()
    {
        var config = new OpenAICompatibleConfig { BaseUrl = _origin, Path = "/v1-openai" };

        var path = await RequestedPathAsync(config.ToOpenAI());

        path.Should().Be("/v1-openai/models",
            "a service-specific path must survive intact — this is why the plain adapter synthesises nothing");
    }
}
