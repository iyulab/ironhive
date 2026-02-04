using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Workflow;
using System.Text;
using System.Text.RegularExpressions;

namespace IronHive.Core.Memory.Pipelines;

/// <summary>
/// TextChunkerHandler는 주어진 텍스트를 청크로 나누는 메모리 파이프라인 핸들러입니다.
/// 의미적 경계를 고려하여 청크를 생성합니다.
/// </summary>
public partial class TextChunkingPipeline : IMemoryPipeline<TextChunkingPipeline.Options>
{
    private readonly IEmbeddingService _embedder;

    // 문장 분리 패턴 (마침표, 느낌표, 물음표 + 공백/줄바꿈)
    private static readonly Regex SentencePattern = SentenceSplitRegex();

    // 구분자 우선순위: 문단 > 문장 > 절 > 단어
    private static readonly string[] ParagraphSeparators = ["\n\n", "\r\n\r\n"];
    private static readonly char[] ClauseSeparators = [',', ';', ':', '，', '；', '：'];
    private static readonly char[] WordSeparators = [' ', '\t'];

    public TextChunkingPipeline(IEmbeddingService embedder)
    {
        _embedder = embedder;
    }

    /// <summary>
    /// 텍스트 청킹 옵션입니다.
    /// </summary>
    /// <param name="ChunkSize">청크당 최대 토큰 수 (기본값: 512)</param>
    /// <param name="ChunkOverlap">청크 간 오버랩 토큰 수 (기본값: 50)</param>
    /// <param name="MinChunkSize">최소 청크 크기 - 이보다 작으면 이전 청크에 병합 (기본값: 50)</param>
    public record Options(int ChunkSize = 512, int ChunkOverlap = 50, int MinChunkSize = 50);

    /// <summary>
    /// 텍스트를 청크로 나누는 핸들러입니다.
    /// 의미적 경계(문단, 문장, 절, 단어)를 고려하여 분할합니다.
    /// </summary>
    public async Task<TaskStepResult> ExecuteAsync(
        MemoryContext context,
        Options options,
        CancellationToken cancellationToken = default)
    {
        if (context.Target is not VectorMemoryTarget target)
            throw new InvalidOperationException("target is not a MemoryVectorTarget");
        if (!context.Payload.TryGetValue<string>("text", out var text))
            throw new InvalidOperationException("payload is not a string");

        var chunks = await ChunkTextAsync(
            text,
            target.EmbeddingProvider,
            target.EmbeddingModel,
            options,
            cancellationToken).ConfigureAwait(false);

        if (chunks.Count == 0)
            throw new InvalidOperationException("the document content is empty");

        context.Payload.Add("chunks", chunks);
        return TaskStepResult.Success();
    }

    /// <summary>
    /// 텍스트를 의미적 경계를 고려하여 청크로 분할합니다.
    /// </summary>
    private async Task<List<string>> ChunkTextAsync(
        string text,
        string provider,
        string model,
        Options options,
        CancellationToken cancellationToken)
    {
        var chunks = new List<string>();

        // 1단계: 문단으로 분할
        var paragraphs = SplitByParagraphs(text);

        var currentChunk = new StringBuilder();
        var currentTokens = 0;
        var overlapBuffer = new Queue<string>(); // 오버랩을 위한 버퍼

        foreach (var paragraph in paragraphs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(paragraph))
                continue;

            var paragraphTokens = await CountTokensAsync(provider, model, paragraph, cancellationToken)
                .ConfigureAwait(false);

            // 문단이 청크 크기보다 큰 경우 -> 문장 단위로 분할
            if (paragraphTokens > options.ChunkSize)
            {
                // 현재 청크가 있으면 먼저 저장
                if (currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk.ToString().Trim());
                    currentChunk.Clear();
                    currentTokens = 0;
                }

                // 문장 단위로 분할하여 청크 생성
                var sentenceChunks = await ChunkBySentencesAsync(
                    paragraph, provider, model, options, cancellationToken).ConfigureAwait(false);
                chunks.AddRange(sentenceChunks);
                continue;
            }

