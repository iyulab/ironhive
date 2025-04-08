using IronHive.Abstractions.Memory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WebApiSample.Data;

namespace WebApiSample.Services;

public class PipelineWorkerObserver : IPipelineObserver
{
    private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly AppDbContext _db;

    public PipelineWorkerObserver(AppDbContext db)
    {
        _db = db;
    }

    public Task OnStartedAsync(string sourceId)
    {
        // Initialize the file status to Processing
        var file = _db.Files.FirstOrDefault(f => f.Id == sourceId)
            ?? throw new InvalidOperationException($"File with ID {sourceId} not found.");

        file.Status = VectorizationStatus.Processing;
        file.StatusMessage = "Vectorization in progress...";
        file.LastUpdatedAt = DateTime.UtcNow;
        _db.Files.Update(file);
        _db.SaveChanges();
        return Task.CompletedTask;
    }

    public async Task OnProcessBeforeAsync(string sourceId, string step, PipelineContext context)
    {
        _cache.Set(sourceId, step, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            Priority = CacheItemPriority.Normal,
        });

        await Task.CompletedTask;
    }

    public Task OnProcessAfterAsync(string sourceId, string step, PipelineContext context)
    {
        // Process after logic can be implemented here if needed
        return Task.CompletedTask;
    }

    public async Task OnCompletedAsync(string sourceId)
    {
        var file = await _db.Files.FirstOrDefaultAsync(f => f.Id == sourceId)
            ?? throw new InvalidOperationException($"File with ID {sourceId} not found.");

        file.Status = VectorizationStatus.Completed;
        file.StatusMessage = "Vectorization completed successfully.";
        file.LastUpdatedAt = DateTime.UtcNow;
        _db.Files.Update(file);
        await _db.SaveChangesAsync();
    }

    public async Task OnFailedAsync(string sourceId, Exception exception)
    {
        var file = await _db.Files.FirstOrDefaultAsync(f => f.Id == sourceId)
            ?? throw new InvalidOperationException($"File with ID {sourceId} not found.");

        file.Status = VectorizationStatus.Failed;
        file.LastUpdatedAt = DateTime.UtcNow;
        file.StatusMessage = exception.Message;
        _db.Files.Update(file);
        await _db.SaveChangesAsync();
    }
}
