using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;
using static IronHive.Providers.Endpoints.WireJson;

namespace IronHive.Providers.Endpoints;

/// <summary>
/// The Gemini wire (<c>POST models/{model}:generateContent</c> and <c>:streamGenerateContent</c>, the latter as server-sent events).
/// One instance per route: the model comes from the route and whether it streams is the route's, not the body's.
/// </summary>
internal sealed class GeminiGenerateContentCodec(bool streaming) : IWireCodec
{
    public static readonly GeminiGenerateContentCodec Unary = new(streaming: false);
    public static readonly GeminiGenerateContentCodec Streaming = new(streaming: true);

    public string Name => streaming ? "gemini-stream-generate-content" : "gemini-generate-content";

    // ---- request -------------------------------------------------------------------------------------------------

    public WireRequest Parse(JsonElement body, HttpContext context)
    {
        if (body.ValueKind != JsonValueKind.Object)
        {
            throw new WireRequestException("The request body must be a JSON object.");
        }

        if (!body.TryGetProperty("contents", out var contentsElement) || contentsElement.ValueKind != JsonValueKind.Array
            || contentsElement.GetArrayLength() == 0)
        {
            throw new WireRequestException("'contents' is required and must be a non-empty array.", "contents");
        }

        var messages = new List<ChatMessage>();
        if (body.TryGetProperty("systemInstruction", out var system) && system.ValueKind == JsonValueKind.Object)
        {
            var text = string.Concat(PartsOf(system).Where(p => GetString(p, "text") is not null && !IsThought(p)).Select(p => GetString(p, "text")));
            if (text.Length > 0)
            {
                messages.Add(new ChatMessage(ChatRole.System, text));
            }
        }

        var index = 0;
        foreach (var content in contentsElement.EnumerateArray())
        {
            messages.AddRange(ParseContent(content, $"contents[{index}]"));
            index++;
        }

        var model = context.Request.RouteValues.TryGetValue("model", out var routed) ? routed?.ToString() : null;
        var options = new ChatOptions { ModelId = model };
        if (body.TryGetProperty("generationConfig", out var config) && config.ValueKind == JsonValueKind.Object)
        {
            ApplyGenerationConfig(config, options);
        }

        if (body.TryGetProperty("tools", out var tools) && tools.ValueKind == JsonValueKind.Array)
        {
            options.Tools = ParseTools(tools);
        }

        if (body.TryGetProperty("toolConfig", out var toolConfig) && toolConfig.ValueKind == JsonValueKind.Object
            && toolConfig.TryGetProperty("functionCallingConfig", out var calling) && calling.ValueKind == JsonValueKind.Object)
        {
            options.ToolMode = ParseToolMode(calling);
        }

        return new WireRequest(model, messages, options, streaming, IncludeUsage: true);
    }

    private static void ApplyGenerationConfig(JsonElement config, ChatOptions options)
    {
        if (GetInt(config, "candidateCount") is { } candidates && candidates != 1)
        {
            throw new WireRequestException("Only one candidate per request is supported (candidateCount must be 1).", "generationConfig.candidateCount");
        }

        options.Temperature = GetFloat(config, "temperature");
        options.TopP = GetFloat(config, "topP");
        options.TopK = GetInt(config, "topK");
        options.MaxOutputTokens = GetInt(config, "maxOutputTokens");
        options.FrequencyPenalty = GetFloat(config, "frequencyPenalty");
        options.PresencePenalty = GetFloat(config, "presencePenalty");
        if (config.TryGetProperty("seed", out var seed) && seed.ValueKind == JsonValueKind.Number && seed.TryGetInt64(out var seedValue))
        {
            options.Seed = seedValue;
        }

        if (config.TryGetProperty("stopSequences", out var stops) && stops.ValueKind == JsonValueKind.Array)
        {
            options.StopSequences = [.. stops.EnumerateArray().Where(s => s.ValueKind == JsonValueKind.String).Select(s => s.GetString()!)];
        }

        if (GetString(config, "responseMimeType") == "application/json")
        {
            var schema = config.TryGetProperty("responseJsonSchema", out var jsonSchema) && jsonSchema.ValueKind == JsonValueKind.Object
                ? jsonSchema.Clone()
                : config.TryGetProperty("responseSchema", out var responseSchema) && responseSchema.ValueKind == JsonValueKind.Object
                    ? LowerCaseTypes(responseSchema)
                    : (JsonElement?)null;
            options.ResponseFormat = schema is { } s ? ChatResponseFormat.ForJsonSchema(s) : ChatResponseFormat.Json;
        }
    }

