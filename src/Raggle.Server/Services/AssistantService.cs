using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Extensions;
using Raggle.Abstractions.Messages;
using Raggle.Abstractions.Tools;
using Raggle.Server.Data;
using Raggle.Server.Entities;
using Raggle.Server.ToolKits;
using System.Runtime.CompilerServices;

namespace Raggle.Server.Services;

public class AssistantService
{
    private readonly string _id;
    private readonly RaggleDbContext _db;
    private readonly IRaggle _raggle;

    public AssistantService(RaggleDbContext dbContext, IRaggle raggle, string serviceId)
    {
        _id = serviceId;
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
        var exists = await _db.Assistants.AnyAsync(a => a.Id == assistant.Id);

        if (exists)
        {
            assistant.LastUpdatedAt = DateTime.UtcNow;
            _db.Assistants.Update(assistant);
        }
        else
        {
            _db.Assistants.Add(assistant);
        }

        await _db.SaveChangesAsync();
        return assistant;
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
        var instructions = entity.Instruction;
        if (entity.ToolOptions != null && 
            entity.ToolOptions.TryGetValue<IEnumerable<string>>(RaggleServiceKeys.VectorSearch, out var collectionNames))
        {
            instructions += "\nyou have below vector collections:\n";
            foreach (var name in collectionNames)
            {
                var collection = await _db.Collections.FindAsync(name)
                    ?? throw new KeyNotFoundException($"Collection not found: {name}");
                instructions += $"""
                - collectionName: {collection.Id}
                - description: {collection.Description}
                """;
            }
        };

        var tools = new FunctionToolCollection();
        if (entity.Tools != null && entity.Tools.Any())
        {
            foreach (var tool in entity.Tools)
            {
                if (tool == RaggleServiceKeys.VectorSearch)
                {
                    var toolService = _raggle.Services.GetRequiredKeyedService<VectorSearchTool>(RaggleServiceKeys.VectorSearch);
                    var functions = FunctionToolFactory.CreateFromInstance(toolService);
                    tools.AddRange(functions);
                }
                else
                {
                    throw new InvalidOperationException($"Tool not found: {tool}");
                }
            }
        }

        var assistant = _raggle.CreateAssistant(
            service: entity.Service,
            model: entity.Model,
            id: entity.Id,
            name: entity.Name,
            description: entity.Description,
            instruction: instructions,
            options: entity.Options,
            tools: tools);

        await foreach (var message in assistant.StreamingInvokeAsync(messages, cancellationToken))
        {
            yield return message;
        }
    }
}
