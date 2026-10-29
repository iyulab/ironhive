using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;
using static IronHive.Providers.Endpoints.WireJson;

namespace IronHive.Providers.Endpoints;

/// <summary>The OpenAI Chat Completions wire (<c>POST /v1/chat/completions</c>).</summary>
internal sealed class OpenAIChatCompletionsCodec : IWireCodec
{
    public static readonly OpenAIChatCompletionsCodec Instance = new();

    public string Name => "chat-completions";

    // ---- request -------------------------------------------------------------------------------------------------

    public WireRequest Parse(JsonElement body, HttpContext context)
    {
        if (body.ValueKind != JsonValueKind.Object)
        {
            throw new WireRequestException("The request body must be a JSON object.");
        }

        if (body.TryGetProperty("n", out var n) && n.ValueKind == JsonValueKind.Number && !(n.TryGetInt32(out var choices) && choices == 1))
        {
            throw new WireRequestException("Only one choice per request is supported (n must be 1).", "n");
        }

        if (!body.TryGetProperty("messages", out var messagesElement) || messagesElement.ValueKind != JsonValueKind.Array
            || messagesElement.GetArrayLength() == 0)
        {
            throw new WireRequestException("'messages' is required and must be a non-empty array.", "messages");
        }

        var messages = new List<ChatMessage>();
        var index = 0;
        foreach (var element in messagesElement.EnumerateArray())
        {
            messages.Add(ParseMessage(element, $"messages[{index}]"));
            index++;
        }

        var options = new ChatOptions();
        var model = GetString(body, "model");
        options.ModelId = model;
        options.Temperature = GetFloat(body, "temperature");
        options.TopP = GetFloat(body, "top_p");
        options.FrequencyPenalty = GetFloat(body, "frequency_penalty");
        options.PresencePenalty = GetFloat(body, "presence_penalty");
        options.MaxOutputTokens = GetInt(body, "max_completion_tokens") ?? GetInt(body, "max_tokens");
        if (body.TryGetProperty("seed", out var seed) && seed.ValueKind == JsonValueKind.Number && seed.TryGetInt64(out var seedValue))
        {
            options.Seed = seedValue;
        }

        if (body.TryGetProperty("stop", out var stop))
        {
            options.StopSequences = stop.ValueKind switch
            {
                JsonValueKind.String => [stop.GetString()!],
                JsonValueKind.Array => [.. stop.EnumerateArray().Where(s => s.ValueKind == JsonValueKind.String).Select(s => s.GetString()!)],
                JsonValueKind.Null => null,
                _ => throw new WireRequestException("'stop' must be a string or an array of strings.", "stop"),
            };
        }

        if (body.TryGetProperty("tools", out var tools) && tools.ValueKind == JsonValueKind.Array)
        {
            options.Tools = ParseTools(tools);
        }

        if (body.TryGetProperty("tool_choice", out var toolChoice))
        {
            options.ToolMode = ParseToolChoice(toolChoice);
        }

        if (body.TryGetProperty("parallel_tool_calls", out var parallel) && parallel.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            options.AllowMultipleToolCalls = parallel.GetBoolean();
        }

        if (body.TryGetProperty("response_format", out var format) && format.ValueKind == JsonValueKind.Object)
        {
            options.ResponseFormat = ParseResponseFormat(format);
        }

        var stream = body.TryGetProperty("stream", out var streamElement) && streamElement.ValueKind == JsonValueKind.True;

        var includeUsage = body.TryGetProperty("stream_options", out var streamOptions) && streamOptions.ValueKind == JsonValueKind.Object
            && streamOptions.TryGetProperty("include_usage", out var include) && include.ValueKind == JsonValueKind.True;

        return new WireRequest(model, messages, options, stream, includeUsage);
    }

