using Raggle.Server.API.Repositories;
using StackExchange.Redis;

namespace Raggle.Server.API.Stores;

public class ConnectionStore
{
    private readonly ILogger<ConnectionStore> _logger;
    private readonly IDatabase _db;

    public ConnectionStore(ILogger<ConnectionStore> logger, IConfiguration config)
    {
        _logger = logger;
        _db = ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")).GetDatabase();
    }

    public async Task Set(string connectionId, string userId)
    {
        await _db.StringSetAsync(connectionId, userId);
    }

    public async Task<string> Get(string connectionId)
    {
        var result = await _db.StringGetAsync(connectionId);
        return result.ToString();
    }

    public async Task Remove(string connectionId)
    {
        await _db.KeyDeleteAsync(connectionId);
    }
}
