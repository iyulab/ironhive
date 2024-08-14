using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Raggle.Server.Web.Options;

# pragma warning disable SKEXP0110

namespace Raggle.Server.Web.Services;

public class ChatGenerateService
{
    private readonly Kernel _kernel;

    public ChatGenerateService(IOptions<OpenAIOptions> option)
    {
        _kernel = CreateKernel(option.Value);
    }

    public async IAsyncEnumerable<StreamingKernelContent> ExplainAsync(string query)
    {
        var prompt = $"""
        Describe the below information:

        {query}
        """;
        await foreach (var item in _kernel.InvokePromptStreamingAsync(prompt))
        {
            yield return item;
        }
    }

    private static Kernel CreateKernel(OpenAIOptions options)
    {
        return Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId: options.ChatModel, apiKey: options.ApiKey)
            .Build();
    }
}
