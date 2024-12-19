import { 
  AssistantEntity, 
  CollectionEntity, 
  Message, 
  AIServiceModels,
} from '../models';
import { HttpController } from './http/HttpController';
import { HttpResponse } from './http/HttpResponse';

export class Api {
  private static readonly _client: HttpController = new HttpController({
    baseUrl: import.meta.env.DEV 
      ? import.meta.env.VITE_API 
      : window.location.origin + '/v1'
  });

  // Model API

  public static async getChatModelsAsync(): Promise<AIServiceModels> {
    const res = await this._client.get('/models/chat');
    return await res.json<AIServiceModels>();
  }

  public static async getEmbeddingModelsAsync(): Promise<AIServiceModels> {
    const res = await this._client.get('/models/embedding');
    return await res.json<AIServiceModels>();
  }

  // Assistant API

  public static async getAssistantsAsync(skip: number = 0, limit: number = 20): Promise<AssistantEntity[]> {
    const res = await this._client.get('/assistants', { 
      params: { skip, limit } 
    });
    return await res.json<AssistantEntity[]>();
  }

  public static async getAssistantAsync(assistantId: string): Promise<AssistantEntity> {
    const res = await this._client.get(`/assistants/${assistantId}`);
    return await res.json<AssistantEntity>();
  }

  public static async upsertAssistantAsync(assistant: AssistantEntity): Promise<AssistantEntity> {
    const res = await this._client.post('/assistants', assistant);
    return await res.json<AssistantEntity>();
  }

  public static async deleteAssistantAsync(assistantId: string): Promise<void> {
    await this._client.delete(`/assistants/${assistantId}`);
  }

  public static async chatAssistantAsync(assistantId: string, messages: Message[]): Promise<HttpResponse> {
    return await this._client.post(`/assistants/${assistantId}/chat`, messages);
  }

  // Memory API
  
  public static async findCollectionsAsync(
    name?: string,
    limit: number = 10,
    skip: number = 0,
    order: string = 'desc'
  ): Promise<CollectionEntity[]> {
    const res = await this._client.get('/memory', { 
      params: { limit, skip, order, name: name ?? '' }
    });
    return await res.json<CollectionEntity[]>();
  }

  public static async getCollectionAsync(collectionId: string): Promise<CollectionEntity> {
    const res = await this._client.get(`/memory/${collectionId}`);
    return await res.json<CollectionEntity>();
  }

  public static async upsertCollectionAsync(collection: CollectionEntity): Promise<CollectionEntity> {
    const res = await this._client.post('/memory', collection);
    return await res.json<CollectionEntity>();
  }

  public static async deleteCollectionAsync(collectionId: string): Promise<void> {
    await this._client.delete(`/memory/${collectionId}`);
  }

  public static async searchCollectionAsync(collectionId: string, query: string): Promise<any> {
    const res = await this._client.post(`/memory/${collectionId}/search`, {
      query,
    });
    return res;
  }

  public static async findDocumentsAsync(
    collectionId: string,
    name?: string,
    limit: number = 10,
    skip: number = 0,
    order: string = 'desc'
  ): Promise<Document[]> {
    const res = await this._client.get(`/memory/${collectionId}/documents`,{ 
      params: { limit, skip, order, name: name ?? '' }
    });
    return await res.json<Document[]>();
  }

  public static uploadDocument(
    collectionId: string, 
    files: File[], 
    tags: string[] = []
  ): XMLHttpRequest {
    const formData = new FormData();
    files.forEach((file) => {
      formData.append('files', file);
    });
    tags.forEach((tag) => {
      formData.append('tags', tag);
    });
    return this._client.upload(`/memory/${collectionId}/documents`, formData,
      (e) => {
        console.log(e);
      },
      (e) => {
        console.log(e);
      },
      (e) => {
        console.log(e);
      }
    );
  }

  public static async deleteDocumentAsync(collectionId: string, documentId: string): Promise<void> {
    await this._client.delete(`/memory/${collectionId}/documents/${documentId}`);
  }
  
}
