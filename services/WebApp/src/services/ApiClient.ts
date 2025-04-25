import { CancelToken, HttpClient } from '@iyulab/http-client';
import { ChatCompletionRequest, StreamingResponse } from './models';

export class Api {
  private static readonly controller: HttpClient = new HttpClient({
    baseUrl: import.meta.env.VITE_SERVER_URL,
  });

  public static async *conversation(
    request: ChatCompletionRequest,
    token?: CancelToken,
  ) : AsyncGenerator<StreamingResponse> {

    const res = await this.controller.send({
      method: 'POST',
      path: '/conversation',
      body: request,
    }, token);

    if (!res.ok) {
      console.warn('Error:', await res.json());
    }

    for await (const msg of res.stream()) {
      if (msg.event === 'delta') {
        yield JSON.parse(msg.data.join('')) as StreamingResponse;
      }
    }
    
    console.log('Conversation completed');
  }

  public static async *upload(form: FormData) {
    for await (const msg of this.controller.upload({
      method: 'POST',
      path: '/upload',
      body: form,
    })) {
      yield msg;
    }
  }

  public static async download(fileName: string) {
    this.controller.download({
      path: `/download/${fileName}`,
    });
  }

}
