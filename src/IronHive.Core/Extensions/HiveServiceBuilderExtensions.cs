using Microsoft.Extensions.DependencyInjection;
using IronHive.Core.Decoders;
using IronHive.Core.Handlers;
using IronHive.Core.Storages;
using IronHive.Abstractions;
using IronHive.Abstractions.Tools;
using IronHive.Core.Tools;

namespace IronHive.Core;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Add a function tool plugin to the service builder.
    /// </summary>
    public static IHiveServiceBuilder AddFunctionTools<T>(this IHiveServiceBuilder builder, string pluginKey) where T : class
    {
        builder.Services.AddSingleton<IToolPlugin, FunctionToolPlugin<T>>(sp =>
        {
            return new FunctionToolPlugin<T>(sp)
            {
                PluginName = pluginKey
            };
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
        builder.AddPipelineHandler<TextExtractionHandler>("extract_text");
        builder.AddPipelineHandler<TextChunkerHandler>("split_text");
        builder.AddPipelineHandler<QnAExtractionHandler>("gen_QnA");
        builder.AddPipelineHandler<VectorEmbeddingHandler>("gen_vectors");
        return builder;
    }

    /// <summary>
    /// "local" file storage is registered by default.
    /// </summary>
    public static IHiveServiceBuilder AddDefaultFileStorages(this IHiveServiceBuilder builder)
    {
        builder.AddFileStorage<LocalFileStorage>("local");
        return builder;
    }
}
