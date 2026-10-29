using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;
using static IronHive.Providers.Endpoints.WireJson;

namespace IronHive.Providers.Endpoints;

/// <summary>
/// The stateless subset of the OpenAI Responses wire (<c>POST /v1/responses</c>): the whole conversation arrives as <c>input</c> and nothing
/// is kept, so <c>previous_response_id</c>, <c>conversation</c> and <c>background</c> are refused rather than half honoured.
/// </summary>
internal sealed class OpenAIResponsesCodec : IWireCodec
{
    public static readonly OpenAIResponsesCodec Instance = new();

    public string Name => "responses";

    // ---- request -------------------------------------------------------------------------------------------------

    public WireRequest Parse(JsonElement body, HttpContext context)
    {
        if (body.ValueKind != JsonValueKind.Object)
        {
            throw new WireRequestException("The request body must be a JSON object.");
        }

        foreach (var stateful in new[] { "previous_response_id", "conversation" })
        {
            if (body.TryGetProperty(stateful, out var value) && value.ValueKind != JsonValueKind.Null)
            {
                throw new WireRequestException("This endpoint keeps no conversation state; send the whole conversation as 'input'.", stateful);
            }
        }

        if (body.TryGetProperty("background", out var background) && background.ValueKind == JsonValueKind.True)
        {
            throw new WireRequestException("Background responses are not supported.", "background");
        }

        var messages = new List<ChatMessage>();
        if (GetString(body, "instructions") is { Length: > 0 } instructions)
        {
            messages.Add(new ChatMessage(ChatRole.System, instructions));
        }

        if (!body.TryGetProperty("input", out var input))
        {
            throw new WireRequestException("'input' is required.", "input");
        }

        switch (input.ValueKind)
        {
            case JsonValueKind.String:
                messages.Add(new ChatMessage(ChatRole.User, input.GetString()));
                break;
            case JsonValueKind.Array when input.GetArrayLength() > 0:
                ParseItems(input, messages);
                break;
            default:
                throw new WireRequestException("'input' must be a string or a non-empty array of items.", "input");
        }

        var options = new ChatOptions
        {
            ModelId = GetString(body, "model"),
            Temperature = GetFloat(body, "temperature"),
            TopP = GetFloat(body, "top_p"),
            MaxOutputTokens = GetInt(body, "max_output_tokens"),
        };
        if (body.TryGetProperty("tools", out var tools) && tools.ValueKind == JsonValueKind.Array)
        {
            options.Tools = ParseTools(tools);
        }

        if (body.TryGetProperty("tool_choice", out var choice))
        {
            options.ToolMode = ParseToolChoice(choice);
        }

        if (body.TryGetProperty("parallel_tool_calls", out var parallel) && parallel.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            options.AllowMultipleToolCalls = parallel.GetBoolean();
        }

        if (body.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.Object
            && text.TryGetProperty("format", out var format) && format.ValueKind == JsonValueKind.Object)
        {
            options.ResponseFormat = ParseFormat(format);
        }

        var stream = body.TryGetProperty("stream", out var streamElement) && streamElement.ValueKind == JsonValueKind.True;
        return new WireRequest(options.ModelId, messages, options, stream, IncludeUsage: true);
    }

