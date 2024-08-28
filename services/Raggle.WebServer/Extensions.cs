using Microsoft.EntityFrameworkCore;
using Raggle.Server.Web.Database;
using Raggle.Server.Web.Models;

namespace Raggle.Server.Web;

public static class ServiceExtensions
{
    /// <summary>
    /// 데이터베이스 서비스를 추가합니다.
    /// </summary>
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 데이터베이스 컨텍스트 추가
        services.AddDbContext<AppDbContext>(options =>
        {
            var dbPath = configuration.GetConnectionString("Sqlite");
            if (string.IsNullOrEmpty(dbPath))
            {
                dbPath = Path.Combine(AppContext.BaseDirectory, "Raggle.db");
            }
            else
            {
                var directory = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            var connectionString = $"Data Source={dbPath}";
            options.UseSqlite(connectionString);
        });

        // 리포지토리 서비스 추가
        services.AddScoped<AppRepository<User>>();
        services.AddScoped<AppRepository<Assistant>>();
        services.AddScoped<AppRepository<Knowledge>>();
        services.AddScoped<AppRepository<Connection>>();
        services.AddScoped<AppRepository<OpenAPI>>();

        return services;
    }

    /// <summary>
    /// 데이터베이스를 초기화합니다.
    /// </summary>
    public static IApplicationBuilder InitializeDatabase(this IApplicationBuilder app)
    {
        // 데이터베이스 초기화
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureCreated();
        }

        return app;
    }
}
