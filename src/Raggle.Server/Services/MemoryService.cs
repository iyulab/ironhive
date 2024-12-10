using Google.Protobuf;
using Microsoft.EntityFrameworkCore;
using Raggle.Abstractions;
using Raggle.Abstractions.Memory;
using Raggle.Server.Data;
using Raggle.Server.Entities;
using System.Text.Json.Nodes;

namespace Raggle.Server.Services;

public class MemoryService
{
    private readonly RaggleDbContext _db;
    private readonly IRaggleMemory _memory;

    public MemoryService(RaggleDbContext dbContext, IRaggle raggle)
    {
        _db = dbContext;
        _memory = raggle.Memory;
    }

    #region Collection

    public async Task<IEnumerable<CollectionEntity>> FindCollectionsAsync(
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

    public async Task<CollectionEntity> UpsertCollectionAsync(CollectionEntity collection)
    {
        var existing = await _db.Collections.FindAsync(collection.CollectionId);
        var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            if (existing == null)
            {
                _db.Collections.Add(collection);
                await _memory.CreateCollectionAsync(
                    collection.CollectionId,
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

    public async Task DeleteCollectionAsync(string collectionId)
    {
        var collection = await _db.Collections.FindAsync(collectionId)
            ?? throw new InvalidOperationException("Collection not found.");

        using (var transaction = await _db.Database.BeginTransactionAsync())
        {
            try
            {
                _db.Collections.Remove(collection);
                await _memory.DeleteCollectionAsync(collection.CollectionId);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    #endregion

    #region Document

    public async Task<IEnumerable<DocumentEntity>> FindDocumentsAsync(
        string collectionId,
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

    public async Task UploadDocumentAsync(string collectionId, DocumentEntity document, Stream data)
    {
        var collection = await _db.Collections.FindAsync(collectionId)
            ?? throw new InvalidOperationException("Collection not found.");

        var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            await _db.Documents.AddAsync(document);

            await _memory.MemorizeDocumentAsync(
                collectionName: collection.CollectionId,
                documentId: document.DocumentId.ToString(),
                fileName: document.FileName,
                content: data,
                tags: document.Tags?.ToArray(),
                steps:
                [
                    RaggleServiceKeys.Decoding,
                    RaggleServiceKeys.Chunking,
                    RaggleServiceKeys.Dialogue,
                    RaggleServiceKeys.Embeddings,
                ],
                options: collection.HandlerOptions);

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

    public async Task DeleteDocumentAsync(string collectionId, string documentId)
    {
        var collection = await _db.Collections.FindAsync(collectionId)
            ?? throw new InvalidOperationException("Collection not found.");

        var document = await _db.Documents.FindAsync(documentId)
            ?? throw new InvalidOperationException("Document not found.");

        var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            _db.Documents.Remove(document);
            await _memory.UnMemorizeDocumentAsync(collectionId, documentId);

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

    public async Task<IEnumerable<ScoredVectorPoint>> SearchDocumentAsync(string collectionId, string query)
    {
        var collection = await _db.Collections.FindAsync(collectionId)
            ?? throw new InvalidOperationException("Collection not found.");

        return await _memory.GetNearestVectorAsync(
            collectionName: collection.CollectionId,
            embedServiceKey: collection.EmbedServiceKey,
            embedModelName: collection.EmbedModelName,
            query: query);
    }

    #endregion
}
