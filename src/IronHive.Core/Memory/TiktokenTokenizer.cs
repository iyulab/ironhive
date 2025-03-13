using IronHive.Abstractions.Memory;
using Tiktoken;

namespace IronHive.Core.Memory;

// 임시
public class TiktokenTokenizer : ITextTokenizer
{
    private readonly Encoder _tokenizer = ModelToEncoder.For("gpt-4o");

    /// <inheritdoc />
    public int CountTokens(string text)
    {
        return _tokenizer.CountTokens(text);
    }

    /// <inheritdoc />
    public IReadOnlyList<int> Encode(string text)
    {
        return _tokenizer.Encode(text).ToList();
    }

    /// <inheritdoc />
    public string Decode(IReadOnlyList<int> values)
    {
        return _tokenizer.Decode(values);
    }
}
