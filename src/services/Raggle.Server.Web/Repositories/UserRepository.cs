using Microsoft.Data.Sqlite;
using Microsoft.SemanticKernel.ChatCompletion;
using Raggle.Server.API.Models;
using System.Text.Json;

namespace Raggle.Server.API.Repositories;

public class UserRepository
{
    private readonly ILogger<UserRepository> _logger;
    private readonly string _connectionString;

    public UserRepository(ILogger<UserRepository> logger, IConfiguration config)
    {
        _logger = logger;
        _connectionString = config.GetConnectionString("Sqlite");
        CheckTable().Wait();
    }

    public async Task CheckTable()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS Users (ID TEXT PRIMARY KEY, ChatHistory TEXT)";
        await command.ExecuteNonQueryAsync();
    }

    public async Task<User> GetUser(Guid id)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Users WHERE ID = @ID";
        command.Parameters.AddWithValue("@ID", id);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var user = new User
            {
                ID = reader.GetGuid(0),
                ChatHistory = JsonSerializer.Deserialize<ChatHistory>(reader.GetString(1))
            };
            return user;
        }
        else
        {
            var user = new User
            {
                ID = id,
                ChatHistory = new ChatHistory()
            };
            await CreateUser(user);
            return user;
        }
    }

    public async Task CreateUser(User user)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Users (ID, ChatHistory) VALUES (@ID, @ChatHistory)";
        command.Parameters.AddWithValue("@ID", user.ID);
        command.Parameters.AddWithValue("@ChatHistory", JsonSerializer.Serialize(user.ChatHistory));
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateUser(User user)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Users SET ChatHistory = @ChatHistory WHERE ID = @ID";
        command.Parameters.AddWithValue("@ID", user.ID);
        command.Parameters.AddWithValue("@ChatHistory", JsonSerializer.Serialize(user.ChatHistory));
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteUser(string id)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Users WHERE ID = @ID";
        command.Parameters.AddWithValue("@ID", Guid.Parse(id));
        await command.ExecuteNonQueryAsync();
    }
}
