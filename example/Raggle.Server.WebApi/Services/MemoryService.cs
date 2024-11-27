using Microsoft.EntityFrameworkCore;
using Raggle.Abstractions;
using Raggle.Abstractions.Memory;
using Raggle.Server.WebApi.Configuration;
using Raggle.Server.WebApi.Data;
using Raggle.Server.WebApi.Models;

namespace Raggle.Server.WebApi.Services;

public class MemoryService
{
    private readonly AppDbContext _db;
    private readonly IRaggleMemory _memory;

    public MemoryService(AppDbContext dbContext, IRaggle raggle)
    {
        _db = dbContext;
        _memory = raggle.Memory;
    }

    #region Collection

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
        var existing = await _db.Collections.FindAsync(collection.CollectionId);
        var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            if (existing == null)
            {
                _db.Collections.Add(collection);
                await _memory.CreateCollectionAsync(
                    collection.CollectionId.ToString(),
                    collection.EmbedServiceKey,
                    collection.EmbedModelName);

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
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public async Task DeleteCollectionAsync(Guid collectionId)
    {
        var collection = await _db.Collections.FindAsync(collectionId)
            ?? throw new InvalidOperationException("Collection not found.");

        var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            _db.Collections.Remove(collection);
            await _memory.DeleteCollectionAsync(collection.CollectionId.ToString());

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    #endregion

    #region Document

    public async Task<IEnumerable<DocumentModel>> FindDocumentsAsync(
        Guid collectionId,
        string? fileName = null,
        int limit = 10,
        int skip = 0,
        string order = "desc")
    {
        var query = _db.Documents.AsQueryable();

        if (fileName != null)
            query = query.Where(c => c.FileName.Contains(fileName));

        if (order == "desc")
            query = query.OrderByDescending(c => c.LastUpdatedAt);
        else
            query = query.OrderBy(c => c.ContentType);

        var documents = await query.Skip(skip).Take(limit).ToArrayAsync();
        return documents;
    }

    public async Task UploadDocumentAsync(Guid collectionId, DocumentModel document, Stream data)
    {
        var collection = await _db.Collections.FindAsync(collectionId)
            ?? throw new InvalidOperationException("Collection not found.");

        var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            await _db.Documents.AddAsync(document);

            await _memory.MemorizeDocumentAsync(
                collectionName: collection.CollectionId.ToString(),
                documentId: document.DocumentId.ToString(),
                fileName: document.FileName,
                content: data,
                tags: document.Tags?.ToArray(),
                steps:
                [
                    HandlerServiceKeys.Decoding.ToString(),
                    HandlerServiceKeys.Chunking.ToString(),
                    HandlerServiceKeys.GenerateQA.ToString(),
                    HandlerServiceKeys.Embeddings.ToString(),
                ],
                metadata: collection.HandlerOptions);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public async Task DeleteDocumentAsync(Guid collectionId, Guid documentId)
    {
        var collection = await _db.Collections.FindAsync(collectionId)
            ?? throw new InvalidOperationException("Collection not found.");

        var document = await _db.Documents.FindAsync(documentId)
            ?? throw new InvalidOperationException("Document not found.");

        var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            _db.Documents.Remove(document);
            await _memory.UnMemorizeDocumentAsync(collectionId.ToString(), documentId.ToString());

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    #endregion
}
