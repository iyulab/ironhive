namespace Raggle.Core.ChatCompletion;

public interface IModelParser
{
    /// <summary>
    /// Parse the text data to a specific model.
    /// </summary>
    (string, string) Parse(string text);

    /// <summary>
    /// Convert the model to a string.
    /// </summary>
    string Stringify((string, string) model);
}
