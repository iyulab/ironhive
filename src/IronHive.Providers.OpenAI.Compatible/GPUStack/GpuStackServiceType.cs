namespace IronHive.Providers.OpenAI.Compatible.GpuStack;

/// <summary>
/// Services available through GPUStack's OpenAI-compatible API.
/// </summary>
[Flags]
public enum GpuStackServiceType
{
    /// <summary>Model catalog queries.</summary>
    Models = 1 << 0,

    /// <summary>Chat completion (language generation).</summary>
    Language = 1 << 1,

    /// <summary>Embedding generation.</summary>
    Embeddings = 1 << 2,

    /// <summary>Reranking (<c>POST /v1/rerank</c>, Jina/Cohere-shaped, llama-box backend only).</summary>
    Rerank = 1 << 3,

    /// <summary>Image generation/editing (SGLang backend).</summary>
    Images = 1 << 4,

    /// <summary>Audio processing (TTS/STT, VoxBox backend).</summary>
    Audio = 1 << 5,

    /// <summary>All supported services.</summary>
    All = Models | Language | Embeddings | Rerank | Images | Audio
}
