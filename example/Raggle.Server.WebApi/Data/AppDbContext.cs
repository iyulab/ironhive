using Microsoft.EntityFrameworkCore;

namespace Raggle.Server.WebApi.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
