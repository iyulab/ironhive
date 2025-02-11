using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;
using Raggle.Abstractions.Tools;
using Raggle.Server.Data;
using Raggle.Server.Entities;
using Raggle.Server.Tools;
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

    public async IAsyncEnumerable<StreamingMessageResponse> ChatAssistantAsync(
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
                    var functions = FunctionToolFactory.CreateFromObject(toolService);
                    tools.AddRange(functions);
                }
                else
                {
                    throw new InvalidOperationException($"Tool not found: {tool}");
                }
            }
        }

        // 테스트용 코드
        if (true)
        {
            instructions += "\n\nYou have database connection:\n";
            instructions += "- type: ms_sql_server\n";
            instructions += "- description: Current user company information\n";

            var dbService = _raggle.Services.GetRequiredKeyedService<DatabaseTool>("database_search");
            var functions = FunctionToolFactory.CreateFromObject(dbService);
            tools.AddRange(functions);

            var t = _raggle.Services.GetRequiredKeyedService<PythonTool>("python-interpreter");
            var f = FunctionToolFactory.CreateFromObject(t);
            tools.AddRange(f);
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
