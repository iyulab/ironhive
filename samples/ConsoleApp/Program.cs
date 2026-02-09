using dotenv.net;
using IronHive.Abstractions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Tools;
using IronHive.Core;
using IronHive.Core.Services;
using IronHive.Core.Tools;
using IronHive.Providers.Anthropic;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Text.Json;

DotEnv.Load(new DotEnvOptions(
    envFilePaths: [".env"],
    trimValues: true,
    overwriteExistingVars: false
));
var request = new MessageGenerationRequest
{
    Model = string.Empty,
    Messages = [
        new UserMessage
        {
            Content = [new TextMessageContent
            {
                Value = "Write a poem about IronHive in Korean.",
            }]
        }
    ],
    System = "cool",
    //ThinkingEffort = MessageThinkingEffort.Low
};

//request.Model = "gpt-5";
//var key = Environment.GetEnvironmentVariable("OPENAI")
//    ?? throw new Exception("OPENAI_API_KEY is not set in .env file");
//var openai = new OpenAIResponseMessageGenerator(new OpenAIConfig
//{
//    ApiKey = key
//});
//var msg = await openai.GenerateMessageAsync(request);
//Console.WriteLine(JsonSerializer.Serialize(msg));
//await foreach (var chunk in openai.GenerateStreamingMessageAsync(request))
//{
//    Console.Write(JsonSerializer.Serialize(chunk));
//}


//request.Model = "claude-haiku-4-5";
//var key = Environment.GetEnvironmentVariable("ANTHROPIC")
//    ?? throw new Exception("ANTHROPIC_API_KEY is not set in .env file");
//var anthro = new AnthropicMessageGenerator(new AnthropicConfig
//{
//    ApiKey = key
//});
//var msg = await anthro.GenerateMessageAsync(request);
//Console.WriteLine(JsonSerializer.Serialize(msg));
//await foreach (var chunk in anthro.GenerateStreamingMessageAsync(request))
//{
//    Console.Write(JsonSerializer.Serialize(chunk));
//}


//request.Model = "gemini-3-flash-preview";
//var key = Environment.GetEnvironmentVariable("GOOGLE")
//    ?? throw new Exception("GOOGLE_API_KEY is not set in .env file");
//var google = new GoogleAIMessageGenerator(new GoogleAIConfig
//{
//    ApiKey = key
//});
//var msg = await google.GenerateMessageAsync(request);
//Console.WriteLine(JsonSerializer.Serialize(msg));
//await foreach (var chunk in google.GenerateStreamingMessageAsync(request))
//{
//    Console.Write(JsonSerializer.Serialize(chunk));
//}


request.Model = "grok-4-1-fast-reasoning";
var key = Environment.GetEnvironmentVariable("XAI")
    ?? throw new Exception("XAI_API_KEY is not set in .env file");
var openai = new OpenAIChatMessageGenerator(new OpenAIConfig
{
    BaseUrl = "https://api.x.ai/v1",
    ApiKey = key
});
var msg = await openai.GenerateMessageAsync(request);
Console.WriteLine(JsonSerializer.Serialize(msg));
await foreach (var chunk in openai.GenerateStreamingMessageAsync(request))
{
    Console.Write(JsonSerializer.Serialize(chunk));
}

return;
