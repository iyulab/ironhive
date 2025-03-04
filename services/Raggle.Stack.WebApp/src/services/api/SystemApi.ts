import { AIServiceModels } from '../../models';
import { HttpController } from '../http/HttpController';

export class SystemApi {
  constructor(private client: HttpController) {}

  public async getModels(type: "chat" | "embed"): Promise<AIServiceModels> {
    const endpoint = type === "chat" 
      ? '/models/chat' 
      : '/models/embedding';
    const res = await this.client.get(endpoint);
    return await res.json<AIServiceModels>();
  }

}
