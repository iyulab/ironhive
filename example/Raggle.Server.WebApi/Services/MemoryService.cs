using Microsoft.EntityFrameworkCore;
using Raggle.Abstractions;
using Raggle.Abstractions.Memory;
using Raggle.Server.WebApi.Data;
using Raggle.Server.WebApi.Models;

namespace Raggle.Server.WebApi.Services;

public class MemoryService
{
    private readonly AppDbContext _db;
    private readonly IRaggle _raggle;

    private IRaggleMemory? _memory => _raggle.Services.GetService<IRaggleMemory>();

    public MemoryService(AppDbContext dbContext, IRaggle raggle)
    {
        _db = dbContext;
        _raggle = raggle;
    }

    public async Task<IEnumerable<CollectionModel>> FindCollectionsAsync(
        string? name = null,
        int limit = 10,
        int skip = 0,
        string order = "desc")
    {
        var query = _db.Collections.AsQueryable();

        if (name != null)
            query = query.Where(c => c.Name.Contains(name));

        if (order == "desc")
            query = query.OrderByDescending(c => c.CreatedAt);
        else
            query = query.OrderBy(c => c.CreatedAt);

        var collections = await query.Skip(skip).Take(limit).ToArrayAsync();
        return collections;
    }

    public async Task<CollectionModel> UpsertCollectionAsync(CollectionModel collection)
    {
        var existing = await _db.Collections.FindAsync(collection.Id);
        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            if (existing == null)
            {
                _db.Collections.Add(collection);
                await _memory.CreateCollectionAsync(
                    collection.Id.ToString(),
                    collection.EmbedProvider.ToString(),
                    collection.EmbedModel);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return collection;
            }
            else
            {
                existing.Name = collection.Name;
                existing.Description = collection.Description;
                existing.LastUpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return existing;
            }
        }
        catch
        {
            throw;
        }
    }

    public async Task DeleteCollectionAsync(Guid id)
    {
        var collection = await _db.Collections.FindAsync(id);

        if (collection != null)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.Collections.Remove(collection);
                await _memory.DeleteCollectionAsync(collection.Id.ToString());

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                throw;
            }
        }
        else
        {
            throw new InvalidOperationException("Collection not found.");
        }
    }
}
