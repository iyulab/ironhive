using Raggle.Connector.OpenAI.Base;

namespace Raggle.Connector.OpenAI.Embeddings;

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