    private static List<JsonElement> PartsOf(JsonElement content) =>
        content.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array ? [.. parts.EnumerateArray()] : [];

    private static bool IsThought(JsonElement part) => part.TryGetProperty("thought", out var thought) && thought.ValueKind == JsonValueKind.True;

    private static IEnumerable<ChatMessage> ParseContent(JsonElement content, string path)
    {
        if (content.ValueKind != JsonValueKind.Object)
        {
            throw new WireRequestException("A content entry must be a JSON object.", path);
        }

        var role = GetString(content, "role") ?? "user";
        if (role is not ("user" or "model"))
        {
            throw new WireRequestException("A content role must be 'user' or 'model'.", $"{path}.role");
        }

        var chatRole = role == "user" ? ChatRole.User : ChatRole.Assistant;
        var results = new List<AIContent>();
        var rest = new List<AIContent>();
        var partIndex = 0;
        foreach (var part in PartsOf(content))
        {
            var partPath = $"{path}.parts[{partIndex}]";
            if (IsThought(part))
            {
                partIndex++;
                continue; // thought summaries are the model's own and are not carried back
            }

            if (GetString(part, "text") is { } text)
            {
                rest.Add(new TextContent(text));
            }
            else if (part.TryGetProperty("inlineData", out var inline) && inline.ValueKind == JsonValueKind.Object)
            {
                rest.Add(ParseInline(inline, partPath));
            }
            else if (part.TryGetProperty("fileData", out var file) && file.ValueKind == JsonValueKind.Object
                && GetString(file, "fileUri") is { Length: > 0 } uri)
            {
                try
                {
                    rest.Add(new UriContent(uri, GetString(file, "mimeType") ?? "application/octet-stream"));
                }
                catch (Exception ex) when (ex is ArgumentException or UriFormatException)
                {
                    throw new WireRequestException("The fileData URI is not valid.", $"{partPath}.fileData.fileUri");
                }
            }
            else if (part.TryGetProperty("functionCall", out var call) && call.ValueKind == JsonValueKind.Object && chatRole == ChatRole.Assistant)
            {
                rest.Add(ParseFunctionCall(call, partPath));
            }
            else if (part.TryGetProperty("functionResponse", out var response) && response.ValueKind == JsonValueKind.Object && chatRole == ChatRole.User)
            {
                results.Add(ParseFunctionResponse(response, partPath));
            }
            else
            {
                throw new WireRequestException("The part is not supported here (text, inlineData, fileData, functionCall in a model turn, functionResponse in a user turn).", partPath);
            }

            partIndex++;
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

    private static DataContent ParseInline(JsonElement inline, string path)
    {
        try
        {
            return new DataContent(
                Convert.FromBase64String(GetString(inline, "data") ?? string.Empty),
                GetString(inline, "mimeType") ?? throw new WireRequestException("inlineData needs 'mimeType'.", $"{path}.inlineData.mimeType"));
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            throw new WireRequestException("The inlineData is not valid base64.", $"{path}.inlineData.data");
        }
    }

    // Gemini pairs a result with its call by name (the id is optional); where there is no id the name stands in for it on both sides.
    private static FunctionCallContent ParseFunctionCall(JsonElement call, string path)
    {
        var name = GetString(call, "name") ?? throw new WireRequestException("A functionCall needs a 'name'.", $"{path}.functionCall.name");
        IDictionary<string, object?>? arguments = null;
        if (call.TryGetProperty("args", out var args) && args.ValueKind == JsonValueKind.Object)
        {
            arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(args.GetRawText(), AIJsonUtilities.DefaultOptions);
        }

        return new FunctionCallContent(GetString(call, "id") ?? name, name, arguments);
    }

    private static FunctionResultContent ParseFunctionResponse(JsonElement response, string path)
    {
        var name = GetString(response, "name") ?? throw new WireRequestException("A functionResponse needs a 'name'.", $"{path}.functionResponse.name");
        object? result = response.TryGetProperty("response", out var payload) ? payload.Clone() : null;
        return new FunctionResultContent(GetString(response, "id") ?? name, result);
    }

    private static List<AITool> ParseTools(JsonElement tools)
    {
        var result = new List<AITool>();
        var index = 0;
        foreach (var tool in tools.EnumerateArray())
        {
            if (!tool.TryGetProperty("functionDeclarations", out var declarations) || declarations.ValueKind != JsonValueKind.Array)
            {
                throw new WireRequestException("Only tools with 'functionDeclarations' are supported; built-in tools are not served.", $"tools[{index}]");
            }

            foreach (var declaration in declarations.EnumerateArray())
            {
                var name = GetString(declaration, "name") ?? throw new WireRequestException("A function declaration needs a 'name'.", $"tools[{index}].functionDeclarations");
                var schema = declaration.TryGetProperty("parametersJsonSchema", out var jsonSchema) && jsonSchema.ValueKind == JsonValueKind.Object
                    ? jsonSchema.Clone()
                    : declaration.TryGetProperty("parameters", out var parameters) && parameters.ValueKind == JsonValueKind.Object
                        ? LowerCaseTypes(parameters)
                        : JsonDocument.Parse("""{"type":"object","properties":{}}""").RootElement.Clone();
                result.Add(AIFunctionFactory.CreateDeclaration(name, GetString(declaration, "description"), schema));
            }

            index++;
        }

        return result;
    }

    private static ChatToolMode? ParseToolMode(JsonElement calling)
    {
        switch (GetString(calling, "mode"))
        {
            case null or "AUTO" or "MODE_UNSPECIFIED":
                return ChatToolMode.Auto;
            case "NONE":
                return ChatToolMode.None;
            case "ANY":
                var names = calling.TryGetProperty("allowedFunctionNames", out var allowed) && allowed.ValueKind == JsonValueKind.Array
                    ? allowed.EnumerateArray().Where(n => n.ValueKind == JsonValueKind.String).Select(n => n.GetString()!).ToList()
                    : [];
                return names.Count == 1 ? ChatToolMode.RequireSpecific(names[0]) : ChatToolMode.RequireAny;
            case var other:
                throw new WireRequestException($"The function calling mode '{other}' is not supported.", "toolConfig.functionCallingConfig.mode");
        }
    }

    // Gemini's Schema spells types in capitals (OBJECT, STRING); the chat clients behind this endpoint speak JSON Schema.
    private static JsonElement LowerCaseTypes(JsonElement schema)
    {
        var node = JsonNode.Parse(schema.GetRawText())!;
        Lower(node);
        return JsonDocument.Parse(node.ToJsonString()).RootElement.Clone();

        static void Lower(JsonNode? current)
        {
            switch (current)
            {
                case JsonObject obj:
                    foreach (var property in obj.ToList())
                    {
                        if (property.Key == "type" && property.Value is JsonValue value && value.TryGetValue<string>(out var type))
                        {
                            obj[property.Key] = type.ToLowerInvariant();
                        }
                        else
                        {
                            Lower(property.Value);
                        }
                    }

                    break;
                case JsonArray array:
                    foreach (var item in array)
                    {
                        Lower(item);
                    }

                    break;
            }
        }
    }

    // ---- response ------------------------------------------------------------------------------------------------

    public async Task WriteResponseAsync(HttpContext context, WireRequest request, ChatResponse response, CancellationToken cancellationToken)
    {
        var parts = new List<AIContent>();
        foreach (var content in response.Messages.Where(m => m.Role == ChatRole.Assistant).SelectMany(m => m.Contents))
        {
            if (content is TextContent or FunctionCallContent)
            {
                parts.Add(content);
            }
        }

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "application/json";
        await using var writer = new Utf8JsonWriter(context.Response.Body);
        WriteCandidate(writer, parts, FinishReason(response.FinishReason), response.Usage, response.ModelId ?? request.Model, response.ResponseId);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void WriteCandidate(Utf8JsonWriter writer, List<AIContent> parts, string? finishReason, UsageDetails? usage, string? model, string? responseId)
    {
        writer.WriteStartObject();
        writer.WriteStartArray("candidates");
        writer.WriteStartObject();
        writer.WriteStartObject("content");
        writer.WriteString("role", "model");
        writer.WriteStartArray("parts");
        foreach (var part in parts)
        {
            writer.WriteStartObject();
            if (part is FunctionCallContent call)
            {
                writer.WriteStartObject("functionCall");
                writer.WriteString("name", call.Name);
                writer.WritePropertyName("args");
                writer.WriteRawValue(Arguments(call));
                if (!string.Equals(call.CallId, call.Name, StringComparison.Ordinal) && call.CallId is { Length: > 0 } id)
                {
                    writer.WriteString("id", id);
                }

                writer.WriteEndObject();
            }
            else
            {
                writer.WriteString("text", ((TextContent)part).Text);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        if (finishReason is not null)
        {
            writer.WriteString("finishReason", finishReason);
        }

        writer.WriteNumber("index", 0);
        writer.WriteEndObject();
        writer.WriteEndArray();
        if (usage is not null)
        {
            var input = usage.InputTokenCount ?? 0;
            var output = usage.OutputTokenCount ?? 0;
            writer.WriteStartObject("usageMetadata");
            writer.WriteNumber("promptTokenCount", input);
            writer.WriteNumber("candidatesTokenCount", output);
            writer.WriteNumber("totalTokenCount", usage.TotalTokenCount ?? input + output);
            if (usage.CachedInputTokenCount is { } cached) writer.WriteNumber("cachedContentTokenCount", cached);
            if (usage.ReasoningTokenCount is { } reasoning) writer.WriteNumber("thoughtsTokenCount", reasoning);
            writer.WriteEndObject();
        }

        if (model is { Length: > 0 }) writer.WriteString("modelVersion", model);
        if (responseId is { Length: > 0 }) writer.WriteString("responseId", responseId);
        writer.WriteEndObject();
    }

    internal static string FinishReason(ChatFinishReason? reason)
    {
        if (reason == ChatFinishReason.Length) return "MAX_TOKENS";
        if (reason == ChatFinishReason.ContentFilter) return "SAFETY";
        return "STOP"; // a reply with function calls also finishes as STOP
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

        var model = request.Model;
        var responseId = "resp_" + Guid.NewGuid().ToString("N");
        ChatFinishReason? finish = null;
        UsageDetails? usage = null;

        await foreach (var update in updates.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (update.ModelId is { Length: > 0 } updated) model = updated;
            if (update.FinishReason is { } reason) finish = reason;
            var parts = new List<AIContent>();
            foreach (var content in update.Contents)
            {
                switch (content)
                {
                    case TextContent { Text.Length: > 0 } or FunctionCallContent:
                        parts.Add(content);
                        break;
                    case UsageContent used:
                        if (usage is null) usage = used.Details; else usage.Add(used.Details);
                        break;
                }
            }

            if (parts.Count > 0)
            {
                await SendAsync(response, w => WriteCandidate(w, parts, null, null, model, responseId), cancellationToken).ConfigureAwait(false);
            }
        }

        // The closing chunk carries the finish reason and the usage, as Gemini's own does (with an empty text part).
        await SendAsync(response, w => WriteCandidate(w, [new TextContent(string.Empty)], FinishReason(finish), usage, model, responseId), cancellationToken).ConfigureAwait(false);
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
        await response.WriteAsync("\r\n\r\n", cancellationToken).ConfigureAwait(false);
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
        var status = error.Kind switch
        {
            WireErrorKind.InvalidRequest or WireErrorKind.PayloadTooLarge => "INVALID_ARGUMENT",
            WireErrorKind.NotFound => "NOT_FOUND",
            WireErrorKind.RateLimited => "RESOURCE_EXHAUSTED",
            _ => "INTERNAL",
        };
        writer.WriteStartObject();
        writer.WriteStartObject("error");
        writer.WriteNumber("code", error.Status);
        writer.WriteString("message", error.Param is null ? error.Message : $"{error.Param}: {error.Message}");
        writer.WriteString("status", status);
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
