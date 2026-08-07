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
            // OutputFormat = OutputFormat.For<AnswerShape>()
        };

        // 자격증명이 있는 provider만 등록한다. 키를 하나만 가진 사람도 이 샘플을 실행할 수 있어야 하고,
        // 키가 필수인 provider(Gemini 등)는 빈 키로 등록하면 Build() 자체가 실패한다.
        var builder = new HiveServiceBuilder();

        var openAIKey = Environment.GetEnvironmentVariable("OPENAI");
        if (!string.IsNullOrWhiteSpace(openAIKey))
            builder.AddOpenAIProviders("openai", new OpenAIConfig { ApiKey = openAIKey });

        var anthropicKey = Environment.GetEnvironmentVariable("ANTHROPIC");
        if (!string.IsNullOrWhiteSpace(anthropicKey))
            builder.AddAnthropicProviders("anthropic", new AnthropicConfig { ApiKey = anthropicKey });

        var googleKey = Environment.GetEnvironmentVariable("GOOGLE");
        if (!string.IsNullOrWhiteSpace(googleKey))
            builder.AddGoogleAIProviders("google", new GoogleAIConfig { ApiKey = googleKey });

        // OpenAI 호환 서버(llama.cpp, LM Studio, vLLM, Ollama 등)는 보통 자격증명이 필요 없다.
        // LOCAL_BASE_URL 로 주소를 바꿀 수 있고, LOCAL_API_KEY 는 요구하는 서버에서만 설정한다.
        builder.AddOpenAICompatibleProviders("openai-compatible", new OpenAICompatibleConfig
        {
            BaseUrl = Environment.GetEnvironmentVariable("LOCAL_BASE_URL") ?? "http://localhost:8080",
            ApiKey = Environment.GetEnvironmentVariable("LOCAL_API_KEY") ?? string.Empty
        });

        var hive = builder.Build();

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

public class AnswerShape
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
