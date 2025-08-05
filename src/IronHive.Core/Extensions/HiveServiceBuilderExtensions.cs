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
}