    private static ChatMessage ParseMessage(JsonElement element, string path)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new WireRequestException("A message must be a JSON object.", path);
        }

        var role = GetString(element, "role") ?? throw new WireRequestException("A message needs a 'role'.", $"{path}.role");
        var author = GetString(element, "name");
        ChatMessage message;
        switch (role)
        {
            case "system" or "developer":
                message = new ChatMessage(ChatRole.System, ParseContent(element, path, allowMedia: false));
                break;
            case "user":
                message = new ChatMessage(ChatRole.User, ParseContent(element, path, allowMedia: true));
                break;
            case "assistant":
            {
                var contents = ParseContent(element, path, allowMedia: false);
                if (element.TryGetProperty("tool_calls", out var calls) && calls.ValueKind == JsonValueKind.Array)
                {
                    var callIndex = 0;
                    foreach (var call in calls.EnumerateArray())
                    {
                        contents.Add(ParseToolCall(call, $"{path}.tool_calls[{callIndex}]"));
                        callIndex++;
                    }
                }

                message = new ChatMessage(ChatRole.Assistant, contents);
                break;
            }
            case "tool":
            {
                var callId = GetString(element, "tool_call_id")
                    ?? throw new WireRequestException("A tool message needs a 'tool_call_id'.", $"{path}.tool_call_id");
                var text = string.Concat(ParseContent(element, path, allowMedia: false).OfType<TextContent>().Select(t => t.Text));
                message = new ChatMessage(ChatRole.Tool, [new FunctionResultContent(callId, text)]);
                break;
            }
            default:
                throw new WireRequestException($"The role '{role}' is not supported.", $"{path}.role");
        }

        message.AuthorName = author;
        return message;
    }

    private static List<AIContent> ParseContent(JsonElement message, string path, bool allowMedia)
    {
        var contents = new List<AIContent>();
        if (!message.TryGetProperty("content", out var content))
        {
            return contents;
        }

        switch (content.ValueKind)
        {
            case JsonValueKind.Null:
                return contents;
            case JsonValueKind.String:
                contents.Add(new TextContent(content.GetString()));
                return contents;
            case JsonValueKind.Array:
            {
                var partIndex = 0;
                foreach (var part in content.EnumerateArray())
                {
                    var partPath = $"{path}.content[{partIndex}]";
                    var type = GetString(part, "type");
                    switch (type)
                    {
                        case "text":
                            contents.Add(new TextContent(GetString(part, "text") ?? string.Empty));
                            break;
                        case "refusal" when !allowMedia:
                            contents.Add(new TextContent(GetString(part, "refusal") ?? string.Empty));
                            break;
                        case "image_url" when allowMedia:
                            contents.Add(ParseImage(part, partPath));
                            break;
                        case "input_audio" when allowMedia:
                            contents.Add(ParseAudio(part, partPath));
                            break;
                        default:
                            throw new WireRequestException($"The content part type '{type}' is not supported here.", $"{partPath}.type");
                    }

                    partIndex++;
                }

                return contents;
            }
            default:
                throw new WireRequestException("'content' must be a string, an array of parts, or null.", $"{path}.content");
        }
    }

    private static AIContent ParseImage(JsonElement part, string path)
    {
        if (!part.TryGetProperty("image_url", out var image) || image.ValueKind != JsonValueKind.Object
            || GetString(image, "url") is not { Length: > 0 } url)
        {
            throw new WireRequestException("An image part needs 'image_url.url'.", $"{path}.image_url.url");
        }

        AIContent content;
        try
        {
            content = url.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
                ? new DataContent(url)
                : new UriContent(url, "image/*");
        }
        catch (Exception ex) when (ex is ArgumentException or UriFormatException or FormatException)
        {
            throw new WireRequestException("The image URL is not a valid data URI or URL.", $"{path}.image_url.url");
        }

        if (GetString(image, "detail") is { } detail)
        {
            (content.AdditionalProperties ??= [])["detail"] = detail;
        }

        return content;
    }

    private static DataContent ParseAudio(JsonElement part, string path)
    {
        if (!part.TryGetProperty("input_audio", out var audio) || audio.ValueKind != JsonValueKind.Object
            || GetString(audio, "data") is not { Length: > 0 } data)
        {
            throw new WireRequestException("An audio part needs 'input_audio.data'.", $"{path}.input_audio.data");
        }

        try
        {
            return new DataContent(Convert.FromBase64String(data), "audio/" + (GetString(audio, "format") ?? "wav"));
        }
        catch (FormatException)
        {
            throw new WireRequestException("The audio data is not valid base64.", $"{path}.input_audio.data");
        }
    }

    private static FunctionCallContent ParseToolCall(JsonElement call, string path)
    {
        if (!call.TryGetProperty("function", out var function) || function.ValueKind != JsonValueKind.Object
            || GetString(function, "name") is not { Length: > 0 } name)
        {
            throw new WireRequestException("A tool call needs 'function.name'.", $"{path}.function.name");
        }

        var id = GetString(call, "id") ?? throw new WireRequestException("A tool call needs an 'id'.", $"{path}.id");
        IDictionary<string, object?>? arguments = null;
        if (GetString(function, "arguments") is { Length: > 0 } json)
        {
            try
            {
                arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(json, AIJsonUtilities.DefaultOptions);
            }
            catch (JsonException)
            {
                throw new WireRequestException("The tool call arguments are not a JSON object.", $"{path}.function.arguments");
            }
        }

        return new FunctionCallContent(id, name, arguments);
    }

    private static List<AITool> ParseTools(JsonElement tools)
    {
        var result = new List<AITool>();
        var index = 0;
        foreach (var tool in tools.EnumerateArray())
        {
            var path = $"tools[{index}]";
            if (GetString(tool, "type") != "function" || !tool.TryGetProperty("function", out var function)
                || function.ValueKind != JsonValueKind.Object || GetString(function, "name") is not { Length: > 0 } name)
            {
                throw new WireRequestException("Only tools of type 'function' with a 'function.name' are supported.", path);
            }

            var schema = function.TryGetProperty("parameters", out var parameters) && parameters.ValueKind == JsonValueKind.Object
                ? parameters.Clone()
                : JsonDocument.Parse("""{"type":"object","properties":{}}""").RootElement.Clone();
            result.Add(AIFunctionFactory.CreateDeclaration(name, GetString(function, "description"), schema));
            index++;
        }

        return result;
    }

    private static ChatToolMode? ParseToolChoice(JsonElement choice)
    {
        switch (choice.ValueKind)
        {
            case JsonValueKind.String:
                return choice.GetString() switch
                {
                    "auto" => ChatToolMode.Auto,
                    "none" => ChatToolMode.None,
                    "required" => ChatToolMode.RequireAny,
                    var other => throw new WireRequestException($"The tool_choice '{other}' is not supported.", "tool_choice"),
                };
            case JsonValueKind.Object when choice.TryGetProperty("function", out var function) && GetString(function, "name") is { } name:
                return ChatToolMode.RequireSpecific(name);
            case JsonValueKind.Null:
                return null;
            default:
                throw new WireRequestException("'tool_choice' must be a string or a function selector.", "tool_choice");
        }
    }

    private static ChatResponseFormatJson? ParseResponseFormat(JsonElement format)
    {
        switch (GetString(format, "type"))
        {
            case "text" or null:
                return null;
            case "json_object":
                return ChatResponseFormat.Json;
            case "json_schema":
                if (!format.TryGetProperty("json_schema", out var spec) || spec.ValueKind != JsonValueKind.Object
                    || !spec.TryGetProperty("schema", out var schema) || schema.ValueKind != JsonValueKind.Object)
                {
                    throw new WireRequestException("A json_schema response format needs 'json_schema.schema'.", "response_format.json_schema.schema");
                }

                return ChatResponseFormat.ForJsonSchema(schema.Clone(), GetString(spec, "name"), GetString(spec, "description"));
            default:
                throw new WireRequestException("The response_format type is not supported.", "response_format.type");
        }
    }

    // ---- response ------------------------------------------------------------------------------------------------

    public async Task WriteResponseAsync(HttpContext context, WireRequest request, ChatResponse response, CancellationToken cancellationToken)
    {
        var text = new StringBuilder();
        var calls = new List<FunctionCallContent>();
        foreach (var message in response.Messages.Where(m => m.Role == ChatRole.Assistant))
        {
            foreach (var content in message.Contents)
            {
                switch (content)
                {
                    case TextContent t:
                        text.Append(t.Text);
                        break;
                    case FunctionCallContent call:
                        calls.Add(call);
                        break;
                }
            }
        }

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "application/json";
        await using var writer = new Utf8JsonWriter(context.Response.Body);
        writer.WriteStartObject();
        writer.WriteString("id", "chatcmpl-" + Guid.NewGuid().ToString("N"));
        writer.WriteString("object", "chat.completion");
        writer.WriteNumber("created", (response.CreatedAt ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds());
        writer.WriteString("model", response.ModelId ?? request.Model ?? string.Empty);
        writer.WriteStartArray("choices");
        writer.WriteStartObject();
        writer.WriteNumber("index", 0);
        writer.WriteStartObject("message");
        writer.WriteString("role", "assistant");
        if (text.Length > 0 || calls.Count == 0)
        {
            writer.WriteString("content", text.ToString());
        }
        else
        {
            writer.WriteNull("content");
        }

        if (calls.Count > 0)
        {
            writer.WriteStartArray("tool_calls");
            foreach (var call in calls)
            {
                writer.WriteStartObject();
                writer.WriteString("id", call.CallId is { Length: > 0 } id ? id : "call_" + Guid.NewGuid().ToString("N"));
                writer.WriteString("type", "function");
                writer.WriteStartObject("function");
                writer.WriteString("name", call.Name);
                writer.WriteString("arguments", Arguments(call));
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
        writer.WriteString("finish_reason", FinishReason(response.FinishReason, calls.Count > 0));
        writer.WriteEndObject();
        writer.WriteEndArray();
        WriteUsage(writer, response.Usage);
        writer.WriteEndObject();
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task WriteStreamAsync(HttpContext context, WireRequest request, IAsyncEnumerable<ChatResponseUpdate> updates, CancellationToken cancellationToken)
    {
        var response = context.Response;
        response.StatusCode = StatusCodes.Status200OK;
        response.ContentType = "text/event-stream";
        response.Headers.CacheControl = "no-cache";
        response.Headers["X-Accel-Buffering"] = "no";
        context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpResponseBodyFeature>()?.DisableBuffering();

        var id = "chatcmpl-" + Guid.NewGuid().ToString("N");
        var created = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var model = request.Model ?? string.Empty;
        var started = false;
        var toolCalls = 0;
        ChatFinishReason? finish = null;
        UsageDetails? usage = null;

        Task Chunk(Action<Utf8JsonWriter> delta, string? finishReason) => SendAsync(response, w =>
        {
            w.WriteStartObject();
            w.WriteString("id", id);
            w.WriteString("object", "chat.completion.chunk");
            w.WriteNumber("created", created);
            w.WriteString("model", model);
            w.WriteStartArray("choices");
            w.WriteStartObject();
            w.WriteNumber("index", 0);
            w.WriteStartObject("delta");
            delta(w);
            w.WriteEndObject();
            if (finishReason is null) w.WriteNull("finish_reason"); else w.WriteString("finish_reason", finishReason);
            w.WriteEndObject();
            w.WriteEndArray();
            w.WriteEndObject();
        }, cancellationToken);

        async Task StartAsync()
        {
            started = true;
            await Chunk(w =>
            {
                w.WriteString("role", "assistant");
                w.WriteString("content", string.Empty);
            }, null).ConfigureAwait(false);
        }

        await foreach (var update in updates.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (!started)
            {
                if (update.ModelId is { Length: > 0 } first) model = first;
                await StartAsync().ConfigureAwait(false);
            }

            if (update.FinishReason is { } reason) finish = reason;
            foreach (var content in update.Contents)
            {
                switch (content)
                {
                    case TextContent { Text.Length: > 0 } text:
                        await Chunk(w => w.WriteString("content", text.Text), null).ConfigureAwait(false);
                        break;
                    case FunctionCallContent call:
                    {
                        var index = toolCalls++;
                        await Chunk(w =>
                        {
                            w.WriteStartArray("tool_calls");
                            w.WriteStartObject();
                            w.WriteNumber("index", index);
                            w.WriteString("id", call.CallId is { Length: > 0 } callId ? callId : "call_" + Guid.NewGuid().ToString("N"));
                            w.WriteString("type", "function");
                            w.WriteStartObject("function");
                            w.WriteString("name", call.Name);
                            w.WriteString("arguments", Arguments(call));
                            w.WriteEndObject();
                            w.WriteEndObject();
                            w.WriteEndArray();
                        }, null).ConfigureAwait(false);
                        break;
                    }
                    case UsageContent used:
                        if (usage is null) usage = used.Details; else usage.Add(used.Details);
                        break;
                }
            }
        }

        if (!started)
        {
            await StartAsync().ConfigureAwait(false);
        }

        await Chunk(_ => { }, FinishReason(finish, toolCalls > 0)).ConfigureAwait(false);
        if (request.IncludeUsage && usage is not null)
        {
            await SendAsync(response, w =>
            {
                w.WriteStartObject();
                w.WriteString("id", id);
                w.WriteString("object", "chat.completion.chunk");
                w.WriteNumber("created", created);
                w.WriteString("model", model);
                w.WriteStartArray("choices");
                w.WriteEndArray();
                WriteUsage(w, usage);
                w.WriteEndObject();
            }, cancellationToken).ConfigureAwait(false);
        }

        await response.WriteAsync("data: [DONE]\n\n", cancellationToken).ConfigureAwait(false);
        await response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task WriteStreamErrorAsync(HttpContext context, WireError error, CancellationToken cancellationToken) =>
        SendAsync(context.Response, w => WriteErrorBody(w, error), cancellationToken);

    private static async Task SendAsync(HttpResponse response, Action<Utf8JsonWriter> write, CancellationToken cancellationToken)
    {
        var buffer = new System.Buffers.ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            write(writer);
        }

        await response.WriteAsync("data: ", cancellationToken).ConfigureAwait(false);
        await response.Body.WriteAsync(buffer.WrittenMemory, cancellationToken).ConfigureAwait(false);
        await response.WriteAsync("\n\n", cancellationToken).ConfigureAwait(false);
        await response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    internal static string FinishReason(ChatFinishReason? reason, bool hasToolCalls)
    {
        if (reason == ChatFinishReason.ToolCalls) return "tool_calls";
        if (reason == ChatFinishReason.Length) return "length";
        if (reason == ChatFinishReason.ContentFilter) return "content_filter";
        if (reason is null && hasToolCalls) return "tool_calls";
        return "stop";
    }

    internal static void WriteUsage(Utf8JsonWriter writer, UsageDetails? usage)
    {
        if (usage is null)
        {
            return;
        }

        var input = usage.InputTokenCount ?? 0;
        var output = usage.OutputTokenCount ?? 0;
        writer.WriteStartObject("usage");
        writer.WriteNumber("prompt_tokens", input);
        writer.WriteNumber("completion_tokens", output);
        writer.WriteNumber("total_tokens", usage.TotalTokenCount ?? input + output);
        if (usage.CachedInputTokenCount is { } cached)
        {
            writer.WriteStartObject("prompt_tokens_details");
            writer.WriteNumber("cached_tokens", cached);
            writer.WriteEndObject();
        }

        if (usage.ReasoningTokenCount is { } reasoning)
        {
            writer.WriteStartObject("completion_tokens_details");
            writer.WriteNumber("reasoning_tokens", reasoning);
            writer.WriteEndObject();
        }

        writer.WriteEndObject();
    }

    public async Task WriteErrorAsync(HttpContext context, WireError error, CancellationToken cancellationToken)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = error.Status;
        context.Response.ContentType = "application/json";
        await using var writer = new Utf8JsonWriter(context.Response.Body);
        WriteErrorBody(writer, error);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>The OpenAI <c>code</c> for an error, for the wires that share this error object.</summary>
    internal static string ErrorCode(WireError error) => error.Kind switch
    {
        WireErrorKind.NotFound => "model_not_found",
        WireErrorKind.PayloadTooLarge => "request_too_large",
        WireErrorKind.RateLimited => "rate_limit_exceeded",
        WireErrorKind.InvalidRequest => "invalid_request_error",
        _ => "server_error",
    };

    private static void WriteErrorBody(Utf8JsonWriter writer, WireError error)
    {
        var (type, code) = error.Kind switch
        {
            WireErrorKind.InvalidRequest => ("invalid_request_error", (string?)null),
            WireErrorKind.NotFound => ("invalid_request_error", "model_not_found"),
            WireErrorKind.PayloadTooLarge => ("invalid_request_error", "request_too_large"),
            WireErrorKind.RateLimited => ("rate_limit_error", "rate_limit_exceeded"),
            _ => ("server_error", (string?)null),
        };
        writer.WriteStartObject();
        writer.WriteStartObject("error");
        writer.WriteString("message", error.Message);
        writer.WriteString("type", type);
        if (error.Param is null) writer.WriteNull("param"); else writer.WriteString("param", error.Param);
        if (code is null) writer.WriteNull("code"); else writer.WriteString("code", code);
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
