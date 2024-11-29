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

  public static async getChatModelsAsync(): Promise<Models> {
    try {
      const res = await this._client.get<Models>('/models/chat');
      return res.data;
    } catch (error) {
      console.error('Error fetching chat models:', error);
      throw error;
    }
  }

  public static async getEmbeddingModelsAsync(): Promise<Models> {
    try {
      const res = await this._client.get<Models>('/models/embedding');
      return res.data;
    } catch (error) {
      console.error('Error fetching embedding models:', error);
      throw error;
    }
  }

  // Assistant API

  public static async getAssistantsAsync(skip: number = 0, limit: number = 20): Promise<Assistant[]> {
    try {
      const res = await this._client.get<Assistant[]>('/assistants', {
        params: { skip, limit },
      });
      return res.data;
    } catch (error) {
      console.error('Error fetching assistants:', error);
      throw error;
    }
  }

  public static async getAssistantAsync(assistantId: string): Promise<Assistant> {
    try {
      const res = await this._client.get<Assistant>(`/assistants/${assistantId}`);
      return res.data;
    } catch (error) {
      console.error('Error fetching assistant:', error);
      throw error;
    }
  }

  public static async upsertAssistantAsync(assistant: Assistant): Promise<Assistant> {
    try {
      const res = await this._client.post<Assistant>('/assistants', assistant);
      return res.data;
    } catch (error) {
      console.error('Error upserting assistant:', error);
      throw error;
    }
  }

  public static async deleteAssistantAsync(assistantId: string): Promise<void> {
    try {
      await this._client.delete(`/assistants/${assistantId}`);
    } catch (error) {
      console.error('Error deleting assistant:', error);
      throw error;
    }
  }

  // Memory API
  
  public static async findCollectionsAsync(
    name?: string,
    limit: number = 10,
    skip: number = 0,
    order: string = 'desc'
  ): Promise<Collection[]> {
    try {
      const response = await this._client.get<Collection[]>('/memory', { 
        params: { limit, skip, order, name }
      });
      return response.data;
    } catch (error) {
      console.error('Error finding collections:', error);
      throw error;
    }
  }

  public static async upsertCollectionAsync(collection: Collection): Promise<Collection> {
    try {
      const response = await this._client.post<Collection>('/memory', collection);
      return response.data;
    } catch (error) {
      console.error('Error upserting collection:', error);
      throw error;
    }
  }

  public static async deleteCollectionAsync(collectionId: string): Promise<void> {
    try {
      await this._client.delete(`/memory/${collectionId}`);
    } catch (error) {
      console.error('Error deleting collection:', error);
      throw error;
    }
  }

  public static async searchCollectionAsync(collectionId: string, query: string): Promise<any> {
    try {
      const response = await this._client.post(`/memory/${collectionId}/search`, {
        query,
      });
      return response;
    } catch (error) {
      console.error('Error searching document:', error);
      throw error;
    }
  }

  public static async findDocumentsAsync(
    collectionId: string,
    name?: string,
    limit: number = 10,
    skip: number = 0,
    order: string = 'desc'
  ): Promise<Document[]> {
    try {
      const params: any = { limit, skip, order };
      if (name) {
        params.name = name;
      }

      const response = await this._client.get<Document[]>(`/memory/${collectionId}/documents`,
        { params }
      );
      return response.data;
    } catch (error) {
      console.error('문서 조회 실패:', error);
      throw error;
    }
  }

  public static async uploadDocumentAsync(collectionId: string, file: File): Promise<void> {
    const formData = new FormData();
    formData.append("file", file);
    try {
      await this._client.post(`/memory/${collectionId}/documents`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
        onUploadProgress(progressEvent) {
          console.log('Upload progress:', progressEvent.progress);
        },
      });
    } catch (error) {
      console.error('File upload failed:', error);
    }
  }

  public static async deleteDocumentAsync(collectionId: string, documentId: string): Promise<void> {
    try {
      await this._client.delete(`/memory/${collectionId}/documents/${documentId}`);
      console.log(`문서 ${documentId} 삭제 완료`);
    } catch (error) {
      console.error('문서 삭제 실패:', error);
    }
  }
  
}
