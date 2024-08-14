using StackExchange.Redis;

namespace Raggle.Server.Web.Stores;

public class ConnectionStore
{
    private readonly IDatabase _db;

    public ConnectionStore(IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Redis")
                               ?? throw new ArgumentNullException("Redis connection string is missing");
        _db = ConnectionMultiplexer.Connect(connectionString).GetDatabase();
    }

    public async Task SetAsync(Guid userId, string connectionId)
    {
        await _db.StringSetAsync(userId.ToString(), connectionId);
    }

    public async Task<string> GetAsync(Guid userId)
    {
        var result = await _db.StringGetAsync(userId.ToString());
        return result.ToString();
    }

    public async Task RemoveAsync(Guid userId)
    {
        await _db.KeyDeleteAsync(userId.ToString());
    }
}
