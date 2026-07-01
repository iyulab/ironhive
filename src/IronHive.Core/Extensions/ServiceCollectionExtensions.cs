using IronHive.Abstractions;
using IronHive.Abstractions.Files;
using IronHive.Core;
using IronHive.Core.Files;
using IronHive.Core.Files.Parsers;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Service Collection에 기존 IHiveService 인스턴스를 등록합니다.
    /// </summary>
    public static IServiceCollection AddHiveService(
        this IServiceCollection services,
        IHiveService service,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        services.Add(new ServiceDescriptor(typeof(IHiveService), _ => service, lifetime));
        return services;
    }

    /// <summary>
    /// Service Collection에 IronHive 서비스를 팩토리 방식으로 등록합니다.
    /// 기본 lifetime은 Scoped입니다 — 요청마다 IServiceProvider가 주입되어 DbContext 등 Scoped 서비스를 도구 실행에서 사용할 수 있습니다.
    /// </summary>
    public static IServiceCollection AddHiveService(
        this IServiceCollection services,
        Func<IHiveServiceBuilder, IServiceProvider, IHiveService> configure,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        services.Add(new ServiceDescriptor(typeof(IHiveService), sp =>
        {
            var builder = new HiveServiceBuilder();
            return configure(builder, sp);
        }, lifetime));
        return services;
    }

    /// <summary>
    /// PDF, Word, Excel, PowerPoint, Image 파서를 등록합니다.
    /// 미지원 파일은 null byte 휴리스틱으로 텍스트/바이너리 자동 판별합니다.
    /// </summary>
    public static IServiceCollection AddFileParser(
        this IServiceCollection services)
    {
        services.AddSingleton<IFileParser, PdfParser>();
        services.AddSingleton<IFileParser, WordParser>();
        services.AddSingleton<IFileParser, ExcelParser>();
        services.AddSingleton<IFileParser, PowerPointParser>();
        services.AddSingleton<IFileParser, ImageParser>();
        services.AddSingleton<IFileParserService, FileParserService>();
        return services;
    }
}
