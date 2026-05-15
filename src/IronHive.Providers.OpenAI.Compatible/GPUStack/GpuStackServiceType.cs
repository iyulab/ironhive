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

    /// <summary>All supported services.</summary>
    All = Models | Language | Embeddings
}
