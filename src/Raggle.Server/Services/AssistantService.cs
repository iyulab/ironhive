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
        var existingAssistant = await _db.Assistants.AsTracking()
            .FirstOrDefaultAsync(a => a.AssistantId == assistant.AssistantId);

        if (existingAssistant != null)
        {
            existingAssistant.Name = assistant.Name;
            existingAssistant.Description = assistant.Description;
            existingAssistant.Instruction = assistant.Instruction;
            existingAssistant.LastUpdatedAt = DateTime.UtcNow;
            _db.Entry(existingAssistant).State = EntityState.Modified;
        }
        else
        {
            _db.Assistants.Add(assistant);
        }

        await _db.SaveChangesAsync();
        return existingAssistant ?? assistant;
    }

    public async Task DeleteAssistantAsync(string assistantId)
    {
        var existing = await _db.Assistants.FindAsync(assistantId)
            ?? throw new KeyNotFoundException($"Assistant not found");
        _db.Assistants.Remove(existing);
        await _db.SaveChangesAsync();
    }

    public async IAsyncEnumerable<IStreamingChatCompletionResponse> ChatAssistantAsync(
        string assistantId, 
        MessageCollection messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var entity = await _db.Assistants.FindAsync(assistantId)
            ?? throw new KeyNotFoundException($"Assistant not found");

        var chat = _raggle.Services.GetRequiredKeyedService<IChatCompletionService>(entity.Provider);
        var request = new ChatCompletionRequest
        {
            Model = entity.Model,
            System = entity.Instruction,
            Messages = messages,
            MaxTokens = entity.MaxTokens,
            Temperature = entity.Temperature,
            TopK = entity.TopK,
            TopP = entity.TopP,
            StopSequences = entity.StopSequences,
        };
        
        await foreach (var message in chat.StreamingChatCompletionAsync(request, cancellationToken))
        {
            yield return message;
        }
    }
}
