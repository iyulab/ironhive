// =============================================================================
// IronHive Local Provider Tests
// =============================================================================
// This program tests LLM providers locally using API keys from environment variables.
// NOT for CI - requires .env file with actual API keys.
// =============================================================================

using IronHive.Abstractions.Catalog;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Core;
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
var providerTests = new Dictionary<string, Func<Task<TestResult>>>
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

// Filter providers if specified
if (!string.IsNullOrEmpty(targetProvider))
{
    if (!providerTests.ContainsKey(targetProvider))
    {
        Console.WriteLine($"[ERROR] Unknown provider: {targetProvider}");
        Console.WriteLine($"Available: {string.Join(", ", providerTests.Keys)}");
        return 1;
    }
    providerTests = new Dictionary<string, Func<Task<TestResult>>>
    {
        [targetProvider] = providerTests[targetProvider]
    };
}

// Run tests
var results = new List<(string Provider, TestResult Result)>();
foreach (var (provider, testFunc) in providerTests)
{
    Console.WriteLine($"Testing {provider}...");
    try
    {
        var result = await testFunc();
        results.Add((provider, result));
        PrintResult(provider, result);
    }
    catch (Exception ex)
    {
        var result = TestResult.Error(ex.Message);
        results.Add((provider, result));
        PrintResult(provider, result);
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

return failed > 0 ? 1 : 0;

// =============================================================================
// Helper Methods
// =============================================================================

void PrintResult(string provider, TestResult result)
{
    var status = result.Success ? "[PASS]" : result.Skipped ? "[SKIP]" : "[FAIL]";
    var color = result.Success ? ConsoleColor.Green : result.Skipped ? ConsoleColor.Yellow : ConsoleColor.Red;

    Console.ForegroundColor = color;
    Console.Write($"  {status}");
    Console.ResetColor();
    Console.WriteLine($" {provider}: {result.Message}");

    if (!string.IsNullOrEmpty(result.Model))
        Console.WriteLine($"         model: {result.Model}");
    if (!string.IsNullOrEmpty(result.Thinking))
        Console.WriteLine($"         thinking: {result.Thinking}");
    if (!string.IsNullOrEmpty(result.Response))
        Console.WriteLine($"         message: {result.Response}");
}

string? GetEnv(string name) => Environment.GetEnvironmentVariable(name);

async Task<TestResult> TestWithProvider(
    string providerName,
    IModelCatalog? catalog,
    IMessageGenerator generator,
    string defaultModel)
{
    string? selectedModel = defaultModel;

    // Try to list models first if catalog available
    if (catalog != null)
    {
        try
        {
            var models = await catalog.ListModelsAsync();
            if (models.Any())
            {
                Console.WriteLine($"         Available models: {models.Count()}");
                if (string.IsNullOrEmpty(defaultModel))
                {
                    selectedModel = models.FirstOrDefault()?.ModelId;
                }
            }
        }
        catch
        {
            // Ignore catalog errors, proceed with default model
        }
    }

    if (string.IsNullOrEmpty(selectedModel))
    {
        return TestResult.Error("No model available");
    }

    // Test message generation with thinking enabled
    var request = new MessageGenerationRequest
    {
        Model = selectedModel,
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

    // Extract response content
    var thinkingContent = response.Message?.Content?.OfType<ThinkingMessageContent>().FirstOrDefault();
    var textContent = response.Message?.Content?.OfType<TextMessageContent>().FirstOrDefault();
    var responseStr = textContent?.Value ?? "(no response)";

    // Truncate response for display
    if (responseStr.Length > 100)
        responseStr = responseStr[..100] + "...";

    // If thinking content present, include it in output
    if (thinkingContent != null)
    {
        var thinkingText = thinkingContent.Value ?? "(empty)";
        if (thinkingText.Length > 80)
            thinkingText = thinkingText[..80] + "...";

        return TestResult.Pass(
            $"with thinking ({thinkingContent.Format})",
            selectedModel,
            thinkingText,
            responseStr);
    }

    return TestResult.Pass("ok", selectedModel, null, responseStr);
}

// =============================================================================
// Provider Test Functions
// =============================================================================

async Task<TestResult> TestOpenAI()
{
    var apiKey = GetEnv("OPENAI_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("sk-xxxx"))
        return TestResult.Skip("OPENAI_API_KEY not configured");

    var config = new OpenAIConfig
    {
        ApiKey = apiKey,
        Organization = GetEnv("OPENAI_ORGANIZATION") ?? "",
        Project = GetEnv("OPENAI_PROJECT") ?? ""
    };

    var catalog = new OpenAIModelCatalog(config);
    var generator = new OpenAIMessageGenerator(config);
    var model = GetEnv("OPENAI_MODEL") ?? "gpt-4o-mini";

    return await TestWithProvider("openai", catalog, generator, model);
}

async Task<TestResult> TestAzureOpenAI()
{
    var apiKey = GetEnv("AZURE_OPENAI_API_KEY");
    var endpoint = GetEnv("AZURE_OPENAI_ENDPOINT");
    var deployment = GetEnv("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4o-mini";
    var apiVersion = GetEnv("AZURE_OPENAI_API_VERSION") ?? "2024-02-15-preview";

    if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("xxxx"))
        return TestResult.Skip("AZURE_OPENAI_API_KEY not configured");
    if (string.IsNullOrWhiteSpace(endpoint) || endpoint.Contains("your-resource"))
        return TestResult.Skip("AZURE_OPENAI_ENDPOINT not configured");

    // Azure OpenAI uses OpenAI SDK with custom endpoint
    var baseUrl = endpoint.TrimEnd('/') + $"/openai/deployments/{deployment}/?api-version={apiVersion}";

    var config = new OpenAIConfig
    {
        BaseUrl = baseUrl,
        ApiKey = apiKey
    };

    var generator = new OpenAIMessageGenerator(config);

    return await TestWithProvider("azure-openai", null, generator, deployment);
}

async Task<TestResult> TestAnthropic()
{
    var apiKey = GetEnv("ANTHROPIC_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("sk-ant-xxxx"))
        return TestResult.Skip("ANTHROPIC_API_KEY not configured");

    var config = new AnthropicConfig
    {
        ApiKey = apiKey
    };

    var catalog = new AnthropicModelCatalog(config);
    var generator = new AnthropicMessageGenerator(config);
    var model = GetEnv("ANTHROPIC_MODEL") ?? "claude-3-5-haiku-latest";

    return await TestWithProvider("anthropic", catalog, generator, model);
}

async Task<TestResult> TestGoogleAI()
{
    var apiKey = GetEnv("GOOGLE_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("AIza-xxxx"))
        return TestResult.Skip("GOOGLE_API_KEY not configured");

    var config = new GoogleAIConfig
    {
        ApiKey = apiKey
    };

    var catalog = new GoogleAIModelCatalog(config);
    var generator = new GoogleAIMessageGenerator(config);
    var model = GetEnv("GOOGLE_MODEL") ?? "gemini-2.0-flash";

    return await TestWithProvider("google", catalog, generator, model);
}

async Task<TestResult> TestXAI()
{
    var apiKey = GetEnv("XAI_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("xai-xxxx"))
        return TestResult.Skip("XAI_API_KEY not configured");

    // xAI uses OpenAI-compatible API
    var config = new OpenAIConfig
    {
        BaseUrl = "https://api.x.ai/v1/",
        ApiKey = apiKey
    };

    var catalog = new OpenAIModelCatalog(config);
    var generator = new OpenAIMessageGenerator(config);
    var envModel = GetEnv("XAI_MODEL");

    // Always list available models for debugging
    string model;
    try
    {
        var models = await catalog.ListModelsAsync();
        var modelList = models.ToList();
        Console.WriteLine($"         xAI models: {string.Join(", ", modelList.Select(m => m.ModelId))}");

        if (!string.IsNullOrEmpty(envModel))
        {
            model = envModel;
            Console.WriteLine($"         Using env model: {model}");
        }
        else
        {
            // Prefer text-only models (exclude image/vision/imagine/code)
            var chatModel = modelList.FirstOrDefault(m => m.ModelId == "grok-3-mini")
                         ?? modelList.FirstOrDefault(m => m.ModelId == "grok-3")
                         ?? modelList.FirstOrDefault(m => m.ModelId == "grok-4-1-fast-non-reasoning")
                         ?? modelList.FirstOrDefault(m =>
                             !m.ModelId.Contains("image") &&
                             !m.ModelId.Contains("vision") &&
                             !m.ModelId.Contains("imagine") &&
                             !m.ModelId.Contains("code"));
            model = chatModel?.ModelId ?? "grok-3-mini";
            Console.WriteLine($"         Auto-selected: {model}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"         Catalog error: {ex.Message}");
        model = envModel ?? "grok-3-mini";
    }

    return await TestWithProvider("xai", catalog, generator, model);
}

async Task<TestResult> TestOllama()
{
    var baseUrl = GetEnv("OLLAMA_BASE_URL") ?? "http://localhost:11434/api/";
    var envModel = GetEnv("OLLAMA_MODEL");

    var config = new OllamaConfig
    {
        BaseUrl = baseUrl
    };

    var catalog = new OllamaModelCatalog(config);
    var generator = new OllamaMessageGenerator(config);

    try
    {
        // Check if Ollama is running by listing models
        var models = await catalog.ListModelsAsync();
        if (!models.Any())
            return TestResult.Skip("Ollama running but no models installed");

        // Use environment model if set, otherwise use first available
        var model = !string.IsNullOrEmpty(envModel) ? envModel : models.First().ModelId;
        return await TestWithProvider("ollama", catalog, generator, model);
    }
    catch (HttpRequestException)
    {
        return TestResult.Skip("Ollama not running");
    }
}

async Task<TestResult> TestLMStudio()
{
    var baseUrl = GetEnv("LMSTUDIO_BASE_URL") ?? "http://localhost:1234/v1/";
    var apiKey = GetEnv("LMSTUDIO_API_KEY") ?? "lm-studio";
    var envModel = GetEnv("LMSTUDIO_MODEL");

    var config = new OpenAIConfig
    {
        BaseUrl = baseUrl,
        ApiKey = apiKey
    };

    var catalog = new OpenAIModelCatalog(config);
    var generator = new OpenAIMessageGenerator(config);

    try
    {
        // Check if LM Studio is running
        var models = await catalog.ListModelsAsync();
        if (!models.Any())
            return TestResult.Skip("LM Studio running but no models loaded");

        // Use environment model if set, otherwise use first available
        var model = !string.IsNullOrEmpty(envModel) ? envModel : models.First().ModelId;
        return await TestWithProvider("lmstudio", catalog, generator, model);
    }
    catch (HttpRequestException)
    {
        return TestResult.Skip("LM Studio not running");
    }
}

async Task<TestResult> TestGPUStack()
{
    var baseUrl = GetEnv("GPUSTACK_BASE_URL") ?? "http://localhost:80/v1-openai/";
    var apiKey = GetEnv("GPUSTACK_API_KEY");
    var envModel = GetEnv("GPUSTACK_MODEL");

    if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("gpustack-xxxx"))
        return TestResult.Skip("GPUSTACK_API_KEY not configured");

    var config = new OpenAIConfig
    {
        BaseUrl = baseUrl,
        ApiKey = apiKey
    };

    var catalog = new OpenAIModelCatalog(config);
    var generator = new OpenAIMessageGenerator(config);

    try
    {
        // Check if GPUStack is running
        var models = await catalog.ListModelsAsync();
        if (!models.Any())
            return TestResult.Skip("GPUStack running but no models deployed");

        // Use environment model if set, otherwise use first available
        var model = !string.IsNullOrEmpty(envModel) ? envModel : models.First().ModelId;
        return await TestWithProvider("gpustack", catalog, generator, model);
    }
    catch (HttpRequestException)
    {
        return TestResult.Skip("GPUStack not running");
    }
}

// =============================================================================
// Test Result Class
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
