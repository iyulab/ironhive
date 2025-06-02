import { CancelToken, HttpClient, CanceledError } from '@iyulab/http-client';
import { MessageGenerationRequest, StreamingMessageResponse } from './models';

export class Api {
  private static readonly controller: HttpClient = new HttpClient({
    baseUrl: import.meta.env.DEV ? import.meta.env.VITE_SERVER_URL : window.location.origin,
  });

  public static async *conversation(
    request: MessageGenerationRequest,
    token?: CancelToken,
  ) : AsyncGenerator<StreamingMessageResponse> {

    const res = await this.controller.send({
      method: 'POST',
      path: '/api/conversation',
      body: request,
    }, token);

    if (!res.ok) {
      const body = await res.json() as any;
      throw new Error(body.message);
    }

    for await (const chunk of res.stream()) {
      if (chunk.event === 'delta') {
        const data = JSON.parse(chunk.data) as StreamingMessageResponse;
        if (data.type === 'message.error') {
          if (data.code === 499)
            throw new CanceledError(data.message);
          else
            throw new Error(data.message || 'An error occurred during the conversation.');
        }
        else {
          yield data;
        }
      }
    }
  }

  public static async *upload(form: FormData) {
    for await (const chunk of this.controller.upload({
      method: 'POST',
      path: '/api/upload',
      body: form,
    })) {
      yield chunk;
    }
  }

  public static async download(fileName: string) {
    this.controller.download({
      path: `/api/download/${fileName}`,
    });
  }

}
