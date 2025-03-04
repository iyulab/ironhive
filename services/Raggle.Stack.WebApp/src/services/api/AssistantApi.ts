// AssistantApi.ts
import { AssistantEntity, Message } from '../../models';
import { HttpController } from '../http/HttpController';
import { HttpResponse } from '../http/HttpResponse';

export class AssistantApi {
  constructor(private client: HttpController) {}

  public async find(
    skip: number = 0, 
    limit: number = 20
  ): Promise<AssistantEntity[]> {
    const res = await this.client.get('/assistants', { 
      params: { skip, limit } 
    });
    return await res.json<AssistantEntity[]>();
  }

  public async get(
    id: string
  ): Promise<AssistantEntity> {
    const res = await this.client.get(`/assistants/${id}`);
    return await res.json<AssistantEntity>();
  }

  public async upsert(
    entity: AssistantEntity
  ): Promise<AssistantEntity> {
    const res = await this.client.post('/assistants', entity);
    return await res.json<AssistantEntity>();
  }

  public async delete(
    id: string
  ): Promise<void> {
    await this.client.delete(`/assistants/${id}`);
  }

  public async message(
    id: string,
    messages: Message[],
    onReceive: (message: Message) => void
  ): Promise<HttpResponse> {
    return await this.client.post(`/assistants/${id}/chat`, messages, {
      onReceive: onReceive
    });
  }
}
