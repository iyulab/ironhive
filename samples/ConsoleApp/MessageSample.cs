using IronHive.Abstractions;
using IronHive.Abstractions.Messages;
using IronHive.Core.Extensions;
using IronHive.Abstractions.Messages.Content;
using IronHive.Core;
using IronHive.Core.Tools;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible;
using System.ComponentModel;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ConsoleApp;

public static class MessageSample
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static async Task Run()
    {
        var request = new MessageRequest
        {
            Provider = string.Empty,
            Model = string.Empty,
            ThinkingEffort = MessageThinkingEffort.Minimal,
            System = "you are a helpful assistant that can answer questions and solve problems.",
            Messages = [
                new Message { 
                    Role = MessageRole.User,
                    Content =
                    [
                        new TextMessageContent
                        {
                            Value = "Please calculate 3292 * 1234 - 2222 using tools, and Write a poem about with image in Korean.",
                        },
                        new ImageMessageContent
                        {
                            Format = ImageFormat.Jpeg,
                            Base64 = Convert.ToBase64String(File.ReadAllBytes("dragon.jpg"))
                        }
                    ]
                }
            ],
            Tools = new ToolCollection([
                ..FunctionToolFactory.CreateFrom<Calculator>()
            ]),
            Suggestions = new SuggestionOptions(),
            // Output = OutputOptions.For<OutputFormat>()
        };

        var hive = new HiveServiceBuilder()
            .AddOpenAIProviders("openai", new OpenAIConfig
            {
                ApiKey = Environment.GetEnvironmentVariable("OPENAI") ?? string.Empty
            })
            .AddAnthropicProviders("anthropic", new AnthropicConfig
            {
                ApiKey = Environment.GetEnvironmentVariable("ANTHROPIC") ?? string.Empty
            })
            .AddGoogleAIProviders("google", new GoogleAIConfig
            {
                ApiKey = Environment.GetEnvironmentVariable("GOOGLE") ?? string.Empty
            })
            .AddOpenAICompatibleProviders("openai-compatible", new OpenAICompatibleConfig
            {
                BaseUrl = "http://labs.iyulab.com:10150/v1",
                ApiKey = Environment.GetEnvironmentVariable("LOCAL") ?? string.Empty
            })
            .Build();

        // OpenAI 샘플
        // request.Provider = "openai";
        // request.Model = "gpt-5.5";

        // Anthropic 샘플
        // request.Provider = "anthropic";
        // request.Model = "claude-sonnet-5";

        // Google AI 샘플
        // request.Provider = "google";
        // request.Model = "gemini-3.5-flash";

        // OpenAI-compatible 샘플 (Chat Completions API — GPUStack/llama.cpp 서버용)
        request.Provider = "openai-compatible";
        request.Model = "qwen3.6-35b-a3b";

        if (request.Provider != "openai-compatible")
        {
            var tokenCount = await hive.Messages.CountTokensAsync(request);
            Console.WriteLine($"[CountTokens] Input tokens: {tokenCount}");
        }

        var msg = await hive.Messages.GenerateMessageAsync(request);
        Console.WriteLine(JsonSerializer.Serialize(msg, JsonOptions));
        await foreach (var chunk in hive.Messages.GenerateStreamingMessageAsync(request))
        {
            Console.WriteLine(JsonSerializer.Serialize(chunk, JsonOptions));
        }

        await Task.CompletedTask;
    }
}

public class OutputFormat
{
    public string CalculateResult { get; set; } = string.Empty;
    public string PoemContent { get; set; } = string.Empty;
}

public class Calculator
{
    [FunctionTool]
    [Description("Adds two integers and returns the result.")]
    public static int Add(int a, int b) => a + b;

    [FunctionTool]
    [Description("Subtracts the second integer from the first and returns the result.")]
    public static int Subtract(int a, int b) => a - b;

    [FunctionTool]
    [Description("Multiplies two integers and returns the result.")]
    public static int Multiply(int a, int b) => a * b;

    [FunctionTool]
    [Description("Divides the first integer by the second and returns the result as a double.")]
    public static double Divide(int a, int b) => a / (double)b;
}
