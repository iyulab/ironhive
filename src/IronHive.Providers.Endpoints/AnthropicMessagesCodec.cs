using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;
using static IronHive.Providers.Endpoints.WireJson;

namespace IronHive.Providers.Endpoints;

/// <summary>The Anthropic Messages wire (<c>POST /v1/messages</c>).</summary>
internal sealed class AnthropicMessagesCodec : IWireCodec
{
    public static readonly AnthropicMessagesCodec Instance = new();

    public string Name => "anthropic-messages";

    // ---- request -------------------------------------------------------------------------------------------------

    public WireRequest Parse(JsonElement body, HttpContext context)
    {
        if (body.ValueKind != JsonValueKind.Object)
        {
            throw new WireRequestException("The request body must be a JSON object.");
        }

        if (GetInt(body, "max_tokens") is not { } maxTokens || maxTokens < 1)
        {
            throw new WireRequestException("'max_tokens' is required and must be a positive integer.", "max_tokens");
        }

        if (!body.TryGetProperty("messages", out var messagesElement) || messagesElement.ValueKind != JsonValueKind.Array
            || messagesElement.GetArrayLength() == 0)
        {
            throw new WireRequestException("'messages' is required and must be a non-empty array.", "messages");
        }

        var messages = new List<ChatMessage>();
        if (body.TryGetProperty("system", out var system))
        {
            var text = ParseSystem(system);
            if (text.Length > 0)
            {
                messages.Add(new ChatMessage(ChatRole.System, text));
            }
        }

        var index = 0;
        foreach (var element in messagesElement.EnumerateArray())
        {
            messages.AddRange(ParseMessage(element, $"messages[{index}]"));
            index++;
        }

        var options = new ChatOptions
        {
            ModelId = GetString(body, "model"),
            MaxOutputTokens = maxTokens,
            Temperature = GetFloat(body, "temperature"),
            TopP = GetFloat(body, "top_p"),
            TopK = GetInt(body, "top_k"),
        };
        if (body.TryGetProperty("stop_sequences", out var stops) && stops.ValueKind == JsonValueKind.Array)
        {
            options.StopSequences = [.. stops.EnumerateArray().Where(s => s.ValueKind == JsonValueKind.String).Select(s => s.GetString()!)];
        }

        if (body.TryGetProperty("tools", out var tools) && tools.ValueKind == JsonValueKind.Array)
        {
            options.Tools = ParseTools(tools);
        }

        if (body.TryGetProperty("tool_choice", out var choice) && choice.ValueKind == JsonValueKind.Object)
        {
            options.ToolMode = GetString(choice, "type") switch
            {
                "auto" => ChatToolMode.Auto,
                "any" => ChatToolMode.RequireAny,
                "none" => ChatToolMode.None,
                "tool" when GetString(choice, "name") is { Length: > 0 } name => ChatToolMode.RequireSpecific(name),
                var other => throw new WireRequestException($"The tool_choice type '{other}' is not supported.", "tool_choice.type"),
            };
            if (choice.TryGetProperty("disable_parallel_tool_use", out var disable) && disable.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                options.AllowMultipleToolCalls = !disable.GetBoolean();
            }
        }

        var stream = body.TryGetProperty("stream", out var streamElement) && streamElement.ValueKind == JsonValueKind.True;
        return new WireRequest(options.ModelId, messages, options, stream, IncludeUsage: true);
    }

    private static string ParseSystem(JsonElement system)
    {
        switch (system.ValueKind)
        {
            case JsonValueKind.String:
                return system.GetString() ?? string.Empty;
            case JsonValueKind.Array:
                return string.Concat(system.EnumerateArray().Where(b => GetString(b, "type") == "text").Select(b => GetString(b, "text")));
            case JsonValueKind.Null:
                return string.Empty;
            default:
                throw new WireRequestException("'system' must be a string or an array of text blocks.", "system");
        }
    }

