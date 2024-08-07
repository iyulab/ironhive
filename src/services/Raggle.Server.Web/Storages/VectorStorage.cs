using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI.OpenAI;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using Raggle.Server.API.Models;

namespace Raggle.Server.API.Storages;

public class VectorStorage
{
    private readonly string _baseDirectory;
    private readonly IKernelMemory _memory;

    public VectorStorage(IConfiguration config)
    {
        _baseDirectory = config.GetSection("VectorStorage:Path").Value;
        var openAIOption = config.GetSection("OpenAI").Get<OpenAIOptions>();
        var openAIConfig = new OpenAIConfig
        {
            APIKey = openAIOption.ApiKey,
            TextModel = openAIOption.ChatModel,
            EmbeddingModel = openAIOption.EmbeddingModel,
        };
        openAIConfig.Validate();

        var builder = new KernelMemoryBuilder();
        var textGenerationTokenizer = new GPT4oTokenizer();
        var textEmbeddingTokenizer = new GPT4oTokenizer();
        builder.Services.AddOpenAITextEmbeddingGeneration(openAIConfig, textEmbeddingTokenizer);
        builder.Services.AddOpenAITextGeneration(openAIConfig, textGenerationTokenizer);
        builder.WithSimpleVectorDb(new SimpleVectorDbConfig
         {
             Directory = _baseDirectory,
             StorageType = FileSystemTypes.Disk
         });
        _memory = builder.Build();
    }
}
