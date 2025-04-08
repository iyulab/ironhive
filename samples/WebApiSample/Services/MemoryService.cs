using IronHive.Abstractions.Memory;
using IronHive.Core.Handlers;
using Microsoft.EntityFrameworkCore;
using WebApiSample.Data;
using WebApiSample.Settings;

namespace WebApiSample.Services;

public class MemoryService
{
    private readonly string _embeddingModel;
    private readonly string _collectionName;
    private readonly IVectorMemory _memory;
    private readonly AppDbContext _db;
    private readonly IEnumerable<string> _pipelines;
    private readonly IDictionary<string, object?> _pipelineOptions;

    public MemoryService(IVectorMemory memory, AppDbContext db, MemorySettings settings)
    {
        _memory = memory;
        _db = db;
        _embeddingModel = settings.EmbeddingModel;
        _collectionName = AppUtility.ConvertCollectionName(_embeddingModel);

        if (string.IsNullOrWhiteSpace(settings.QnAGenerationModel))
        {
            _pipelines = new[] 
            {
                AppConstants.ExtractStep,
                AppConstants.ChunkStep,
                AppConstants.EmbeddingStep 
            };
            _pipelineOptions = new Dictionary<string, object?>
            {
                { AppConstants.ChunkStep, new TextChunkerHandler.Options
                {
                    ChunkSize = settings.ChunkSize,
                }}
            };
        }
        else
        {
            var (provider, model) = AppUtility.ParseModelIdentifier(settings.QnAGenerationModel);
            _pipelines = new[]
            {
                AppConstants.ExtractStep,
                AppConstants.ChunkStep,
                AppConstants.QnAStep,
                AppConstants.EmbeddingStep
            };
            _pipelineOptions = new Dictionary<string, object?>
            {
                { AppConstants.ChunkStep, new TextChunkerHandler.Options
                {
                    ChunkSize = settings.ChunkSize,
                }},
                { AppConstants.QnAStep, new QnAExtractionHandler.Options
                {
                    Provider = provider,
                    Model = model,
                }}
            };
        }
    }

    public async Task<IEnumerable<FileEntity>> ListFilesAsync(string? prefix = null)
    {
        var files = await _db.Files.Where(f => f.EmbeddingModel == _embeddingModel)
            .Where(f => string.IsNullOrEmpty(prefix) || f.FilePath.StartsWith(prefix))
            .OrderByDescending(f => f.LastUpdatedAt)
            .ToListAsync();
        return files;
    }

    public async Task<FileEntity?> GetFileAsync(string filePath)
    {
        var file = await _db.Files.FirstOrDefaultAsync(f => f.FilePath == filePath);
        return file;
    }

    public async Task<FileEntity> AddFileAsync(string filePath)
    {
        var file = new FileEntity
        {
            Id = Guid.NewGuid().ToString(),
            FilePath = filePath,
            EmbeddingModel = _embeddingModel,
            Status = VectorizationStatus.Queued,
            LastUpdatedAt = DateTime.UtcNow
        };
        _db.Files.Add(file);
        await _db.SaveChangesAsync();

        // 메모리에 추가
        await _memory.ScheduleVectorizationAsync(
            _collectionName,
            new FileMemorySource
            {
                Id = file.Id,
                Provider = AppConstants.LocalFileStorage,
                FilePath = filePath,
                FileSize = new FileInfo(filePath).Length,
            },
            _pipelines,
            _pipelineOptions);

        return file;
    }

    public async Task<FileEntity> UpdateFileAsync(string filePath)
    {
        // db에서 찾기
        var file = await _db.Files.FirstOrDefaultAsync(f => f.FilePath == filePath)
            ?? throw new InvalidOperationException($"File '{filePath}' not found.");

        // vector db에서 삭제
        await _memory.DeleteVectorsAsync(
            _collectionName,
            file.Id);

        // vector db에 다시 추가
        await _memory.ScheduleVectorizationAsync(
            _collectionName,
            new FileMemorySource
            {
                Id = file.Id,
                Provider = AppConstants.LocalFileStorage,
                FilePath = filePath,
                FileSize = new FileInfo(filePath).Length,
            },
            _pipelines,
            _pipelineOptions);

        // db에서 업데이트
        file.Status = VectorizationStatus.Queued;
        file.LastUpdatedAt = DateTime.UtcNow;
        _db.Files.Update(file);
        await _db.SaveChangesAsync();
        return file;
    }

    public async Task RemoveFileAsync(string filePath)
    {
        // db에서 찾기
        var file = await _db.Files.FirstOrDefaultAsync(f => f.FilePath == filePath)
            ?? throw new InvalidOperationException($"File '{filePath}' not found.");

        // vector db에서 삭제
        await _memory.DeleteVectorsAsync(
            _collectionName,
            file.Id);

        // db에서 삭제
        _db.Files.Remove(file);
        await _db.SaveChangesAsync();
    }

}
