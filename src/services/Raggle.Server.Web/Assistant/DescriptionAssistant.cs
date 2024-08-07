using Microsoft.SemanticKernel;
using Raggle.Server.API.Models;

# pragma warning disable SKEXP0110

namespace Raggle.Server.API.Assistant;

public class DescriptionAssistant
{
    private readonly Kernel _kernel;
    private readonly ILogger<SearchAssistant> _logger;

    public DescriptionAssistant(IConfiguration config, ILogger<SearchAssistant> logger)
    {
        _logger = logger;
        var option = config.GetSection("OpenAI").Get<OpenAIOptions>();
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId: option.ChatModel, apiKey: option.ApiKey)
            .Build();
    }

    public async IAsyncEnumerable<string> Describe(string sourceId)
    {
        var prompt = $"""
        Describe the data.
        {sourceId}
        """;
        await foreach (var item in _kernel.InvokePromptStreamingAsync(prompt))
        {
            yield return item.InnerContent.ToString();
        }
    }
}
