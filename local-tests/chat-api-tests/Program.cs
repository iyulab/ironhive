// =============================================================================
// IronHive Chat API Tests
// =============================================================================
// Tests chat functionality across LLM providers: basic generation, streaming,
// token usage, done reasons. Drives SDK feature verification.
// NOT for CI - requires .env file with actual API keys.
// =============================================================================

using System.Text;
using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;
using IronHive.Providers.OpenAI;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.Ollama;

// Parse command line arguments
string? targetProvider = null;
for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--provider" && i + 1 < args.Length)
    {
        targetProvider = args[i + 1].ToLowerInvariant();
        break;
    }
}

// Define all provider tests
var providerTests = new Dictionary<string, Func<Task<List<(string Scenario, TestResult Result)>>>>
{
    ["openai"] = TestOpenAI,
    ["azure-openai"] = TestAzureOpenAI,
    ["anthropic"] = TestAnthropic,
    ["google"] = TestGoogleAI,
    ["xai"] = TestXAI,
    ["ollama"] = TestOllama,
    ["lmstudio"] = TestLMStudio,
    ["gpustack"] = TestGPUStack,
};

// Available providers list (for -List flag)
if (args.Contains("-List") || args.Contains("--list"))
{
    Console.WriteLine("\nAvailable providers:");
    foreach (var key in providerTests.Keys)
        Console.WriteLine($"  - {key}");
    Console.WriteLine();
    return 0;
}

// Filter providers if specified
if (!string.IsNullOrEmpty(targetProvider))
{
    if (!providerTests.ContainsKey(targetProvider))
    {
        Console.WriteLine($"[ERROR] Unknown provider: {targetProvider}");
        Console.WriteLine($"Available: {string.Join(", ", providerTests.Keys)}");
        return 1;
    }
    providerTests = new Dictionary<string, Func<Task<List<(string, TestResult)>>>>
    {
        [targetProvider] = providerTests[targetProvider]
    };
}

// Run tests
var results = new List<(string Provider, string Scenario, TestResult Result)>();
foreach (var (provider, testFunc) in providerTests)
{
    Console.WriteLine($"\n{"",2}[{provider}]");
    try
    {
        var scenarioResults = await testFunc();
        foreach (var (scenario, result) in scenarioResults)
        {
            results.Add((provider, scenario, result));
            PrintResult(scenario, result);
        }
    }
    catch (Exception ex)
    {
        var result = TestResult.Error(ex.Message);
        results.Add((provider, "setup", result));
        PrintResult("setup", result);
    }
}

// Summary
Console.WriteLine("\n" + new string('=', 60));
Console.WriteLine("SUMMARY");
Console.WriteLine(new string('=', 60));

var passed = results.Count(r => r.Result.Success);
var skipped = results.Count(r => r.Result.Skipped);
var failed = results.Count(r => !r.Result.Success && !r.Result.Skipped);

Console.WriteLine($"  Passed:  {passed}");
Console.WriteLine($"  Skipped: {skipped}");
Console.WriteLine($"  Failed:  {failed}");
Console.WriteLine($"  Total:   {results.Count}");

if (failed > 0)
{
    Console.WriteLine("\nFailed tests:");
    foreach (var (provider, scenario, result) in results.Where(r => !r.Result.Success && !r.Result.Skipped))
        Console.WriteLine($"  - {provider}/{scenario}: {result.Message}");
}

return failed > 0 ? 1 : 0;

// =============================================================================
// Shared Test Scenarios
// =============================================================================

