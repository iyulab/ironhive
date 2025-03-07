import { HttpController } from '../common';

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

  public async getChatCompletionModelsAysnc() {
    return this.controller.get('/chat/models');
  }

  public async getEmbeddingModelsAysnc() {
    return this.controller.get('/embedding/models');
  }

}