    private static IEnumerable<ChatMessage> ParseMessage(JsonElement element, string path)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new WireRequestException("A message must be a JSON object.", path);
        }

        var role = GetString(element, "role");
        if (role is not ("user" or "assistant"))
        {
            throw new WireRequestException("A message role must be 'user' or 'assistant'.", $"{path}.role");
        }

        var chatRole = role == "user" ? ChatRole.User : ChatRole.Assistant;
        if (!element.TryGetProperty("content", out var content))
        {
            throw new WireRequestException("A message needs 'content'.", $"{path}.content");
        }

        if (content.ValueKind == JsonValueKind.String)
        {
            yield return new ChatMessage(chatRole, content.GetString());
            yield break;
        }

        if (content.ValueKind != JsonValueKind.Array)
        {
            throw new WireRequestException("'content' must be a string or an array of content blocks.", $"{path}.content");
        }

        // Anthropic puts a user turn's tool results first, then its text: they are a tool message and a user message here.
        var results = new List<AIContent>();
        var rest = new List<AIContent>();
        var blockIndex = 0;
        foreach (var block in content.EnumerateArray())
        {
            var blockPath = $"{path}.content[{blockIndex}]";
            switch (GetString(block, "type"))
            {
                case "text":
                    rest.Add(new TextContent(GetString(block, "text") ?? string.Empty));
                    break;
                case "image":
                    rest.Add(ParseImage(block, blockPath));
                    break;
                case "tool_use" when chatRole == ChatRole.Assistant:
                    rest.Add(ParseToolUse(block, blockPath));
                    break;
                case "tool_result" when chatRole == ChatRole.User:
                    results.Add(ParseToolResult(block, blockPath));
                    break;
                case "thinking" or "redacted_thinking" when chatRole == ChatRole.Assistant:
                    break; // not carried: a stateless endpoint cannot verify a signature it did not issue
                case var other:
                    throw new WireRequestException($"The content block type '{other}' is not supported here.", $"{blockPath}.type");
            }

            blockIndex++;
        }

        if (results.Count > 0)
        {
            yield return new ChatMessage(ChatRole.Tool, results);
        }

        if (rest.Count > 0 || results.Count == 0)
        {
            yield return new ChatMessage(chatRole, rest);
        }
    }

    private static AIContent ParseImage(JsonElement block, string path)
    {
        if (!block.TryGetProperty("source", out var source) || source.ValueKind != JsonValueKind.Object)
        {
            throw new WireRequestException("An image block needs a 'source'.", $"{path}.source");
        }

        try
        {
            return GetString(source, "type") switch
            {
                "base64" => new DataContent(Convert.FromBase64String(GetString(source, "data") ?? string.Empty),
                    GetString(source, "media_type") ?? throw new WireRequestException("An image source needs 'media_type'.", $"{path}.source.media_type")),
                "url" => new UriContent(GetString(source, "url") ?? string.Empty, "image/*"),
                var other => throw new WireRequestException($"The image source type '{other}' is not supported.", $"{path}.source.type"),
            };
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException or UriFormatException)
        {
            throw new WireRequestException("The image source is not valid.", $"{path}.source");
        }
    }

    private static FunctionCallContent ParseToolUse(JsonElement block, string path)
    {
        var id = GetString(block, "id") ?? throw new WireRequestException("A tool_use block needs an 'id'.", $"{path}.id");
        var name = GetString(block, "name") ?? throw new WireRequestException("A tool_use block needs a 'name'.", $"{path}.name");
        IDictionary<string, object?>? arguments = null;
        if (block.TryGetProperty("input", out var input) && input.ValueKind == JsonValueKind.Object)
        {
            arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(input.GetRawText(), AIJsonUtilities.DefaultOptions);
        }

        return new FunctionCallContent(id, name, arguments);
    }

    private static FunctionResultContent ParseToolResult(JsonElement block, string path)
    {
        var id = GetString(block, "tool_use_id") ?? throw new WireRequestException("A tool_result block needs a 'tool_use_id'.", $"{path}.tool_use_id");
        object? result = null;
        if (block.TryGetProperty("content", out var content))
        {
            if (content.ValueKind == JsonValueKind.String)
            {
                result = content.GetString();
            }
            else if (content.ValueKind == JsonValueKind.Array)
            {
                var parts = new List<AIContent>();
                var partIndex = 0;
                foreach (var part in content.EnumerateArray())
                {
                    parts.Add(GetString(part, "type") switch
                    {
                        "text" => new TextContent(GetString(part, "text") ?? string.Empty),
                        "image" => ParseImage(part, $"{path}.content[{partIndex}]"),
                        var other => throw new WireRequestException($"The tool result block type '{other}' is not supported.", $"{path}.content[{partIndex}].type"),
                    });
                    partIndex++;
                }

                result = parts.All(p => p is TextContent) ? string.Concat(parts.Cast<TextContent>().Select(t => t.Text)) : parts;
            }
        }

        var functionResult = new FunctionResultContent(id, result);
        if (block.TryGetProperty("is_error", out var isError) && isError.ValueKind == JsonValueKind.True)
        {
            functionResult.Exception = new InvalidOperationException(result as string ?? "The tool reported an error.");
        }

        return functionResult;
    }

    private static List<AITool> ParseTools(JsonElement tools)
    {
        var result = new List<AITool>();
        var index = 0;
        foreach (var tool in tools.EnumerateArray())
        {
            var path = $"tools[{index}]";
            if (GetString(tool, "name") is not { Length: > 0 } name)
            {
                throw new WireRequestException("A tool needs a 'name'.", path);
            }

            if (GetString(tool, "type") is { } type && type != "custom")
            {
                throw new WireRequestException($"The server tool type '{type}' is not supported; only client tools are.", $"{path}.type");
            }

            var schema = tool.TryGetProperty("input_schema", out var inputSchema) && inputSchema.ValueKind == JsonValueKind.Object
                ? inputSchema.Clone()
                : JsonDocument.Parse("""{"type":"object","properties":{}}""").RootElement.Clone();
            result.Add(AIFunctionFactory.CreateDeclaration(name, GetString(tool, "description"), schema));
            index++;
        }

        return result;
    }

    // ---- response ------------------------------------------------------------------------------------------------

    public async Task WriteResponseAsync(HttpContext context, WireRequest request, ChatResponse response, CancellationToken cancellationToken)
    {
        var blocks = new List<AIContent>();
        var text = new StringBuilder();
        foreach (var content in response.Messages.Where(m => m.Role == ChatRole.Assistant).SelectMany(m => m.Contents))
        {
            switch (content)
            {
                case TextContent t:
                    text.Append(t.Text);
                    break;
                case FunctionCallContent call:
                    if (text.Length > 0)
                    {
                        blocks.Add(new TextContent(text.ToString()));
                        text.Clear();
                    }

                    blocks.Add(call);
                    break;
            }
        }

        if (text.Length > 0 || blocks.Count == 0)
        {
            blocks.Add(new TextContent(text.ToString()));
        }

        var hasCalls = blocks.OfType<FunctionCallContent>().Any();
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "application/json";
        await using var writer = new Utf8JsonWriter(context.Response.Body);
        writer.WriteStartObject();
        writer.WriteString("id", "msg_" + Guid.NewGuid().ToString("N"));
        writer.WriteString("type", "message");
        writer.WriteString("role", "assistant");
        writer.WriteString("model", response.ModelId ?? request.Model ?? string.Empty);
        writer.WriteStartArray("content");
        foreach (var block in blocks)
        {
            WriteBlock(writer, block);
        }

        writer.WriteEndArray();
        writer.WriteString("stop_reason", StopReason(response.FinishReason, hasCalls));
        writer.WriteNull("stop_sequence");
        WriteUsage(writer, response.Usage, includeInput: true);
        writer.WriteEndObject();
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void WriteBlock(Utf8JsonWriter writer, AIContent block)
    {
        writer.WriteStartObject();
        if (block is FunctionCallContent call)
        {
            writer.WriteString("type", "tool_use");
            writer.WriteString("id", call.CallId is { Length: > 0 } id ? id : "toolu_" + Guid.NewGuid().ToString("N"));
            writer.WriteString("name", call.Name);
            writer.WritePropertyName("input");
            writer.WriteRawValue(Arguments(call));
        }
        else
        {
            writer.WriteString("type", "text");
            writer.WriteString("text", ((TextContent)block).Text);
        }

        writer.WriteEndObject();
    }

    internal static string StopReason(ChatFinishReason? reason, bool hasToolCalls)
    {
        if (reason == ChatFinishReason.ToolCalls) return "tool_use";
        if (reason == ChatFinishReason.Length) return "max_tokens";
        if (reason == ChatFinishReason.ContentFilter) return "refusal";
        if (reason is null && hasToolCalls) return "tool_use";
        return "end_turn";
    }

    private static void WriteUsage(Utf8JsonWriter writer, UsageDetails? usage, bool includeInput)
    {
        writer.WriteStartObject("usage");
        if (includeInput)
        {
            writer.WriteNumber("input_tokens", usage?.InputTokenCount ?? 0);
        }

        writer.WriteNumber("output_tokens", usage?.OutputTokenCount ?? 0);
        if (usage?.CachedInputTokenCount is { } cached)
        {
            writer.WriteNumber("cache_read_input_tokens", cached);
        }

        writer.WriteEndObject();
    }

    // ---- streaming -----------------------------------------------------------------------------------------------

    public async Task WriteStreamAsync(HttpContext context, WireRequest request, IAsyncEnumerable<ChatResponseUpdate> updates, CancellationToken cancellationToken)
    {
        var response = context.Response;
        response.StatusCode = StatusCodes.Status200OK;
        response.ContentType = "text/event-stream";
        response.Headers.CacheControl = "no-cache";
        response.Headers["X-Accel-Buffering"] = "no";
        context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpResponseBodyFeature>()?.DisableBuffering();

        var id = "msg_" + Guid.NewGuid().ToString("N");
        var model = request.Model ?? string.Empty;
        var started = false;
        var blockIndex = -1;
        var textOpen = false;
        var toolCalls = 0;
        ChatFinishReason? finish = null;
        UsageDetails? usage = null;

        Task Event(string name, Action<Utf8JsonWriter> write) => SendAsync(response, name, write, cancellationToken);

        async Task StartAsync()
        {
            started = true;
            await Event("message_start", w =>
            {
                w.WriteStartObject();
                w.WriteString("type", "message_start");
                w.WriteStartObject("message");
                w.WriteString("id", id);
                w.WriteString("type", "message");
                w.WriteString("role", "assistant");
                w.WriteString("model", model);
                w.WriteStartArray("content");
                w.WriteEndArray();
                w.WriteNull("stop_reason");
                w.WriteNull("stop_sequence");
                w.WriteStartObject("usage");
                w.WriteNumber("input_tokens", 0);
                w.WriteNumber("output_tokens", 0);
                w.WriteEndObject();
                w.WriteEndObject();
                w.WriteEndObject();
            }).ConfigureAwait(false);
        }

        async Task CloseTextAsync()
        {
            if (!textOpen)
            {
                return;
            }

            textOpen = false;
            var closed = blockIndex;
            await Event("content_block_stop", w =>
            {
                w.WriteStartObject();
                w.WriteString("type", "content_block_stop");
                w.WriteNumber("index", closed);
                w.WriteEndObject();
            }).ConfigureAwait(false);
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
                    {
                        if (!textOpen)
                        {
                            textOpen = true;
                            var opened = ++blockIndex;
                            await Event("content_block_start", w =>
                            {
                                w.WriteStartObject();
                                w.WriteString("type", "content_block_start");
                                w.WriteNumber("index", opened);
                                w.WriteStartObject("content_block");
                                w.WriteString("type", "text");
                                w.WriteString("text", string.Empty);
                                w.WriteEndObject();
                                w.WriteEndObject();
                            }).ConfigureAwait(false);
                        }

                        var at = blockIndex;
                        await Event("content_block_delta", w =>
                        {
                            w.WriteStartObject();
                            w.WriteString("type", "content_block_delta");
                            w.WriteNumber("index", at);
                            w.WriteStartObject("delta");
                            w.WriteString("type", "text_delta");
                            w.WriteString("text", text.Text);
                            w.WriteEndObject();
                            w.WriteEndObject();
                        }).ConfigureAwait(false);
                        break;
                    }
                    case FunctionCallContent call:
                    {
                        await CloseTextAsync().ConfigureAwait(false);
                        toolCalls++;
                        var at = ++blockIndex;
                        await Event("content_block_start", w =>
                        {
                            w.WriteStartObject();
                            w.WriteString("type", "content_block_start");
                            w.WriteNumber("index", at);
                            w.WriteStartObject("content_block");
                            w.WriteString("type", "tool_use");
                            w.WriteString("id", call.CallId is { Length: > 0 } callId ? callId : "toolu_" + Guid.NewGuid().ToString("N"));
                            w.WriteString("name", call.Name);
                            w.WriteStartObject("input");
                            w.WriteEndObject();
                            w.WriteEndObject();
                            w.WriteEndObject();
                        }).ConfigureAwait(false);
                        await Event("content_block_delta", w =>
                        {
                            w.WriteStartObject();
                            w.WriteString("type", "content_block_delta");
                            w.WriteNumber("index", at);
                            w.WriteStartObject("delta");
                            w.WriteString("type", "input_json_delta");
                            w.WriteString("partial_json", Arguments(call));
                            w.WriteEndObject();
                            w.WriteEndObject();
                        }).ConfigureAwait(false);
                        await Event("content_block_stop", w =>
                        {
                            w.WriteStartObject();
                            w.WriteString("type", "content_block_stop");
                            w.WriteNumber("index", at);
                            w.WriteEndObject();
                        }).ConfigureAwait(false);
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

        await CloseTextAsync().ConfigureAwait(false);
        await Event("message_delta", w =>
        {
            w.WriteStartObject();
            w.WriteString("type", "message_delta");
            w.WriteStartObject("delta");
            w.WriteString("stop_reason", StopReason(finish, toolCalls > 0));
            w.WriteNull("stop_sequence");
            w.WriteEndObject();
            WriteUsage(w, usage, includeInput: true);
            w.WriteEndObject();
        }).ConfigureAwait(false);
        await Event("message_stop", w =>
        {
            w.WriteStartObject();
            w.WriteString("type", "message_stop");
            w.WriteEndObject();
        }).ConfigureAwait(false);
    }

    public Task WriteStreamErrorAsync(HttpContext context, WireError error, CancellationToken cancellationToken) =>
        SendAsync(context.Response, "error", w => WriteErrorBody(w, error), cancellationToken);

    private static async Task SendAsync(HttpResponse response, string name, Action<Utf8JsonWriter> write, CancellationToken cancellationToken)
    {
        var buffer = new System.Buffers.ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            write(writer);
        }

        await response.WriteAsync("event: " + name + "\ndata: ", cancellationToken).ConfigureAwait(false);
        await response.Body.WriteAsync(buffer.WrittenMemory, cancellationToken).ConfigureAwait(false);
        await response.WriteAsync("\n\n", cancellationToken).ConfigureAwait(false);
        await response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    // ---- errors --------------------------------------------------------------------------------------------------

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

    private static void WriteErrorBody(Utf8JsonWriter writer, WireError error)
    {
        var type = error.Kind switch
        {
            WireErrorKind.InvalidRequest => "invalid_request_error",
            WireErrorKind.NotFound => "not_found_error",
            WireErrorKind.PayloadTooLarge => "request_too_large",
            WireErrorKind.RateLimited => "rate_limit_error",
            _ => "api_error",
        };
        writer.WriteStartObject();
        writer.WriteString("type", "error");
        writer.WriteStartObject("error");
        writer.WriteString("type", type);
        writer.WriteString("message", error.Param is null ? error.Message : $"{error.Param}: {error.Message}");
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
