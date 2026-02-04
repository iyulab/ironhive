1. IronHive.Abstractions.Messages의 ToolLimit관련 사항 재검토
  - 현재 MaxTool이라는 property로 툴갯수에 대한 제한이 설정되어 있는데 툴을 갯수로 제한하는건 근본적 해결방안이 아님
  - 이유: LLM에서의 툴 사용은 텍스트 토큰으로 이루어지며, 각 툴마다의 토큰 수는 제각기 이므로, 갯수로 제한하는건 올바르지 않음, 예) 툴하나의 토큰이 툴열개의 토큰과 동일할 수 있음
  - 근본적으로는 툴의 토큰갯수를 미리 카운트하는 것이 최선의 방법이기는 하나 토큰카운트를 제공하는 서비스들만 가능하므로, 현재로써는 예외를 주는게 최선의 방법으로 사료됨
  - 검토 후 삭제 및 TODO 사항으로 업데이트
2. IronHive.Core.Resilience 및 Streaming의 용도가 분명하지 않음, 자세한 설명추가
3. IronHive.Providers.OpenAI에 다른 provider 지원금지
  - 이 패키지는 OpenAI만의 특성을 가져야합니다. 절대 다른 서비스의 추가제공 프로퍼티들을 추가하지마세요.
  - 이유: 다른 서비스가 들어올 경우, 유지/보수에 혼란이 오며, 각자의 버전을 제대로 맞추기 어려워집니다.
  - OpenAI의 규격을 따르나 조금 다른 기능들이 있을경우, 따로 IronHive.Providers.(others)패키지를 생성하고, IronHive.Providers.OpenAI를 상속하여 따로 구현하세요.
4. 
