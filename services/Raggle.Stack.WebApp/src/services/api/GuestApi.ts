import { Message } from "../../models";
import { HttpController, HttpResponse } from "../http";

export class GuestApi {
  constructor(private client: HttpController) {}

  public async message(
    service: string,
    model: string,
    messages: Message[],
    onReceive: (message: Message) => void
  ): Promise<HttpResponse> {
    return await this.client.post(`/guest`, {
      service: service,
      model: model,
      messages: messages
    }, {
      onReceive: onReceive
    });
  }
}