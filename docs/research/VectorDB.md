# VectorDB 비교 분석

| VectorDB   | Meta | Embed  | Local | 비고 |
|------------|------|--------|-------|------|
| Pinecone   | O    | O / X  | X     | 클라우드 전용, 강력한 임베딩 |
| Milvus     | O    | X      | O     | 임베딩 모델 직접 필요, Python SDK |
| Qdrant     | X    | X      | O     | 메타데이터 불가, 빠름, DotNet SDK |
| Weaviate   | X    | O      | O     | 외부 임베딩 불가 |
| Chroma     | O    | O / X  | O     | 가벼운 설치, 강력한 임베딩, Python SDK |

- **Meta**: 컬렉션 외에 별도 메타 정보 저장 가능 여부  
- **Embed**: 자체 임베딩 또는 외부 모델 (O/X: 선택 가능)  
- **Local**: 로컬 설치 지원 여부 (X는 클라우드 전용)
