import { HttpClient } from './http';
import { ChatCompletionRequest } from './models';

export class Api {
  private static readonly controller: HttpClient = new HttpClient({
    baseUrl: import.meta.env.VITE_SERVER_URL,
  });

  public static async *conversation(request: ChatCompletionRequest) {
    const res = await this.controller.send({
      method: 'POST',
      path: '/conversation',
      body: request,
    });

    for await (const msg of res.stream()) {
      console.log(msg);
      yield msg;
    }
  }

}