            // 현재 청크에 문단 추가 시 크기 초과하면 청크 완료
            if (currentTokens + paragraphTokens > options.ChunkSize && currentChunk.Length > 0)
            {
                var chunkText = currentChunk.ToString().Trim();
                chunks.Add(chunkText);

                // 오버랩 처리
                currentChunk.Clear();
                currentTokens = 0;

                if (options.ChunkOverlap > 0)
                {
                    var overlap = await GetOverlapTextAsync(
                        chunkText, provider, model, options.ChunkOverlap, cancellationToken)
                        .ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(overlap))
                    {
                        currentChunk.Append(overlap);
                        currentChunk.Append(' ');
                        currentTokens = await CountTokensAsync(provider, model, overlap, cancellationToken)
                            .ConfigureAwait(false);
                    }
                }
            }

            // 문단 추가
            currentChunk.AppendLine(paragraph);
            currentTokens += paragraphTokens;
        }

        // 마지막 청크 저장
        if (currentChunk.Length > 0)
        {
            var finalChunk = currentChunk.ToString().Trim();
            // 너무 작은 청크는 이전 청크에 병합
            if (chunks.Count > 0 && currentTokens < options.MinChunkSize)
            {
                var lastChunk = chunks[^1];
                chunks[^1] = lastChunk + "\n" + finalChunk;
            }
            else
            {
                chunks.Add(finalChunk);
            }
        }

        return chunks;
    }

    /// <summary>
    /// 문장 단위로 청크를 생성합니다 (큰 문단용).
    /// </summary>
    private async Task<List<string>> ChunkBySentencesAsync(
        string text,
        string provider,
        string model,
        Options options,
        CancellationToken cancellationToken)
    {
        var chunks = new List<string>();
        var sentences = SplitBySentences(text);

        var currentChunk = new StringBuilder();
        var currentTokens = 0;

        foreach (var sentence in sentences)
        {
            if (string.IsNullOrWhiteSpace(sentence))
                continue;

            var sentenceTokens = await CountTokensAsync(provider, model, sentence, cancellationToken)
                .ConfigureAwait(false);

            // 문장이 청크 크기보다 큰 경우 -> 절/단어 단위로 분할
            if (sentenceTokens > options.ChunkSize)
            {
                if (currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk.ToString().Trim());
                    currentChunk.Clear();
                    currentTokens = 0;
                }

                var subChunks = await ChunkLongTextAsync(
                    sentence, provider, model, options.ChunkSize, cancellationToken)
                    .ConfigureAwait(false);
                chunks.AddRange(subChunks);
                continue;
            }

            if (currentTokens + sentenceTokens > options.ChunkSize && currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
                currentChunk.Clear();
                currentTokens = 0;
            }

            currentChunk.Append(sentence);
            currentChunk.Append(' ');
            currentTokens += sentenceTokens;
        }

        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }

        return chunks;
    }

    /// <summary>
    /// 매우 긴 텍스트를 절/단어 단위로 분할합니다.
    /// </summary>
    private async Task<List<string>> ChunkLongTextAsync(
        string text,
        string provider,
        string model,
        int maxTokens,
        CancellationToken cancellationToken)
    {
        var chunks = new List<string>();

        // 절 단위로 먼저 시도
        var parts = SplitByClauses(text);
        var currentChunk = new StringBuilder();
        var currentTokens = 0;

        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part))
                continue;

            var partTokens = await CountTokensAsync(provider, model, part, cancellationToken)
                .ConfigureAwait(false);

            // 절도 너무 긴 경우 단어 단위로 분할
            if (partTokens > maxTokens)
            {
                if (currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk.ToString().Trim());
                    currentChunk.Clear();
                    currentTokens = 0;
                }

                var wordChunks = await ChunkByWordsAsync(part, provider, model, maxTokens, cancellationToken)
                    .ConfigureAwait(false);
                chunks.AddRange(wordChunks);
                continue;
            }

            if (currentTokens + partTokens > maxTokens && currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
                currentChunk.Clear();
                currentTokens = 0;
            }

            currentChunk.Append(part);
            currentTokens += partTokens;
        }

        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }

        return chunks;
    }

    /// <summary>
    /// 단어 단위로 분할합니다 (최후의 수단).
    /// </summary>
    private async Task<List<string>> ChunkByWordsAsync(
        string text,
        string provider,
        string model,
        int maxTokens,
        CancellationToken cancellationToken)
    {
        var chunks = new List<string>();
        var words = text.Split(WordSeparators, StringSplitOptions.RemoveEmptyEntries);

        var currentChunk = new StringBuilder();
        var currentTokens = 0;

        foreach (var word in words)
        {
            var wordTokens = await CountTokensAsync(provider, model, word, cancellationToken)
                .ConfigureAwait(false);

            if (currentTokens + wordTokens > maxTokens && currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
                currentChunk.Clear();
                currentTokens = 0;
            }

            currentChunk.Append(word);
            currentChunk.Append(' ');
            currentTokens += wordTokens;
        }

        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }

        return chunks;
    }

    /// <summary>
    /// 오버랩 텍스트를 추출합니다.
    /// </summary>
    private async Task<string> GetOverlapTextAsync(
        string text,
        string provider,
        string model,
        int overlapTokens,
        CancellationToken cancellationToken)
    {
        // 문장 단위로 뒤에서부터 오버랩 토큰 수만큼 추출
        var sentences = SplitBySentences(text);
        var overlap = new List<string>();
        var tokens = 0;

        for (int i = sentences.Length - 1; i >= 0 && tokens < overlapTokens; i--)
        {
            var sentence = sentences[i];
            if (string.IsNullOrWhiteSpace(sentence))
                continue;

            var sentenceTokens = await CountTokensAsync(provider, model, sentence, cancellationToken)
                .ConfigureAwait(false);

            if (tokens + sentenceTokens <= overlapTokens)
            {
                overlap.Insert(0, sentence);
                tokens += sentenceTokens;
            }
            else
            {
                break;
            }
        }

        return string.Join(" ", overlap);
    }

    /// <summary>
    /// 토큰 수를 계산합니다.
    /// </summary>
    private async Task<int> CountTokensAsync(
        string provider,
        string model,
        string text,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _embedder.CountTokensAsync(provider, model, text, cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            // 토큰 카운트 실패 시 문자 수 기반 추정 (대략 4자 = 1토큰)
            return text.Length / 4;
        }
    }

    /// <summary>
    /// 문단 단위로 분할합니다.
    /// </summary>
    private static string[] SplitByParagraphs(string text)
    {
        // 빈 줄로 구분된 문단 분할
        return text.Split(ParagraphSeparators, StringSplitOptions.RemoveEmptyEntries)
                   .Select(p => p.Trim())
                   .Where(p => !string.IsNullOrWhiteSpace(p))
                   .ToArray();
    }

    /// <summary>
    /// 문장 단위로 분할합니다.
    /// </summary>
    private static string[] SplitBySentences(string text)
    {
        return SentencePattern.Split(text)
                              .Select(s => s.Trim())
                              .Where(s => !string.IsNullOrWhiteSpace(s))
                              .ToArray();
    }

    /// <summary>
    /// 절 단위로 분할합니다.
    /// </summary>
    private static string[] SplitByClauses(string text)
    {
        return text.Split(ClauseSeparators, StringSplitOptions.RemoveEmptyEntries)
                   .Select(c => c.Trim())
                   .Where(c => !string.IsNullOrWhiteSpace(c))
                   .ToArray();
    }

    [GeneratedRegex(@"(?<=[.!?。！？])\s+", RegexOptions.Compiled)]
    private static partial Regex SentenceSplitRegex();
}