    private static void ParseItems(JsonElement items, List<ChatMessage> messages)
    {
        var index = 0;
        foreach (var item in items.EnumerateArray())
        {
            var path = $"input[{index}]";
            var type = GetString(item, "type") ?? (GetString(item, "role") is not null ? "message" : null);
            switch (type)
            {
                case "message":
                    messages.Add(ParseMessage(item, path));
                    break;
                case "function_call":
                {
                    var callId = GetString(item, "call_id") ?? throw new WireRequestException("A function_call needs a 'call_id'.", $"{path}.call_id");
                    var name = GetString(item, "name") ?? throw new WireRequestException("A function_call needs a 'name'.", $"{path}.name");
                    IDictionary<string, object?>? arguments = null;
                    if (GetString(item, "arguments") is { Length: > 0 } json)
                    {
                        try
                        {
                            arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(json, AIJsonUtilities.DefaultOptions);
                        }
                        catch (JsonException)
                        {
                            throw new WireRequestException("The function_call arguments are not a JSON object.", $"{path}.arguments");
                        }
                    }

                    var call = new FunctionCallContent(callId, name, arguments);
                    if (messages.Count > 0 && messages[^1].Role == ChatRole.Assistant && messages[^1].Contents.All(c => c is FunctionCallContent))
                    {
                        messages[^1].Contents.Add(call); // consecutive calls are one assistant turn
                    }
                    else
                    {
                        messages.Add(new ChatMessage(ChatRole.Assistant, [call]));
                    }

                    break;
                }
                case "function_call_output":
                {
                    var callId = GetString(item, "call_id") ?? throw new WireRequestException("A function_call_output needs a 'call_id'.", $"{path}.call_id");
                    var output = item.TryGetProperty("output", out var o) ? (o.ValueKind == JsonValueKind.String ? o.GetString() : o.GetRawText()) : null;
                    var result = new FunctionResultContent(callId, output);
                    if (messages.Count > 0 && messages[^1].Role == ChatRole.Tool)
                    {
                        messages[^1].Contents.Add(result);
                    }
                    else
                    {
                        messages.Add(new ChatMessage(ChatRole.Tool, [result]));
                    }

                    break;
                }
                case "reasoning":
                    break; // the model's own reasoning is not carried back through a stateless endpoint
                default:
                    throw new WireRequestException($"The input item type '{type}' is not supported.", $"{path}.type");
            }

            index++;
        }
    }

    private static ChatMessage ParseMessage(JsonElement item, string path)
    {
        var role = GetString(item, "role") ?? throw new WireRequestException("A message needs a 'role'.", $"{path}.role");
        var chatRole = role switch
        {
            "user" => ChatRole.User,
            "assistant" => ChatRole.Assistant,
            "system" or "developer" => ChatRole.System,
            _ => throw new WireRequestException($"The role '{role}' is not supported.", $"{path}.role"),
        };

        var contents = new List<AIContent>();
        if (item.TryGetProperty("content", out var content))
        {
            if (content.ValueKind == JsonValueKind.String)
            {
                contents.Add(new TextContent(content.GetString()));
            }
            else if (content.ValueKind == JsonValueKind.Array)
            {
                var partIndex = 0;
                foreach (var part in content.EnumerateArray())
                {
                    var partPath = $"{path}.content[{partIndex}]";
                    switch (GetString(part, "type"))
                    {
                        case "input_text" or "output_text" or "text":
                            contents.Add(new TextContent(GetString(part, "text") ?? string.Empty));
                            break;
                        case "refusal":
                            contents.Add(new TextContent(GetString(part, "refusal") ?? string.Empty));
                            break;
                        case "input_image":
                            contents.Add(ParseImage(part, partPath));
                            break;
                        case var other:
                            throw new WireRequestException($"The content part type '{other}' is not supported.", $"{partPath}.type");
                    }

                    partIndex++;
                }
            }
            else if (content.ValueKind != JsonValueKind.Null)
            {
                throw new WireRequestException("'content' must be a string or an array of parts.", $"{path}.content");
            }
        }

        return new ChatMessage(chatRole, contents);
    }

    private static AIContent ParseImage(JsonElement part, string path)
    {
        var url = GetString(part, "image_url");
        if (url is not { Length: > 0 })
        {
            throw new WireRequestException("An input_image part needs 'image_url'.", $"{path}.image_url");
        }

        try
        {
            return url.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ? new DataContent(url) : new UriContent(url, "image/*");
        }
        catch (Exception ex) when (ex is ArgumentException or UriFormatException or FormatException)
        {
            throw new WireRequestException("The image URL is not a valid data URI or URL.", $"{path}.image_url");
        }
    }

