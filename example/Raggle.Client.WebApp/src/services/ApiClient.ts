import axios, { AxiosInstance } from 'axios';
import { 
  AssistantEntity, 
  CollectionEntity, 
  Message, 
  ServiceModels, 
  StreamingResponse
} from '../models';

export class Api {
  private static readonly _baseURL: string = import.meta.env.DEV
    ? import.meta.env.VITE_API || 'http://localhost:5000/v1'
    : window.location.origin + '/v1';
  private static readonly _client: AxiosInstance = axios.create({
    baseURL: this._baseURL,
    headers: {
      'Content-Type': 'application/json',
    },
  });

  // Model API

  public static async getChatModelsAsync(): Promise<ServiceModels> {
    const res = await this._client.get<ServiceModels>('/models/chat');
    return res.data;
  }

  public static async getEmbeddingModelsAsync(): Promise<ServiceModels> {
    const res = await this._client.get<ServiceModels>('/models/embedding');
    return res.data;
  }

  // Assistant API

  public static async getAssistantsAsync(skip: number = 0, limit: number = 20): Promise<AssistantEntity[]> {
    const res = await this._client.get<AssistantEntity[]>('/assistants', {
      params: { skip, limit },
    });
    return res.data;
  }

  public static async getAssistantAsync(assistantId: string): Promise<AssistantEntity> {
    const res = await this._client.get<AssistantEntity>(`/assistants/${assistantId}`);
    return res.data;
  }

  public static async upsertAssistantAsync(assistant: AssistantEntity): Promise<AssistantEntity> {
    const res = await this._client.post<AssistantEntity>('/assistants', assistant);
    return res.data;
  }

  public static async deleteAssistantAsync(assistantId: string): Promise<void> {
    await this._client.delete(`/assistants/${assistantId}`);
  }

  public static chatAssistantAsync(
    assistantId: string,
    messages: Message[],
    on: (message: StreamingResponse) => void
  ): AbortController {
    const controller = new AbortController();
    const { signal } = controller;
  
    (async () => {
      try {
        console.log('Sending messages:', assistantId, messages);
  
        const response = await fetch(`${this._baseURL}/assistants/${assistantId}/chat`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(messages),
          signal,
        });
  
        if (!response.ok || !response.body) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
  
        const reader = response.body.getReader();
        const decoder = new TextDecoder('utf-8');
        let buffer = '';
  
        while (true) {
          const { done, value } = await reader.read();
          if (done) break;
  
          buffer += decoder.decode(value, { stream: true });
          let lines = buffer.split('\n');
          buffer = lines.pop() || '';
  
          for (const line of lines) {
            if (line.trim()) { // 빈 줄 무시
              try {
                const parsed = JSON.parse(line);
                on(parsed);
              } catch (e) {
                console.error('Error parsing JSON:', e);
              }
            }
          }
        }
  
        // 남아있는 버퍼 처리
        if (buffer.trim()) {
          try {
            const parsed = JSON.parse(buffer);
            on(parsed);
          } catch (e) {
            console.error('Error parsing JSON:', e);
          }
        }
  
      } catch (error: any) {
        if (error.name === 'AbortError') {
          console.log('Stream aborted');
        } else {
          console.error('Fetch error:', error);
        }
      }
    })(); // IIFE를 사용하여 비동기 작업 실행
  
    return controller; // AbortController를 즉시 반환
  }

  // Memory API
  
  public static async findCollectionsAsync(
    name?: string,
    limit: number = 10,
    skip: number = 0,
    order: string = 'desc'
  ): Promise<CollectionEntity[]> {
    const response = await this._client.get<CollectionEntity[]>('/memory', { 
      params: { limit, skip, order, name }
    });
    return response.data;
  }

  public static async upsertCollectionAsync(collection: CollectionEntity): Promise<CollectionEntity> {
    const response = await this._client.post<CollectionEntity>('/memory', collection);
    return response.data;
  }

  public static async deleteCollectionAsync(collectionId: string): Promise<void> {
    await this._client.delete(`/memory/${collectionId}`);
  }

  public static async searchCollectionAsync(collectionId: string, query: string): Promise<any> {
    const response = await this._client.post(`/memory/${collectionId}/search`, {
      query,
    });
    return response;
  }

  public static async findDocumentsAsync(
    collectionId: string,
    name?: string,
    limit: number = 10,
    skip: number = 0,
    order: string = 'desc'
  ): Promise<Document[]> {
    const params: any = { limit, skip, order };
    if (name) {
      params.name = name;
    }

    const response = await this._client.get<Document[]>(`/memory/${collectionId}/documents`,
      { params }
    );
    return response.data;
  }

  public static async uploadDocumentAsync(collectionId: string, file: File): Promise<void> {
    const formData = new FormData();
    formData.append("file", file);    
    await this._client.post(`/memory/${collectionId}/documents`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
      onUploadProgress(progressEvent) {
        console.log('Upload progress:', progressEvent.progress);
      },
    });
  }

  public static async deleteDocumentAsync(collectionId: string, documentId: string): Promise<void> {
    await this._client.delete(`/memory/${collectionId}/documents/${documentId}`);
  }
  
}
