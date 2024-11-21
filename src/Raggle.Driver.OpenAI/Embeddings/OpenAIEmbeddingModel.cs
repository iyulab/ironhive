using Raggle.Driver.OpenAI.Base;

namespace Raggle.Driver.OpenAI.Embeddings;

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
