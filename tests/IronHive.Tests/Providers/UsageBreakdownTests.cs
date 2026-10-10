using AwesomeAssertions;
using IronHive.Abstractions.Messages;
using IronHive.Extensions.AI;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using IronHive.Tests.Conventions.Providers;
using Microsoft.Extensions.AI;

namespace IronHive.Tests.Providers;

/// <summary>
/// A consumer that re-prices a call needs the parts the vendors price differently: cache writes (and, for Anthropic, the
/// TTL they were written with) and reasoning tokens. Each case serves a recorded buffered body through the real SDK and
/// generator and reads the usage IronHive reports.
/// </summary>
public class UsageBreakdownTests
{
    [Fact]
    public async Task Anthropic_reports_cache_writes_and_their_TTLs()
    {
        const string json = """
            {"id":"msg_1","type":"message","role":"assistant","model":"claude-sonnet-4-5","content":[{"type":"text","text":"Hi"}],"stop_reason":"end_turn","stop_sequence":null,
             "usage":{"input_tokens":10,"output_tokens":3,"cache_read_input_tokens":100,"cache_creation_input_tokens":300,
                      "cache_creation":{"ephemeral_5m_input_tokens":100,"ephemeral_1h_input_tokens":200}}}
            """;
        using var generator = new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "k", HttpClient = Client(json) });

        var usage = (await generator.GenerateMessageAsync(Request(), TestContext.Current.CancellationToken)).TokenUsage!;

        usage.InputTokens.Should().Be(410, "the whole input: uncached + read + written");
        usage.CachedInputTokens.Should().Be(100);
        usage.CacheWriteInputTokens.Should().Be(300);
        usage.CacheWritesByTtl.Should().Equal(
            new CacheWriteTokens(TimeSpan.FromMinutes(5), 100),
            new CacheWriteTokens(TimeSpan.FromHours(1), 200));
        usage.ReasoningTokens.Should().BeNull("Anthropic does not report thinking tokens apart from the output");
    }

    [Fact]
    public async Task OpenAI_Responses_reports_cache_writes_and_reasoning()
    {
        const string json = """
            {"id":"resp_1","object":"response","created_at":1757635200,"status":"completed","error":null,"incomplete_details":null,"instructions":null,"max_output_tokens":null,"model":"gpt-5.6",
             "output":[{"type":"message","id":"msg_1","status":"completed","role":"assistant","content":[{"type":"output_text","text":"Hi","annotations":[]}]}],
             "parallel_tool_calls":true,"previous_response_id":null,"reasoning":{"effort":null,"summary":null},"store":true,"temperature":1.0,"text":{"format":{"type":"text"}},"tool_choice":"auto","tools":[],"top_p":1.0,"truncation":"disabled",
             "usage":{"input_tokens":500,"input_tokens_details":{"cached_tokens":100,"cache_write_tokens":200},"output_tokens":50,"output_tokens_details":{"reasoning_tokens":30},"total_tokens":550},"user":null,"metadata":{}}
            """;
        using var generator = new OpenAIMessageGenerator(new OpenAIConfig { ApiKey = "k", HttpClient = Client(json) });

        var usage = (await generator.GenerateMessageAsync(Request(), TestContext.Current.CancellationToken)).TokenUsage!;

        usage.CachedInputTokens.Should().Be(100);
        usage.CacheWriteInputTokens.Should().Be(200);
        usage.ReasoningTokens.Should().Be(30);
    }

    [Fact]
    public async Task Chat_Completions_reports_cache_writes_and_reasoning()
    {
        const string json = """
            {"id":"chatcmpl-1","object":"chat.completion","created":1757635200,"model":"m","choices":[{"index":0,"message":{"role":"assistant","content":"Hi"},"finish_reason":"stop","logprobs":null}],
             "usage":{"prompt_tokens":500,"completion_tokens":50,"total_tokens":550,"prompt_tokens_details":{"cached_tokens":100,"cache_write_tokens":200},"completion_tokens_details":{"reasoning_tokens":30}}}
            """;
        using var generator = new ChatCompletionMessageGenerator(new OpenAIConfig
        {
            ApiKey = "k",
            BaseUrl = "https://compatible.invalid/v1",
            HttpClient = Client(json),
        });

        var usage = (await generator.GenerateMessageAsync(Request(), TestContext.Current.CancellationToken)).TokenUsage!;

        usage.CacheWriteInputTokens.Should().Be(200);
        usage.ReasoningTokens.Should().Be(30);
    }

    [Fact]
    public async Task Gemini_reports_thoughts_as_reasoning()
    {
        const string json = """
            {"candidates":[{"content":{"parts":[{"text":"Hi"}],"role":"model"},"finishReason":"STOP","index":0}],
             "usageMetadata":{"promptTokenCount":11,"candidatesTokenCount":7,"thoughtsTokenCount":20,"totalTokenCount":38},"modelVersion":"gemini-2.5-flash","responseId":"r"}
            """;
        var client = Client(json);
        using var generator = new GoogleAIMessageGenerator(new GoogleAIConfig { ApiKey = "k", HttpClientFactory = () => client });

        var usage = (await generator.GenerateMessageAsync(Request(), TestContext.Current.CancellationToken)).TokenUsage!;

        usage.OutputTokens.Should().Be(27, "thoughts are part of the output");
        usage.ReasoningTokens.Should().Be(20);
    }

    [Fact]
    public void Adding_two_calls_sums_the_breakdown_and_keeps_unreported_as_null()
    {
        var first = new MessageTokenUsage
        {
            InputTokens = 10,
            CacheWriteInputTokens = 5,
            CacheWritesByTtl = [new CacheWriteTokens(TimeSpan.FromMinutes(5), 5)],
        };
        var second = new MessageTokenUsage
        {
            InputTokens = 20,
            CacheWriteInputTokens = 7,
            CacheWritesByTtl = [new CacheWriteTokens(TimeSpan.FromHours(1), 3), new CacheWriteTokens(TimeSpan.FromMinutes(5), 4)],
        };

        var sum = MessageTokenUsage.Add(first, second)!;

        sum.CacheWriteInputTokens.Should().Be(12);
        sum.CacheWritesByTtl.Should().Equal(
            new CacheWriteTokens(TimeSpan.FromMinutes(5), 9),
            new CacheWriteTokens(TimeSpan.FromHours(1), 3));
        sum.ReasoningTokens.Should().BeNull("neither call reported it");
    }

    [Fact]
    public async Task The_chat_client_adapter_carries_reasoning_and_cache_writes()
    {
        const string json = """
            {"id":"chatcmpl-1","object":"chat.completion","created":1757635200,"model":"m","choices":[{"index":0,"message":{"role":"assistant","content":"Hi"},"finish_reason":"stop","logprobs":null}],
             "usage":{"prompt_tokens":500,"completion_tokens":50,"total_tokens":550,"prompt_tokens_details":{"cached_tokens":100,"cache_write_tokens":200},"completion_tokens_details":{"reasoning_tokens":30}}}
            """;
        using var generator = new ChatCompletionMessageGenerator(new OpenAIConfig
        {
            ApiKey = "k",
            BaseUrl = "https://compatible.invalid/v1",
            HttpClient = Client(json),
        });
        using var client = new ChatClientAdapter(generator, "m");

        var response = await client.GetResponseAsync([new global::Microsoft.Extensions.AI.ChatMessage(ChatRole.User, "Hi")], cancellationToken: TestContext.Current.CancellationToken);

        response.Usage!.ReasoningTokenCount.Should().Be(30);
        response.Usage.AdditionalCounts![ChatClientAdapter.CacheWriteInputTokenCountKey].Should().Be(200);
    }

    private static HttpClient Client(string json)
        => new(new StubHttpHandler(json, "")) { Timeout = Timeout.InfiniteTimeSpan };

    private static MessageGenerationRequest Request()
        => new() { Model = "m", MaxTokens = 64, Messages = [Message.User("Hi")] };
}
