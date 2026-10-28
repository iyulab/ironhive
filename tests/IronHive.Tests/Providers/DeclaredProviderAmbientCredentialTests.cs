using System.Net;
using System.Net.Sockets;
using AwesomeAssertions;
using IronHive.Providers.Declarations;
using Microsoft.Extensions.AI;

namespace IronHive.Tests.Providers;

/// <summary>
/// A provider declared as data sends only the credential its declaration carries, to the address the declaration names.
/// The vendor SDKs fill an empty credential slot from the machine (<c>ANTHROPIC_API_KEY</c>, <c>GOOGLE_API_KEY</c>, ...): for
/// a declaration that is a person's own key for another product, sent to whatever third-party address they declared.
/// Observed on an in-process stub server with the ambient variables set to a sentinel.
/// </summary>
[Collection(nameof(ProcessEnvironment))]
public sealed class DeclaredProviderAmbientCredentialTests : IDisposable
{
    private const string Sentinel = "AMBIENT-SENTINEL-DO-NOT-SEND";
    private const string Key = "k-123";

    private readonly HttpListener _listener = new();
    private readonly List<Dictionary<string, string>> _requests = [];
    private readonly string _origin;

    public DeclaredProviderAmbientCredentialTests()
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

    public void Dispose() => _listener.Close();

    private async Task ServeAsync()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext ctx;
            try { ctx = await _listener.GetContextAsync(); }
            catch { return; }

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in ctx.Request.Headers.AllKeys)
                if (name is not null)
                    headers[name] = ctx.Request.Headers[name] ?? string.Empty;
            lock (_requests)
                _requests.Add(headers);

            ctx.Response.StatusCode = 400;
            ctx.Response.ContentType = "application/json";
            var body = "{\"error\":{\"message\":\"stub\",\"type\":\"invalid_request_error\"}}"u8.ToArray();
            await ctx.Response.OutputStream.WriteAsync(body);
            ctx.Response.Close();
        }
    }

    private async Task<Dictionary<string, string>> SentAsync(ProviderDeclaration declaration, string[] ambientVariables)
    {
        var before = ambientVariables.ToDictionary(name => name, Environment.GetEnvironmentVariable);
        try
        {
            foreach (var name in ambientVariables)
                Environment.SetEnvironmentVariable(name, Sentinel);

            int seen;
            lock (_requests) seen = _requests.Count;
            using var client = ProviderClientFactory.Create(declaration, "m");
            try { await client.GetResponseAsync("hi", cancellationToken: TestContext.Current.CancellationToken); }
            catch { /* the stub answers 400; only the request matters here */ }

            for (var i = 0; i < 100; i++)
            {
                lock (_requests)
                {
                    if (_requests.Count > seen)
                        return _requests[seen];
                }
                await Task.Delay(20, TestContext.Current.CancellationToken);
            }

            throw new InvalidOperationException("the client made no request");
        }
        finally
        {
            foreach (var (name, value) in before)
                Environment.SetEnvironmentVariable(name, value);
        }
    }

    private ProviderDeclaration Declare(WireFormat wire, string? key, string? keyHeader = null) => new()
    {
        Id = "stub",
        Wire = wire,
        BaseUrl = _origin,
        ApiKey = key,
        KeyHeader = keyHeader,
    };

    public static TheoryData<WireFormat, string[]> WiresAndTheirAmbientVariables => new()
    {
        { WireFormat.Anthropic, ["ANTHROPIC_API_KEY", "ANTHROPIC_AUTH_TOKEN"] },
        { WireFormat.GoogleAI, ["GOOGLE_API_KEY", "GEMINI_API_KEY"] },
        { WireFormat.OpenAIChatCompletions, ["OPENAI_API_KEY"] },
        { WireFormat.OpenAIResponses, ["OPENAI_API_KEY"] },
    };

    [Theory]
    [MemberData(nameof(WiresAndTheirAmbientVariables))]
    public async Task A_declaration_without_a_key_sends_no_ambient_credential(WireFormat wire, string[] ambientVariables)
    {
        var seen = await SentAsync(Declare(wire, key: null), ambientVariables);

        seen.Values.Should().NotContain(v => v.Contains(Sentinel), "the machine's own key is not the declaration's to send");
    }

    [Fact]
    public async Task Anthropic_with_the_key_in_a_gateway_header_sends_no_ambient_credential()
    {
        var seen = await SentAsync(Declare(WireFormat.Anthropic, Key, keyHeader: "X-Gateway-Key"), ["ANTHROPIC_API_KEY", "ANTHROPIC_AUTH_TOKEN"]);

        seen["X-Gateway-Key"].Should().Be(Key);
        seen.Values.Should().NotContain(v => v.Contains(Sentinel));
        seen.Should().NotContainKey("Authorization");
    }

    [Fact]
    public async Task A_declared_key_wins_over_the_ambient_one()
    {
        var seen = await SentAsync(Declare(WireFormat.Anthropic, Key), ["ANTHROPIC_API_KEY", "ANTHROPIC_AUTH_TOKEN"]);

        seen["Authorization"].Should().Be("Bearer " + Key);
        seen.Values.Should().NotContain(v => v.Contains(Sentinel));
    }
}
