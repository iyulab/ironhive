import axios, { AxiosInstance } from 'axios';
import { Assistant, Collection, Models } from './Models';

export class API {
  private static readonly _client: AxiosInstance = axios.create({
    baseURL: import.meta.env.DEV
      ? import.meta.env.VITE_API || 'http://localhost:5000/v1'
      : window.location.origin + '/v1',
    headers: {
      'Content-Type': 'application/json',
    },
  });

  // Model API

  public static async getChatModels(): Promise<Models> {
    const res = await this._client.get<Models>('/models/chat');
    return res.data;
  }

  public static async getEmbeddingModels(): Promise<Models> {
    const res = await this._client.get<Models>('/models/embedding');
    return res.data;
  }

  // Assistant API

  public static async getAssistants(skip: number = 0, limit: number = 20): Promise<Assistant[]> {
    const res = await this._client.get<Assistant[]>('/assistants', {
      params: { skip, limit },
    });
    return res.data;
  }

  public static async getAssistant(assistantId: string): Promise<Assistant> {
    const res = await this._client.get<Assistant>(`/assistants/${assistantId}`);
    return res.data;
  }

  public static async upsertAssistant(assistant: Assistant): Promise<Assistant> {
    const res = await this._client.post<Assistant>('/assistants', assistant);
    return res.data;
  }

  public static async deleteAssistant(assistantId: string): Promise<void> {
    await this._client.delete(`/assistants/${assistantId}`);
  }

  // Memory API
  
  public static async findCollections(
    name?: string,
    limit: number = 10,
    skip: number = 0,
    order: string = 'desc'
  ): Promise<Collection[]> {
    const response = await this._client.get<Collection[]>('/memory', { 
      params: { limit, skip, order, name }
    });
    return response.data;
  }

  public static async upsertCollection(collection: Collection): Promise<Collection> {
    const response = await this._client.post<Collection>('/memory', collection);
    return response.data;
  }

  public static async deleteCollection(collectionId: string): Promise<void> {
    await this._client.delete(`/memory/${collectionId}`);
  }

  // 업로드

  public static async uploadFile(collectionId: string, file: File): Promise<void> {
    const formData = new FormData();
    formData.append("fileName", "file");
    formData.append("file", file);
    console.log('Uploading file:', file);
    try {
      await this._client.post(`/memory/${collectionId}/upload`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
        onUploadProgress(progressEvent) {
          const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
          console.log('Upload progress:', percentCompleted);
        },
      });
    } catch (error) {
      console.error('File upload failed:', error);
      throw error;
    }

    
  }
  
}
