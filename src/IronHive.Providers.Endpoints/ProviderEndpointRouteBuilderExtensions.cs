using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.AI;

namespace IronHive.Providers.Endpoints;

/// <summary>
/// Mounts an <see cref="IChatClient"/> as the HTTP endpoint a provider-shaped client calls. Mounting is explicit - nothing is
/// registered by default - and the host chooses the route, the authentication, the origin policy and the model.
/// </summary>
public static class ProviderEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps an OpenAI Chat Completions endpoint (<c>POST</c>, request and reply as <c>/v1/chat/completions</c>) at
    /// <paramref name="pattern"/>. Supports text, images and audio input, tools and tool results, <c>response_format</c>, and the
    /// sampling parameters of the wire; one choice per request.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    /// <param name="pattern">The route, for example <c>/v1/chat/completions</c>.</param>
    /// <param name="configure">Sets the <see cref="ProviderEndpointOptions"/>; <see cref="ProviderEndpointOptions.ClientResolver"/> is required.</param>
    /// <returns>The endpoint convention builder, to add authorization, CORS or other endpoint conventions.</returns>
    /// <exception cref="InvalidOperationException">No client resolver was configured.</exception>
    public static IEndpointConventionBuilder MapOpenAIChatCompletions(
        this IEndpointRouteBuilder endpoints, string pattern, Action<ProviderEndpointOptions> configure)
        => Map(endpoints, pattern, OpenAIChatCompletionsCodec.Instance, configure);

    /// <summary>Maps an OpenAI Chat Completions endpoint answered by one fixed <paramref name="client"/>.</summary>
    /// <param name="endpoints">The route builder.</param>
    /// <param name="pattern">The route, for example <c>/v1/chat/completions</c>.</param>
    /// <param name="client">The client that answers every request.</param>
    /// <returns>The endpoint convention builder.</returns>
    public static IEndpointConventionBuilder MapOpenAIChatCompletions(this IEndpointRouteBuilder endpoints, string pattern, IChatClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        return Map(endpoints, pattern, OpenAIChatCompletionsCodec.Instance, o => o.ClientResolver = (_, _) => new ValueTask<IChatClient?>(client));
    }

    /// <summary>
    /// Maps an Anthropic Messages endpoint (<c>POST</c>, request and reply as <c>/v1/messages</c>) at <paramref name="pattern"/>. Supports a
    /// <c>system</c> prompt, text and image blocks, client tools with <c>tool_use</c> / <c>tool_result</c>, <c>tool_choice</c>, the sampling
    /// parameters and streaming (<c>message_start</c> ... <c>message_stop</c>). Server tools and thinking are not served.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    /// <param name="pattern">The route, for example <c>/v1/messages</c>.</param>
    /// <param name="configure">Sets the <see cref="ProviderEndpointOptions"/>; <see cref="ProviderEndpointOptions.ClientResolver"/> is required.</param>
    /// <returns>The endpoint convention builder, to add authorization, CORS or other endpoint conventions.</returns>
    /// <exception cref="InvalidOperationException">No client resolver was configured.</exception>
    public static IEndpointConventionBuilder MapAnthropicMessages(
        this IEndpointRouteBuilder endpoints, string pattern, Action<ProviderEndpointOptions> configure)
        => Map(endpoints, pattern, AnthropicMessagesCodec.Instance, configure);

    /// <summary>Maps an Anthropic Messages endpoint answered by one fixed <paramref name="client"/>.</summary>
    /// <param name="endpoints">The route builder.</param>
    /// <param name="pattern">The route, for example <c>/v1/messages</c>.</param>
    /// <param name="client">The client that answers every request.</param>
    /// <returns>The endpoint convention builder.</returns>
    public static IEndpointConventionBuilder MapAnthropicMessages(this IEndpointRouteBuilder endpoints, string pattern, IChatClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        return Map(endpoints, pattern, AnthropicMessagesCodec.Instance, o => o.ClientResolver = (_, _) => new ValueTask<IChatClient?>(client));
    }

    /// <summary>
    /// Maps the Gemini wire under <paramref name="pattern"/>: <c>POST {pattern}/models/{model}:generateContent</c> and
    /// <c>POST {pattern}/models/{model}:streamGenerateContent</c> (server-sent events). The model name comes from the route and reaches
    /// <see cref="ProviderEndpointOptions.ClientResolver"/>. Supports <c>systemInstruction</c>, text, inline and file data, function
    /// declarations with <c>functionCall</c> / <c>functionResponse</c> (a result is paired with its call by name when there is no id),
    /// <c>toolConfig</c>, <c>generationConfig</c> (sampling, <c>responseMimeType</c> / schema) and usage. Built-in tools are not served.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    /// <param name="pattern">The version prefix, for example <c>/v1beta</c> (the empty string mounts <c>models/...</c> at the root).</param>
    /// <param name="configure">Sets the <see cref="ProviderEndpointOptions"/>; <see cref="ProviderEndpointOptions.ClientResolver"/> is required.</param>
    /// <returns>The endpoint convention builder for both routes, to add authorization, CORS or other endpoint conventions.</returns>
    /// <exception cref="InvalidOperationException">No client resolver was configured.</exception>
    public static IEndpointConventionBuilder MapGeminiGenerateContent(
        this IEndpointRouteBuilder endpoints, string pattern, Action<ProviderEndpointOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(pattern);
        var options = BuildOptions(configure);
        var group = endpoints.MapGroup(pattern.TrimEnd('/'));
        group.MapPost("models/{model}:generateContent", (HttpContext context) => EndpointPipeline.HandleAsync(context, GeminiGenerateContentCodec.Unary, options));
        group.MapPost("models/{model}:streamGenerateContent", (HttpContext context) => EndpointPipeline.HandleAsync(context, GeminiGenerateContentCodec.Streaming, options));
        return group;
    }

    /// <summary>Maps the Gemini wire (see <see cref="MapGeminiGenerateContent(IEndpointRouteBuilder, string, Action{ProviderEndpointOptions})"/>) answered by one fixed <paramref name="client"/>.</summary>
    /// <param name="endpoints">The route builder.</param>
    /// <param name="pattern">The version prefix, for example <c>/v1beta</c>.</param>
    /// <param name="client">The client that answers every request.</param>
    /// <returns>The endpoint convention builder for both routes.</returns>
    public static IEndpointConventionBuilder MapGeminiGenerateContent(this IEndpointRouteBuilder endpoints, string pattern, IChatClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        return MapGeminiGenerateContent(endpoints, pattern, o => o.ClientResolver = (_, _) => new ValueTask<IChatClient?>(client));
    }

    /// <summary>
    /// Maps the stateless subset of the OpenAI Responses wire (<c>POST</c>, request and reply as <c>/v1/responses</c>) at
    /// <paramref name="pattern"/>: the whole conversation arrives as <c>input</c> (a string or message, <c>function_call</c> and
    /// <c>function_call_output</c> items), with <c>instructions</c>, function tools, <c>tool_choice</c>, <c>text.format</c> and
    /// streaming (<c>response.created</c> ... <c>response.completed</c>). Nothing is kept, so <c>previous_response_id</c>,
    /// <c>conversation</c> and <c>background</c> are refused with a 400, and built-in tools are not served.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    /// <param name="pattern">The route, for example <c>/v1/responses</c>.</param>
    /// <param name="configure">Sets the <see cref="ProviderEndpointOptions"/>; <see cref="ProviderEndpointOptions.ClientResolver"/> is required.</param>
    /// <returns>The endpoint convention builder, to add authorization, CORS or other endpoint conventions.</returns>
    /// <exception cref="InvalidOperationException">No client resolver was configured.</exception>
    public static IEndpointConventionBuilder MapOpenAIResponses(
        this IEndpointRouteBuilder endpoints, string pattern, Action<ProviderEndpointOptions> configure)
        => Map(endpoints, pattern, OpenAIResponsesCodec.Instance, configure);

    /// <summary>Maps the stateless Responses subset answered by one fixed <paramref name="client"/>.</summary>
    /// <param name="endpoints">The route builder.</param>
    /// <param name="pattern">The route, for example <c>/v1/responses</c>.</param>
    /// <param name="client">The client that answers every request.</param>
    /// <returns>The endpoint convention builder.</returns>
    public static IEndpointConventionBuilder MapOpenAIResponses(this IEndpointRouteBuilder endpoints, string pattern, IChatClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        return Map(endpoints, pattern, OpenAIResponsesCodec.Instance, o => o.ClientResolver = (_, _) => new ValueTask<IChatClient?>(client));
    }

    private static IEndpointConventionBuilder Map(
        IEndpointRouteBuilder endpoints, string pattern, IWireCodec codec, Action<ProviderEndpointOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentException.ThrowIfNullOrEmpty(pattern);
        var options = BuildOptions(configure);
        return endpoints.MapPost(pattern, (HttpContext context) => EndpointPipeline.HandleAsync(context, codec, options));
    }

    private static ProviderEndpointOptions BuildOptions(Action<ProviderEndpointOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var options = new ProviderEndpointOptions();
        configure(options);
        if (options.ClientResolver is null)
        {
            throw new InvalidOperationException("ProviderEndpointOptions.ClientResolver must be set: the endpoint needs a chat client to answer with.");
        }

        return options;
    }
}
