using ClosedXML;
using LLama;
using LLama.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Tools;
using Raggle.ConsoleTests;
using Raggle.Engines.OpenAI.ChatCompletion;
using Raggle.Engines.OpenAI.Embeddings;
using Raggle.Services;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ChatHistory = Microsoft.SemanticKernel.ChatCompletion.ChatHistory;

//var text = await File.ReadAllTextAsync(@"C:\data\Raggle\src\Raggle.ConsoleTests\Secrets.json");
//var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(text);
//var key = secrets?.GetValueOrDefault("OpenAI") ?? string.Empty;

var services = new ServiceCollection();
services.AddSingleton<DepenDency>();
var provider = services.BuildServiceProvider();
var tools = FunctionToolFactory.CreateFromType<Example>(provider, "PP");

foreach (var tool in tools)
{
    if (tool.Name == "Print")
    {
        Console.WriteLine("Print Method");
        await tool.InvokeAsync();
        Console.WriteLine("Print Method End");
    }
    if (tool.Name == "PrintDependency")
    {
        Console.WriteLine("PrintDependency Method");
        await tool.InvokeAsync("Hello", "World");
        Console.WriteLine("PrintDependency Method End");
    }
    if (tool.Name == "PrintDependencyAsync")
    {
        Console.WriteLine("PrintDependencyAsync Method");
        await tool.InvokeAsync("Hello", "World");
        Console.WriteLine("PrintDependencyAsync Method End");
    }
}

return;

//NativeLibraryConfig.All.WithLogCallback((level, message) => { });

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

var systemPrompt = """

    Environment: ipython
    Tools: brave_search, wolfram_alpha
    Cutting Knowledge Date: December 2023
    Today Date: 23 July 2024

    You are a helpful assistant
    """;
var userPrompt = """

    Can you help me solve this equation: x^3 - 4x^2 + 6x - 24 = 0
    """;

async Task Infer()
{
    var prompt = new ChatPromptBuilder()
        .AddSystemMessage(systemPrompt)
        .AddUserMessage(userPrompt)
        .Build();

    var modelPath = @"C:\Models\Meta-Llama-3.1-8B-Instruct-Q4_K_M.gguf";
    var parameters = new ModelParams(modelPath)
    {
        ContextSize = 1024,
        Seed = 0,
    };
    
    using var model = LLamaWeights.LoadFromFile(parameters);
    //using var context = model.CreateContext(parameters);
    //var inferParams = new InferenceParams();
    //var decoder = new StreamingTokenDecoder(context);
    //var antiprocessor = new AntipromptProcessor(inferParams.AntiPrompts);

    //// Keep track of the last N tokens emitted
    //var repeat_last_n = Math.Max(0, inferParams.RepeatLastTokensCount < 0 ? model.ContextSize : inferParams.RepeatLastTokensCount);
    //var lastTokens = new List<LLamaToken>(repeat_last_n);
    //for (var i = 0; i < repeat_last_n; i++)
    //    lastTokens.Add(0);

    //// Tokenize the prompt
    //var tokens = context.Tokenize(prompt, special: true).ToList();
    //lastTokens.AddRange(tokens);

    //// Evaluate the prompt, in chunks smaller than the max batch size
    //var n_past = 0;
    //var batch = new LLamaBatch();
    //var (r, _, past) = await context.DecodeAsync(tokens, LLamaSeqId.Zero, batch, n_past);
    //n_past = past;

    //if (r != DecodeResult.Ok)
    //    throw new LLamaDecodeError(r);

    //// use the explicitly supplied pipeline, if there is one. Otherwise construct a suitable one.
    //var pipeline = inferParams.SamplingPipeline;

    //var context = model.CreateContext(parameters);
    //Console.WriteLine(context.Tokens.EOS);
    
    //prompt = "<|begin_of_text|><|start_header_id|>system<|end_header_id|>" +
    //     systemPrompt +
    //     "<|eot_id|><|start_header_id|>user<|end_header_id|>" +
    //     userPrompt +
    //     "<|eot_id|><|start_header_id|>assistant<|end_header_id|>";

    Console.WriteLine("\n =============== Prompt ================= \n");
    Console.WriteLine($"{prompt}");
    Console.WriteLine("\n =============== Answer ================= \n");
    //context.Dispose();
    var executor = new StatelessExecutor(model, parameters);
    //var executor = new InteractiveExecutor(context);
    await foreach (var result in executor.InferAsync(prompt))
    {
        Console.Write(result);
    }
}

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
