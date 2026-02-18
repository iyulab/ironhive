using System.Net;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using IronHive.Abstractions.Files;

namespace IronHive.Core.Files.Detectors;

/// <summary>
/// Apache에서 제공하는 mime.types 문서를 주기적으로 가져와 확장자→MIME 매핑을 제공하는 구현체.
/// </summary>
public sealed partial class ApacheFileMediaTypeDetector : IFileMediaTypeDetector, IDisposable
{
    /// <summary>
    /// 기본 Apache mime.types URL
    /// </summary>
    public const string ApacheDocsUrl = "https://svn.apache.org/repos/asf/httpd/httpd/trunk/docs/conf/mime.types";

    private readonly TimeSpan _ttl;
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);

    private Dictionary<string, string> _items = new(StringComparer.OrdinalIgnoreCase);
    private HashSet<string> _extensions = new(StringComparer.OrdinalIgnoreCase);
    private HashSet<string> _mediaTypes = new(StringComparer.OrdinalIgnoreCase);

    // 서버에서 내려준 ETag(리소스 버전 식별자). 다음 요청 시 If-None-Match로 사용
    private string? _etag;
    // 서버에서 내려준 리소스 최종 수정 시각. 다음 요청 시 If-Modified-Since로 사용
    private DateTimeOffset? _lastModified;
    // 마지막으로 데이터를 실제로 가져온 시간 (내부 TTL 캐시 체크용)
    private DateTimeOffset _lastLoaded = DateTimeOffset.MinValue;

    /// <param name="ttl">
    /// 캐시 TTL. 경과 후 접근 시 갱신 시도
    /// </param>
    public ApacheFileMediaTypeDetector(TimeSpan? ttl = null)
    {
        _ttl = ttl ?? TimeSpan.FromHours(12);

        try
        {
            // 초기 로드 (동기 블로킹)
            RefreshIfNeeded(true);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _lock.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public IEnumerable<string> Extensions
    {
        get 
        { 
            RefreshIfNeeded(); 
            _lock.EnterReadLock(); 
            try 
            {
                return _extensions.ToArray(); 
            } 
            finally 
            {
                _lock.ExitReadLock(); 
            } 
        }
    }

    /// <inheritdoc />
    public IEnumerable<string> MediaTypes
    {
        get 
        { 
            RefreshIfNeeded(); 
            _lock.EnterReadLock(); 
            try 
            { 
                return _mediaTypes.ToArray(); 
            }
            finally
            {
                _lock.ExitReadLock(); 
            }
        }
    }

    /// <inheritdoc />
    public string? Detect(string fileName)
        => TryDetect(fileName, out var mimeType) ? mimeType : null;

    /// <inheritdoc />
    public bool TryDetect(string fileName, [MaybeNullWhen(false)] out string mimeType)
    {
        RefreshIfNeeded();
        if (string.IsNullOrWhiteSpace(fileName))
        {
            mimeType = null;
            return false;
        }
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext))
        {
            mimeType = null;
            return false;
        }

        _lock.EnterReadLock();
        try
        {
            return _items.TryGetValue(ext, out mimeType);
        }
        finally
        {
            _lock.ExitReadLock();
        }

    }

    // ================= 내부 구현 =================

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();

    /// <summary>
    /// TTL 경과 시 Apache Docs에서 mime.types를 다시 가져와 갱신합니다.
    /// </summary>
    /// <param name="force">시간 상관없이 강제 갱신 여부</param>
    private void RefreshIfNeeded(bool force = false)
    {
        // double-checked + write lock
        if (!force && DateTimeOffset.UtcNow - _lastLoaded < _ttl) return;

        _lock.EnterWriteLock();
        try
        {
            if (!force && DateTimeOffset.UtcNow - _lastLoaded < _ttl) return;

            var req = new HttpRequestMessage(HttpMethod.Get, ApacheDocsUrl);
            if (!string.IsNullOrEmpty(_etag))
                req.Headers.TryAddWithoutValidation("If-None-Match", _etag);
            if (_lastModified.HasValue)
                req.Headers.IfModifiedSince = _lastModified;

            using var http = new HttpClient();
            using var resp = http.Send(req);
            if (resp.StatusCode == HttpStatusCode.NotModified)
            {
                _lastLoaded = DateTimeOffset.UtcNow;
                return;
            }

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                // 최초 로드 실패면 예외, 그 외에는 캐시 유지
                if (_lastLoaded == DateTimeOffset.MinValue)
                    throw new InvalidOperationException($"Failed to load mime.types: {(int)resp.StatusCode} {resp.ReasonPhrase}");
                return;
            }

            using var stream = resp.Content.ReadAsStream();
            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();

            var newItems = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var newExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var newMediaTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            ParseMimeTypes(new StringReader(text), newItems, newExtensions, newMediaTypes);

            // 스왑
            _items = newItems;
            _extensions = newExtensions;
            _mediaTypes = newMediaTypes;

            // 메타데이터 저장
            _etag = resp.Headers.ETag?.Tag;
            if (resp.Content.Headers.LastModified.HasValue)
                _lastModified = resp.Content.Headers.LastModified;
            else if (resp.Headers.Date.HasValue)
                _lastModified = resp.Headers.Date;

            _lastLoaded = DateTimeOffset.UtcNow;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Apache Docs의 내용을 파싱하여 사전에 채웁니다.
    /// </summary>
    private static void ParseMimeTypes(
        TextReader reader,
        Dictionary<string, string> items,
        HashSet<string> extensions,
        HashSet<string> mediaTypes)
    {
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            line = line.Trim();
            if (line.Length == 0) continue;
            if (line.StartsWith('#')) continue;

            // 공백/탭 기준 토큰화
            var tokens = WhitespaceRegex().Split(line)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToArray();

            if (tokens.Length < 2) continue;

            var mediaType = tokens[0].Trim();
            if (!IsValidMediaType(mediaType)) continue;

            mediaTypes.Add(mediaType);

            for (int i = 1; i < tokens.Length; i++)
            {
                var ext = NormalizeExtension(tokens[i]);
                if (string.IsNullOrEmpty(ext)) continue;

                // 첫 매핑만 등록 (중복 확장자 무시, 후속 매핑 덮어쓰지 않습니다.)
                if (!items.ContainsKey(ext))
                {
                    items[ext] = mediaType;
                    extensions.Add(ext);
                }
                // Duplicate extension mapping — first registration wins (intentionally ignored)
            }
        }
    }

    private static bool IsValidMediaType(string s)
    {
        var idx = s.IndexOf('/');
        return idx > 0 && idx < s.Length - 1;
    }

    private static string NormalizeExtension(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var s = raw.Trim();
        if (s[0] != '.') s = "." + s;
        return s.ToLowerInvariant();
    }
}
