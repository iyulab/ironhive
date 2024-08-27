import { HubConnection, HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";

import { Storage } from "./Storage";
import { App } from "./App";

export class Hub {
  private static readonly reconnectInterval = 10_000;
  private static hub?: HubConnection;

  public static async connect() {
    if (this.isConnected()) return;
    this.hub = new HubConnectionBuilder()
      .withUrl(`${Storage.host}/stream?userId=${Storage.userId}`)
      // .configureLogging({ log: (level, message) => console.log(`[${level}] ${message}`)})
      .build();

    await this.hub.start();
    this.hub.onclose(async () => {
      await new Promise(resolve => setTimeout(resolve, this.reconnectInterval));
      if (this.isDisconnected()) {
        this.hub?.start();
      }
    });
  }

  public static async disconnect() {
    if (this.isConnected()) return;
    await this.hub?.stop();
    this.hub = undefined;
  }

  public static chat(message: string, tags: string[] = []) {
    return this.hub?.stream('Chat', App.assistant.id, message, tags);
  }

  public static explain(message: string) {
    return this.hub?.stream('Explain', message);
  }

  private static isConnected() {
    return this.hub && this.hub.state !== HubConnectionState.Disconnected;
  }

  private static isDisconnected() {
    return this.hub && this.hub.state === HubConnectionState.Disconnected;
  }

}