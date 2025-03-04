using Microsoft.EntityFrameworkCore;
using Raggle.Stack.EFCore.Entities;
using Raggle.Stack.WebApi.Data;

namespace Raggle.Stack.WebApi.Services;

public class SessionService
{
    private readonly string _id;
    private readonly RaggleDbContext _db;

    public SessionService(RaggleDbContext dbContext, string serviceId)
    {
        _id = serviceId;
        _db = dbContext;
    }

    public async Task<IEnumerable<SessionEntity>> FindAsync(
        int skip = 0,
        int limit = 10)
    {
        var query = _db.Conversations.AsQueryable();
        return await query.OrderByDescending(c => c.LastUpdatedAt)
                          .Skip(skip)
                          .Take(limit)
                          .ToListAsync();
    }

    public async Task<SessionEntity?> FindAsync(string id)
    {
        return await _db.Conversations.FindAsync(id);
    }

    public async Task<SessionEntity> UpsertAsync(SessionEntity conversation)
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
