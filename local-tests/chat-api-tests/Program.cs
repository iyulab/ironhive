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
        ApiKey = apiKey
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
