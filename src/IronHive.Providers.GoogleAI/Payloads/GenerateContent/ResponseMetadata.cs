using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.GenerateContent;

/// <summary>인용 메타데이터(출처 배열).</summary>
internal sealed class CitationMetadata
{
    [JsonPropertyName("citationSources")]
    public ICollection<Source>? Sources { get; set; }

    internal sealed class Source
    {
        /// <summary>응답 텍스트 내 시작 바이트 오프셋(포함).</summary>
        [JsonPropertyName("startIndex")]
        public int? StartIndex { get; set; }

        /// <summary>응답 텍스트 내 종료 바이트 오프셋(제외).</summary>
        [JsonPropertyName("endIndex")]
        public int? EndIndex { get; set; }

        /// <summary>출처 URI.</summary>
        [JsonPropertyName("uri")]
        public string? Uri { get; set; }

        /// <summary>코드 인용 시 라이선스 명시 필요.</summary>
        [JsonPropertyName("license")]
        public string? License { get; set; }
    }
}

/// <summary>그라운딩(검색/문서) 관련 메타데이터.</summary>
internal sealed class GroundingMetadata
{
    /// <summary>그라운딩에 사용된 청크(웹/패시지 등).</summary>
    [JsonPropertyName("groundingChunks")]
    public ICollection<Chunk>? Chunks { get; set; }

    /// <summary>응답 텍스트의 특정 구간과 청크의 연결과 신뢰도.</summary>
    [JsonPropertyName("groundingSupports")]
    public ICollection<Support>? Supports { get; set; }

    [JsonPropertyName("webSearchQueries")]
    public ICollection<string>? Queries { get; set; }

    /// <summary>검색 진입점(렌더링된 스니펫 등).</summary>
    [JsonPropertyName("searchEntryPoint")]
    public EntryPoint? SearchEntryPoint { get; set; }

    /// <summary>검색 동적 트리거 점수 등.</summary>
    [JsonPropertyName("retrievalMetadata")]
    public Metadata? RetrievalMetadata { get; set; }

    /// <summary>구글 지도 토큰 리소스의 이름입니다.</summary>
    [JsonPropertyName("googleMapsWidgetContextToken")]
    public string? GoogleMapsToken { get; set; }

    /// <summary>그라운딩 청크(유니온 타입, 셋중 하나)/summary>
    internal sealed class Chunk
    {
        /// <summary>웹 청크.</summary>
        [JsonPropertyName("web")]
        public GroundingWeb? Web { get; set; }

        /// <summary>파일검색 청크</summary>
        [JsonPropertyName("retrievedContext")]
        public GroundingContext? Context { get; set; }

        /// <summary>맵 청크</summary>
        [JsonPropertyName("maps")]
        public GroundingMap? Maps { get; set; }

        internal sealed class GroundingWeb
        {
            /// <summary>참조 URI.</summary>
            [JsonPropertyName("uri")]
            public string? Uri { get; set; }

            /// <summary>타이틀.</summary>
            [JsonPropertyName("title")]
            public string? Title { get; set; }
        }

        internal sealed class GroundingContext
        {
            /// <summary>참조 URI.</summary>
            [JsonPropertyName("uri")]
            public string? Uri { get; set; }

            /// <summary>문서 제목 또는 이름.</summary>
            [JsonPropertyName("title")]
            public string? Title { get; set; }

            /// <summary>문서의 텍스트.</summary>
            [JsonPropertyName("text")]
            public string? Text { get; set; }

            /// <summary>문서가 저장된 파일 검색 스토어.</summary>
            [JsonPropertyName("fileSearchStore")]
            public string? FileStore { get; set; }
        }

        internal sealed class GroundingMap
        {
            /// <summary>참조 URI.</summary>
            [JsonPropertyName("uri")]
            public string? Uri { get; set; }

            /// <summary>장소의 이름</summary>
            [JsonPropertyName("title")]
            public string? Title { get; set; }

            /// <summary>장소의 텍스트.</summary>
            [JsonPropertyName("text")]
            public string? Text { get; set; }

            /// <summary>장소의 ID</summary>
            [JsonPropertyName("placeId")]
            public string? PlaceId { get; set; }

            /// <summary>특정 장소의 리뷰글</summary>
            [JsonPropertyName("placeAnswerSources")]
            public JsonObject? AnswerSources { get; set; }
        }
    }

    /// <summary>응답 텍스트 특정 구간과 그라운딩 청크의 연결.</summary>
    internal sealed class Support
    {
        /// <summary>연결된 청크 인덱스들.</summary>
        [JsonPropertyName("groundingChunkIndices")]
        public ICollection<int>? GroundingChunkIndices { get; set; }

        /// <summary>각 연결에 대한 신뢰도(0~1).</summary>
        [JsonPropertyName("confidenceScores")]
        public ICollection<float>? ConfidenceScores { get; set; }

        /// <summary>응답 텍스트의 세그먼트(부분 문자열 범위).</summary>
        [JsonPropertyName("segment")]
        public Segment? Segment { get; set; }
    }

    /// <summary>응답 텍스트 내 세그먼트(파트/바이트 오프셋 기반).</summary>
    internal sealed class Segment
    {
        /// <summary>부모 Content의 parts에서 파트 인덱스.</summary>
        [JsonPropertyName("partIndex")]
        public int? PartIndex { get; set; }

        /// <summary>해당 파트 내 시작 바이트 오프셋(포함).</summary>
        [JsonPropertyName("startIndex")]
        public int? StartIndex { get; set; }

        /// <summary>해당 파트 내 종료 바이트 오프셋(제외).</summary>
        [JsonPropertyName("endIndex")]
        public int? EndIndex { get; set; }

        /// <summary>세그먼트 텍스트.</summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    internal sealed class EntryPoint
    {
        /// <summary>웹뷰 등에 임베드 가능한 렌더링 콘텐츠.</summary>
        [JsonPropertyName("renderedContent")]
        public string? Content { get; set; }

        /// <summary>Base64 encoded JSON representing array of (search term, search url) tuple.</summary>
        [JsonPropertyName("sdkBlob")]
        public string? Blob { get; set; }
    }

    internal sealed class Metadata
    {
        /// <summary>구글 검색 동적 검색 점수(0~1).</summary>
        [JsonPropertyName("googleSearchDynamicRetrievalScore")]
        public float? Score { get; set; }
    }
}

/// <summary>URL 컨텍스트 도구 메타데이터.</summary>
internal sealed class UrlContextMetadata
{
    [JsonPropertyName("urlMetadata")]
    public ICollection<Metadata>? UrlMetadata { get; set; }

    internal sealed class Metadata
    {
        /// <summary>가져온 URL.</summary>
        [JsonPropertyName("retrievedUrl")]
        public string? RetrievedUrl { get; set; }

        [JsonPropertyName("urlRetrievalStatus")]
        public string? RetrievalStatus { get; set; }

        internal enum Status
        {
            URL_RETRIEVAL_STATUS_UNSPECIFIED,
            URL_RETRIEVAL_STATUS_SUCCESS,
            URL_RETRIEVAL_STATUS_ERROR,
            URL_RETRIEVAL_STATUS_PAYWALL,
            URL_RETRIEVAL_STATUS_UNSAFE
        }
    }
}