using System.Net;
using System.Text;

namespace IronHive.Tests.Conventions.Providers;

// The HTTP seam for the per-provider equivalence tests (docs/CONVENTIONS.md section 5).
//
// Every IMessageGenerator sits on a vendor client that ultimately sends an HttpRequestMessage, and
// every provider config exposes the client to inject (OpenAIConfig.HttpClient,
// AnthropicConfig.HttpClient, GoogleAIConfig.HttpClientFactory; the chat-completion client shares
// OpenAIConfig). Handing that client this handler puts a recorded vendor response - one JSON body
// for the buffered call, one SSE body for the streaming call - under the real SDK parsing and the
// real generator mapping, which is exactly the pair of code paths the convention says must agree.
//
// Which body to serve is decided the way the vendors themselves decide it: OpenAI, Anthropic and
// the chat-completion protocol send `"stream": true` in the request body; Google switches the URL
// to `streamGenerateContent`. Anything else is a test-authoring mistake and fails loudly.
internal sealed class StubHttpHandler : HttpMessageHandler
{
    private readonly string _json;
    private readonly string _sse;

    public StubHttpHandler(string bufferedJson, string streamingSse)
    {
        _json = bufferedJson;
        _sse = streamingSse;
    }

    public List<(bool Streaming, string Body)> Requests { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
        var streaming = request.RequestUri?.AbsoluteUri.Contains("stream", StringComparison.OrdinalIgnoreCase) == true
            || body.Replace(" ", string.Empty).Contains("\"stream\":true", StringComparison.Ordinal);
        Requests.Add((streaming, body));

        return streaming
            ? new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_sse, Encoding.UTF8, "text/event-stream"),
            }
            : new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_json, Encoding.UTF8, "application/json"),
            };
    }

    /// <summary>Joins SSE frames that carry only a data line — Google's <c>streamGenerateContent?alt=sse</c> emits no event names.</summary>
    public static string SseData(params string[] frames)
    {
        var sb = new StringBuilder();
        foreach (var data in frames)
        {
            sb.Append("data: ").Append(data).Append("\n\n");
        }
        return sb.ToString();
    }

    /// <summary>Joins SSE events written as (event, data) pairs into one body, the way a vendor emits them.</summary>
    public static string Sse(params (string Event, string Data)[] events)
    {
        var sb = new StringBuilder();
        foreach (var (evt, data) in events)
        {
            sb.Append("event: ").Append(evt).Append('\n');
            sb.Append("data: ").Append(data).Append("\n\n");
        }
        return sb.ToString();
    }
}
