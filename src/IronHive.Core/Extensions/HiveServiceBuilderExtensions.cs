using Microsoft.Extensions.DependencyInjection;
using IronHive.Core.Storages;
using IronHive.Core.Tools;
using IronHive.Core.Files.Decoders;
using IronHive.Core.Memory.Pipelines;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// 내장 기능 툴 플러그인을 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddFunctionTool<T>(this IHiveServiceBuilder builder) 
        where T : class
    {
        var tools = FunctionToolFactory.CreateFromType<T>();
        foreach (var tool in tools)
        {
            builder.Agents.AddTool(tool);
        }
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
        builder.Files.AddDecoder(new TextDecoder());
        builder.Files.AddDecoder(new WordDecoder());
        builder.Files.AddDecoder(new PDFDecoder());
        builder.Files.AddDecoder(new PPTDecoder());
        builder.Files.AddDecoder(new ImageDecoder());
        return builder;
    }

    /// <summary>
    /// "extract_text", "split_text", "gen_QnA", "gen_vectors" 등 기본 파이프라인 핸들러를 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddDefaultPipelineHandlers(this IHiveServiceBuilder builder)
    {
        //builder.AddMemoryPipelineHandler<TextExtractionPipeline>("extract_text");
        //builder.AddMemoryPipelineHandler<TextChunkingPipeline>("split_text");
        //builder.AddMemoryPipelineHandler<DialogueExtractionPipeline>("gen_QnA");
        //builder.AddMemoryPipelineHandler<TextEmbeddingPipeline>("gen_vectors");
        return builder;
    }

    /// <summary>
    /// 지정 타입의 서비스가 하나라도 등록되어 있는지 확인합니다.
    /// </summary>
    public static bool ContainsAny<TService>(this IHiveServiceBuilder builder)
    {
        return builder.Services.Any(x => x.ServiceType == typeof(TService));
    }
}
