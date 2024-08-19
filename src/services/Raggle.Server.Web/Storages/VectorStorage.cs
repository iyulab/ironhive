using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI.OpenAI;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using Raggle.Server.Web.Models;
using Raggle.Server.Web.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

#pragma warning disable KMEXP01

namespace Raggle.Server.Web.Storages;

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
                Directory = Path.Combine(_baseDirectory, "files"),
                StorageType = FileSystemTypes.Disk,
            })
            .WithSimpleVectorDb(new SimpleVectorDbConfig
            {
                Directory = _baseDirectory,
                StorageType = FileSystemTypes.Disk
            })
            .AddIngestionEmbeddingGenerator(new OpenAITextEmbeddingGenerator(
                config: openAIConfig,
                textTokenizer: textEmbeddingTokenizer
            ));

        _memory = builder.Build();
        _file = fileStorage;
    }

    public async Task<ICollection<SearchReference>> SearchAsync(string query, Guid knowledgeId, ICollection<string>? filenames)
    {
        var results = new List<SearchReference>();
        var filters = new List<MemoryFilter>();
        
        if (filenames != null)
        {
            foreach (var filename in filenames)
            {
                var documentId = GetDocumentId(knowledgeId, filename);
                filters.Add(MemoryFilters.ByDocument(documentId));
            }
        }
        var result = await _memory.SearchAsync(
            query: query,
            index: knowledgeId.ToString(),
            filters: filters,
            minRelevance: 0.3,
            limit: 10);

        if (result.Results.Count > 0)
        {
            foreach (var r in result.Results)
            {
                foreach (var p in r.Partitions)
                {
                    results.Add(new SearchReference
                    {
                        Name = r.SourceName,
                        Type = r.SourceContentType,
                        Relevance = p.Relevance,
                        Content = Regex.Unescape(p.Text),
                    });
                }
            }
            return results;
        }

        return results;
    }

    public async Task<IEnumerable<DataPipelineStatus>> GetKnowledgeStatusAsync(Knowledge knowledge)
    {
        var statusList = new List<DataPipelineStatus>();
        foreach (var file in knowledge.Files)
        {
            var documentId = GetDocumentId(knowledge.ID, file.Name);
            var status = await GetDocumentStatusAsync(documentId, knowledge.ID);
            statusList.Add(status);
        }
        return statusList;
    }

    public async Task<DataPipelineStatus?> GetDocumentStatusAsync(string documentId, Guid knowledgeId)
    {
        var status = await _memory.GetDocumentStatusAsync(documentId: documentId, index: knowledgeId.ToString());
        return status;
    }

    public async Task MemorizeDocumentAsync(Guid knowledgeId, string documentId, string fileName, Stream content)
    {
        await _memory.ImportDocumentAsync(
            fileName: fileName,
            content: content,
            documentId: documentId,
            index: knowledgeId.ToString());
    }

    public async Task MemorizeKnowledgeAsync(Knowledge knowledge)
    {
        var knowledgeId = knowledge.ID;
        var files = _file.GetUploadedFiles(knowledgeId.ToString());
        foreach (var file in files)
        {
            var documentId = GetDocumentId(knowledgeId, file.FileName);
            await MemorizeDocumentAsync(knowledgeId, documentId, file.FileName, file.Content);
        }
        _file.DeleteDirectory(knowledgeId.ToString());
    }

    public async Task ReMemorizeKnowledgeAsync(Knowledge oldKnowledge, Knowledge newKnowledge)
    {
        var knowledgeId = oldKnowledge.ID;
        var oldFileNames = oldKnowledge.Files.Select(f => f.Name).ToList();
        var newFileNames = newKnowledge.Files.Select(f => f.Name).ToList();

        // 삭제된 파일 처리
        var deletedFiles = oldFileNames.Except(newFileNames).ToList();
        foreach (var deletedFile in deletedFiles)
        {
            await DeleteDocumentAsync(knowledgeId, deletedFile);
        }

        await MemorizeKnowledgeAsync(newKnowledge);
    }

    public async Task DeleteDocumentAsync(Guid knowledgeId, string filename)
    {
        _file.DeleteFile(knowledgeId.ToString(), filename);
        var documentId = GetDocumentId(knowledgeId, filename);
        await _memory.DeleteDocumentAsync(documentId: documentId, index: knowledgeId.ToString());
    }

    public async Task DeleteKnowledgeAsync(Guid knowledgeId)
    {
        _file.DeleteDirectory(knowledgeId.ToString());
        await _memory.DeleteIndexAsync(knowledgeId.ToString());
    }

    public string GetDocumentId(Guid knowledgeId, string fileName)
    {
        var content = $"{knowledgeId}{fileName}";
        var encryption = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(encryption).ToUpperInvariant();
    }
}