import axios, { AxiosInstance } from 'axios';
import { Assistant } from './Models';

export class API {
  private static readonly _client: AxiosInstance = axios.create({
    baseURL: import.meta.env.DEV
      ? import.meta.env.VITE_API || 'http://localhost:5000/v1'
      : window.location.origin + '/v1',
    headers: {
      'Content-Type': 'application/json',
    },
  });

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
  
}
