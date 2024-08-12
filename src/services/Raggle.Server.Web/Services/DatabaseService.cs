using Microsoft.Data.Sqlite;
using MongoDB.Driver.Core.Configuration;

namespace Raggle.Server.Web.Services;

public static class DatabaseService
{
    public static async Task InitializeSqlite(string connectionString)
    {
        await EnableWALModeAsync(connectionString);
        await CreateUsersAsync(connectionString);
        await CreateSourcesAsync(connectionString);
    }

    private static async Task EnableWALModeAsync(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode=WAL";
        await command.ExecuteNonQueryAsync();
    }

    private static async Task CreateUsersAsync(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                ID TEXT PRIMARY KEY,
                ChatHistory TEXT NOT NULL,
                LastAccessAt TEXT,
                CreatedAt TEXT
            )";

        await command.ExecuteNonQueryAsync();
    }

    private static async Task CreateSourcesAsync(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Sources (
                ID TEXT PRIMARY KEY,
                UserID TEXT NOT NULL,
                Name TEXT NOT NULL, 
                Description TEXT, 
                Type TEXT NOT NULL, 
                Details TEXT, 
                CreatedAt TEXT,
                UpdatedAt TEXT,
                FOREIGN KEY (UserID) REFERENCES Users(ID)
            )";
        await command.ExecuteNonQueryAsync();
    }
}
