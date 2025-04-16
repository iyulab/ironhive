import { HttpController } from './http';
import { ChatCompletionRequest } from './models';

export class Api {
  private static readonly controller: HttpController = new HttpController({
    baseUrl: import.meta.env.VITE_SERVER_URL,
  });

  public static async chatAsync(request: ChatCompletionRequest, onReceive?: (data: any) => void) {
    const res = await this.controller.post('/conversation', request, {
      onReceive: onReceive
    });
    return res;
  }

}
