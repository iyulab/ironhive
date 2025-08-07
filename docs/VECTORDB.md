# VectorDB 기능 비교

| VectorDB   | Meta | Embed  | Local | 비고 |
|------------|------|--------|-------|------|
| Pinecone   | O    | O / X  | X     | 클라우드 전용, 선택적 임베딩 |
| Milvus     | O    | X      | O     | 임베딩 직접 넣어야 함, Python SDK |
| Qdrant     | X    | X      | O     | 메타데이터 불가, 경량, DotNet SDK |
| Weaviate   | X    | O      | O     | 외부 임베딩 불가 |
| Chroma     | O    | O / X  | O     | 유연한 구성, 선택적 임베딩, Python SDK |

- **Meta**: 컬렉션 생성 시 임의 데이터 저장 가능 여부  
- **Embed**: 자체 임베딩 모델 요구 여부 (O/X: 선택 가능)  
- **Local**: 로컬 설치 가능 여부 (X는 클라우드 전용)
- 