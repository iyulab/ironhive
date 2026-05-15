using IronHive.Abstractions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Tools;
using IronHive.Core;
using IronHive.Core.Tools;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.XAI;
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
            Messages = [
                new UserMessage
                {
                    Content =
                    [
                        new TextMessageContent
                        {
                            Value = "Please calculate 3292 * 1234 - 2222, and Write a poem about with image in Korean.",
                        },
                        new ImageMessageContent
                        {
                            Format = ImageFormat.Jpeg,
                            Base64 = Convert.ToBase64String(File.ReadAllBytes("dragon.jpg"))
                        }
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
            //Output = typeof(OutputFormat)
        };
        
        var hive = new HiveServiceBuilder()
            .AddOpenAIProviders("openai", new OpenAIConfig
            {
                //ApiKey = Environment.GetEnvironmentVariable("OPENAI") ?? string.Empty
                BaseUrl = "http://172.30.1.54/v1",
                ApiKey = "gpustack_8c5d65c2ceddbe20_0f8bfbd4c422ff42d01fe7004e9b3e8b"
            }, OpenAIServiceType.ChatCompletion)
            .AddAnthropicProviders("anthropic", new AnthropicConfig
            {
                ApiKey = Environment.GetEnvironmentVariable("ANTHROPIC") ?? string.Empty
            })
            .AddGoogleAIProviders("google", new GoogleAIConfig
            {
                ApiKey = Environment.GetEnvironmentVariable("GOOGLE") ?? string.Empty
            })
            .AddXAIProviders("xai", new XAIConfig
            {
                ApiKey = Environment.GetEnvironmentVariable("XAI") ?? string.Empty
            })
            .AddFunctionTool<Calculator>()
            .Build();
        var generator = hive.Services.GetRequiredService<IMessageService>();

        // OpenAI 샘플
        request.Provider = "openai";
        //request.Model = "gpt-5.5";
        request.Model = "qwen3.6-27b-awq-int4";

        // Anthropic 샘플
        //request.Provider = "anthropic";
        //request.Model = "claude-opus-4-7";

        // Google AI 샘플
        //request.Provider = "google";
        //request.Model = "gemini-3.1-flash-lite-preview";

        // XAI 샘플
        //request.Provider = "xai";
        //request.Model = "grok-4.3";

        var clone = request.Clone();
        clone.Output = typeof(OutputFormat);
        
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
