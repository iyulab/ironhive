using Microsoft.EntityFrameworkCore;
using Raggle.Server.Data;
using Raggle.Server.Entities;

namespace Raggle.Server.Services;

public class ConversationService
{
    private readonly string _id;
    private readonly RaggleDbContext _db;

    public ConversationService(RaggleDbContext dbContext, string serviceId)
    {
        _id = serviceId;
        _db = dbContext;
    }

    public async Task<IEnumerable<ConversationEntity>> FindAsync(
        int skip = 0,
        int limit = 10)
    {
        var query = _db.Conversations.AsQueryable();
        return await query.OrderByDescending(c => c.LastUpdatedAt)
                          .Skip(skip)
                          .Take(limit)
                          .ToListAsync();
    }

    public async Task<ConversationEntity?> FindAsync(string id)
    {
        return await _db.Conversations.FindAsync(id);
    }

    public async Task<ConversationEntity> UpsertAsync(ConversationEntity conversation)
    {
        var exists = await _db.Conversations.AnyAsync(c => c.Id == conversation.Id);

        if (exists)
        {
            conversation.LastUpdatedAt = DateTime.UtcNow;
            _db.Conversations.Update(conversation);
        }
        else
        {
            _db.Conversations.Add(conversation);
        }

        await _db.SaveChangesAsync();
        return conversation;
    }

    public async Task DeleteAsync(string id)
    {
        var conversation = await FindAsync(id);
        if (conversation != null)
        {
            _db.Conversations.Remove(conversation);
            await _db.SaveChangesAsync();
        }
        else
        {
            throw new InvalidOperationException($"Conversation '{id}' not found.");
        }
    }
}
