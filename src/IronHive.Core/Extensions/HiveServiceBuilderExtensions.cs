using Microsoft.Extensions.DependencyInjection;
using IronHive.Core.Storages;
using IronHive.Core.Tools;
using IronHive.Core.Files.Decoders;
using IronHive.Abstractions.Files;
using IronHive.Core.Files.Detectors;

namespace IronHive.Abstractions;

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

    /// <summary>
    /// "text", "word", "pdf", "ppt", "image" 등 기본 파일 디코더를 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddFileExtractor(this IHiveServiceBuilder builder)
    {
        builder.Services.AddSingleton<IFileDecoder<string>, TextDecoder>();
        builder.Services.AddSingleton<IFileDecoder<string>, WordDecoder>();
        builder.Services.AddSingleton<IFileDecoder<string>, PDFDecoder>();
        builder.Services.AddSingleton<IFileDecoder<string>, PPTDecoder>();
        builder.Services.AddSingleton<IFileDecoder<string>, ImageDecoder>();
        builder.Services.AddSingleton<IFileMediaTypeDetector, BasicFileMediaTypeDetector>();
        builder.Services.AddSingleton<IFileExtractionService<string>, FileExtractionService<string>>();
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
}
