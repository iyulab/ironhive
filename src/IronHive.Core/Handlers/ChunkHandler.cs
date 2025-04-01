using IronHive.Abstractions.Memory;
using System.Text;
using Tiktoken;

namespace IronHive.Core.Handlers;

public class ChunkHandler : IPipelineHandler
{
    private readonly Tiktoken.Encoder _tokenizer = ModelToEncoder.For("gpt-4o");

    public class Options
    {
        public int ChunkSize { get; set; } = 2048;
    }

    public Task<DataPipeline> ProcessAsync(DataPipeline pipeline, CancellationToken cancellationToken)
    {
        if(pipeline.Content.TryConvertTo<string>(out var content))
        {
            var fragments = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var options = pipeline.GetCurrentOptions<Options>() ?? new Options();
            var chunks = new List<string>();

            long total = 0;
            var sb = new StringBuilder();
            foreach (var item in fragments)
            {
                var tokenCount = _tokenizer.CountTokens(item);
                if (total + tokenCount > options.ChunkSize)
                {
                    chunks.Add(sb.ToString());
                    sb.Clear();
                    total = 0;
                }
                sb.AppendLine(item);
                total += tokenCount;
            }

            if (sb.Length > 0)
            {
                chunks.Add(sb.ToString());
            }

            pipeline.Content = chunks;
            return Task.FromResult(pipeline);
        }
        else
        {
            throw new InvalidOperationException("The document content is not a string.");
        }
    }
}
