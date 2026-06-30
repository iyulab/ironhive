using IronHive.Abstractions;
using IronHive.Abstractions.Files;
using IronHive.Core.Files;
using IronHive.Core.Files.Decoders;
using IronHive.Core.Files.Detectors;
using IronHive.Core.Storages;
using IronHive.Core.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core.Extensions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// 내장 함수 도구를 <typeparamref name="T"/> 타입에서 모두 찾아 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddFunctionTool<T>(this IHiveServiceBuilder builder)
        where T : class
    {
        var tools = FunctionToolFactory.CreateFrom<T>();
        foreach (var tool in tools)
        {
            builder.AddTool(tool);
        }
        return builder;
    }

    /// <summary>
    /// 로컬 디스크 파일 스토리지를 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddLocalFileStorage(
        this IHiveServiceBuilder builder,
        string storageName)
    {
        builder.AddFileStorage(storageName, new LocalFileStorage());
        return builder;
    }

    /// <summary>
    /// 로컬 벡터 스토리지를 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddLocalVectorStorage(
        this IHiveServiceBuilder builder,
        string storageName,
        LocalVectorConfig config)
    {
        builder.AddVectorStorage(storageName, new LocalVectorStorage(config));
        return builder;
    }

    /// <summary>
    /// 로컬 큐 스토리지를 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddLocalQueueStorage(
        this IHiveServiceBuilder builder,
        string storageName,
        LocalQueueConfig config)
    {
        builder.AddQueueStorage(storageName, new LocalQueueStorage(config));
        return builder;
    }
}

public static class ServiceCollectionFileExtensions
{
    /// <summary>
    /// "text", "word", "pdf", "ppt", "image" 등 기본 파일 디코더를 등록합니다.
    /// </summary>
    public static IServiceCollection AddBasicFileExtractor(this IServiceCollection services)
    {
        services.AddSingleton<IFileDecoder<string>, TextDecoder>();
        services.AddSingleton<IFileDecoder<string>, WordDecoder>();
        services.AddSingleton<IFileDecoder<string>, PDFDecoder>();
        services.AddSingleton<IFileDecoder<string>, PPTDecoder>();
        services.AddSingleton<IFileDecoder<string>, ImageDecoder>();
        services.AddSingleton<IFileMediaTypeDetector, BasicFileMediaTypeDetector>();
        services.AddSingleton<IFileExtractionService<string>, FileExtractionService<string>>();
        return services;
    }
}