async Task<List<(string, TestResult)>> RunScenarios(
    IModelCatalog? catalog,
    IMessageGenerator generator,
    string model)
{
    var results = new List<(string, TestResult)>();

    // Print model catalog info if available
    if (catalog != null)
    {
        try
        {
            var models = await catalog.ListModelsAsync();
            Console.WriteLine($"{"",4}models available: {models.Count()}");
        }
        catch { /* ignore catalog errors */ }
    }

    // Scenario 1: Basic generation
    try
    {
        results.Add(("basic", await TestBasic(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("basic", TestResult.Error(ex.Message)));
    }

    // Scenario 2: Streaming generation
    try
    {
        results.Add(("streaming", await TestStreaming(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("streaming", TestResult.Error(ex.Message)));
    }

    // Scenario 3: Tool calling
    try
    {
        results.Add(("tool-call", await TestToolCalling(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("tool-call", TestResult.Error(ex.Message)));
    }

    // Scenario 4: Tool round-trip (call → execute → return result → final response)
    try
    {
        results.Add(("tool-roundtrip", await TestToolRoundTrip(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("tool-roundtrip", TestResult.Error(ex.Message)));
    }

    // Scenario 5: Multi-turn conversation (context retention across turns)
    try
    {
        results.Add(("multi-turn", await TestMultiTurn(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("multi-turn", TestResult.Error(ex.Message)));
    }

    // Scenario 6: Tool with parameters (JSON schema argument passing)
    try
    {
        results.Add(("tool-params", await TestToolWithParams(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("tool-params", TestResult.Error(ex.Message)));
    }

    // Scenario 7: Streaming tool call (tool call detection in streaming mode)
    try
    {
        results.Add(("stream-tool", await TestStreamingToolCall(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("stream-tool", TestResult.Error(ex.Message)));
    }

    // Scenario 8: Multiple tools (model selects correct tool from set)
    try
    {
        results.Add(("multi-tool", await TestMultipleTools(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("multi-tool", TestResult.Error(ex.Message)));
    }

    // Scenario 9: ThinkingEffort None (reasoning disabled)
    try
    {
        results.Add(("think-none", await TestThinkingEffortNone(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("think-none", TestResult.Error(ex.Message)));
    }

    // Scenario 10: MaxTokens (output truncation + DoneReason.MaxTokens)
    try
    {
        results.Add(("max-tokens", await TestMaxTokens(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("max-tokens", TestResult.Error(ex.Message)));
    }

    // Scenario 11: ThinkingEffort High (deep reasoning verification)
    try
    {
        results.Add(("think-high", await TestThinkingEffortHigh(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("think-high", TestResult.Error(ex.Message)));
    }

    // Scenario 12: Stop sequences (StopSequences parameter + DoneReason.StopSequence)
    try
    {
        results.Add(("stop-seq", await TestStopSequences(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("stop-seq", TestResult.Error(ex.Message)));
    }

    // Scenario 13: Streaming tool round-trip (stream → tool call → execute → stream continuation)
    try
    {
        results.Add(("stream-roundtrip", await TestStreamingRoundTrip(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("stream-roundtrip", TestResult.Error(ex.Message)));
    }

    // Scenario 14: Invalid model name (error handling)
    try
    {
        results.Add(("error-model", await TestErrorModel(generator)));
    }
    catch (Exception ex)
    {
        results.Add(("error-model", TestResult.Error(ex.Message)));
    }

    // Scenario 15: No system prompt (basic generation without SystemPrompt)
    try
    {
        results.Add(("no-system", await TestNoSystemPrompt(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("no-system", TestResult.Error(ex.Message)));
    }

    // Scenario 16: Temperature=0 (deterministic output)
    try
    {
        results.Add(("temperature", await TestTemperature(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("temperature", TestResult.Error(ex.Message)));
    }

    // Scenario 17: Image input (multimodal — image + text question)
    try
    {
        results.Add(("image-input", await TestImageInput(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("image-input", TestResult.Error(ex.Message)));
    }

    // Scenario 18: Image input streaming (multimodal streaming response)
    try
    {
        results.Add(("image-stream", await TestImageStream(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("image-stream", TestResult.Error(ex.Message)));
    }

    // Scenario 19: Parallel tool calls (multiple tools in single response)
    try
    {
        results.Add(("parallel-tools", await TestParallelToolCalls(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("parallel-tools", TestResult.Error(ex.Message)));
    }

    // Scenario 20: TopP parameter (nucleus sampling verification)
    try
    {
        results.Add(("top-p", await TestTopP(generator, model)));
    }
    catch (Exception ex)
    {
        results.Add(("top-p", TestResult.Error(ex.Message)));
    }

    return results;
}

async Task<TestResult> TestBasic(IMessageGenerator generator, string model)
{
    var request = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = "You are a helpful assistant. Respond briefly.",
        Messages =
        [
            new UserMessage
            {
                Content = [new TextMessageContent { Value = "Say 'Hello, IronHive!' and nothing else." }]
            }
        ],
        ThinkingEffort = MessageThinkingEffort.Low
    };

    var response = await generator.GenerateMessageAsync(request);

    // Verify DoneReason
    var doneReason = response.DoneReason;

    // Verify TokenUsage
    var tokens = response.TokenUsage;
    var tokenInfo = tokens != null
        ? $"in={tokens.InputTokens},out={tokens.OutputTokens}"
        : "no-usage";

    // Extract response content
    var thinkingContent = response.Message?.Content?.OfType<ThinkingMessageContent>().FirstOrDefault();
    var textContent = response.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault();
    var responseStr = Truncate(textContent?.Value, 100) ?? "(no response)";

    if (thinkingContent != null)
    {
        var thinkingText = Truncate(thinkingContent.Value, 80) ?? "(empty)";
        return TestResult.Pass(
            $"reason={doneReason}, {tokenInfo}, thinking={thinkingContent.Format}",
            model, thinkingText, responseStr);
    }

    return TestResult.Pass($"reason={doneReason}, {tokenInfo}", model, null, responseStr);
}

async Task<TestResult> TestStreaming(IMessageGenerator generator, string model)
{
    var request = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = "You are a helpful assistant. Respond briefly.",
        Messages =
        [
            new UserMessage
            {
                Content = [new TextMessageContent { Value = "Count from 1 to 5." }]
            }
        ]
    };

    var textBuilder = new StringBuilder();
    bool hasBegin = false, hasDone = false;
    int chunkCount = 0;
    int deltaCount = 0;
    MessageDoneReason? doneReason = null;
    MessageTokenUsage? tokenUsage = null;
    var eventTypes = new List<string>();

    await foreach (var chunk in generator.GenerateStreamingMessageAsync(request))
    {
        chunkCount++;
        switch (chunk)
        {
            case StreamingMessageBeginResponse:
                hasBegin = true;
                eventTypes.Add("Begin");
                break;
            case StreamingContentAddedResponse added:
                eventTypes.Add($"Added({added.Content.GetType().Name})");
                // Capture initial text from ContentAdded
                if (added.Content is TextMessageContent initialText)
                    textBuilder.Append(initialText.Value);
                break;
            case StreamingContentDeltaResponse delta:
                if (delta.Delta is TextDeltaContent textDelta)
                {
                    deltaCount++;
                    textBuilder.Append(textDelta.Value);
                }
                eventTypes.Add($"Delta({delta.Delta.GetType().Name})");
                break;
            case StreamingContentUpdatedResponse:
                eventTypes.Add("Updated");
                break;
            case StreamingContentCompletedResponse:
                eventTypes.Add("Completed");
                break;
            case StreamingMessageDoneResponse done:
                hasDone = true;
                doneReason = done.DoneReason;
                tokenUsage = done.TokenUsage;
                eventTypes.Add("Done");
                break;
            case StreamingMessageErrorResponse err:
                eventTypes.Add($"Error({err.Code})");
                break;
        }
    }

    var text = textBuilder.ToString();

    if (!hasBegin)
        return TestResult.Error($"Missing Begin. Events: {string.Join(",", eventTypes)}");
    if (!hasDone)
        return TestResult.Error($"Missing Done. Events: {string.Join(",", eventTypes)}");
    if (string.IsNullOrEmpty(text))
        return TestResult.Error($"No text content. Events: {string.Join(",", eventTypes)}");

    var tokenInfo = tokenUsage != null
        ? $"in={tokenUsage.InputTokens},out={tokenUsage.OutputTokens}"
        : "no-usage";

    return TestResult.Pass(
        $"{chunkCount} chunks ({deltaCount} text deltas), reason={doneReason}, {tokenInfo}",
        model, null, Truncate(text, 100));
}

async Task<TestResult> TestToolCalling(IMessageGenerator generator, string model)
{
    // Define a simple tool
    var tool = new FunctionTool(new Func<string>(() => DateTime.UtcNow.ToString("o")))
    {
        Name = "get_current_time",
        Description = "Returns the current UTC date and time",
        Parameters = null,
        RequiresApproval = false
    };
    var tools = new ToolCollection([tool]);

    var request = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = "You are a helpful assistant. Use the provided tools when appropriate.",
        Messages =
        [
            new UserMessage
            {
                Content = [new TextMessageContent { Value = "What is the current time? Use the tool to answer." }]
            }
        ],
        Tools = tools
    };

    var response = await generator.GenerateMessageAsync(request);

    var doneReason = response.DoneReason;
    var toolCalls = response.Message?.Content?.OfType<ToolMessageContent>().ToList() ?? [];

    if (toolCalls.Count == 0)
    {
        // Model didn't call the tool — report what it did instead
        var textContent = response.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault();
        var fallback = Truncate(textContent?.Value, 80) ?? "(no content)";
        return TestResult.Error($"No tool call produced (reason={doneReason}). Response: {fallback}");
    }

    var firstCall = toolCalls[0];
    return TestResult.Pass(
        $"reason={doneReason}, tools={toolCalls.Count}, name={firstCall.Name}",
        model);
}

async Task<TestResult> TestToolRoundTrip(IMessageGenerator generator, string model)
{
    // Define a tool that returns a known value we can verify in the final response
    var secretValue = $"IRON-{DateTime.UtcNow:HHmmss}";
    var tool = new FunctionTool(new Func<string>(() => secretValue))
    {
        Name = "get_secret_code",
        Description = "Returns a secret code string",
        Parameters = null,
        RequiresApproval = false
    };
    var tools = new ToolCollection([tool]);

    // Step 1: Initial request — model should call the tool
    var messages = new List<Message>
    {
        new UserMessage
        {
            Content = [new TextMessageContent { Value = "Call the get_secret_code tool and tell me the exact code it returns. Reply with only the code, nothing else." }]
        }
    };

    var request = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = "You are a helpful assistant. Always use the provided tools when asked. After getting a tool result, respond with only the result value.",
        Messages = messages,
        Tools = tools
    };

    var response1 = await generator.GenerateMessageAsync(request);
    var toolCalls = response1.Message?.Content?.OfType<ToolMessageContent>().ToList() ?? [];

    if (toolCalls.Count == 0)
    {
        var text = response1.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault();
        return TestResult.Error($"Step 1 failed: no tool call (reason={response1.DoneReason}). Response: {Truncate(text?.Value, 80) ?? "(none)"}");
    }

    // Step 2: Execute the tool and populate Output
    foreach (var tc in toolCalls)
    {
        var input = new ToolInput(tc.Input);
        tc.Output = tools.TryGet(tc.Name, out var t)
            ? await t.InvokeAsync(input)
            : ToolOutput.Failure($"Tool '{tc.Name}' not found");
    }

    // Step 3: Build the continuation — add assistant message with tool results, then re-request
    messages.Add(response1.Message!);

    var request2 = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = request.SystemPrompt,
        Messages = messages,
        Tools = tools
    };

    var response2 = await generator.GenerateMessageAsync(request2);

    var finalText = response2.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "";
    var doneReason = response2.DoneReason;
    var hasSecret = finalText.Contains(secretValue);

    if (response2.DoneReason == MessageDoneReason.ToolCall)
    {
        // Model called another tool instead of responding — not necessarily wrong, but not what we expect
        var extraCalls = response2.Message?.Content?.OfType<ToolMessageContent>().ToList() ?? [];
        return TestResult.Error($"Step 2: model called tool again ({extraCalls.Count} calls) instead of text response");
    }

    if (string.IsNullOrWhiteSpace(finalText))
        return TestResult.Error($"Step 2: empty text response (reason={doneReason})");

    var verifyInfo = hasSecret ? "verified" : "not-found";
    return TestResult.Pass(
        $"reason={doneReason}, secret={verifyInfo}, tools={toolCalls.Count}",
        model, null, Truncate(finalText, 100));
}

async Task<TestResult> TestMultiTurn(IMessageGenerator generator, string model)
{
    var secretCode = $"HIVE-{DateTime.UtcNow:HHmmss}";

    // Turn 1: Establish context with a secret code
    var messages = new List<Message>
    {
        new UserMessage
        {
            Content = [new TextMessageContent { Value = $"Remember this secret code: {secretCode}. Just confirm you got it." }]
        }
    };

    var request1 = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = "You are a helpful assistant with perfect memory. Keep responses brief.",
        Messages = messages,
    };

    var response1 = await generator.GenerateMessageAsync(request1);
    var turn1Text = response1.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "";

    if (string.IsNullOrWhiteSpace(turn1Text))
        return TestResult.Error($"Turn 1: empty response (reason={response1.DoneReason})");

    // Add assistant response to conversation history
    messages.Add(response1.Message!);

    // Turn 2: Ask about the secret code (requires context from turn 1)
    messages.Add(new UserMessage
    {
        Content = [new TextMessageContent { Value = "What was the secret code I told you? Reply with only the code, nothing else." }]
    });

    var request2 = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = request1.SystemPrompt,
        Messages = messages,
    };

    var response2 = await generator.GenerateMessageAsync(request2);
    var turn2Text = response2.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "";

    if (string.IsNullOrWhiteSpace(turn2Text))
        return TestResult.Error($"Turn 2: empty response (reason={response2.DoneReason})");

    var hasSecret = turn2Text.Contains(secretCode);
    var verifyInfo = hasSecret ? "verified" : "not-found";

    return TestResult.Pass(
        $"reason={response2.DoneReason}, context={verifyInfo}, turns=2",
        model, null, Truncate(turn2Text, 100));
}

async Task<TestResult> TestToolWithParams(IMessageGenerator generator, string model)
{
    // Tool with parameters: add(a, b) → returns sum
    var tool = new FunctionTool(new Func<int, int, int>((a, b) => a + b))
    {
        Name = "add_numbers",
        Description = "Adds two integers and returns the sum",
        Parameters = new System.Text.Json.Nodes.JsonObject
        {
            ["type"] = "object",
            ["properties"] = new System.Text.Json.Nodes.JsonObject
            {
                ["a"] = new System.Text.Json.Nodes.JsonObject
                {
                    ["type"] = "integer",
                    ["description"] = "First number"
                },
                ["b"] = new System.Text.Json.Nodes.JsonObject
                {
                    ["type"] = "integer",
                    ["description"] = "Second number"
                }
            },
            ["required"] = new System.Text.Json.Nodes.JsonArray("a", "b")
        },
        RequiresApproval = false
    };
    var tools = new ToolCollection([tool]);

    var request = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = "You are a helpful assistant. Use tools when asked to calculate.",
        Messages =
        [
            new UserMessage
            {
                Content = [new TextMessageContent { Value = "What is 17 + 28? Use the add_numbers tool." }]
            }
        ],
        Tools = tools
    };

    var response = await generator.GenerateMessageAsync(request);
    var toolCalls = response.Message?.Content?.OfType<ToolMessageContent>().ToList() ?? [];

    if (toolCalls.Count == 0)
    {
        var text = response.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault();
        return TestResult.Error($"No tool call (reason={response.DoneReason}). Response: {Truncate(text?.Value, 80) ?? "(none)"}");
    }

    var tc = toolCalls[0];
    var hasInput = !string.IsNullOrWhiteSpace(tc.Input);
    var inputPreview = Truncate(tc.Input, 60) ?? "(null)";

    // Execute and verify
    var input = new ToolInput(tc.Input);
    var output = await tool.InvokeAsync(input);
    var resultValue = output.Result ?? "";
    var hasCorrectResult = resultValue.Contains("45");

    return TestResult.Pass(
        $"reason={response.DoneReason}, hasInput={hasInput}, result={resultValue}, correct={hasCorrectResult}",
        model, null, $"input={inputPreview}");
}

async Task<TestResult> TestStreamingToolCall(IMessageGenerator generator, string model)
{
    var tool = new FunctionTool(new Func<string>(() => "streaming-ok"))
    {
        Name = "check_status",
        Description = "Returns a status string",
        Parameters = null,
        RequiresApproval = false
    };
    var tools = new ToolCollection([tool]);

    var request = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = "You are a helpful assistant. Use the provided tools when asked.",
        Messages =
        [
            new UserMessage
            {
                Content = [new TextMessageContent { Value = "Call the check_status tool now." }]
            }
        ],
        Tools = tools
    };

    bool hasBegin = false, hasDone = false;
    bool hasToolAdded = false;
    int chunkCount = 0;
    MessageDoneReason? doneReason = null;
    var eventTypes = new List<string>();
    string? toolName = null;

    await foreach (var chunk in generator.GenerateStreamingMessageAsync(request))
    {
        chunkCount++;
        switch (chunk)
        {
            case StreamingMessageBeginResponse:
                hasBegin = true;
                eventTypes.Add("Begin");
                break;
            case StreamingContentAddedResponse added:
                eventTypes.Add($"Added({added.Content.GetType().Name})");
                if (added.Content is ToolMessageContent toolContent)
                {
                    hasToolAdded = true;
                    toolName = toolContent.Name;
                }
                break;
            case StreamingContentDeltaResponse delta:
                eventTypes.Add($"Delta({delta.Delta.GetType().Name})");
                break;
            case StreamingContentUpdatedResponse:
                eventTypes.Add("Updated");
                break;
            case StreamingContentCompletedResponse:
                eventTypes.Add("Completed");
                break;
            case StreamingMessageDoneResponse done:
                hasDone = true;
                doneReason = done.DoneReason;
                eventTypes.Add("Done");
                break;
            case StreamingMessageErrorResponse err:
                eventTypes.Add($"Error({err.Code})");
                break;
        }
    }

    if (!hasBegin)
        return TestResult.Error($"Missing Begin. Events: {string.Join(",", eventTypes)}");
    if (!hasDone)
        return TestResult.Error($"Missing Done. Events: {string.Join(",", eventTypes)}");
    if (!hasToolAdded)
        return TestResult.Error($"No tool ContentAdded. Events: {string.Join(",", eventTypes)}");

    return TestResult.Pass(
        $"{chunkCount} chunks, reason={doneReason}, tool={toolName}",
        model, null, $"events: {string.Join(",", eventTypes.Distinct())}");
}

async Task<TestResult> TestMultipleTools(IMessageGenerator generator, string model)
{
    // Define 3 tools, ask model to pick the right one
    var toolWeather = new FunctionTool(new Func<string, string>((city) => $"Sunny, 25°C in {city}"))
    {
        Name = "get_weather",
        Description = "Returns the current weather for a city",
        Parameters = new System.Text.Json.Nodes.JsonObject
        {
            ["type"] = "object",
            ["properties"] = new System.Text.Json.Nodes.JsonObject
            {
                ["city"] = new System.Text.Json.Nodes.JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "City name"
                }
            },
            ["required"] = new System.Text.Json.Nodes.JsonArray("city")
        },
        RequiresApproval = false
    };

    var toolTime = new FunctionTool(new Func<string>(() => DateTime.UtcNow.ToString("o")))
    {
        Name = "get_time",
        Description = "Returns the current UTC time",
        Parameters = null,
        RequiresApproval = false
    };

    var toolCalc = new FunctionTool(new Func<int, int, int>((a, b) => a * b))
    {
        Name = "multiply",
        Description = "Multiplies two integers",
        Parameters = new System.Text.Json.Nodes.JsonObject
        {
            ["type"] = "object",
            ["properties"] = new System.Text.Json.Nodes.JsonObject
            {
                ["a"] = new System.Text.Json.Nodes.JsonObject { ["type"] = "integer" },
                ["b"] = new System.Text.Json.Nodes.JsonObject { ["type"] = "integer" }
            },
            ["required"] = new System.Text.Json.Nodes.JsonArray("a", "b")
        },
        RequiresApproval = false
    };

    var tools = new ToolCollection([toolWeather, toolTime, toolCalc]);

    var request = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = "You are a helpful assistant. Use the appropriate tool to answer.",
        Messages =
        [
            new UserMessage
            {
                Content = [new TextMessageContent { Value = "What's the weather in Seoul?" }]
            }
        ],
        Tools = tools
    };

    var response = await generator.GenerateMessageAsync(request);
    var toolCalls = response.Message?.Content?.OfType<ToolMessageContent>().ToList() ?? [];

    if (toolCalls.Count == 0)
    {
        var text = response.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault();
        return TestResult.Error($"No tool call (reason={response.DoneReason}). Response: {Truncate(text?.Value, 80) ?? "(none)"}");
    }

    var selectedTool = toolCalls[0].Name;
    var isCorrect = selectedTool == "func_get_weather";
    var inputPreview = Truncate(toolCalls[0].Input, 60) ?? "(null)";

    return TestResult.Pass(
        $"reason={response.DoneReason}, selected={selectedTool}, correct={isCorrect}, tools={toolCalls.Count}",
        model, null, $"input={inputPreview}");
}

async Task<TestResult> TestThinkingEffortNone(IMessageGenerator generator, string model)
{
    var request = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = "You are a helpful assistant. Respond briefly.",
        Messages =
        [
            new UserMessage
            {
                Content = [new TextMessageContent { Value = "Say 'No thinking!' and nothing else." }]
            }
        ],
        ThinkingEffort = MessageThinkingEffort.None
    };

    var response = await generator.GenerateMessageAsync(request);

    var thinkingContent = response.Message?.Content?.OfType<ThinkingMessageContent>().ToList() ?? [];
    var textContent = response.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault();
    var responseStr = Truncate(textContent?.Value, 100) ?? "(no response)";

    var tokens = response.TokenUsage;
    var tokenInfo = tokens != null
        ? $"in={tokens.InputTokens},out={tokens.OutputTokens}"
        : "no-usage";

    var hasThinking = thinkingContent.Count > 0;

    return TestResult.Pass(
        $"reason={response.DoneReason}, {tokenInfo}, thinking={hasThinking}, thinkingBlocks={thinkingContent.Count}",
        model, null, responseStr);
}

async Task<TestResult> TestMaxTokens(IMessageGenerator generator, string model)
{
    var request = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = "You are a helpful assistant.",
        Messages =
        [
            new UserMessage
            {
                Content = [new TextMessageContent { Value = "List the first 20 prime numbers, one per line." }]
            }
        ],
        MaxTokens = 20,
        ThinkingEffort = MessageThinkingEffort.None
    };

    var response = await generator.GenerateMessageAsync(request);

    var doneReason = response.DoneReason;
    var textContent = response.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault();
    var responseStr = Truncate(textContent?.Value, 100) ?? "(no response)";
    var tokens = response.TokenUsage;
    var tokenInfo = tokens != null
        ? $"in={tokens.InputTokens},out={tokens.OutputTokens}"
        : "no-usage";

    var isMaxTokens = doneReason == MessageDoneReason.MaxTokens;

    return TestResult.Pass(
        $"reason={doneReason}, maxTokensHit={isMaxTokens}, {tokenInfo}",
        model, null, responseStr);
}

async Task<TestResult> TestThinkingEffortHigh(IMessageGenerator generator, string model)
{
    var request = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = "You are a helpful assistant. Think step by step.",
        Messages =
        [
            new UserMessage
            {
                Content = [new TextMessageContent { Value = "What is 15 * 37? Think carefully before answering." }]
            }
        ],
        ThinkingEffort = MessageThinkingEffort.High
    };

    var response = await generator.GenerateMessageAsync(request);

    var thinkingContent = response.Message?.Content?.OfType<ThinkingMessageContent>().ToList() ?? [];
    var textContent = response.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault();
    var responseStr = Truncate(textContent?.Value, 100) ?? "(no response)";

    var tokens = response.TokenUsage;
    var tokenInfo = tokens != null
        ? $"in={tokens.InputTokens},out={tokens.OutputTokens}"
        : "no-usage";

    var hasThinking = thinkingContent.Count > 0;
    var thinkingLen = thinkingContent.Sum(t => t.Value?.Length ?? 0);
    var thinkingPreview = hasThinking ? Truncate(thinkingContent[0].Value, 80) : null;

    return TestResult.Pass(
        $"reason={response.DoneReason}, {tokenInfo}, thinking={hasThinking}, thinkingBlocks={thinkingContent.Count}, thinkingLen={thinkingLen}",
        model, thinkingPreview, responseStr);
}

async Task<TestResult> TestStopSequences(IMessageGenerator generator, string model)
{
    var request = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = "You are a helpful assistant. Always respond exactly as instructed.",
        Messages =
        [
            new UserMessage
            {
                Content = [new TextMessageContent { Value = "List these colors in order, separated by commas: red, blue, green, yellow, purple, orange" }]
            }
        ],
        StopSequences = ["yellow"],
        ThinkingEffort = MessageThinkingEffort.None
    };

    var response = await generator.GenerateMessageAsync(request);

    var doneReason = response.DoneReason;
    var textContent = response.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault();
    var responseStr = textContent?.Value ?? "(no response)";
    var tokens = response.TokenUsage;
    var tokenInfo = tokens != null
        ? $"in={tokens.InputTokens},out={tokens.OutputTokens}"
        : "no-usage";

    var isStopSeq = doneReason == MessageDoneReason.StopSequence;
    var containsYellow = responseStr.Contains("yellow", StringComparison.OrdinalIgnoreCase);
    var containsPurple = responseStr.Contains("purple", StringComparison.OrdinalIgnoreCase);

    return TestResult.Pass(
        $"reason={doneReason}, stopSeqHit={isStopSeq}, hasYellow={containsYellow}, hasPurple={containsPurple}, {tokenInfo}",
        model, null, Truncate(responseStr, 100));
}

async Task<TestResult> TestStreamingRoundTrip(IMessageGenerator generator, string model)
{
    // Define a tool with a verifiable secret value
    var secretValue = $"BEAM-{DateTime.UtcNow:HHmmss}";
    var tool = new FunctionTool(new Func<string>(() => secretValue))
    {
        Name = "get_beam_code",
        Description = "Returns a beam code string",
        Parameters = null,
        RequiresApproval = false
    };
    var tools = new ToolCollection([tool]);

    // Step 1: Stream the initial request — expect tool call
    var messages = new List<Message>
    {
        new UserMessage
        {
            Content = [new TextMessageContent { Value = "Call the get_beam_code tool and tell me the exact code it returns. Reply with only the code." }]
        }
    };

    var request1 = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = "You are a helpful assistant. Always use the provided tools when asked. After getting a tool result, respond with only the result value.",
        Messages = messages,
        Tools = tools
    };

    // Collect streaming step 1 — manually assemble AssistantMessage from stream
    var step1Message = new AssistantMessage();
    MessageDoneReason? step1Reason = null;
    int step1Chunks = 0;
    var step1Events = new List<string>();

    await foreach (var chunk in generator.GenerateStreamingMessageAsync(request1))
    {
        step1Chunks++;
        switch (chunk)
        {
            case StreamingMessageBeginResponse begin:
                step1Message.Id = begin.Id;
                step1Events.Add("Begin");
                break;
            case StreamingContentAddedResponse added:
                step1Events.Add($"Added({added.Content.GetType().Name})");
                step1Message.Content.Add(added.Content);
                break;
            case StreamingContentDeltaResponse delta:
                step1Events.Add($"Delta({delta.Delta.GetType().Name})");
                // Use Merge() on the target content at the given index
                if (delta.Index < step1Message.Content.Count)
                {
                    step1Message.Content.ElementAt(delta.Index).Merge(delta.Delta);
                }
                break;
            case StreamingMessageDoneResponse done:
                step1Reason = done.DoneReason;
                step1Events.Add("Done");
                break;
        }
    }

    var toolCalls = step1Message.Content.OfType<ToolMessageContent>().ToList();

    if (toolCalls.Count == 0)
    {
        var text = step1Message.Content.OfType<TextMessageContent>().FirstOrDefault();
        return TestResult.Error($"Step 1: no tool call in stream (reason={step1Reason}). Response: {Truncate(text?.Value, 80) ?? "(none)"}. Events: {string.Join(",", step1Events.Distinct())}");
    }

    // Step 2: Execute the tool
    foreach (var tc in toolCalls)
    {
        var input = new ToolInput(tc.Input);
        tc.Output = tools.TryGet(tc.Name, out var t)
            ? await t.InvokeAsync(input)
            : ToolOutput.Failure($"Tool '{tc.Name}' not found");
    }

    // Step 3: Stream continuation with tool results
    messages.Add(step1Message);

    var request2 = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = request1.SystemPrompt,
        Messages = messages,
        Tools = tools
    };

    var textBuilder = new StringBuilder();
    int step2Chunks = 0;
    MessageDoneReason? step2Reason = null;
    var step2Events = new List<string>();

    await foreach (var chunk in generator.GenerateStreamingMessageAsync(request2))
    {
        step2Chunks++;
        switch (chunk)
        {
            case StreamingMessageBeginResponse:
                step2Events.Add("Begin");
                break;
            case StreamingContentAddedResponse added:
                step2Events.Add($"Added({added.Content.GetType().Name})");
                if (added.Content is TextMessageContent addedText)
                    textBuilder.Append(addedText.Value);
                break;
            case StreamingContentDeltaResponse delta:
                step2Events.Add($"Delta({delta.Delta.GetType().Name})");
                if (delta.Delta is TextDeltaContent td)
                    textBuilder.Append(td.Value);
                break;
            case StreamingMessageDoneResponse done:
                step2Reason = done.DoneReason;
                step2Events.Add("Done");
                break;
        }
    }

    var finalText = textBuilder.ToString();
    var hasSecret = finalText.Contains(secretValue);
    var verifyInfo = hasSecret ? "verified" : "not-found";

    return TestResult.Pass(
        $"step1={step1Chunks}chunks/{step1Reason}, step2={step2Chunks}chunks/{step2Reason}, secret={verifyInfo}",
        model, null, $"step1events={string.Join(",", step1Events.Distinct())} | text={Truncate(finalText, 60)}");
}

async Task<TestResult> TestErrorModel(IMessageGenerator generator)
{
    var request = new MessageGenerationRequest
    {
        Model = "nonexistent-model-12345",
        SystemPrompt = "You are a helpful assistant.",
        Messages =
        [
            new UserMessage
            {
                Content = [new TextMessageContent { Value = "Hello" }]
            }
        ]
    };

    try
    {
        var response = await generator.GenerateMessageAsync(request);
        // If we get here, the provider didn't throw — report what happened
        var textContent = response.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault();
        return TestResult.Pass(
            $"no-error (reason={response.DoneReason})",
            "nonexistent-model-12345", null, Truncate(textContent?.Value, 60) ?? "(no response)");
    }
    catch (HttpRequestException ex)
    {
        return TestResult.Pass(
            $"HttpRequestException: {ex.StatusCode}",
            "nonexistent-model-12345", null, Truncate(ex.Message, 80));
    }
    catch (Exception ex)
    {
        return TestResult.Pass(
            $"{ex.GetType().Name}: caught",
            "nonexistent-model-12345", null, Truncate(ex.Message, 80));
    }
}

async Task<TestResult> TestNoSystemPrompt(IMessageGenerator generator, string model)
{
    var request = new MessageGenerationRequest
    {
        Model = model,
        // SystemPrompt intentionally omitted
        Messages =
        [
            new UserMessage
            {
                Content = [new TextMessageContent { Value = "Say 'works' and nothing else." }]
            }
        ],
        ThinkingEffort = MessageThinkingEffort.None
    };

    var response = await generator.GenerateMessageAsync(request);

    var textContent = response.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault();
    var responseStr = Truncate(textContent?.Value, 100) ?? "(no response)";
    var tokens = response.TokenUsage;
    var tokenInfo = tokens != null
        ? $"in={tokens.InputTokens},out={tokens.OutputTokens}"
        : "no-usage";

    if (string.IsNullOrWhiteSpace(textContent?.Value))
        return TestResult.Error($"Empty response without SystemPrompt (reason={response.DoneReason})");

    return TestResult.Pass(
        $"reason={response.DoneReason}, {tokenInfo}",
        model, null, responseStr);
}

async Task<TestResult> TestTemperature(IMessageGenerator generator, string model)
{
    // Run the same prompt twice with Temperature=0, compare outputs
    var request = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = "You are a helpful assistant. Be precise.",
        Messages =
        [
            new UserMessage
            {
                Content = [new TextMessageContent { Value = "What is the capital of France? Reply with only the city name." }]
            }
        ],
        Temperature = 0f,
        ThinkingEffort = MessageThinkingEffort.None
    };

    var response1 = await generator.GenerateMessageAsync(request);
    var text1 = response1.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "";

    var response2 = await generator.GenerateMessageAsync(request);
    var text2 = response2.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "";

    var match = text1.Trim().Equals(text2.Trim(), StringComparison.OrdinalIgnoreCase);
    var hasParis = text1.Contains("Paris", StringComparison.OrdinalIgnoreCase);

    return TestResult.Pass(
        $"reason={response1.DoneReason}, deterministic={match}, correct={hasParis}",
        model, null, $"r1=\"{Truncate(text1.Trim(), 40)}\" r2=\"{Truncate(text2.Trim(), 40)}\"");
}

async Task<TestResult> TestImageInput(IMessageGenerator generator, string model)
{
    // Locate test image — try multiple paths
    var candidates = new[]
    {
        Path.Combine("local-tests", "multimodal-test-image-1.png"),
        Path.Combine("..", "multimodal-test-image-1.png"),
        "multimodal-test-image-1.png"
    };
    var imagePath = candidates.FirstOrDefault(File.Exists);
    if (imagePath == null)
        return TestResult.Skip("Test image not found (multimodal-test-image-1.png)");

    var imageBytes = await File.ReadAllBytesAsync(imagePath);
    var base64 = Convert.ToBase64String(imageBytes);

    var request = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = "You are a helpful assistant. Describe images concisely.",
        Messages =
        [
            new UserMessage
            {
                Content =
                [
                    new ImageMessageContent { Format = ImageFormat.Png, Base64 = base64 },
                    new TextMessageContent { Value = "Describe what this image shows in 2-3 sentences." }
                ]
            }
        ],
        ThinkingEffort = MessageThinkingEffort.None
    };

    var response = await generator.GenerateMessageAsync(request);
    var text = response.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "";

    if (string.IsNullOrWhiteSpace(text))
        return TestResult.Error("Empty response for image input");

    // Check response mentions something relevant to the map/trip image
    var keywords = new[] { "map", "boundary", "water", "trip", "earth", "route", "island", "lake", "trail", "day", "legend", "satellite" };
    var matched = keywords.Where(k => text.Contains(k, StringComparison.OrdinalIgnoreCase)).ToList();

    return TestResult.Pass(
        $"reason={response.DoneReason}, keywords={string.Join(",", matched)}, len={text.Length}",
        model, null, Truncate(text, 150));
}

async Task<TestResult> TestImageStream(IMessageGenerator generator, string model)
{
    // Locate test image
    var candidates = new[]
    {
        Path.Combine("local-tests", "multimodal-test-image-1.png"),
        Path.Combine("..", "multimodal-test-image-1.png"),
        "multimodal-test-image-1.png"
    };
    var imagePath = candidates.FirstOrDefault(File.Exists);
    if (imagePath == null)
        return TestResult.Skip("Test image not found (multimodal-test-image-1.png)");

    var imageBytes = await File.ReadAllBytesAsync(imagePath);
    var base64 = Convert.ToBase64String(imageBytes);

    var request = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = "You are a helpful assistant. Describe images concisely.",
        Messages =
        [
            new UserMessage
            {
                Content =
                [
                    new ImageMessageContent { Format = ImageFormat.Png, Base64 = base64 },
                    new TextMessageContent { Value = "What type of image is this? Reply in one sentence." }
                ]
            }
        ],
        ThinkingEffort = MessageThinkingEffort.None
    };

    var sb = new StringBuilder();
    bool hasBegin = false, hasDelta = false, hasDone = false;
    int chunkCount = 0;

    await foreach (var chunk in generator.GenerateStreamingMessageAsync(request))
    {
        chunkCount++;
        switch (chunk)
        {
            case StreamingMessageBeginResponse:
                hasBegin = true;
                break;
            case StreamingContentAddedResponse added:
                if (added.Content is TextMessageContent initialText)
                    sb.Append(initialText.Value);
                break;
            case StreamingContentDeltaResponse delta:
                hasDelta = true;
                if (delta.Delta is TextDeltaContent textDelta)
                    sb.Append(textDelta.Value);
                break;
            case StreamingMessageDoneResponse:
                hasDone = true;
                break;
        }
    }

    var text = sb.ToString();

    if (string.IsNullOrWhiteSpace(text))
        return TestResult.Error("No text collected from image streaming");

    var lifecycle = $"begin={hasBegin},delta={hasDelta},done={hasDone}";

    return TestResult.Pass(
        $"{lifecycle}, chunks={chunkCount}, len={text.Length}",
        model, null, Truncate(text, 150));
}

async Task<TestResult> TestParallelToolCalls(IMessageGenerator generator, string model)
{
    // Define 3 independent tools — ask model to call all of them in one request
    var toolWeather = new FunctionTool(new Func<string, string>((city) => $"Sunny, 22°C in {city}"))
    {
        Name = "get_weather",
        Description = "Returns the current weather for a city",
        Parameters = new System.Text.Json.Nodes.JsonObject
        {
            ["type"] = "object",
            ["properties"] = new System.Text.Json.Nodes.JsonObject
            {
                ["city"] = new System.Text.Json.Nodes.JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "City name"
                }
            },
            ["required"] = new System.Text.Json.Nodes.JsonArray("city")
        },
        RequiresApproval = false
    };

    var toolTime = new FunctionTool(new Func<string, string>((timezone) => $"2025-01-15T10:30:00 {timezone}"))
    {
        Name = "get_time",
        Description = "Returns the current time for a timezone",
        Parameters = new System.Text.Json.Nodes.JsonObject
        {
            ["type"] = "object",
            ["properties"] = new System.Text.Json.Nodes.JsonObject
            {
                ["timezone"] = new System.Text.Json.Nodes.JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "Timezone name (e.g. UTC, KST)"
                }
            },
            ["required"] = new System.Text.Json.Nodes.JsonArray("timezone")
        },
        RequiresApproval = false
    };

    var toolTranslate = new FunctionTool(new Func<string, string, string>((text, lang) => $"[{lang}] {text}"))
    {
        Name = "translate_text",
        Description = "Translates text to a target language",
        Parameters = new System.Text.Json.Nodes.JsonObject
        {
            ["type"] = "object",
            ["properties"] = new System.Text.Json.Nodes.JsonObject
            {
                ["text"] = new System.Text.Json.Nodes.JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "Text to translate"
                },
                ["target_language"] = new System.Text.Json.Nodes.JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "Target language code (e.g. ko, ja, es)"
                }
            },
            ["required"] = new System.Text.Json.Nodes.JsonArray("text", "target_language")
        },
        RequiresApproval = false
    };

    var tools = new ToolCollection([toolWeather, toolTime, toolTranslate]);

    var request = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = "You are a helpful assistant. When the user asks for multiple things, call ALL relevant tools in a single response. Do NOT respond with text before calling all tools.",
        Messages =
        [
            new UserMessage
            {
                Content = [new TextMessageContent { Value = "I need three things at once: 1) the weather in Tokyo, 2) the current time in KST timezone, 3) translate 'Hello' to Korean. Call all three tools now." }]
            }
        ],
        Tools = tools,
        ThinkingEffort = MessageThinkingEffort.None
    };

    var response = await generator.GenerateMessageAsync(request);
    var toolCalls = response.Message?.Content?.OfType<ToolMessageContent>().ToList() ?? [];

    if (toolCalls.Count == 0)
    {
        var text = response.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault();
        return TestResult.Error($"No tool calls (reason={response.DoneReason}). Response: {Truncate(text?.Value, 80) ?? "(none)"}");
    }

    var toolNames = toolCalls.Select(tc => tc.Name).ToList();
    var isParallel = toolCalls.Count >= 2;

    // Check which of the 3 tools were called
    var hasWeather = toolNames.Any(n => n.Contains("weather"));
    var hasTime = toolNames.Any(n => n.Contains("time"));
    var hasTranslate = toolNames.Any(n => n.Contains("translate"));

    return TestResult.Pass(
        $"reason={response.DoneReason}, calls={toolCalls.Count}, parallel={isParallel}, weather={hasWeather}, time={hasTime}, translate={hasTranslate}",
        model, null, $"tools=[{string.Join(", ", toolNames)}]");
}

async Task<TestResult> TestTopP(IMessageGenerator generator, string model)
{
    // Test with TopP=0.1 (very focused, deterministic-like)
    var request = new MessageGenerationRequest
    {
        Model = model,
        SystemPrompt = "You are a helpful assistant. Be precise and brief.",
        Messages =
        [
            new UserMessage
            {
                Content = [new TextMessageContent { Value = "What is the chemical symbol for water? Reply with only the symbol." }]
            }
        ],
        TopP = 0.1f,
        ThinkingEffort = MessageThinkingEffort.None
    };

    var response1 = await generator.GenerateMessageAsync(request);
    var text1 = response1.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "";

    if (string.IsNullOrWhiteSpace(text1))
        return TestResult.Error($"Empty response with TopP=0.1 (reason={response1.DoneReason})");

    // Run again with TopP=1.0 (full distribution — maximum diversity)
    request.TopP = 1.0f;
    var response2 = await generator.GenerateMessageAsync(request);
    var text2 = response2.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault()?.Value ?? "";

    if (string.IsNullOrWhiteSpace(text2))
        return TestResult.Error($"Empty response with TopP=1.0 (reason={response2.DoneReason})");

    var hasH2O_1 = text1.Contains("H2O", StringComparison.OrdinalIgnoreCase)
                   || text1.Contains("H₂O", StringComparison.OrdinalIgnoreCase);
    var hasH2O_2 = text2.Contains("H2O", StringComparison.OrdinalIgnoreCase)
                   || text2.Contains("H₂O", StringComparison.OrdinalIgnoreCase);

    var tokens1 = response1.TokenUsage;
    var tokens2 = response2.TokenUsage;
    var tokenInfo1 = tokens1 != null ? $"in={tokens1.InputTokens},out={tokens1.OutputTokens}" : "no-usage";
    var tokenInfo2 = tokens2 != null ? $"in={tokens2.InputTokens},out={tokens2.OutputTokens}" : "no-usage";

    return TestResult.Pass(
        $"reason={response1.DoneReason}, topP=0.1→correct={hasH2O_1} [{tokenInfo1}], topP=1.0→correct={hasH2O_2} [{tokenInfo2}]",
        model, null, $"p0.1=\"{Truncate(text1.Trim(), 30)}\" p1.0=\"{Truncate(text2.Trim(), 30)}\"");
}

// =============================================================================
// Helper Methods
// =============================================================================

void PrintResult(string scenario, TestResult result)
{
    var status = result.Success ? "[PASS]" : result.Skipped ? "[SKIP]" : "[FAIL]";
    var color = result.Success ? ConsoleColor.Green : result.Skipped ? ConsoleColor.Yellow : ConsoleColor.Red;

    Console.ForegroundColor = color;
    Console.Write($"{"",4}{status}");
    Console.ResetColor();
    Console.WriteLine($" {scenario}: {result.Message}");

    if (!string.IsNullOrEmpty(result.Model))
        Console.WriteLine($"{"",11}model: {result.Model}");
    if (!string.IsNullOrEmpty(result.Thinking))
        Console.WriteLine($"{"",11}thinking: {result.Thinking}");
    if (!string.IsNullOrEmpty(result.Response))
        Console.WriteLine($"{"",11}response: {result.Response}");
}

string? GetEnv(string name) => Environment.GetEnvironmentVariable(name);

string? Truncate(string? value, int maxLength)
{
    if (value == null) return null;
    return value.Length > maxLength ? value[..maxLength] + "..." : value;
}

// =============================================================================
// Provider Test Functions
// =============================================================================

async Task<List<(string, TestResult)>> TestOpenAI()
{
    var apiKey = GetEnv("OPENAI_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("sk-xxxx"))
        return [("skip", TestResult.Skip("OPENAI_API_KEY not configured"))];

    var config = new OpenAIConfig
    {
        ApiKey = apiKey,
        Organization = GetEnv("OPENAI_ORGANIZATION") ?? "",
        Project = GetEnv("OPENAI_PROJECT") ?? ""
    };

    var catalog = new OpenAIModelCatalog(config);
    var generator = new OpenAIMessageGenerator(config);
    var model = GetEnv("OPENAI_MODEL") ?? "gpt-4o-mini";

    return await RunScenarios(catalog, generator, model);
}

async Task<List<(string, TestResult)>> TestAzureOpenAI()
{
    var apiKey = GetEnv("AZURE_OPENAI_API_KEY");
    var endpoint = GetEnv("AZURE_OPENAI_ENDPOINT");
    var deployment = GetEnv("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4o-mini";
    var apiVersion = GetEnv("AZURE_OPENAI_API_VERSION") ?? "2024-02-15-preview";

    if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("xxxx"))
        return [("skip", TestResult.Skip("AZURE_OPENAI_API_KEY not configured"))];
    if (string.IsNullOrWhiteSpace(endpoint) || endpoint.Contains("your-resource"))
        return [("skip", TestResult.Skip("AZURE_OPENAI_ENDPOINT not configured"))];

    var baseUrl = endpoint.TrimEnd('/') + $"/openai/deployments/{deployment}/?api-version={apiVersion}";

    var config = new OpenAIConfig
    {
        BaseUrl = baseUrl,
        ApiKey = apiKey
    };

    var generator = new OpenAIMessageGenerator(config);

    return await RunScenarios(null, generator, deployment);
}

async Task<List<(string, TestResult)>> TestAnthropic()
{
    var apiKey = GetEnv("ANTHROPIC_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("sk-ant-xxxx"))
        return [("skip", TestResult.Skip("ANTHROPIC_API_KEY not configured"))];

    var config = new AnthropicConfig
    {
        ApiKey = apiKey
    };

    var catalog = new AnthropicModelCatalog(config);
    var generator = new AnthropicMessageGenerator(config);
    var model = GetEnv("ANTHROPIC_MODEL") ?? "claude-3-5-haiku-latest";

    return await RunScenarios(catalog, generator, model);
}

async Task<List<(string, TestResult)>> TestGoogleAI()
{
    var apiKey = GetEnv("GOOGLE_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("AIza-xxxx"))
        return [("skip", TestResult.Skip("GOOGLE_API_KEY not configured"))];

    var config = new GoogleAIConfig
    {
        ApiKey = apiKey
    };

    var catalog = new GoogleAIModelCatalog(config);
    var generator = new GoogleAIMessageGenerator(config);
    var model = GetEnv("GOOGLE_MODEL") ?? "gemini-2.0-flash";

    return await RunScenarios(catalog, generator, model);
}

async Task<List<(string, TestResult)>> TestXAI()
{
    var apiKey = GetEnv("XAI_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("xai-xxxx"))
        return [("skip", TestResult.Skip("XAI_API_KEY not configured"))];

    var config = new OpenAIConfig
    {
        BaseUrl = "https://api.x.ai/v1/",
        ApiKey = apiKey,
        Compatibility = OpenAICompatibility.XAI
    };

    var catalog = new OpenAIModelCatalog(config);
    var generator = new OpenAIMessageGenerator(config);
    var envModel = GetEnv("XAI_MODEL");

    string model;
    try
    {
        var models = await catalog.ListModelsAsync();
        var modelList = models.ToList();
        Console.WriteLine($"{"",4}xAI models: {string.Join(", ", modelList.Select(m => m.ModelId))}");

        if (!string.IsNullOrEmpty(envModel))
        {
            model = envModel;
        }
        else
        {
            var chatModel = modelList.FirstOrDefault(m => m.ModelId == "grok-3-mini")
                         ?? modelList.FirstOrDefault(m => m.ModelId == "grok-3")
                         ?? modelList.FirstOrDefault(m =>
                             !m.ModelId.Contains("image") &&
                             !m.ModelId.Contains("vision") &&
                             !m.ModelId.Contains("imagine") &&
                             !m.ModelId.Contains("code"));
            model = chatModel?.ModelId ?? "grok-3-mini";
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{"",4}Catalog error: {ex.Message}");
        model = envModel ?? "grok-3-mini";
    }

    return await RunScenarios(null, generator, model);
}

async Task<List<(string, TestResult)>> TestOllama()
{
    var baseUrl = GetEnv("OLLAMA_BASE_URL") ?? "http://localhost:11434/api/";
    var envModel = GetEnv("OLLAMA_MODEL");

    var config = new OllamaConfig { BaseUrl = baseUrl };
    var catalog = new OllamaModelCatalog(config);
    var generator = new OllamaMessageGenerator(config);

    try
    {
        var models = await catalog.ListModelsAsync();
        if (!models.Any())
            return [("skip", TestResult.Skip("Ollama running but no models installed"))];

        var model = !string.IsNullOrEmpty(envModel) ? envModel : models.First().ModelId;
        return await RunScenarios(null, generator, model);
    }
    catch (HttpRequestException)
    {
        return [("skip", TestResult.Skip("Ollama not running"))];
    }
}

async Task<List<(string, TestResult)>> TestLMStudio()
{
    var baseUrl = GetEnv("LMSTUDIO_BASE_URL") ?? "http://localhost:1234/v1/";
    var apiKey = GetEnv("LMSTUDIO_API_KEY") ?? "lm-studio";
    var envModel = GetEnv("LMSTUDIO_MODEL");

    var config = new OpenAIConfig { BaseUrl = baseUrl, ApiKey = apiKey };
    var catalog = new OpenAIModelCatalog(config);
    var generator = new OpenAIMessageGenerator(config);

    try
    {
        var models = await catalog.ListModelsAsync();
        if (!models.Any())
            return [("skip", TestResult.Skip("LM Studio running but no models loaded"))];

        var model = !string.IsNullOrEmpty(envModel) ? envModel : models.First().ModelId;
        return await RunScenarios(null, generator, model);
    }
    catch (HttpRequestException)
    {
        return [("skip", TestResult.Skip("LM Studio not running"))];
    }
}

async Task<List<(string, TestResult)>> TestGPUStack()
{
    var baseUrl = GetEnv("GPUSTACK_BASE_URL") ?? "http://localhost:80/v1-openai/";
    var apiKey = GetEnv("GPUSTACK_API_KEY");
    var envModel = GetEnv("GPUSTACK_MODEL");

    if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("gpustack-xxxx"))
        return [("skip", TestResult.Skip("GPUSTACK_API_KEY not configured"))];

    var config = new OpenAIConfig { BaseUrl = baseUrl, ApiKey = apiKey };
    var catalog = new OpenAIModelCatalog(config);
    var generator = new OpenAIMessageGenerator(config);

    try
    {
        var models = await catalog.ListModelsAsync();
        if (!models.Any())
            return [("skip", TestResult.Skip("GPUStack running but no models deployed"))];

        var model = !string.IsNullOrEmpty(envModel) ? envModel : models.First().ModelId;
        return await RunScenarios(null, generator, model);
    }
    catch (HttpRequestException)
    {
        return [("skip", TestResult.Skip("GPUStack not running"))];
    }
}

// =============================================================================
// Test Result
// =============================================================================

record TestResult(bool Success, bool Skipped, string Message, string? Model = null, string? Thinking = null, string? Response = null)
{
    public static TestResult Pass(string message, string? model = null, string? thinking = null, string? response = null)
        => new(true, false, message, model, thinking, response);

    public static TestResult Skip(string reason)
        => new(false, true, reason);

    public static TestResult Error(string error)
        => new(false, false, error);
}
