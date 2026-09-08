namespace IronHive.Providers.OpenAI.Compatible;

/// <summary>
/// Services available through a generic OpenAI-compatible API.
/// </summary>
[Flags]
public enum OpenAICompatibleServiceType
{
    /// <summary>Model catalog queries.</summary>
    Models = 1 << 0,

    /// <summary>Chat completion (language generation).</summary>
    Language = 1 << 1,

    /// <summary>Embedding generation.</summary>
    Embeddings = 1 << 2,

    /// <summary>Reranking (<c>POST /rerank</c>, Cohere-shaped).</summary>
    Rerank = 1 << 3,

    /// <summary>Image generation/editing.</summary>
    Images = 1 << 4,

    /// <summary>Audio processing (TTS/STT).</summary>
    Audio = 1 << 5,

    /// <summary>All supported services.</summary>
    All = Models | Language | Embeddings | Rerank | Images | Audio
}
