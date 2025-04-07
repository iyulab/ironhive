using System.Text.Json;
using WebApiSample.Entities;
using WebApiSample.Services;

namespace WebApiSample;

public static class Extensions
{
    public const string ConfigFilePath = "sample_settings.json";

    /// <summary>
    /// Ensure the configuration file exists and load it.
    /// </summary>
    public static IConfigurationBuilder EnsureConfiguration(this IConfigurationBuilder builder)
    {
        // 파일이 존재하지 않을시 새로운 파일 생성
        if (!File.Exists(ConfigFilePath))
        {
            var settings = new ServicesSettings();
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = new
            {
                Services = settings,
            };
            File.WriteAllText(ConfigFilePath, JsonSerializer.Serialize(json, options));
        }
        builder.AddJsonFile(ConfigFilePath, false, true);
        return builder;
    }

    /// <summary>
    /// Add a singleton instance of ApplicationService to the service collection.
    /// </summary>
    public static IServiceCollection AddSampleServices(this IServiceCollection services)
    {
        // IOptions<T> 옵션 서비스 등록
        services.Configure<ServicesSettings>(options =>
        {
            var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            config.GetSection("Services").Bind(options);
        });

        // 메인 서비스 등록
        services.AddSingleton<AppService>();

        return services;
    }
}
