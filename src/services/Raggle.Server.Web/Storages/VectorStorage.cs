using Amazon.Util;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI.OpenAI;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using Microsoft.KernelMemory.Pipeline;
using Raggle.Server.API.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

#pragma warning disable KMEXP01

namespace Raggle.Server.API.Storages;

public class VectorStorage
{
    private readonly string _baseDirectory;
    private readonly IKernelMemory _memory;
    private readonly FileStorage _file;

    public VectorStorage(IConfiguration config, FileStorage fileStorage)
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
        builder
            .WithSimpleFileStorage(new SimpleFileStorageConfig
            {
                StorageType = FileSystemTypes.Volatile
            })
            .WithSimpleVectorDb(new SimpleVectorDbConfig
            {
                Directory = _baseDirectory,
                StorageType = FileSystemTypes.Disk
            });
            //.AddIngestionEmbeddingGenerator(new OpenAITextEmbeddingGenerator(
            //    config: openAIConfig,
            //    textTokenizer: textEmbeddingTokenizer
            //));

        _memory = builder.Build();
        _file = fileStorage;
    }

    private IKernelMemory GetMemory(OpenAIOptions options, string baseDir)
    {
        return _memory;
    }

    public async Task<DataPipelineStatus?> GetDocumentStatusAsync(string documentId, string index)
    {
        var status = await _memory.GetDocumentStatusAsync(documentId: documentId, index: index);
        return status;
    }

    public async Task<SearchResult> SearchAsync(string query, string index, ICollection<MemoryFilter> filters)
    {
        var result = await _memory.SearchAsync(
            query: query,
            index: index,
            filters: filters);
        return result;
    }

    public async Task MemorizeTextAsync(string text, string documentId, string index)
    {
        await _memory.ImportTextAsync(
            text: text,
            documentId: documentId,
            index: index);
    }

    public async Task MemorizeDocumentAsync(string sourceId, string documentId, string fileName, Stream content)
    {
        await _memory.ImportDocumentAsync(
            fileName: fileName,
            content: content,
            documentId: documentId,
            index: sourceId);
    }

    public async Task DeleteDocumentAsync(string index, string documentId)
    {
        await _memory.DeleteDocumentAsync(documentId: documentId, index: index);
    }

    public async Task DeleteIndexAsync(string index)
    {
        await _memory.DeleteIndexAsync(index);        
    }

    public async Task MemorizeSourceAsync(DataSource source)
    {
        var sourceId = source.ID;
        if (source.Type == "file")
        {
            var files = _file.GetUploadedFiles(sourceId.ToString());
            foreach (var file in files)
            {
                var documentId = GetDocumentId(sourceId, file.FileName);
                await MemorizeDocumentAsync(sourceId.ToString(), documentId, file.FileName, file.Content);
            }
            _file.DeleteDirectory(sourceId.ToString());
        }
    }

    public async Task ReMemorizeSourceAsync(DataSource oldSource, DataSource newSource)
    {
        if (newSource.Type == "file")
        {
            if (oldSource.Details.HasValue && newSource.Details.HasValue)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var oldFileDetails = JsonSerializer.Deserialize<FileDetails>(oldSource.Details.Value.GetRawText(), options);
                var newFileDetails = JsonSerializer.Deserialize<FileDetails>(newSource.Details.Value.GetRawText(), options);
                var oldFileNames = oldFileDetails?.Files.Select(f => f.Name).ToList() ?? [];
                var newFileNames = newFileDetails?.Files.Select(f => f.Name).ToList() ?? [];

                var deletedFiles = oldFileNames.Except(newFileNames).ToList();
                foreach (var deletedFile in deletedFiles)
                {
                    await DeleteDocumentAsync(oldSource.ID.ToString(), GetDocumentId(oldSource.ID, deletedFile));
                }
            }
            await MemorizeSourceAsync(newSource);
        }
    }

    private string GetDocumentId(Guid sourceId, string fileName)
    {
        var content = $"{sourceId}{fileName}";
        var encryption = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(encryption).ToUpperInvariant();
    }
}