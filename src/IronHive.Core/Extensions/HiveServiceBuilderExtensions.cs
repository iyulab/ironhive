using Microsoft.Extensions.DependencyInjection;
using IronHive.Core.Decoders;
using IronHive.Core.Handlers;
using IronHive.Core.Storages;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Add a function tool plugin to the service builder.
    /// </summary>
    public static IHiveServiceBuilder AddFunctionToolPlugin<T>(this IHiveServiceBuilder builder, string pluginName) where T : class
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
    /// Add a local file storage to the service builder.
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
    /// text, word, pdf, ppt, image decoders are registered by default.
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
    /// "extract_text", "split_text", "gen_QnA", "gen_vectors" handlers are registered by default.
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
