using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using AwesomeAssertions;
using IronHive.Abstractions.Http;
using IronHive.Abstractions.Messages;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using IronHive.Providers.OpenAI.Compatible.GpuStack;

namespace IronHive.Tests.Providers;

/// <summary>
/// <c>StreamIdleTimeout</c>: the longest a streamed response may stay silent — from the request until the first
/// event, then between events. A whole-request deadline cannot separate a slow stream (minutes of prompt evaluation,
/// then a long answer) from a dead one; the gap between reads can.
/// </summary>
public sealed class StreamIdleTimeoutTests
{
    private static readonly TimeSpan Short = TimeSpan.FromMilliseconds(400);

    private const string RoleChunk = """data: {"id":"c","object":"chat.completion.chunk","model":"m","choices":[{"index":0,"delta":{"role":"assistant"}}]}""";
    private static string TextChunk(string text) =>
        $$$"""data: {"id":"c","object":"chat.completion.chunk","model":"m","choices":[{"index":0,"delta":{"content":"{{{text}}}"}}]}""";
    private const string StopChunk = """data: {"id":"c","object":"chat.completion.chunk","model":"m","choices":[{"index":0,"delta":{},"finish_reason":"stop"}]}""";

    // ---- Chat Completions (hand-written reader) ----

    [Fact]
    public async Task A_stream_that_goes_silent_after_its_first_chunk_ends_with_an_idle_timeout()
    {
        // The requester's repro: role chunk at once, then 3 s of silence, budget 400 ms.
        await using var server = SseServer.Start(Step.At(0, RoleChunk), Step.At(3000, TextChunk("late")), Step.At(0, StopChunk), Step.At(0, "data: [DONE]"));
        using var client = new ChatCompletionHttpClient(new OpenAIConfig { BaseUrl = server.BaseUrl, StreamIdleTimeout = Short, MaxRetries = 0 });

        var watch = Stopwatch.StartNew();
        var act = () => Drain(client, TestContext.Current.CancellationToken);

        var thrown = await act.Should().ThrowAsync<TimeoutException>();
        thrown.Which.Message.Should().Contain("stream idle timeout");
        watch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2.5));
    }

    [Fact]
    public async Task The_same_silence_within_the_budget_completes()
    {
        // Positive control for the test above: only the budget differs.
        await using var server = SseServer.Start(Step.At(0, RoleChunk), Step.At(1000, TextChunk("late")), Step.At(0, StopChunk), Step.At(0, "data: [DONE]"));
        using var client = new ChatCompletionHttpClient(new OpenAIConfig { BaseUrl = server.BaseUrl, StreamIdleTimeout = TimeSpan.FromSeconds(5), MaxRetries = 0 });

        var chunks = await Drain(client, TestContext.Current.CancellationToken);

        chunks.Should().Be(3);
    }

    [Fact]
    public async Task A_long_answer_that_keeps_streaming_is_not_cut_off()
    {
        // 12 chunks, 300 ms apart — 3.6 s in total, longer than the 3 s budget, every gap a tenth of it. The margin is
        // wide on purpose: a shared CI runner running the suite in parallel stretched a 250 ms gap past 600 ms, and later a
        // 400 ms gap past 1.5 s (a publish run on 2026-10-09). What this fact holds is «total longer than the budget, each
        // gap inside it», so the ratio is what must survive a loaded runner, not the absolute times.
        var steps = new List<Step> { Step.At(0, RoleChunk) };
        for (var i = 0; i < 12; i++)
            steps.Add(Step.At(300, TextChunk($"t{i}")));
        steps.Add(Step.At(0, "data: [DONE]"));
        await using var server = SseServer.Start([.. steps]);
        using var client = new ChatCompletionHttpClient(new OpenAIConfig { BaseUrl = server.BaseUrl, StreamIdleTimeout = TimeSpan.FromSeconds(3), MaxRetries = 0 });

        var chunks = await Drain(client, TestContext.Current.CancellationToken);

        chunks.Should().Be(13);
    }

    [Fact]
    public async Task Silence_before_the_first_chunk_counts()
    {
        await using var server = SseServer.Start(Step.At(2000, RoleChunk), Step.At(0, "data: [DONE]"));
        using var client = new ChatCompletionHttpClient(new OpenAIConfig { BaseUrl = server.BaseUrl, StreamIdleTimeout = Short, MaxRetries = 0 });

        var act = () => Drain(client, TestContext.Current.CancellationToken);

        (await act.Should().ThrowAsync<TimeoutException>()).Which.Message.Should().Contain("stream idle timeout");
    }

    [Fact]
    public async Task Silence_before_the_response_starts_counts()
    {
        // A server that accepts the request and never answers is the same dead stream.
        await using var server = SseServer.Start(headerDelayMs: 2000, Step.At(0, RoleChunk), Step.At(0, "data: [DONE]"));
        using var client = new ChatCompletionHttpClient(new OpenAIConfig { BaseUrl = server.BaseUrl, StreamIdleTimeout = Short, MaxRetries = 0 });

        var act = () => Drain(client, TestContext.Current.CancellationToken);

        (await act.Should().ThrowAsync<TimeoutException>()).Which.Message.Should().Contain("stream idle timeout");
    }

    [Fact]
    public async Task Keep_alive_lines_count_as_the_stream_being_alive()
    {
        // Keep-alives 300 ms apart for 1.8 s against a 1.2 s budget: only the keep-alives keep it open.
        var steps = new List<Step> { Step.At(0, RoleChunk) };
        for (var i = 0; i < 6; i++)
            steps.Add(Step.At(300, ": keep-alive"));
        steps.Add(Step.At(0, TextChunk("after")));
        steps.Add(Step.At(0, "data: [DONE]"));
        await using var server = SseServer.Start([.. steps]);
        using var client = new ChatCompletionHttpClient(new OpenAIConfig { BaseUrl = server.BaseUrl, StreamIdleTimeout = TimeSpan.FromMilliseconds(1200), MaxRetries = 0 });

        var chunks = await Drain(client, TestContext.Current.CancellationToken);

        chunks.Should().Be(2);
    }

    [Fact]
    public async Task Timeout_bounds_only_the_start_of_a_stream()
    {
        // Documented meaning (OpenAIConfig.Timeout): a stream that has started is not cut off by it.
        await using var server = SseServer.Start(Step.At(0, RoleChunk), Step.At(1000, TextChunk("late")), Step.At(0, "data: [DONE]"));
        using var client = new ChatCompletionHttpClient(new OpenAIConfig { BaseUrl = server.BaseUrl, Timeout = Short, MaxRetries = 0 });

        var chunks = await Drain(client, TestContext.Current.CancellationToken);

        chunks.Should().Be(2);
    }

    [Fact]
    public async Task A_request_timeout_is_worded_as_one_not_as_idle()
    {
        await using var server = SseServer.Start(headerDelayMs: 2000, Step.At(0, RoleChunk));
        using var client = new ChatCompletionHttpClient(new OpenAIConfig
        {
            BaseUrl = server.BaseUrl,
            Timeout = Short,
            StreamIdleTimeout = TimeSpan.FromSeconds(30),
            MaxRetries = 0,
        });

        var act = () => Drain(client, TestContext.Current.CancellationToken);

        (await act.Should().ThrowAsync<TimeoutException>()).Which.Message.Should().NotContain("idle");
    }

    [Fact]
    public async Task The_callers_cancellation_stays_a_cancellation()
    {
        await using var server = SseServer.Start(Step.At(0, RoleChunk), Step.At(3000, TextChunk("late")));
        using var client = new ChatCompletionHttpClient(new OpenAIConfig { BaseUrl = server.BaseUrl, StreamIdleTimeout = TimeSpan.FromSeconds(30), MaxRetries = 0 });
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

        var act = () => Drain(client, cts.Token);

        var thrown = await act.Should().ThrowAsync<OperationCanceledException>();
        thrown.Which.Should().NotBeOfType<TimeoutException>();
    }

    [Fact]
    public async Task The_generator_surfaces_the_idle_timeout_too()
    {
        await using var server = SseServer.Start(Step.At(0, RoleChunk), Step.At(3000, TextChunk("late")));
        var generator = new ChatCompletionMessageGenerator(new OpenAIConfig { BaseUrl = server.BaseUrl, StreamIdleTimeout = Short, MaxRetries = 0 });

        var act = async () =>
        {
            await foreach (var _ in generator.GenerateStreamingMessageAsync(Request(), TestContext.Current.CancellationToken)) { }
        };

        (await act.Should().ThrowAsync<TimeoutException>()).Which.Message.Should().Contain("stream idle timeout");
    }

    // ---- Configs carry the budget to the client that reads it ----

    [Fact]
    public void Compatible_and_GPUStack_configs_carry_both_timeouts()
    {
        var timeout = TimeSpan.FromSeconds(7);
        var idle = TimeSpan.FromSeconds(9);

        var fromCompatible = new OpenAICompatibleConfig { BaseUrl = "http://h/v1", Timeout = timeout, StreamIdleTimeout = idle }.ToOpenAI();
        var gpuStack = new GpuStackConfig { BaseUrl = "http://h", Timeout = timeout, StreamIdleTimeout = idle };
        var viaCompatible = gpuStack.ToOpenAICompatible().ToOpenAI();
        var direct = gpuStack.ToOpenAI();

        foreach (var config in new[] { fromCompatible, viaCompatible, direct })
        {
            config.Timeout.Should().Be(timeout);
            config.StreamIdleTimeout.Should().Be(idle);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void A_budget_that_is_neither_positive_nor_infinite_is_refused(int milliseconds)
    {
        var act = () => new ChatCompletionMessageGenerator(new OpenAIConfig { StreamIdleTimeout = TimeSpan.FromMilliseconds(milliseconds) });

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("OpenAIConfig.StreamIdleTimeout");
    }

    // ---- SDK-backed wires: the same budget, applied to the SDK's stream ----

    [Fact]
    public async Task Responses_stream_silent_before_its_first_event_ends_with_an_idle_timeout()
    {
        await using var server = SseServer.Start(headerDelayMs: 0, Step.At(3000, "data: {}"));
        var generator = new OpenAIMessageGenerator(new OpenAIConfig { BaseUrl = server.BaseUrl, ApiKey = "k", StreamIdleTimeout = Short, MaxRetries = 0 });

        await AssertIdleTimeout(generator);
    }

    [Fact]
    public async Task Anthropic_stream_silent_before_its_first_event_ends_with_an_idle_timeout()
    {
        await using var server = SseServer.Start(headerDelayMs: 0, Step.At(3000, "event: ping"));
        var generator = new AnthropicMessageGenerator(new AnthropicConfig { BaseUrl = server.Origin, ApiKey = "k", StreamIdleTimeout = Short, MaxRetries = 0 });

        await AssertIdleTimeout(generator);
    }

    [Fact]
    public async Task Gemini_stream_silent_before_its_first_event_ends_with_an_idle_timeout()
    {
        await using var server = SseServer.Start(headerDelayMs: 0, Step.At(3000, "data: {}"));
        var generator = new GoogleAIMessageGenerator(new GoogleAIConfig
        {
            ApiKey = "k",
            StreamIdleTimeout = Short,
            HttpOptions = new Google.GenAI.Types.HttpOptions { BaseUrl = server.Origin + "/" },
        });

        await AssertIdleTimeout(generator);
    }

    [Fact]
    public void Responses_client_has_no_hidden_network_timeout()
    {
        // Left unset, System.ClientModel applies 100 s to every network operation — each read of a stream included.
        OpenAIClientFactory.BuildOptions(new OpenAIConfig()).NetworkTimeout.Should().BeGreaterThan(TimeSpan.FromDays(24));
        OpenAIClientFactory.BuildOptions(new OpenAIConfig { Timeout = TimeSpan.FromSeconds(5) }).NetworkTimeout.Should().Be(TimeSpan.FromSeconds(5));
    }

    // ---- The shared helper ----

    [Fact]
    public async Task Time_the_consumer_spends_on_an_event_is_not_counted()
    {
        var events = ProviderStreams.WithIdleTimeout(Immediate, TimeSpan.FromMilliseconds(200), TestContext.Current.CancellationToken);

        var seen = 0;
        await foreach (var _ in events)
        {
            seen++;
            await Task.Delay(500, TestContext.Current.CancellationToken);
        }

        seen.Should().Be(3);
    }

    [Fact]
    public void An_infinite_budget_passes_the_stream_through_unchanged()
    {
        var source = Immediate(CancellationToken.None);

        ProviderStreams.WithIdleTimeout(_ => source, Timeout.InfiniteTimeSpan, TestContext.Current.CancellationToken).Should().BeSameAs(source);
    }

    // ---- helpers ----

    private static async IAsyncEnumerable<int> Immediate([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (var i = 0; i < 3; i++)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            yield return i;
        }
    }

    private static MessageGenerationRequest Request() => new()
    {
        Model = "m",
        Messages = [Message.User("hi")],
    };

    private static async Task AssertIdleTimeout(IMessageGenerator generator)
    {
        var watch = Stopwatch.StartNew();
        var act = async () =>
        {
            await foreach (var _ in generator.GenerateStreamingMessageAsync(Request(), TestContext.Current.CancellationToken)) { }
        };

        (await act.Should().ThrowAsync<TimeoutException>()).Which.Message.Should().Contain("stream idle timeout");
        watch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2.5));
    }

    private static async Task<int> Drain(ChatCompletionHttpClient client, CancellationToken cancellationToken)
    {
        var request = new ChatCompletionRequest { Model = "m", Messages = [] };
        var count = 0;
        await foreach (var _ in client.PostStreamingAsync(request, cancellationToken: cancellationToken))
            count++;
        return count;
    }

    private readonly record struct Step(int DelayMs, string Text)
    {
        public static Step At(int delayMs, string line) => new(delayMs, line);
    }

    /// <summary>
    /// A one-request HTTP/1.1 server on 127.0.0.1 that answers with <c>text/event-stream</c> and writes each line after
    /// its delay — raw sockets, so the timing of the headers and of every line is exact.
    /// </summary>
    private sealed class SseServer : IAsyncDisposable
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _stop = new();
        private readonly Task _serve;

        private SseServer(int headerDelayMs, Step[] steps)
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            var port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            Origin = $"http://127.0.0.1:{port}";
            _serve = Task.Run(() => ServeAsync(headerDelayMs, steps));
        }

        public string Origin { get; }

        public string BaseUrl => Origin + "/v1";

        public static SseServer Start(params Step[] steps) => new(0, steps);

        public static SseServer Start(int headerDelayMs, params Step[] steps) => new(headerDelayMs, steps);

        private async Task ServeAsync(int headerDelayMs, Step[] steps)
        {
            try
            {
                while (!_stop.IsCancellationRequested)
                {
                    using var socket = await _listener.AcceptTcpClientAsync(_stop.Token);
                    await using var stream = socket.GetStream();
                    await ReadRequestAsync(stream, _stop.Token);
                    await Task.Delay(headerDelayMs, _stop.Token);
                    await WriteAsync(stream, "HTTP/1.1 200 OK\r\nContent-Type: text/event-stream\r\nCache-Control: no-cache\r\nConnection: close\r\n\r\n");
                    foreach (var step in steps)
                    {
                        await Task.Delay(step.DelayMs, _stop.Token);
                        await WriteAsync(stream, step.Text + "\n\n");
                    }
                }
            }
            catch (Exception) when (_stop.IsCancellationRequested)
            {
            }
            catch (IOException)
            {
                // The client tore the connection down — which is what an idle timeout does.
            }
            catch (SocketException)
            {
            }
        }

        private static async Task ReadRequestAsync(NetworkStream stream, CancellationToken cancellationToken)
        {
            var buffer = new byte[8192];
            var received = new StringBuilder();
            int headerEnd;
            while ((headerEnd = received.ToString().IndexOf("\r\n\r\n", StringComparison.Ordinal)) < 0)
            {
                var read = await stream.ReadAsync(buffer, cancellationToken);
                if (read == 0)
                    return;
                received.Append(Encoding.ASCII.GetString(buffer, 0, read));
            }

            var headers = received.ToString(0, headerEnd);
            var length = 0;
            foreach (var line in headers.Split("\r\n"))
            {
                if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                    length = int.Parse(line["Content-Length:".Length..].Trim(), System.Globalization.CultureInfo.InvariantCulture);
            }

            var bodyRead = Encoding.ASCII.GetByteCount(received.ToString(headerEnd + 4, received.Length - headerEnd - 4));
            while (bodyRead < length)
            {
                var read = await stream.ReadAsync(buffer, cancellationToken);
                if (read == 0)
                    return;
                bodyRead += read;
            }
        }

        private async Task WriteAsync(NetworkStream stream, string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            await stream.WriteAsync(bytes, _stop.Token);
            await stream.FlushAsync(_stop.Token);
        }

        public async ValueTask DisposeAsync()
        {
            await _stop.CancelAsync();
            _listener.Stop();
            try
            {
                await _serve;
            }
            catch (Exception)
            {
            }

            _stop.Dispose();
        }
    }
}