    private static List<AITool> ParseTools(JsonElement tools)
    {
        var result = new List<AITool>();
        var index = 0;
        foreach (var tool in tools.EnumerateArray())
        {
            if (GetString(tool, "type") != "function" || GetString(tool, "name") is not { Length: > 0 } name)
            {
                throw new WireRequestException("Only tools of type 'function' with a 'name' are supported; built-in tools are not served.", $"tools[{index}]");
            }

            var schema = tool.TryGetProperty("parameters", out var parameters) && parameters.ValueKind == JsonValueKind.Object
                ? parameters.Clone()
                : JsonDocument.Parse("""{"type":"object","properties":{}}""").RootElement.Clone();
            result.Add(AIFunctionFactory.CreateDeclaration(name, GetString(tool, "description"), schema));
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
            case JsonValueKind.Object when GetString(choice, "type") == "function" && GetString(choice, "name") is { Length: > 0 } name:
                return ChatToolMode.RequireSpecific(name);
            case JsonValueKind.Null:
                return null;
            default:
                throw new WireRequestException("'tool_choice' must be a string or a function selector.", "tool_choice");
        }
    }

    private static ChatResponseFormatJson? ParseFormat(JsonElement format)
    {
        switch (GetString(format, "type"))
        {
            case "text" or null:
                return null;
            case "json_object":
                return ChatResponseFormat.Json;
            case "json_schema":
                if (!format.TryGetProperty("schema", out var schema) || schema.ValueKind != JsonValueKind.Object)
                {
                    throw new WireRequestException("A json_schema format needs 'schema'.", "text.format.schema");
                }

                return ChatResponseFormat.ForJsonSchema(schema.Clone(), GetString(format, "name"), GetString(format, "description"));
            default:
                throw new WireRequestException("The text.format type is not supported.", "text.format.type");
        }
    }

    // ---- response ------------------------------------------------------------------------------------------------

    private sealed record Item(string Id, FunctionCallContent? Call, string Text);

    public async Task WriteResponseAsync(HttpContext context, WireRequest request, ChatResponse response, CancellationToken cancellationToken)
    {
        var items = new List<Item>();
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
                        items.Add(new Item("msg_" + Guid.NewGuid().ToString("N"), null, text.ToString()));
                        text.Clear();
                    }

