using Raggle.Engines.OpenAI.Abstractions;

namespace Raggle.Engines.OpenAI.Embeddings;

public class OpenAIEmbeddingModel : OpenAIModel
{
    public bool IsSupportDynamicDimension
    {
        get
        {
            return ID.Contains("text-embedding-3");
        }
    }
}
