using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using AwesomeAssertions;
using IronHive.Providers.Declarations;
using Microsoft.Extensions.AI;

namespace IronHive.Tests.Providers;

/// <summary>
/// A provider declared as data reaches the wire the way its adapter requires: the path under the declared endpoint, the
/// key where the declaration puts it (and nowhere else), the declared headers, the per-model overrides. Observed on an
/// in-process stub server per wire - the request is what is asserted; the stub answers 400 so no adapter retries.
/// </summary>
public sealed class ProviderDeclarationTests : IDisposable
{
    private const string Key = "k-123";

    private readonly HttpListener _listener = new();
    private readonly List<Seen> _requests = [];
    private readonly string _origin;

    private sealed record Seen(string Method, string Path, Dictionary<string, string> Headers);

    public ProviderDeclarationTests()
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
                _requests.Add(new Seen(ctx.Request.HttpMethod, ctx.Request.Url!.AbsolutePath, headers));

            ctx.Response.StatusCode = 400;
            ctx.Response.ContentType = "application/json";
            var body = "{\"error\":{\"message\":\"stub\",\"type\":\"invalid_request_error\"}}"u8.ToArray();
            await ctx.Response.OutputStream.WriteAsync(body);
            ctx.Response.Close();
        }
    }

    private async Task<Seen> SentAsync(ProviderDeclaration declaration, string modelId)
    {
        int before;
        lock (_requests) before = _requests.Count;
        using var client = ProviderClientFactory.Create(declaration, modelId);
        try { await client.GetResponseAsync("hi", cancellationToken: TestContext.Current.CancellationToken); }
        catch { /* the stub answers 400; only the request matters here */ }

        for (var i = 0; i < 100; i++)
        {
            lock (_requests)
            {
                if (_requests.Count > before)
                    return _requests[before];
            }
            await Task.Delay(20, TestContext.Current.CancellationToken);
        }

        throw new InvalidOperationException("the client made no request");
    }

    private ProviderDeclaration Declare(WireFormat wire, string path = "", string? keyHeader = null, string? keyScheme = null,
        IDictionary<string, string>? headers = null, IDictionary<string, ModelDeclaration>? models = null, string? key = Key) =>
        new()
        {
            Id = "stub",
            Wire = wire,
            BaseUrl = _origin + path,
            ApiKey = key,
            KeyHeader = keyHeader,
            KeyScheme = keyScheme,
            Headers = headers,
            Models = models,
        };

    [Theory]
    [InlineData(WireFormat.OpenAIChatCompletions, "/gateway/v1", "/gateway/v1/chat/completions")]
    [InlineData(WireFormat.OpenAIChatCompletions, "/gateway/v1/", "/gateway/v1/chat/completions")]
    [InlineData(WireFormat.OpenAIResponses, "/gateway/v1", "/gateway/v1/responses")]
    [InlineData(WireFormat.Anthropic, "/proxy", "/proxy/v1/messages")]
    public async Task The_request_goes_to_the_path_the_wire_adds_under_the_declared_endpoint(WireFormat wire, string basePath, string expected)
    {
        var seen = await SentAsync(Declare(wire, basePath), "m");

        seen.Method.Should().Be("POST");
        seen.Path.Should().Be(expected);
    }

    [Fact]
    public async Task Gemini_goes_under_the_declared_endpoint_to_the_model_method()
    {
        var seen = await SentAsync(Declare(WireFormat.GoogleAI, "/proxy"), "gemini-x");

        seen.Path.Should().StartWith("/proxy/").And.Contain("gemini-x").And.EndWith(":generateContent");
    }

    [Theory]
    [InlineData(WireFormat.OpenAIChatCompletions, "Authorization", "Bearer " + Key)]
    [InlineData(WireFormat.OpenAIResponses, "Authorization", "Bearer " + Key)]
    [InlineData(WireFormat.Anthropic, "Authorization", "Bearer " + Key)]
    [InlineData(WireFormat.GoogleAI, "x-goog-api-key", Key)]
    public async Task Without_a_placement_the_key_goes_where_the_wire_puts_it(WireFormat wire, string header, string value)
    {
        var seen = await SentAsync(Declare(wire), "m");

        seen.Headers.Should().ContainKey(header).WhoseValue.Should().Be(value);
    }

    [Theory]
    [InlineData(WireFormat.OpenAIChatCompletions)]
    [InlineData(WireFormat.OpenAIResponses)]
    public async Task An_openai_wire_sends_the_key_in_the_declared_header_alone(WireFormat wire)
    {
        var seen = await SentAsync(Declare(wire, keyHeader: "api-key"), "m");

        seen.Headers.Should().ContainKey("api-key").WhoseValue.Should().Be(Key);
        seen.Headers.Should().NotContainKey("Authorization");
    }

    [Theory]
    [InlineData(WireFormat.OpenAIChatCompletions)]
    [InlineData(WireFormat.OpenAIResponses)]
    public async Task An_openai_wire_sends_the_key_with_the_declared_scheme(WireFormat wire)
    {
        var seen = await SentAsync(Declare(wire, keyHeader: "Authorization", keyScheme: "Basic"), "m");

        seen.Headers["Authorization"].Should().Be("Basic " + Key);
    }

    [Fact]
    public async Task An_empty_scheme_sends_the_key_alone()
    {
        var seen = await SentAsync(Declare(WireFormat.OpenAIChatCompletions, keyHeader: "Authorization", keyScheme: ""), "m");

        seen.Headers["Authorization"].Should().Be(Key);
    }

    [Fact]
    public async Task Anthropic_with_a_declared_bearer_key_sends_authorization_alone()
    {
        var seen = await SentAsync(Declare(WireFormat.Anthropic, keyHeader: "Authorization"), "m");

        seen.Headers["Authorization"].Should().Be("Bearer " + Key);
        seen.Headers.Should().NotContainKey("x-api-key");
    }

    [Fact]
    public async Task Anthropic_with_a_gateway_header_sends_the_key_there_and_not_as_the_vendor_key()
    {
        var seen = await SentAsync(Declare(WireFormat.Anthropic, keyHeader: "X-Gateway-Key"), "m");

        seen.Headers["X-Gateway-Key"].Should().Be(Key);
        seen.Headers.Should().NotContainKey("x-api-key");
        seen.Headers.Should().NotContainKey("Authorization");
    }

    [Fact]
    public async Task Gemini_with_a_gateway_header_sends_the_key_there_as_well_as_in_the_vendor_header()
    {
        var seen = await SentAsync(Declare(WireFormat.GoogleAI, keyHeader: "X-Gateway-Key"), "gemini-x");

        seen.Headers["X-Gateway-Key"].Should().Be(Key);
        // The vendor SDK will not build a client without a key and always writes it here; the declaration cannot take it away.
        seen.Headers["x-goog-api-key"].Should().Be(Key);
    }

    [Theory]
    [InlineData(WireFormat.OpenAIChatCompletions)]
    [InlineData(WireFormat.OpenAIResponses)]
    [InlineData(WireFormat.Anthropic)]
    [InlineData(WireFormat.GoogleAI)]
    public async Task Declared_headers_are_sent_on_every_wire(WireFormat wire)
    {
        var seen = await SentAsync(Declare(wire, headers: new Dictionary<string, string> { ["X-Route"] = "blue" }), "m");

        seen.Headers["X-Route"].Should().Be("blue");
    }

    [Fact]
    public async Task A_server_that_takes_no_key_is_sent_none()
    {
        var seen = await SentAsync(Declare(WireFormat.OpenAIChatCompletions, key: null), "m");

        seen.Headers.Should().NotContainKey("Authorization");
    }

    [Fact]
    public async Task A_model_can_be_reached_over_another_wire_and_endpoint()
    {
        var declaration = Declare(WireFormat.OpenAIChatCompletions, "/openai/v1", models: new Dictionary<string, ModelDeclaration>
        {
            ["claude-x"] = new() { Wire = WireFormat.Anthropic, BaseUrl = _origin + "/anthropic" },
        });

        var plain = await SentAsync(declaration, "llama");
        var overridden = await SentAsync(declaration, "claude-x");

        plain.Path.Should().Be("/openai/v1/chat/completions");
        overridden.Path.Should().Be("/anthropic/v1/messages");
        overridden.Headers["Authorization"].Should().Be("Bearer " + Key);
    }

    [Fact]
    public void The_client_reports_the_declaration_id_as_its_provider()
    {
        using var client = ProviderClientFactory.Create(Declare(WireFormat.OpenAIChatCompletions), "m");

        client.GetService<ChatClientMetadata>()!.ProviderName.Should().Be("stub");
    }

    [Fact]
    public void A_declaration_round_trips_through_json_by_wire_name()
    {
        var declaration = Declare(WireFormat.Anthropic, keyHeader: "Authorization", headers: new Dictionary<string, string> { ["X-Route"] = "blue" },
            models: new Dictionary<string, ModelDeclaration> { ["m"] = new() { Wire = WireFormat.OpenAIResponses } });

        var json = JsonSerializer.Serialize(declaration);
        var back = JsonSerializer.Deserialize<ProviderDeclaration>(json)!;

        json.Should().Contain("\"Wire\":\"Anthropic\"").And.Contain("\"Wire\":\"OpenAIResponses\"");
        JsonSerializer.Serialize(back).Should().Be(json);
        back.Wire.Should().Be(WireFormat.Anthropic);
        back.Models!["m"].Wire.Should().Be(WireFormat.OpenAIResponses);
    }

    [Fact]
    public void A_declaration_written_by_hand_reads_with_the_wire_by_name()
    {
        const string json = """{"Id":"local","Wire":"OpenAIChatCompletions","BaseUrl":"http://localhost:11434/v1"}""";

        var declaration = JsonSerializer.Deserialize<ProviderDeclaration>(json)!;

        declaration.Wire.Should().Be(WireFormat.OpenAIChatCompletions);
        declaration.ApiKey.Should().BeNull();
    }

    [Theory]
    [InlineData(WireFormat.OpenAIChatCompletions)]
    [InlineData(WireFormat.OpenAIResponses)]
    [InlineData(WireFormat.Anthropic)]
    [InlineData(WireFormat.GoogleAI)]
    public void A_blank_or_relative_endpoint_is_refused_naming_the_provider(WireFormat wire)
    {
        var blank = () => ProviderClientFactory.Create(Declare(wire) with { BaseUrl = " " }, "m");
        var relative = () => ProviderClientFactory.Create(Declare(wire) with { BaseUrl = "gateway/v1" }, "m");
        var scheme = () => ProviderClientFactory.Create(Declare(wire) with { BaseUrl = "ftp://gateway/v1" }, "m");

        blank.Should().Throw<ArgumentException>().WithMessage("*'stub'*BaseUrl is blank*");
        relative.Should().Throw<ArgumentException>().WithMessage("*'stub'*not an absolute http or https URL*");
        scheme.Should().Throw<ArgumentException>().WithMessage("*'stub'*not an absolute http or https URL*");
    }

    [Fact]
    public void An_unknown_wire_is_refused()
    {
        var act = () => ProviderClientFactory.Create(Declare((WireFormat)99), "m");

        act.Should().Throw<ArgumentException>().WithMessage("*'stub'*99*");
    }

    [Fact]
    public void A_key_placement_the_wire_cannot_send_is_refused_not_dropped()
    {
        var anthropic = () => ProviderClientFactory.Create(Declare(WireFormat.Anthropic, keyHeader: "Authorization", keyScheme: "Basic"), "m");
        var anthropicApiKey = () => ProviderClientFactory.Create(Declare(WireFormat.Anthropic, keyHeader: "x-api-key"), "m");
        var gemini = () => ProviderClientFactory.Create(Declare(WireFormat.GoogleAI, keyHeader: "Authorization"), "m");
        var collision = () => ProviderClientFactory.Create(
            Declare(WireFormat.Anthropic, keyHeader: "X-Gateway-Key", headers: new Dictionary<string, string> { ["x-gateway-key"] = "other" }), "m");

        anthropic.Should().Throw<ArgumentException>().WithMessage("*'stub'*Authorization*");
        anthropicApiKey.Should().Throw<ArgumentException>().WithMessage("*'stub'*x-api-key*");
        gemini.Should().Throw<ArgumentException>().WithMessage("*'stub'*Authorization*");
        collision.Should().Throw<ArgumentException>().WithMessage("*'stub'*X-Gateway-Key*");
    }

    [Fact]
    public void A_blank_id_or_model_is_refused()
    {
        var noId = () => ProviderClientFactory.Create(Declare(WireFormat.OpenAIChatCompletions) with { Id = "" }, "m");
        var noModel = () => ProviderClientFactory.Create(Declare(WireFormat.OpenAIChatCompletions), " ");

        noId.Should().Throw<ArgumentException>().WithMessage("*Id is blank*");
        noModel.Should().Throw<ArgumentException>().WithMessage("*model id is blank*");
    }
}
