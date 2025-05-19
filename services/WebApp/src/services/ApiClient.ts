import { CancelToken, HttpClient, CanceledError } from '@iyulab/http-client';
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
      const body = await res.json() as any;
      throw new Error(body.message);
    }

    for await (const chunk of res.stream()) {
      if (chunk.event === 'delta') {
        yield JSON.parse(chunk.data) as StreamingResponse;
      }
      if (chunk.event === 'cancelled') {
        throw new CanceledError(chunk.data);
      }
      if (chunk.event === 'error') {
        throw new Error(chunk.data);
      }
    }
  }

  public static async *upload(form: FormData) {
    for await (const chunk of this.controller.upload({
      method: 'POST',
      path: '/upload',
      body: form,
    })) {
      yield chunk;
    }
  }

  public static async download(fileName: string) {
    this.controller.download({
      path: `/download/${fileName}`,
    });
  }

}
