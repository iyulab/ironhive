import { HttpController } from '../internal';

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

  public async chatCompletionAsync() {
    return this.controller.post('/chat/completion');
  }

  public async getEmbeddingModelsAysnc() {
    return this.controller.get('/embedding/models');
  }

  public async getEmbeddingAsync() {
    return this.controller.get('/embedding');
  }

}
