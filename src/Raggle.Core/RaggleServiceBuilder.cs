using Microsoft.KernelMemory;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using Raggle.Abstractions;
using Microsoft.KernelMemory.AI.OpenAI;
using Raggle.Abstractions.Attributes;
using static Microsoft.KernelMemory.OpenAIConfig;
using Raggle.Core.Options.Platforms;
using Raggle.Core.Options.VectorDB;
using Raggle.Core.Options.Prompts;
using Raggle.Core.Prompts;
using Raggle.Abstractions.Prompts;

#pragma warning disable KMEXP01

namespace Raggle.Core;

public class RaggleServiceBuilder : IRaggleServiceBuilder
{
    public IKernelBuilder KernelBuilder { get; set; } = Kernel.CreateBuilder();
    public IKernelMemoryBuilder MemoryBuilder { get; set; } = new KernelMemoryBuilder();
    public IPromptProvider? PromptProvider { get; set; }

    public IRaggleService Build()
    {
        var memory = MemoryBuilder.Build();
        var chat = KernelBuilder.Build().GetRequiredService<IChatCompletionService>();
        return new RaggleService(chat, memory, PromptProvider);
    }
}

public static partial class RaggleServiceBuilderExtension
{
    public static IRaggleServiceBuilder WithOpenAI(
        this IRaggleServiceBuilder builder,
        OpenAIOption option)
    {
        var textGenerationTokenizer = new GPT4oTokenizer();
        var textEmbeddingTokenizer = new GPT4oTokenizer();

        var openAIConfig = new OpenAIConfig
        {
            APIKey = option.ApiKey,
            TextGenerationType = TextGenerationTypes.Chat,
            TextModel = option.TextModel,
            TextModelMaxTokenTotal = option.TextModelMaxToken,
            EmbeddingModel = option.EmbeddingModel,
            EmbeddingModelMaxTokenTotal = option.EmbeddingModelMaxToken,
            MaxEmbeddingBatchSize = option.MaxEmbeddingBatchSize,
            MaxRetries = option.MaxRetries,
        };
        openAIConfig.Validate();

        builder.MemoryBuilder.Services.AddOpenAITextEmbeddingGeneration(openAIConfig, textEmbeddingTokenizer);
        builder.MemoryBuilder.Services.AddOpenAITextGeneration(openAIConfig, textGenerationTokenizer);

        //builder.MemoryBuilder.AddIngestionEmbeddingGenerator(new OpenAITextEmbeddingGenerator(
        //    config: openAIConfig,
        //    textTokenizer: textEmbeddingTokenizer
        //));

        builder.KernelBuilder.AddOpenAIChatCompletion(
            modelId: option.TextModel,
            apiKey: option.ApiKey
        );

        return builder;
    }

    public static IRaggleServiceBuilder WithFileVectorDB(
        this IRaggleServiceBuilder builder,
        FileVectorDBOption option)
    {
        builder.MemoryBuilder.WithSimpleFileStorage(new SimpleFileStorageConfig
        {
            StorageType = FileSystemTypes.Volatile
        })
        .WithSimpleVectorDb(new SimpleVectorDbConfig
        {
            Directory = option.VectorDirectory,
            StorageType = FileSystemTypes.Disk
        });

        return builder;
    }

    public static IRaggleServiceBuilder WithSimplePrompt(
        this IRaggleServiceBuilder builder,
        SimplePromptOption option)
    {
        builder.PromptProvider = new SimplePromptProvider(option);
        return builder;
    }
}