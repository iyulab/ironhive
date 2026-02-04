namespace IronHive.Providers.GoogleAI.Payloads.EmbedContent;

internal enum EmbedTaskType
{
    /// <summary>
    /// 타입 지정 안 됨. 작업 유형이 명시되지 않은 기본값입니다.
    /// </summary>
    TASK_TYPE_UNSPECIFIED,

    /// <summary>
    /// 검색 쿼리. 검색/정보 검색 시 사용자의 질문(쿼리) 텍스트임을 명시합니다.
    /// 예시) 검색창에 "대한민국 수도는 어디인가요?"라고 입력한 텍스트.
    /// </summary>
    RETRIEVAL_QUERY,

    /// <summary>
    /// 검색 대상이 되는 코퍼스(corpus)의 문서 텍스트임을 명시합니다.
    /// 예시) "서울은 대한민국의 수도이며..."로 시작하는 위키피디아 문서 전체.
    /// </summary>
    RETRIEVAL_DOCUMENT,

    /// <summary>
    /// 텍스트가 의미적 유사성 비교(Semantic Textual Similarity)에 사용될 것임을 명시합니다.
    /// 예시) "오늘 날씨 정말 좋다"와 "화창한 하루네요"라는 두 문장의 유사도를 계산할 때.
    /// </summary>
    SEMANTIC_SIMILARITY,

    /// <summary>
    /// 주어진 텍스트가 분류 작업에 사용될 것임을 명시합니다.
    /// 예시) 영화 리뷰 텍스트를 보고 '긍정 리뷰', '부정 리뷰'로 분류할 때.
    /// </summary>
    CLASSIFICATION,

    /// <summary>
    /// 임베딩이 군집화(클러스터링) 작업에 사용될 것임을 명시합니다.
    /// 예시) 수많은 뉴스 기사들을 자동으로 '정치', '경제', '스포츠' 등의 주제 그룹으로 묶을 때.
    /// </summary>
    CLUSTERING,

    /// <summary>
    /// 텍스트가 질의응답(Q_A) 작업에 사용될 것임을 명시합니다.
    /// 예시) 특정 법률 조항 문서를 주고 "벌금은 최대 얼마인가?"라는 질문에 대한 답을 찾을 때.
    /// </summary>
    QUESTION_ANSWERING,

    /// <summary>
    /// 텍스트가 사실 확인 작업에 사용될 것임을 명시합니다.
    /// 예시) "화성에는 물이 존재한다"는 주장이 참인지 거짓인지 판별할 때.
    /// </summary>
    FACT_VERIFICATION,

    /// <summary>
    /// 텍스트가 코드 검색을 위한 쿼리로 사용될 것임을 명시합니다.
    /// 예시) "파이썬으로 리스트를 오름차순으로 정렬하는 방법"이라고 검색할 때.
    /// </summary>
    CODE_RETRIEVAL_QUERY
}