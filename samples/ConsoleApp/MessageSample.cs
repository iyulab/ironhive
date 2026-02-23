using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.XAI;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ConsoleApp;

public static class MessageSample
{
    public static async Task Run()
    {
        var request = new MessageGenerationRequest
        {
            Model = string.Empty,
            Messages = [
                new UserMessage
                {
                    Content =
                    [
                        new TextMessageContent
                        {
                            Value = "Write a poem about with image in Korean.",
                        },
                        new ImageMessageContent
                        {
                            Format = ImageFormat.Jpeg,
                            Base64 = Convert.ToBase64String(File.ReadAllBytes("dragon.jpg"))
                        }
                    ]
                }
            ],
            System = "cool",
            ThinkingEffort = MessageThinkingEffort.Low
        };
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        // OpenAI 샘플
        //request.Model = "gpt-5";
        //var key = Environment.GetEnvironmentVariable("OPENAI")
        //    ?? throw new InvalidOperationException("OPENAI_API_KEY is not set in .env file");
        //var openai = new OpenAIResponseMessageGenerator(new OpenAIConfig
        //{
        //    ApiKey = key
        //});
        //var msg = await openai.GenerateMessageAsync(request);
        //Console.WriteLine(JsonSerializer.Serialize(msg, options));
        //await foreach (var chunk in openai.GenerateStreamingMessageAsync(request))
        //{
        //    Console.WriteLine(JsonSerializer.Serialize(chunk, options));
        //}

        // Anthropic 샘플
        //request.Model = "claude-haiku-4-5";
        //var key = Environment.GetEnvironmentVariable("ANTHROPIC")
        //    ?? throw new InvalidOperationException("ANTHROPIC_API_KEY is not set in .env file");
        //var anthro = new AnthropicMessageGenerator(new AnthropicConfig
        //{
        //    ApiKey = key
        //});
        //var msg = await anthro.GenerateMessageAsync(request);
        //Console.WriteLine(JsonSerializer.Serialize(msg, options));
        //await foreach (var chunk in anthro.GenerateStreamingMessageAsync(request))
        //{
        //    Console.WriteLine(JsonSerializer.Serialize(chunk, options));
        //}

        // Google AI 샘플
        //request.Model = "gemini-3-flash-preview";
        //var key = Environment.GetEnvironmentVariable("GOOGLE")
        //    ?? throw new InvalidOperationException("GOOGLE_API_KEY is not set in .env file");
        //var google = new GoogleAIMessageGenerator(new GoogleAIConfig
        //{
        //    ApiKey = key
        //});
        //var msg = await google.GenerateMessageAsync(request);
        //Console.WriteLine(JsonSerializer.Serialize(msg, options));
        //await foreach (var chunk in google.GenerateStreamingMessageAsync(request))
        //{
        //    Console.WriteLine(JsonSerializer.Serialize(chunk, options));
        //}

        // XAI 샘플
        //request.Model = "grok-4-1-fast-reasoning";
        //var key = Environment.GetEnvironmentVariable("XAI")
        //    ?? throw new InvalidOperationException("XAI_API_KEY is not set in .env file");
        //var xai = new XAIMessageGenerator(new XAIConfig
        //{
        //    ApiKey = key
        //});
        //var msg = await xai.GenerateMessageAsync(request);
        //Console.WriteLine(JsonSerializer.Serialize(msg, options));
        //await foreach (var chunk in xai.GenerateStreamingMessageAsync(request))
        //{
        //    Console.WriteLine(JsonSerializer.Serialize(chunk, options));
        //}

        await Task.CompletedTask;
    }
}
