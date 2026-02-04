1. **반영** IronHive.Core.Resilience 및 Streaming의 용도가 분명하지 않음, 자세한 설명추가
2. **반영** IronHive.Abstractions.Messages의 ToolLimit관련 사항 검토
  - 현재 MaxTool이라는 property로 툴갯수에 대한 제한이 설정되어 있는데 툴을 갯수로 제한하는건 근본적 해결방안이 아님
  - 이유: LLM에서의 툴 사용은 텍스트 토큰으로 이루어지며, 각 툴마다의 토큰 수는 제각기 이므로, 갯수로 제한하는건 올바르지 않음, 예) 툴하나의 토큰이 툴열개의 토큰과 동일할 수 있음
  - 근본적으로는 툴의 토큰갯수를 미리 카운트하는 것이 최선의 방법이기는 하나 토큰카운트를 제공하는 서비스들만 가능하므로, 현재로써는 예외를 주는게 최선의 방법으로 사료됨
  - 검토 후 삭제 및 TODO 사항으로 업데이트
3. **주의** IronHive.Providers.OpenAI에 다른 provider 지원금지
  - 이 패키지는 OpenAI만의 특성을 가져야합니다. 절대 다른 서비스의 추가제공 프로퍼티들을 추가하지마세요.
  - 이유: 다른 서비스가 들어올 경우, 유지/보수에 혼란이 오며, 각 서비스들과의 호환성을 완벽히 맞추기 어려워집니다.
  - OpenAI의 규격을 따르나 조금 다른 기능들이 있는 서비스들은 따로 IronHive.Providers.OpenAI.Compatible 패키지에서 처리합니다.
  - IronHive.Providers.OpenAI.Compatible 구현 계획
    1. 각 서비스 제공자별 타입을 추가, 나머지는 Custom
    2. 각 서비스 제공자별 제공하는 서비스 타입을 분류(TextGeneration, Embedding, Models, ...)
    3. 각 서비스 제공자별 서비스 Add(Provider) 확장메서드 제공
    4. 각 서비스 제공자별 메시지 제네레이터 추가, 구현 형태는 다음과 같은 형태
      1. request 
      2. => request.ToOpenAI() 
      3. => 지원하는 않는 요청문 null처리 또는 수정 
      4. => IronHive.Providers.OpenAI.Client(Responses or Chat)들의 응답에 Source 속성을 추가하여 Json응답을 전달
      5. => OpenAI(Responses or Chat)MessageGenerator의 응답 파싱 메서드를 통해 응답을 파싱
      6. => 응답에 Source속성의 사항을 참고하여 추가 또는 수정후 반환
4. **검토** 확인해야할 사항
  - GoogleAI StreamingContent에 ?alt=sse 쿼리확인(응답이 json이 아닐경우 이벤트 방식으로 파싱방법을 변경해야함)
  - LocalVectorStorage(Sqlite-vec0)의 테이블 삭제에서 drop virtual table 문법이 없는지 확인
