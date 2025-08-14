using Microsoft.Extensions.DependencyInjection;
using IronHive.Core.Storages;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;
using IronHive.Core.Memory.Handlers;
using IronHive.Core.Files.Decoders;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// 내장 기능 툴 플러그인을 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddFunctionToolPlugin<T>(this IHiveServiceBuilder builder, string pluginName) 
        where T : class
    {
        builder.Services.AddSingleton<IToolPlugin, FunctionToolPlugin<T>>(sp =>
        {
            return new FunctionToolPlugin<T>(sp)
            {
                PluginName = pluginName
            };
        });
        return builder;
    }

    /// <summary>
    /// 로컬 디스크 파일 스토리지를 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddLocalFileStorage(this IHiveServiceBuilder builder, string storageName)
    {
        builder.AddFileStorage(new LocalFileStorage
        {
            StorageName = storageName
        });
        return builder;
    }

    /// <summary>
    /// "text", "word", "pdf", "ppt", "image" 등 기본 파일 디코더를 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddDefaultFileDecoders(this IHiveServiceBuilder builder)
    {
        builder.AddFileDecoder(new TextDecoder());
        builder.AddFileDecoder(new WordDecoder());
        builder.AddFileDecoder(new PDFDecoder());
        builder.AddFileDecoder(new PPTDecoder());
        builder.AddFileDecoder(new ImageDecoder());
        return builder;
    }

    /// <summary>
    /// "extract_text", "split_text", "gen_QnA", "gen_vectors" 등 기본 파이프라인 핸들러를 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddDefaultPipelineHandlers(this IHiveServiceBuilder builder)
    {
        builder.AddMemoryPipelineHandler<TextExtractionHandler>("extract_text");
        builder.AddMemoryPipelineHandler<TextChunkerHandler>("split_text");
        builder.AddMemoryPipelineHandler<QnAExtractionHandler>("gen_QnA");
        builder.AddMemoryPipelineHandler<VectorEmbeddingHandler>("gen_vectors");
        return builder;
    }

    /// <summary>
    /// 지정 타입의 서비스가 하나라도 등록되어 있는지 확인합니다.
    /// </summary>
    public static bool ContainsAny<TService>(this IHiveServiceBuilder builder)
    {
        return builder.Services.Any(x => x.ServiceType == typeof(TService));
    }

    /// <summary>
    /// Service 등록 로직 처리
    /// </summary>
    public static void AddService<TService, TImplementation>(
        this IHiveServiceBuilder builder,
        ServiceLifetime? lifetime,
        Func<IServiceProvider, TImplementation>? implementationFactory = null)
        where TService : class
        where TImplementation : class, TService
    {
        if (lifetime == ServiceLifetime.Singleton)
        {
            if (implementationFactory != null)
                builder.Services.AddSingleton<TService, TImplementation>(implementationFactory);
            else
                builder.Services.AddSingleton<TService, TImplementation>();
        }
        else if (lifetime == ServiceLifetime.Scoped)
        {
            if (implementationFactory != null)
                builder.Services.AddScoped<TService, TImplementation>(implementationFactory);
            else
                builder.Services.AddScoped<TService, TImplementation>();
        }
        else if (lifetime == ServiceLifetime.Transient)
        {
            if (implementationFactory != null)
                builder.Services.AddTransient<TService, TImplementation>(implementationFactory);
            else
                builder.Services.AddTransient<TService, TImplementation>();
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }
    }

    /// <summary>
    /// Keyed Service 등록 로직 처리
    /// </summary>
    public static void AddKeyedService<TService, TImplementation>(
        this IHiveServiceBuilder builder,
        string serviceKey,
        ServiceLifetime? lifetime,
        Func<IServiceProvider, object?, TImplementation>? implementationFactory = null)
        where TService : class
        where TImplementation : class, TService
    {
        if (lifetime == ServiceLifetime.Singleton)
        {
            if (implementationFactory != null)
                builder.Services.AddKeyedSingleton<TService, TImplementation>(serviceKey, implementationFactory);
            else
                builder.Services.AddKeyedSingleton<TService, TImplementation>(serviceKey);
        }
        else if (lifetime == ServiceLifetime.Scoped)
        {
            if (implementationFactory != null)
                builder.Services.AddKeyedScoped<TService, TImplementation>(serviceKey, implementationFactory);
            else
                builder.Services.AddKeyedScoped<TService, TImplementation>(serviceKey);
        }
        else if (lifetime == ServiceLifetime.Transient)
        {
            if (implementationFactory != null)
                builder.Services.AddKeyedTransient<TService, TImplementation>(serviceKey, implementationFactory);
            else
                builder.Services.AddKeyedTransient<TService, TImplementation>(serviceKey);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }
    }
}
