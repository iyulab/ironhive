using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using Raggle.Server.Data;
using Raggle.Server.Entities;

namespace Raggle.Server.Services;

public class AssistantService
{
    private readonly RaggleDbContext _db;

    public AssistantService(RaggleDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task<IEnumerable<AssistantEntity>> GetAssistantsAsync(int skip = 0, int limit = 10)
    {
        var query = _db.Assistants.AsQueryable();
        var assistants = await query.OrderByDescending(a => a.LastUpdatedAt)
            .Skip(skip).Take(limit).ToArrayAsync();
        return assistants;
    }

    public async Task<AssistantEntity?> GetAssistantAsync(Guid id)
    {
        var assistant = await _db.Assistants.FindAsync(id);
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

    public async Task DeleteAssistantAsync(Guid id)
    {
        var existing = await _db.Assistants.FindAsync(id)
            ?? throw new KeyNotFoundException($"Assistant with id {id} not found");
        _db.Assistants.Remove(existing);
        await _db.SaveChangesAsync();
    }
}
