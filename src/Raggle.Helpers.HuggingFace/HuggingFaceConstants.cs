namespace Raggle.Helpers.HuggingFace;

/// <summary>
/// <see href="https://huggingface.co/docs/hub/api">Hugging Face API Documentation</see>
/// </summary>
internal class HuggingFaceConstants
{
    internal const string Host = "huggingface.co";
    internal const string AuthHeaderName = "Authorization";
    internal const string AuthHeaderValue = $"Bearer {{0}}";

    internal const string GetModelsPath = "/api/models";
    internal const string GetModelsDefaultQuery = "full=true&config=false";
    internal const string GetModelPath = $"/api/models/{{0}}";

    internal const string GetFilePath = $"/{{0}}/resolve/main/{{1}}";
    internal const string GetFileDefaultQuery = "download=true";
}