                    items.Add(new Item("fc_" + Guid.NewGuid().ToString("N"), call, string.Empty));
                    break;
            }
        }

        if (text.Length > 0 || items.Count == 0)
        {
            items.Add(new Item("msg_" + Guid.NewGuid().ToString("N"), null, text.ToString()));
        }

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "application/json";
        await using var writer = new Utf8JsonWriter(context.Response.Body);
        WriteResponseObject(writer, "resp_" + Guid.NewGuid().ToString("N"), (response.CreatedAt ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds(),
            response.ModelId ?? request.Model, items, response.FinishReason, response.Usage, inProgress: false);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void WriteResponseObject(
        Utf8JsonWriter writer, string id, long createdAt, string? model, IReadOnlyList<Item> items, ChatFinishReason? finish, UsageDetails? usage, bool inProgress)
    {
        var incomplete = !inProgress && (finish == ChatFinishReason.Length || finish == ChatFinishReason.ContentFilter);
        writer.WriteStartObject();
        writer.WriteString("id", id);
        writer.WriteString("object", "response");
        writer.WriteNumber("created_at", createdAt);
        writer.WriteString("status", inProgress ? "in_progress" : incomplete ? "incomplete" : "completed");
        writer.WriteString("model", model ?? string.Empty);
        writer.WriteStartArray("output");
        if (!inProgress)
        {
            foreach (var item in items)
            {
                WriteItem(writer, item, completed: true);
            }
        }

        writer.WriteEndArray();
        writer.WriteNull("error");
        if (incomplete)
        {
            writer.WriteStartObject("incomplete_details");
            writer.WriteString("reason", finish == ChatFinishReason.Length ? "max_output_tokens" : "content_filter");
            writer.WriteEndObject();
        }
        else
        {
            writer.WriteNull("incomplete_details");
        }

        writer.WriteBoolean("parallel_tool_calls", true);
        writer.WriteBoolean("store", false);
        writer.WriteStartArray("tools");
        writer.WriteEndArray();
        writer.WriteString("tool_choice", "auto");
        writer.WriteStartObject("metadata");
        writer.WriteEndObject();
        if (!inProgress && usage is not null)
        {
            var input = usage.InputTokenCount ?? 0;
            var output = usage.OutputTokenCount ?? 0;
            writer.WriteStartObject("usage");
            writer.WriteNumber("input_tokens", input);
            writer.WriteStartObject("input_tokens_details");
            writer.WriteNumber("cached_tokens", usage.CachedInputTokenCount ?? 0);
            writer.WriteEndObject();
            writer.WriteNumber("output_tokens", output);
            writer.WriteStartObject("output_tokens_details");
            writer.WriteNumber("reasoning_tokens", usage.ReasoningTokenCount ?? 0);
            writer.WriteEndObject();
            writer.WriteNumber("total_tokens", usage.TotalTokenCount ?? input + output);
            writer.WriteEndObject();
        }

        writer.WriteEndObject();
    }

    private static void WriteItem(Utf8JsonWriter writer, Item item, bool completed)
    {
        writer.WriteStartObject();
        writer.WriteString("id", item.Id);
        if (item.Call is { } call)
        {
            writer.WriteString("type", "function_call");
            writer.WriteString("status", completed ? "completed" : "in_progress");
            writer.WriteString("call_id", call.CallId is { Length: > 0 } callId ? callId : "call_" + Guid.NewGuid().ToString("N"));
            writer.WriteString("name", call.Name);
            writer.WriteString("arguments", completed ? Arguments(call) : string.Empty);
        }
        else
        {
            writer.WriteString("type", "message");
            writer.WriteString("status", completed ? "completed" : "in_progress");
            writer.WriteString("role", "assistant");
            writer.WriteStartArray("content");
            if (completed)
            {
                WriteTextPart(writer, item.Text);
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }

    private static void WriteTextPart(Utf8JsonWriter writer, string text)
    {
        writer.WriteStartObject();
        writer.WriteString("type", "output_text");
        writer.WriteString("text", text);
        writer.WriteStartArray("annotations");
        writer.WriteEndArray();
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

        var id = "resp_" + Guid.NewGuid().ToString("N");
        var createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var model = request.Model;
        var sequence = 0;
        var started = false;
        var done = new List<Item>();
        ChatFinishReason? finish = null;
        UsageDetails? usage = null;

        // Open text item: the message being written, its output index and the text so far.
        string? textId = null;
        var textIndex = -1;
        var text = new StringBuilder();
        var outputIndex = 0;

        Task Event(string name, Action<Utf8JsonWriter> fields) => SendAsync(response, name, w =>
        {
            w.WriteStartObject();
            w.WriteString("type", name);
            w.WriteNumber("sequence_number", sequence++);
            fields(w);
            w.WriteEndObject();
        }, cancellationToken);

        async Task StartAsync()
        {
            started = true;
            await Event("response.created", w =>
            {
                w.WritePropertyName("response");
                WriteResponseObject(w, id, createdAt, model, [], null, null, inProgress: true);
            }).ConfigureAwait(false);
            await Event("response.in_progress", w =>
            {
                w.WritePropertyName("response");
                WriteResponseObject(w, id, createdAt, model, [], null, null, inProgress: true);
            }).ConfigureAwait(false);
        }

        async Task CloseTextAsync()
        {
            if (textId is null)
            {
                return;
            }

            var item = new Item(textId, null, text.ToString());
            var at = textIndex;
            var final = item.Text;
            await Event("response.output_text.done", w =>
            {
                w.WriteString("item_id", item.Id);
                w.WriteNumber("output_index", at);
                w.WriteNumber("content_index", 0);
                w.WriteString("text", final);
            }).ConfigureAwait(false);
            await Event("response.content_part.done", w =>
            {
                w.WriteString("item_id", item.Id);
                w.WriteNumber("output_index", at);
                w.WriteNumber("content_index", 0);
                w.WritePropertyName("part");
                WriteTextPart(w, final);
            }).ConfigureAwait(false);
            await Event("response.output_item.done", w =>
            {
                w.WriteNumber("output_index", at);
                w.WritePropertyName("item");
                WriteItem(w, item, completed: true);
            }).ConfigureAwait(false);
            done.Add(item);
            textId = null;
            text.Clear();
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
                    case TextContent { Text.Length: > 0 } piece:
                    {
                        if (textId is null)
                        {
                            textId = "msg_" + Guid.NewGuid().ToString("N");
                            textIndex = outputIndex++;
                            var opened = new Item(textId, null, string.Empty);
                            var at = textIndex;
                            await Event("response.output_item.added", w =>
                            {
                                w.WriteNumber("output_index", at);
                                w.WritePropertyName("item");
                                WriteItem(w, opened, completed: false);
                            }).ConfigureAwait(false);
                            await Event("response.content_part.added", w =>
                            {
                                w.WriteString("item_id", opened.Id);
                                w.WriteNumber("output_index", at);
                                w.WriteNumber("content_index", 0);
                                w.WritePropertyName("part");
                                WriteTextPart(w, string.Empty);
                            }).ConfigureAwait(false);
                        }

                        text.Append(piece.Text);
                        var itemId = textId;
                        var index = textIndex;
                        await Event("response.output_text.delta", w =>
                        {
                            w.WriteString("item_id", itemId);
                            w.WriteNumber("output_index", index);
                            w.WriteNumber("content_index", 0);
                            w.WriteString("delta", piece.Text);
                        }).ConfigureAwait(false);
                        break;
                    }
                    case FunctionCallContent call:
                    {
                        await CloseTextAsync().ConfigureAwait(false);
                        var item = new Item("fc_" + Guid.NewGuid().ToString("N"), call, string.Empty);
                        var at = outputIndex++;
                        await Event("response.output_item.added", w =>
                        {
                            w.WriteNumber("output_index", at);
                            w.WritePropertyName("item");
                            WriteItem(w, item, completed: false);
                        }).ConfigureAwait(false);
                        await Event("response.function_call_arguments.delta", w =>
                        {
                            w.WriteString("item_id", item.Id);
                            w.WriteNumber("output_index", at);
                            w.WriteString("delta", Arguments(call));
                        }).ConfigureAwait(false);
                        await Event("response.function_call_arguments.done", w =>
                        {
                            w.WriteString("item_id", item.Id);
                            w.WriteNumber("output_index", at);
                            w.WriteString("arguments", Arguments(call));
                        }).ConfigureAwait(false);
                        await Event("response.output_item.done", w =>
                        {
                            w.WriteNumber("output_index", at);
                            w.WritePropertyName("item");
                            WriteItem(w, item, completed: true);
                        }).ConfigureAwait(false);
                        done.Add(item);
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
        var incomplete = finish == ChatFinishReason.Length || finish == ChatFinishReason.ContentFilter;
        await Event(incomplete ? "response.incomplete" : "response.completed", w =>
        {
            w.WritePropertyName("response");
            WriteResponseObject(w, id, createdAt, model, done, finish, usage, inProgress: false);
        }).ConfigureAwait(false);
    }

    public Task WriteStreamErrorAsync(HttpContext context, WireError error, CancellationToken cancellationToken) =>
        SendAsync(context.Response, "error", w =>
        {
            w.WriteStartObject();
            w.WriteString("type", "error");
            w.WriteString("code", OpenAIChatCompletionsCodec.ErrorCode(error));
            w.WriteString("message", error.Message);
            if (error.Param is null) w.WriteNull("param"); else w.WriteString("param", error.Param);
            w.WriteEndObject();
        }, cancellationToken);

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

    // ---- errors (the Responses API uses the same error object as Chat Completions) ---------------------------------

    public Task WriteErrorAsync(HttpContext context, WireError error, CancellationToken cancellationToken) =>
        OpenAIChatCompletionsCodec.Instance.WriteErrorAsync(context, error, cancellationToken);
}
