using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;

namespace IronHive.Providers.Endpoints;

/// <summary>A request parsed off a provider wire into what an <see cref="IChatClient"/> takes.</summary>
internal sealed record WireRequest(
    string? Model,
    IList<ChatMessage> Messages,
    ChatOptions Options,
    bool Stream,
    bool IncludeUsage);

internal enum WireErrorKind
{
    InvalidRequest,
    NotFound,
    PayloadTooLarge,
    RateLimited,
    Server,
}

/// <summary>A failure to answer, before it is dressed in a wire's error shape.</summary>
internal sealed record WireError(int Status, WireErrorKind Kind, string Message, string? Param = null);

/// <summary>A request the wire cannot accept (malformed body, unsupported feature): answered 400 in the wire's error shape.</summary>
internal sealed class WireRequestException(string message, string? param = null) : Exception(message)
{
    public string? Param { get; } = param;
}

/// <summary>What one provider wire contributes: reading its requests and writing its replies and errors.</summary>
internal interface IWireCodec
{
    /// <summary>A short name for logs.</summary>
    string Name { get; }

    /// <exception cref="WireRequestException">The request cannot be accepted.</exception>
    WireRequest Parse(JsonElement body, HttpContext context);

    Task WriteResponseAsync(HttpContext context, WireRequest request, ChatResponse response, CancellationToken cancellationToken);

    Task WriteErrorAsync(HttpContext context, WireError error, CancellationToken cancellationToken);

    /// <summary>Streams the updates as the wire's server-sent events. The first update has already been read, so a failure before it is still an ordinary error response.</summary>
    Task WriteStreamAsync(HttpContext context, WireRequest request, IAsyncEnumerable<ChatResponseUpdate> updates, CancellationToken cancellationToken);

    /// <summary>Ends a stream that has already started with the wire's in-stream error event.</summary>
    Task WriteStreamErrorAsync(HttpContext context, WireError error, CancellationToken cancellationToken);
}
