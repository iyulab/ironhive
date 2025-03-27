using System.Text;
using Tiktoken;

namespace IronHive.Core.Legacy;

/// <summary>
/// 문서 조각 핸들러
/// </summary>
public class LegacyChunkHandler
{
    private readonly Tiktoken.Encoder _tokenizer = ModelToEncoder.For("gpt-4o");
    private readonly int _chunkSize = 2048;

    public class DocumentFragment
    {
        public int Index { get; set; } = 0;

        public string? Unit { get; set; }

        public int From { get; set; } = 0;

        public int To { get; set; } = 0;

        public object? Content { get; set; }
    }

    public object Process(object pipeline, CancellationToken cancellationToken)
    {
        var content = new List<string>();
        var chunks = new List<DocumentFragment>();

        long totalTokenCount = 0;
        int sectionIndex = 0;
        int sectionFrom = 1;
        int sectionTo = 0;
        var sb = new StringBuilder();

        foreach (var item in content)
        {
            sectionTo++;
            var tokenCount = _tokenizer.CountTokens(item);

            if (totalTokenCount + tokenCount > _chunkSize)
            {
                var chunk = new DocumentFragment
                {
                    Index = sectionIndex,
                    Unit = "Page",
                    From = sectionFrom,
                    To = sectionTo,
                    Content = sb.ToString(),
                };
                chunks.Add(chunk);

                sectionIndex++;
                sb.Clear();
                totalTokenCount = 0;
                sectionFrom = sectionTo + 1;
            }

            sb.AppendLine(item);
            totalTokenCount += tokenCount;
        }

        if (sb.Length > 0)
        {
            var chunk = new DocumentFragment
            {
                Index = sectionIndex,
                Unit = "Page",
                From = sectionFrom,
                To = sectionTo,
                Content = sb.ToString(),
            };

            chunks.Add(chunk);
        }

        return pipeline;
    }
}
