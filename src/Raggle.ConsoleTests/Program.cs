using ClosedXML;
using LLama;
using LLama.Common;
using LLama.Native;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Engines;
using Raggle.Abstractions.Models;
using Raggle.Abstractions.Tools;
using Raggle.ConsoleTests;
using Raggle.Engines.Anthropic;
using Raggle.Engines.OpenAI;
using Raggle.Engines.OpenAI.ChatCompletion;
using Raggle.Engines.OpenAI.Embeddings;
using Raggle.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChatHistory = Raggle.Abstractions.Models.ChatHistory;
using ChatSession = Raggle.Abstractions.Models.ChatSession;
using JsonSchema = NJsonSchema.JsonSchema;

var text = await File.ReadAllTextAsync(@"C:\data\Raggle\src\Raggle.ConsoleTests\Secrets.json");
var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(text);
var key = secrets?.GetValueOrDefault("Anthropic") ?? string.Empty;

//var schema = JsonSchemaConverter.ConvertFromType<IEnumerable<string>>();
//var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { 
//    WriteIndented = true,
//    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
//});

var tools = FunctionToolFactory.CreateFromType<MathTools>();
var history = new ChatHistory();
var message = new ChatMessage
{
    Role = MessageRole.User,
    Contents = [ new TextContentBlock {
        Text = "what is (2 + 2) * 50?"
    }]
};
history.Add(message);

var chat = new AnthropicChatEngine(key);
var options = new ChatOptions
{
    ModelId = "claude-3-5-sonnet-20240620",
    MaxTokens = 1024,
    System = "Please run the tool",
    Tools = tools.ToArray()
};

