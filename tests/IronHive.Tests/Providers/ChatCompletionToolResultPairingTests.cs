using System.Text.Json;
using AwesomeAssertions;
using IronHive.Extensions.AI;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using IronHive.Tests.Conventions.Providers;
using IronHive.Tests.Microsoft;
using Microsoft.Extensions.AI;

namespace IronHive.Tests.Providers;

/// <summary>
/// The request body a Chat Completions server receives when an <see cref="IChatClient"/> history reuses a tool
/// call id across turns: every <c>role:"tool"</c> message must carry the result of its own call, in order.
/// </summary>
public class ChatCompletionToolResultPairingTests
{
    private const string ResponseJson =
        """{"id":"chatcmpl-1","object":"chat.completion","created":1757635200,"model":"test-model","choices":[{"index":0,"message":{"role":"assistant","content":"Noodles."},"finish_reason":"stop","logprobs":null}],"usage":{"prompt_tokens":1,"completion_tokens":1,"total_tokens":2}}""";

    [Fact]
    public async Task CallIdReusedAcrossTurns_EachToolMessageCarriesItsOwnResult()
    {
        var handler = new StubHttpHandler(ResponseJson, string.Empty);
        using var generator = new ChatCompletionMessageGenerator(new OpenAIConfig
        {
            ApiKey = "test-key",
            BaseUrl = "https://compatible.invalid/v1",
            HttpClient = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan },
        });
        using var client = generator.AsChatClient("test-model", "openai-compatible");

        await client.GetResponseAsync(ChatClientAdapterTests.ReusedCallIdHistory(),
            cancellationToken: TestContext.Current.CancellationToken);

        using var body = JsonDocument.Parse(handler.Requests.Should().ContainSingle().Which.Body);
        var messages = body.RootElement.GetProperty("messages").EnumerateArray().ToList();

        messages.Select(m => m.GetProperty("role").GetString())
            .Should().Equal(["user", "assistant", "tool", "assistant", "user", "assistant", "tool"]);
        messages.Where(m => m.GetProperty("role").GetString() == "tool")
            .Select(m => (m.GetProperty("tool_call_id").GetString(), m.GetProperty("content").GetString()))
            .Should().Equal([("c1", "Lunch: stew"), ("c1", "Tomorrow: noodles")]);
    }
}
