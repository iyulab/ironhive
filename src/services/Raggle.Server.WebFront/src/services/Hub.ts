import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { Storage } from "./Storage";

export class Hub {
  private static hub?: HubConnection;

  public static async connect() {
    this.hub = new HubConnectionBuilder()
      .withUrl(`${Storage.host}/stream?userId=${Storage.userId}`)
      // .configureLogging({ log: (level, message) => console.log(`[${level}] ${message}`)})
      .build();
    await this.hub.start();
    this.hub.onclose(async () => {
      await new Promise(resolve => setTimeout(resolve, 5_000));
      this.hub?.start();
    });
  }

  public static chat(message: string) {
    return this.hub?.stream('Chat', Storage.userId, message, null);
  }

  public static async describe(content: string) {
    return this.hub?.stream('Describe', content);
  }

}