# TODO LIST

## 서비스 빌더
	- 서비스 빌더 패턴 재수정 고려
	- IConnectorCollection 인터페이스 추가??
	- IConnector에 ConnectorName 속성추가?(IToolPlugin 확인)

## 세션 만들기(?)
	- 세션 타이틀 생성
	- 메시지 토큰 관리(요약, 자르기)
	- 마스터 에이전트 관리
	- 에이전트 추가, 삭제 기능

## 에이전트
	- gemini, xai 커넥터 추가, openrouter 추가 고려
	- 스트리밍 규격 재설정
	- 툴 사용방법 재설정(OpenAPI, 서비스 빌더 패턴 수정)

## 메모리
	- 큐 스토리지에 메시지 Receive 이벤트 추가 고려, 양방향 통신
	- Worker Manager 생성, 복수 Woker와 작업 Cancel 등 작업 상태 관리
	- (?) 파이프라인 워커에 Semaphore 위치를 StartAsync메서드로 이동 고려
