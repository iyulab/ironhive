using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;
using Raggle.Server.Data;
using Raggle.Server.Entities;
using System.Runtime.CompilerServices;

namespace Raggle.Server.Services;

public class AssistantService
{
    private readonly RaggleDbContext _db;
    private readonly IRaggle _raggle;

    public AssistantService(RaggleDbContext dbContext, IRaggle raggle)
    {
        _db = dbContext;
        _raggle = raggle;
    }

    public async Task<IEnumerable<AssistantEntity>> GetAssistantsAsync(
        int skip = 0, 
        int limit = 10)
    {
        var query = _db.Assistants.AsQueryable();
        var assistants = await query.OrderByDescending(a => a.LastUpdatedAt)
            .Skip(skip).Take(limit).ToArrayAsync();
        return assistants;
    }

    public async Task<AssistantEntity?> GetAssistantAsync(string assistantId)
    {
        var assistant = await _db.Assistants.FindAsync(assistantId);
        return assistant;
    }

    public async Task<AssistantEntity> UpsertAssistantAsync(AssistantEntity assistant)
    {
        var existing = await _db.Assistants.AsTracking()
            .FirstOrDefaultAsync(a => a.Id == assistant.Id);

        if (existing != null)
        {
            existing.Name = assistant.Name;
            existing.Description = assistant.Description;
            existing.Instruction = assistant.Instruction;
            existing.LastUpdatedAt = DateTime.UtcNow;
            _db.Entry(existing).State = EntityState.Modified;
        }
        else
        {
            _db.Assistants.Add(assistant);
        }

        await _db.SaveChangesAsync();
        return existing ?? assistant;
    }

    public async Task DeleteAssistantAsync(string assistantId)
    {
        var existing = await _db.Assistants.FindAsync(assistantId)
            ?? throw new KeyNotFoundException($"Assistant not found");
        _db.Assistants.Remove(existing);
        await _db.SaveChangesAsync();
    }

    public async IAsyncEnumerable<ChatCompletionStreamingResponse> ChatAssistantAsync(
        string assistantId, 
        MessageCollection messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var entity = await _db.Assistants.FindAsync(assistantId)
            ?? throw new KeyNotFoundException($"Assistant not found");

        var assistant = _raggle.CreateAssistant(
            id: entity.Id,
            name: entity.Name,
            description: entity.Description,
            instruction: entity.Instruction,
            options: entity.Options);

        await foreach (var message in assistant.StreamingChatCompletionAsync(
            messages: messages,
            options: null,
            cancellationToken: cancellationToken))
        {
            yield return message;
        }
    }
}
