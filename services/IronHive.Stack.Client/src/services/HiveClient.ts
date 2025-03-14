import { HttpController } from '../internal';
import { ChatCompletionRequest } from '../models';

export interface HiveStackConfig {
  baseUrl: string;
  token?: string;
}

export class HiveStack {
  private readonly controller: HttpController;

  constructor(config: HiveStackConfig) {
    this.controller = new HttpController({
      baseUrl: config.baseUrl
    });
  }

  public async getChatModelsAysnc() {
    return this.controller.get('/chat/models');
  }

  public async chatCompletionAsync(request: ChatCompletionRequest, onReceive?: (data: any) => void) {
    const res = await this.controller.post('/chat/completion', request, {
      onReceive: onReceive
    });
    return res;
  }

  public async getEmbeddingModelsAysnc() {
    return this.controller.get('/embedding/models');
  }

  public async getEmbeddingAsync() {
    return this.controller.get('/embedding');
  }

}
