using Microsoft.Data.Sqlite;
using Raggle.Server.API.Models;
using Raggle.Server.API.Storages;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Raggle.Server.Web.Repositories;

public class SourceRepository
{
    private readonly string _connectionString;
    private readonly VectorStorage _vector;
    private readonly FileStorage _file;

    public SourceRepository(IConfiguration config, VectorStorage vectorStorage, FileStorage fileStorage)
    {
        _connectionString = config.GetConnectionString("Sqlite");
        _vector = vectorStorage;
        _file = fileStorage;
    }

    public async Task<IEnumerable<DataSource>> GetAllAsync(Guid userId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Sources WHERE UserID = @UserID";
        command.Parameters.AddWithValue("@UserID", userId.ToString());

        using var reader = await command.ExecuteReaderAsync();
        var sources = new List<DataSource>();

        while (await reader.ReadAsync())
        {
            sources.Add(new DataSource
            {
                ID = Guid.Parse(reader.GetString(0)),
                UserID = Guid.Parse(reader.GetString(1)),
                Name = reader.GetString(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                Type = reader.GetString(4),
                Details = reader.IsDBNull(5) ? null : JsonSerializer.Deserialize<JsonElement>(reader.GetString(5)),
                CreatedAt = DateTime.Parse(reader.GetString(6)),
                UpdatedAt = reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7))
            });
        }

        return sources;
    }

    public async Task<DataSource?> GetAsync(Guid sourceId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Sources WHERE ID = @ID";
        command.Parameters.AddWithValue("@ID", sourceId.ToString());

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new DataSource
            {
                ID = Guid.Parse(reader.GetString(0)),
                UserID = Guid.Parse(reader.GetString(1)),
                Name = reader.GetString(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                Type = reader.GetString(4),
                Details = reader.IsDBNull(5) ? null : JsonSerializer.Deserialize<JsonElement>(reader.GetString(5)),
                CreatedAt = DateTime.Parse(reader.GetString(6)),
                UpdatedAt = reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7))
            };
        }

        return null;
    }

    public async Task<DataSource> InsertAsync(DataSource source)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Sources (ID, UserID, Name, Description, Type, Details, CreatedAt, UpdatedAt)
            VALUES (@ID, @UserID, @Name, @Description, @Type, @Details, @CreatedAt, @UpdatedAt)";
        command.Parameters.AddWithValue("@ID", source.ID.ToString());
        command.Parameters.AddWithValue("@UserID", source.UserID.ToString());
        command.Parameters.AddWithValue("@Name", source.Name);
        command.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(source.Description) ? DBNull.Value : source.Description);
        command.Parameters.AddWithValue("@Type", source.Type);
        command.Parameters.AddWithValue("@Details", JsonSerializer.Serialize(source.Details));
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("o"));
        command.Parameters.AddWithValue("@UpdatedAt", DBNull.Value);

        await command.ExecuteNonQueryAsync();

        _ = _vector.MemorizeSourceAsync(source).ConfigureAwait(false);
        return source;
    }

    public async Task<DataSource> UpdateAsync(Guid sourceId, JsonElement updates)
    {
        var oldSource = await GetAsync(sourceId);
        if (oldSource == null)
            throw new KeyNotFoundException();

        var newSource = new DataSource
        {
            ID = oldSource.ID,
            UserID = oldSource.UserID,
            Name = oldSource.Name,
            Description = oldSource.Description,
            Type = oldSource.Type,
            Details = oldSource.Details,
            CreatedAt = oldSource.CreatedAt,
            UpdatedAt = oldSource.UpdatedAt
        };

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var updateFields = new List<string>();
        var parameters = new List<SqliteParameter>();

        if (updates.TryGetProperty("name", out var name))
        {
            updateFields.Add("Name = @Name");
            parameters.Add(new SqliteParameter("@Name", name.GetString()));
            newSource.Name = name.GetString();
        }
        if (updates.TryGetProperty("description", out var description) &&
            description.ValueKind != JsonValueKind.Null)
        {
            updateFields.Add("Description = @Description");
            parameters.Add(new SqliteParameter("@Description", description.GetString()));
            newSource.Description = description.GetString();
        }
        if (updates.TryGetProperty("details", out var details) && 
            details.ValueKind != JsonValueKind.Null)
        {
            updateFields.Add("Details = @Details");
            parameters.Add(new SqliteParameter("@Details", details.GetRawText()));
            newSource.Details = details;
        }

        if (updateFields.Count == 0)
        {
            throw new ArgumentException("No valid fields to update.", nameof(updates));
        }

        var now = DateTime.UtcNow;
        updateFields.Add("UpdatedAt = @UpdatedAt");
        parameters.Add(new SqliteParameter("@UpdatedAt", now.ToString("o")));
        newSource.UpdatedAt = now;

        var updateQuery = $"UPDATE Sources SET {string.Join(", ", updateFields)} WHERE ID = @ID";

        using var command = connection.CreateCommand();
        command.CommandText = updateQuery;
        command.Parameters.AddWithValue("@ID", sourceId.ToString());

        foreach (var parameter in parameters)
        {
            command.Parameters.Add(parameter);
        }

        await command.ExecuteNonQueryAsync();

        _ = _vector.ReMemorizeSourceAsync(oldSource, newSource).ConfigureAwait(false);
        return newSource;
    }

    public async Task DeleteAsync(Guid sourceId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Sources WHERE ID = @ID";
        command.Parameters.AddWithValue("@ID", sourceId.ToString());

        await command.ExecuteNonQueryAsync();
        _ = _vector.DeleteIndexAsync(sourceId.ToString()).ConfigureAwait(false);
    }
    
}
