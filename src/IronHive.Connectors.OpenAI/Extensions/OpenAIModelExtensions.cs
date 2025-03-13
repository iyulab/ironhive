using IronHive.Connectors.OpenAI.Base;

namespace IronHive.Connectors.OpenAI.Extensions;

internal static class OpenAIModelExtensions
{
    internal static bool IsChatCompletion(this OpenAIModel model)
    {
        // 최신 모델 및 Alias 모델만 추가

        return model.Id.Equals("o3-mini")
            || model.Id.Equals("o1")
            || model.Id.Equals("o1-mini")
            || model.Id.Equals("gpt-4o-mini")
            || model.Id.Equals("gpt-4o");
            //|| model.Id.Equals("gpt-4-turbo")
            //|| model.Id.Equals("gpt-4");
    }

    internal static bool IsEmbedding(this OpenAIModel model)
    {
        return model.Id.Contains("embedding");
    }

    internal static bool IsTextToImage(this OpenAIModel model)
    {
        return model.Id.Contains("dall-e");
    }

    internal static bool IsTextToSpeech(this OpenAIModel model)
    {
        return model.Id.Contains("tts");
    }

    internal static bool IsAudioToText(this OpenAIModel model)
    {
        return model.Id.Contains("whisper");
    }
}
