using IronHive.Abstractions;
using IronHive.Abstractions.Messages;
using IronHive.Core.Extensions;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using IronHive.Core;
using IronHive.Core.Tools;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ConsoleApp;

public class OutputFormat
{
    public string CalculateResult { get; set; } = string.Empty;
    public string PoemContent { get; set; } = string.Empty;
}

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
            ThinkingEffort = MessageThinkingEffort.Low,
            System = "you are a helpful assistant that can answer questions and solve problems.",
            Suggestions = new SuggestionOptions
            {
                MinItems = 1,
                MaxItems = 3,
            },
            Messages = [
                new Message { 
                    Role = MessageRole.User,
                    Content =
                    [
                        new TextMessageContent
                        {
                            Value = "I want to make a poem"
                        },
                        // new TextMessageContent
                        // {
                        //     Value = "Please calculate 3292 * 1234 - 2222, and Write a poem about with image in Korean.",
                        // },
                        // new ImageMessageContent
                        // {
                        //     Format = ImageFormat.Jpeg,
                        //     Base64 = Convert.ToBase64String(File.ReadAllBytes("dragon.jpg"))
                        // }
                    ]
                }
            ],
            Tools =
            [
                new ToolItem { Name = "func_Add", Options = new { } },
                new ToolItem { Name = "func_Subtract", Options = new { } },
                new ToolItem { Name = "func_Multiply", Options = new { } },
                new ToolItem { Name = "func_Divide", Options = new { } }
            ],
            // Output = typeof(OutputFormat)
        };
        
        var hive = new HiveServiceBuilder()
            .AddOpenAIProviders("openai", new OpenAIConfig
            {
                ApiKey = Environment.GetEnvironmentVariable("OPENAI") ?? string.Empty
            }, OpenAIServiceType.Messages)
            .AddAnthropicProviders("anthropic", new AnthropicConfig
            {
                ApiKey = Environment.GetEnvironmentVariable("ANTHROPIC") ?? string.Empty
            })
            .AddGoogleAIProviders("google", new GoogleAIConfig
            {
                ApiKey = Environment.GetEnvironmentVariable("GOOGLE") ?? string.Empty
            })
            .AddFunctionTool<Calculator>()
            .Build();
        var generator = hive.Messages;

        // OpenAI 샘플
        request.Provider = "openai";
        request.Model = "gpt-5.5";

        // Anthropic 샘플
        // request.Provider = "anthropic";
        // request.Model = "claude-sonnet-4-6";

        // Google AI 샘플
        // request.Provider = "google";
        // request.Model = "gemini-3.5-flash";

        var clone = request.Clone();
        // clone.Output = typeof(OutputFormat);
        clone.Suggestions = new SuggestionOptions
        {
            MinItems = 1,
            MaxItems = 3,
        };

        var tokenCount = await generator.CountTokensAsync(request);
        Console.WriteLine($"[CountTokens] Input tokens: {tokenCount}");

        var msg = await generator.GenerateMessageAsync(request);
        Console.WriteLine(JsonSerializer.Serialize(msg, JsonOptions));
        await foreach (var chunk in generator.GenerateStreamingMessageAsync(clone))
        {
            Console.WriteLine(JsonSerializer.Serialize(chunk, JsonOptions));
        }

        await Task.CompletedTask;
    }
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
