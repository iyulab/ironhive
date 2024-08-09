using Microsoft.Data.Sqlite;
using Microsoft.SemanticKernel.ChatCompletion;
using Raggle.Server.API.Models;
using System.Text.Json;

namespace Raggle.Server.API.Repositories;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Sqlite");
    }

    public async Task<User?> GetAsync(Guid userId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Users WHERE ID = @ID";
        command.Parameters.AddWithValue("@ID", userId.ToString());

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var user = new User
            {
                ID = Guid.Parse(reader.GetString(0)),
                ChatHistory = JsonSerializer.Deserialize<ChatHistory>(reader.GetString(1)) ?? [],
                LastAccessAt = DateTime.Parse(reader.GetString(2)),
                CreatedAt = DateTime.Parse(reader.GetString(3)),
            };
            return user;
        }
        return null;
    }

    public async Task InsertAsync(User user)
    {
        var isExist = await GetAsync(user.ID);
        if (isExist != null)
            return;

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
                INSERT INTO Users (ID, ChatHistory, LastAccessAt, CreatedAt) 
                VALUES (@ID, @ChatHistory, @LastAccessAt, @CreatedAt)";
        command.Parameters.AddWithValue("@ID", user.ID.ToString());
        command.Parameters.AddWithValue("@ChatHistory", JsonSerializer.Serialize(user.ChatHistory));
        command.Parameters.AddWithValue("@LastAccessAt", DateTime.UtcNow.ToString("o"));
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("o"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(Guid userId, JsonElement updates)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var updateFields = new List<string>();
        var parameters = new List<SqliteParameter>();

        if (updates.TryGetProperty("chatHistory", out var chatHistory))
        {
            updateFields.Add("ChatHistory = @ChatHistory");
            parameters.Add(new SqliteParameter("@ChatHistory", JsonSerializer.Serialize(chatHistory)));
        }
        if (updates.TryGetProperty("lastAccessAt", out var lastAccessAt))
        {
            updateFields.Add("LastAccessAt = @LastAccessAt");
            parameters.Add(new SqliteParameter("@LastAccessAt", lastAccessAt.GetString()));
        }
        if (updateFields.Count == 0)
        {
            throw new ArgumentException("No valid fields to update.", nameof(updates));
        }

        var updateQuery = $"UPDATE Users SET {string.Join(", ", updateFields)} WHERE ID = @ID";

        using var command = connection.CreateCommand();
        command.CommandText = updateQuery;
        command.Parameters.AddWithValue("@ID", userId.ToString());

        foreach (var parameter in parameters)
        {
            command.Parameters.Add(parameter);
        }

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(Guid userId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Users WHERE ID = @ID";
        command.Parameters.AddWithValue("@ID", userId.ToString());
        
        await command.ExecuteNonQueryAsync();
    }

}