await foreach (var response in chat.StreamingChatCompletionAsync(history, options))
{
    Console.WriteLine(JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
}

return;

public class MathTools
{
    [FunctionTool(Name = "Add", Description = "Adds two numbers together")]
    public int  AddAsync(
        [Description("The first number to add")] int a,
        [Description("The second number to add")] int b)
    {
        return a + b;
    }

    [FunctionTool(Name = "Subtract", Description = "Subtracts the second number from the first number")]
    public int SubtractAsync(
        [Description("The number to subtract from")] int a,
        [Description("The number to subtract")] int b)
    {
        return a - b;
    }

    [FunctionTool(Name = "Multiply", Description = "Multiplies two numbers together")]
    public int MultiplyAsync(
        [Description("The first number to multiply")] int a,
        [Description("The second number to multiply")] int b)
    {
        return a * b;
    }

    [FunctionTool(Name = "Divide", Description = "Divides the first number by the second number")]
    public double DivideAsync(
        [Description("The dividend")] double a,
        [Description("The divisor")] double b)
    {
        if (b == 0)
            throw new DivideByZeroException("Error: Division by zero is not allowed.");
        
        return a / b;
    }

    [FunctionTool(Name = "Power", Description = "Raises a number to the power of another number")]
    public double PowerAsync(
        [Description("The base number")] double baseNumber,
        [Description("The exponent")] double exponent)
    {
        double result = Math.Pow(baseNumber, exponent);
        return result;
    }

    [FunctionTool(Name = "SquareRoot", Description = "Calculates the square root of a number")]
    public double SquareRootAsync(
        [Description("The number to find the square root of")] double number)
    {
        if (number < 0)
            throw new InvalidOperationException("Error: Square root is not defined for negative numbers.");

        double result = Math.Sqrt(number);
        return result;  
    }

    [FunctionTool(Name = "Absolute", Description = "Returns the absolute value of a number")]
    public double AbsoluteAsync(
        [Description("The number to find the absolute value of")] double number)
    {
        double result = Math.Abs(number);
        return result;
    }
}

public enum TheEnum
{
    ww,
    tt,
    Oll_aa,
    GETET
}

public interface TheInterface
{
    [Description("this is my name")]
    public string Name { get; set; }
}

public abstract class TheAbstractClass
{
    [Description("this is my name")]
    public string Name { get; set; }
}

public class TheClass
{
    [Description("this is my name")]
    public required string Name { get; set; }
}

public struct TheStruct
{
    [Description("this is my name")]
    public string Name { get; set; }
}

public record TheRecord([Description("this is my name")] string name);

//var client = new OpenAIChatCompletionClient(key);
//var response = await client.PostChatCompletionAsync(new ChatCompletionRequest
//{
//    Model = "gpt-4o-mini",
//    Messages = [
//        new UserMessage
//        {
//            Content = "Please run the tool",
//        }
//    ],
//    Tools = [
//        new Tool
//        {
//            Function = new Function
//            {
//                Name = "ExampleTool",
//                Description = "A tool that takes various types of parameters",
//                Parameters = new
//                {
//                    type = "object",
//                    description = "A set of parameters",
//                    properties = new
//                    {
//                        dateValue = new
//                        {
//                            type = "string",
//                            format = "date-time",
//                            description = "A date-time string in ISO 8601 format",
//                        },
//                        enumValue = new
//                        {
//                            type = "string",
//                            @enum = new[] { "email", "phone", "fax" },
//                            description = "A string value",
//                        },
//                        IntegerValue = new
//                        {
//                            type = "integer",
//                            format = "int32",
//                            description = "An integer that can also be null",
//                        },
//                        numberValue = new
//                        {
//                            type = "number",
//                            format = "double",
//                            description = "A number that can also be null",
//                        },
//                        stringArray = new
//                        {
//                            type = "array",
//                            items = new {
//                                type = "string",
//                            },
//                            description = "An array of string types",
//                        },
//                        booleanValue = new
//                        {
//                            type = "boolean",
//                            description = "A boolean value",
//                        },
//                        nullValue = new
//                        {
//                            type = "null",
//                            description = "A null value",
//                        },
//                    },
//                    required = new[] { "dateValue", "enumValue", "stringArray" },
//                },
//            }
//        },
//    ]
//});


//var c = response.Choices.First();
//Console.WriteLine(c.Message.ToolCalls.First().Function);

//var services = new ServiceCollection();
//services.AddSingleton<DepenDency>();
//var provider = services.BuildServiceProvider();
//var tools = FunctionToolFactory.CreateFromType<Example>(provider, "PP");

//foreach (var tool in tools)
//{
//    if (tool.Name == "Print")
//    {
//        Console.WriteLine("Print Method");
//        await tool.InvokeAsync(null);
//        Console.WriteLine("Print Method End");
//    }
//    if (tool.Name == "PrintDependency")
//    {
//        Console.WriteLine("PrintDependency Method");
//        await tool.InvokeAsync(new Dictionary<string, object?>
//        {
//            ["args"] = new string[] { "Hello", "World" }
//        });
//        Console.WriteLine("PrintDependency Method End");
//    }
//    if (tool.Name == "묘")
//    {
//        Console.WriteLine("PrintDependencyAsync Method");
//        await tool.InvokeAsync(new Dictionary<string, object?>
//        {
//            ["arg"] = "Hello",
//            ["the"] = new
//            {
//                Name = "째 째"
//            }
//        });
//        Console.WriteLine("PrintDependencyAsync Method End");
//    }
//}

//NativeLibraryConfig.All.WithLogCallback((level, message) => { });

//var systemPrompt = """

//    Environment: ipython
//    Tools: brave_search, wolfram_alpha
//    Cutting Knowledge Date: December 2023
//    Today Date: 23 July 2024

//    You are a helpful assistant
//    """;
//var userPrompt = """

//    Can you help me solve this equation: x^3 - 4x^2 + 6x - 24 = 0
//    """;

//await Infer();
//return;

//var configuration = new ConfigurationBuilder()
//    .AddJsonFile(@"C:\data\Raggle\src\Raggle.ConsoleTests\User.json", false, true)
//    .Build();

//var services = new ServiceCollection();
//services.Configure<ServiceOptions>(configuration.GetSection("Raggle"));
//services.AddSingleton<RaggleService>();
//var provider = services.BuildServiceProvider();

//await Ask("openai/gpt-4o-2024-08-06");

//foreach (var i in Enumerable.Range(0, 100))
//{
//    Console.WriteLine($"This is {i + 1} count asking");

//    //await Ask("Mistral-7B-Instruct-v0.3.Q4_K_M");

//    //await Ask("Grabbe-AI.Q4_K_M");

//    await Ask("Meta-Llama-3.1-8B-Instruct-Q4_K_M");

//    //await Ask("codellama-7b-instruct.Q4_K_M");

//    //await Ask("qwen2-0_5b-instruct-q4_k_m");
//}

//async Task Ask(string modelId)
//{
//    var raggle = provider.GetRequiredService<RaggleService>();
//    var history = new ChatHistory();

//    history.AddSystemMessage(systemPrompt);
//    history.AddUserMessage(userPrompt);
//    Console.WriteLine($"Asking to {modelId}");
//    timer.Reset();
//    timer.Start();
//    await foreach (var message in raggle.AskAsync(modelId, history))
//    {
//        Console.Write(message);
//    }
//    timer.Stop();
//    Console.WriteLine();
//    Console.WriteLine($"Elapsed Time: {timer.Elapsed}");
//    Console.WriteLine();
//}

//async Task Infer()
//{
//    var prompt = new ChatPromptBuilder()
//        .AddSystemMessage(systemPrompt)
//        .AddUserMessage(userPrompt)
//        .Build();

//    var modelPath = @"C:\Models\Meta-Llama-3.1-8B-Instruct-Q4_K_M.gguf";
//    var parameters = new ModelParams(modelPath)
//    {
//        ContextSize = 1024,
//        Seed = 0,
//    };

//    using var model = LLamaWeights.LoadFromFile(parameters);
//    //using var context = model.CreateContext(parameters);
//    //var inferParams = new InferenceParams();
//    //var decoder = new StreamingTokenDecoder(context);
//    //var antiprocessor = new AntipromptProcessor(inferParams.AntiPrompts);

//    //// Keep track of the last N tokens emitted
//    //var repeat_last_n = Math.Max(0, inferParams.RepeatLastTokensCount < 0 ? model.ContextSize : inferParams.RepeatLastTokensCount);
//    //var lastTokens = new List<LLamaToken>(repeat_last_n);
//    //for (var i = 0; i < repeat_last_n; i++)
//    //    lastTokens.Add(0);

//    //// Tokenize the prompt
//    //var tokens = context.Tokenize(prompt, special: true).ToList();
//    //lastTokens.AddRange(tokens);

//    //// Evaluate the prompt, in chunks smaller than the max batch size
//    //var n_past = 0;
//    //var batch = new LLamaBatch();
//    //var (r, _, past) = await context.DecodeAsync(tokens, LLamaSeqId.Zero, batch, n_past);
//    //n_past = past;

//    //if (r != DecodeResult.Ok)
//    //    throw new LLamaDecodeError(r);

//    //// use the explicitly supplied pipeline, if there is one. Otherwise construct a suitable one.
//    //var pipeline = inferParams.SamplingPipeline;

//    //var context = model.CreateContext(parameters);
//    //Console.WriteLine(context.Tokens.EOS);

//    prompt = "<|begin_of_text|><|start_header_id|>system<|end_header_id|>" +
//         systemPrompt +
//         "<|eot_id|><|start_header_id|>user<|end_header_id|>" +
//         userPrompt +
//         "<|eot_id|><|start_header_id|>assistant<|end_header_id|>";

//    Console.WriteLine("\n =============== Prompt ================= \n");
//    Console.WriteLine($"{prompt}");
//    Console.WriteLine("\n =============== Answer ================= \n");
//    //context.Dispose();
//    var executor = new StatelessExecutor(model, parameters);
//    //var executor = new InteractiveExecutor(context);
//    await foreach (var result in executor.InferAsync(prompt))
//    {
//        Console.Write(result);
//    }
//}

class ChatPromptBuilder
{
    private readonly StringBuilder builder = new();

    public string BeginPrompt { get; private set; } = "<|begin_of_text|>";
    public string EndPrompt { get; private set; } = "<|end_of_text|>";

    public string BeginSystem { get; private set; } = "<|start_header_id|>system<|end_header_id|>";
    public string EndSystem { get; private set; } = "<|eot_id|>";
    public string BeginUser { get; private set; } = "<|start_header_id|>user<|end_header_id|>";
    public string EndUser { get; private set; } = "<|eot_id|>";
    public string BeginAssistant { get; private set; } = "<|start_header_id|>assistant<|end_header_id|>";
    public string EndAssistant { get; private set; } = "<|eot_id|>";

    public ChatPromptBuilder AddSystemMessage(string message)
    {
        builder.Append($"{BeginSystem}\n{message}{EndSystem}");
        return this;
    }

    public ChatPromptBuilder AddUserMessage(string message)
    {
        builder.Append($"{BeginUser}\n{message}{EndUser}");
        return this;
    }

    public ChatPromptBuilder AddAssistantMessage(string message)
    {
        builder.Append($"{BeginAssistant}\n{message}{EndAssistant}");
        return this;
    }

    public string Build(bool withBOS = false)
    {
        builder.Append($"{BeginAssistant}");
        var prompt = builder.ToString();
        return withBOS ? $"{BeginPrompt}{prompt}" : prompt;
    }
}
