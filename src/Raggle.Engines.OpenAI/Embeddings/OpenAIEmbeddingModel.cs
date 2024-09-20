using Raggle.Engines.OpenAI.Abstractions;

namespace Raggle.Engines.OpenAI.Embeddings;

internal class OpenAIEmbeddingModel : OpenAIModel
{
    internal bool IsSupportDynamicDimension
    {
        get
        {
            return ID.Contains("text-embedding-3");
        }
    }
}
