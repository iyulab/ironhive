namespace IronHive.Providers.OpenAI;

[Flags]
public enum OpenAIServiceType
{
    None = 0,
    ChatCompletion = 1 << 0,
    Responses = 1 << 1,
    Embeddings = 1 << 2,
    ImageGeneration = 1 << 4,
    TextToSpeech = 1 << 6,
    Models = 1 << 5,

    All = ChatCompletion | Embeddings | ImageGeneration | TextToSpeech | Models,
    AllWithResponses = Responses | Embeddings | ImageGeneration | TextToSpeech | Models
}
