using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;

namespace IronHive.Providers.Endpoints;

/// <summary>
/// What a host decides about a provider-shaped endpoint. The endpoint only translates the wire; which model answers, what the
/// caller may ask for, and where the endpoint is mounted (route, authentication, origin policy) stay with the host.
/// </summary>
public sealed class ProviderEndpointOptions
{
    /// <summary>
    /// Chooses the <see cref="IChatClient"/> that answers a request, from the request and the model name the caller sent
    /// (<c>null</c> when it sent none). Returning <c>null</c> answers the wire's "model not found" error. Required.
    /// </summary>
    /// <remarks>
    /// The endpoint is stateless: the caller's tools reach the client as non-invocable declarations and the model's tool calls
    /// go back to the caller, who sends the results with its next turn. A client that invokes functions itself
    /// (<c>UseFunctionInvocation</c>) is the host's choice, but it would try to invoke those declarations.
    /// </remarks>
    public Func<HttpContext, string?, ValueTask<IChatClient?>>? ClientResolver { get; set; }

    /// <summary>
    /// Adjusts the <see cref="ChatOptions"/> built from the request before the call - the place to clamp what the caller may
    /// ask for (tools, token limits, the model) or to set what it may not. Runs after the request is parsed.
    /// </summary>
    public Func<HttpContext, ChatOptions, ValueTask>? ConfigureChatOptions { get; set; }

    /// <summary>
    /// The largest request body accepted, in bytes (default 32 MiB, enough for a few inline images). A larger body is answered
    /// with a 413 in the wire's error shape.
    /// </summary>
    public long MaxRequestBodyBytes { get; set; } = 32L * 1024 * 1024;

    /// <summary>
    /// Whether an unexpected failure of the chat client puts its message in the error the caller receives. Off by default:
    /// the message can carry an upstream address or detail that the caller of a keyless endpoint should not see.
    /// </summary>
    public bool ExposeErrorDetails { get; set; }
}
