using IronHive.Core.Decoders;
using IronHive.Core.Handlers;
using IronHive.Core.Storages;
using IronHive.Abstractions;

namespace IronHive.Core;

public static class HiveServiceBuilderExtensions
{
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
    /// "decode", "chunk", "qnagen", "embed" handlers are registered by default.
    /// </summary>
    public static IHiveServiceBuilder AddDefaultPipelineHandlers(this IHiveServiceBuilder builder)
    {
        builder.AddPipelineHandler<DecodeHandler>("decode");
        builder.AddPipelineHandler<ChunkHandler>("chunk");
        builder.AddPipelineHandler<QnAGenHandler>("qnagen");
        builder.AddPipelineHandler<EmbedHandler>("embed");
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
