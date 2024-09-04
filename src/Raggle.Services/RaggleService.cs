using LLama;
using LLama.Common;
using LLamaSharp.KernelMemory;
using LLamaSharp.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI;
using Microsoft.KernelMemory.AI.OpenAI;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using AuthorRole = Microsoft.SemanticKernel.ChatCompletion.AuthorRole;
using ChatHistory = Microsoft.SemanticKernel.ChatCompletion.ChatHistory;
using LlamaSharpTextGenerator = LLamaSharp.KernelMemory.LlamaSharpTextGenerator;

#pragma warning disable KMEXP01
#pragma warning disable SKEXP0001

namespace Raggle.Services;

public class RaggleService
{
    private ServiceOptions _options;
    public IKernelMemory? Memory { get; private set; }

    // Test
    public IDictionary<string, LLamaWeights> CachedModels { get; private set; } = new Dictionary<string, LLamaWeights>();

    public RaggleService(IOptionsMonitor<ServiceOptions> options)
    {
        _options = options.CurrentValue;
        BuildMemory();
        options.OnChange((newOptions) =>
        {
            _options = newOptions;
            BuildMemory();
        });
    }

    public LLamaWeights GetCachedModel(string modelId, ModelParams parameters)
    {
        if (CachedModels.ContainsKey(modelId))
        {
            return CachedModels[modelId];
        }

        if (CachedModels.Count > 2)
        {
            CachedModels.Remove(CachedModels.Keys.First(), out var onModel);
            onModel?.Dispose();
        }

        var model = LLamaWeights.LoadFromFile(parameters);
        CachedModels[modelId] = model;
        return model;
    }

    public async IAsyncEnumerable<string> AskAsync(string modelId, ChatHistory history)
    {
        var query = history.Last().Content;
        var memory = await CheckMemoryAsync();
        var searchResult = await memory.SearchAsync(query!);
        var information = string.Empty;
        foreach (var result in searchResult.Results)
        {
            result.Partitions.ForEach(partition =>
            {
                information += partition.Text + "\n";
            });
        }

        IChatCompletionService ? chat = null;
        if (modelId.StartsWith("openai"))
        {
            var model = modelId.Split("/")[1];
            chat = new OpenAIChatCompletionService(model, _options.OpenAIKey);
        }
        else
        {
            var modelPath = Path.Combine(_options.ModelDirectory, $"{modelId}.gguf");
            var parameters = new ModelParams(modelPath)
            {
                ContextSize = 1024,
            };
            var model = GetCachedModel(modelId, parameters);
            var executor = new StatelessExecutor(model, parameters);
            chat = new LLamaSharpChatCompletion(executor);
        }

        if (chat == null)
        {
            throw new Exception("Chat model not found");
        }

        var systemMessage = $"you have bellow information in your memory:\n {information}";
        if (history.Count == 0)
        {
            history.AddSystemMessage(systemMessage);
        }
        else if (history[0].Role != AuthorRole.System)
        {
            history.Insert(0, new ChatMessageContent(AuthorRole.System, systemMessage));
        }
        else
        {
            history[0].Content = systemMessage;
        }

        await foreach (var message in chat.GetStreamingChatMessageContentsAsync(history))
        {
            if (message.Content != null)
            {
                yield return message.Content;
            }
        }
    }

    public async Task<DataPipelineStatus?> GetDocumentStatusAsync(string index, string documentId)
    {
        var memory = await CheckMemoryAsync();
        return await memory.GetDocumentStatusAsync(documentId, index);
    }

    public async Task<bool> IsMemorizedAsync(string index, string documentId)
    {
        var memory = await CheckMemoryAsync();
        return await memory.IsDocumentReadyAsync(documentId, index);
    }

    public async Task<string> MemorizeAsync(string index, Document document)
    {
        var memory = await CheckMemoryAsync();
        return await memory.ImportDocumentAsync(document, index);
    }

    public async Task ReMemorizeAsync(string index, string documentId, Document document)
    {
        var memory = await CheckMemoryAsync();
        document.Id = documentId;
        await memory.DeleteDocumentAsync(documentId, index);
        await memory.ImportDocumentAsync(document, index);
    }

    public async Task UnMemorizeAsync(string index, string documentId)
    {
        var memory = await CheckMemoryAsync();
        await memory.DeleteDocumentAsync(documentId, index);
    }

    public void BuildMemory()
    {
        Memory = null;
        
        var embeddingModel = _options.EmbeddingModel;
        var memoryDirectory = _options.MemoryDirectory;
        var fileDirectory = Path.Combine(memoryDirectory, "files");
        var vectorDirectory = Path.Combine(memoryDirectory, "vectors");

        if (!Directory.Exists(fileDirectory))
        {
            Directory.CreateDirectory(fileDirectory);
        }
        if (!Directory.Exists(vectorDirectory))
        {
            Directory.CreateDirectory(vectorDirectory);
        }

        ITextEmbeddingGenerator? embeddingGenerator;
        ITextGenerator? textGenerator;
        if (embeddingModel.StartsWith("openai"))
        {
            var modelId = embeddingModel.Split("/")[1];
            var config = new OpenAIConfig
            {
                APIKey = _options.OpenAIKey,
                TextModel = "gpt-4o",
                EmbeddingModel = modelId,
            };
            config.Validate();

            embeddingGenerator = new OpenAITextEmbeddingGenerator(config);
            textGenerator = new OpenAITextGenerator(config);;
        }
        else
        {
            var modelPath = Path.Combine(_options.ModelDirectory, $"{embeddingModel}.gguf");
            var config = new LLamaSharpConfig(modelPath)
            {
                DefaultInferenceParams = new InferenceParams
                {
                    AntiPrompts = ["\n\n"]
                },
            };

            embeddingGenerator = new LLamaSharpTextEmbeddingGenerator(config);
            textGenerator = new LlamaSharpTextGenerator(config);
        }

        if (textGenerator == null || embeddingGenerator == null)
        {
            throw new Exception("Text generator or embedding generator not found");
        }

        var builder = new KernelMemoryBuilder();
        builder.Services.AddSingleton<ITextEmbeddingGenerator>(embeddingGenerator);
        builder.Services.AddSingleton<ITextGenerator>(textGenerator);
        builder.AddIngestionEmbeddingGenerator(embeddingGenerator);

        builder.WithSimpleFileStorage(new SimpleFileStorageConfig
        {
            Directory = fileDirectory,
            StorageType = FileSystemTypes.Disk
        }).WithSimpleVectorDb(new SimpleVectorDbConfig
        {
            Directory = vectorDirectory,
            StorageType = FileSystemTypes.Disk
        });

        Memory = builder.Build();
    }

    private async Task<IKernelMemory> CheckMemoryAsync()
    {
        var count = 5;
        var delay = 1_000;
        foreach (var _ in Enumerable.Range(0, count))
        {
            if (Memory != null)
            {
                return Memory;
            }
            await Task.Delay(delay);
        }
        throw new MemoryTimeoutException("Memory not initialized, try again later");
    }
}

public class MemoryTimeoutException : Exception
{
    public MemoryTimeoutException(string message) : base(message)
    {
    }

    public MemoryTimeoutException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
