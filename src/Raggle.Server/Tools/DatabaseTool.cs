using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Raggle.Core.ChatCompletion;
using System.ComponentModel;

namespace Raggle.Server.Tools;

public class DatabaseTool
{
    private readonly string _connectionString;

    public DatabaseTool(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("TestDB");
    }

    [FunctionTool]
    [Description("Fetches a list of all table names in the database.")]
    public async Task<IEnumerable<string>> GetTablesAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
        var reader = await command.ExecuteReaderAsync();
        var tables = new List<string>();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }
        return tables;
    }

    [FunctionTool]
    [Description("Retrieves schema information of a specific table. Run GetTablesAsync first to get a list of table names.")]
    public async Task<object> GetTableSchemeAsync(
        [Description("The name of the table to retrieve schema information for.")] string name)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // 컬럼 정보 조회
        var columnCommand = connection.CreateCommand();
        columnCommand.CommandText = $@"
        SELECT 
            c.COLUMN_NAME, 
            c.DATA_TYPE, 
            c.IS_NULLABLE,
            tc.CONSTRAINT_TYPE
        FROM INFORMATION_SCHEMA.COLUMNS c
        LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
            ON c.TABLE_NAME = kcu.TABLE_NAME AND c.COLUMN_NAME = kcu.COLUMN_NAME
        LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
            ON kcu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME AND tc.TABLE_NAME = c.TABLE_NAME
        WHERE c.TABLE_NAME = '{name}'";

        var reader = await columnCommand.ExecuteReaderAsync();
        var columns = new List<object>();

        while (await reader.ReadAsync())
        {
            var column = new
            {
                ColumnName = reader.GetString(0),
                DataType = reader.GetString(1),
                IsNullable = reader.GetString(2) == "YES",
                ConstraintType = reader.IsDBNull(3) ? "None" : reader.GetString(3) // PRIMARY KEY, FOREIGN KEY, UNIQUE 등
            };
            columns.Add(column);
        }

        return columns;
    }

    [FunctionTool]
    [Description("Executes a SQL query and returns the result. Run GetTableSchemeAsync first for schema validation.")]
    public async Task<object> QueryAsync(
        [Description("The SQL query to execute. Ensure the query is valid and safe.")] string query)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = query;
            var reader = await command.ExecuteReaderAsync();
            var result = new List<object>();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetName(i), reader.GetValue(i));
                }
                result.Add(row);
            }
            return result;
        }
        catch(Exception ex)
        {
            throw ex;
        }
    }
}
