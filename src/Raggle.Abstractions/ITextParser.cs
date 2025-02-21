namespace Raggle.Abstractions;

public interface ITextParser<T>
{
    /// <summary>
    /// Parse the text data to a specific model.
    /// </summary>
    T Parse(string text);

    /// <summary>
    /// Convert the model to a string.
    /// </summary>
    string Stringify(T model);
}
