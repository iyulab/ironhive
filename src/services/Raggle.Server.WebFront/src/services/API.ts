import { Storage } from "./Storage";

export class API {
  public static async getUser() {
    const url = new URL(`${Storage.host}/user/${Storage.userId}`);
    const res = await fetch(url, {
      method: 'GET',
    });
    const user = await res.json();
    return user;
  }

  public static async clearHistory() {
    const url = new URL(`${Storage.host}/user/${Storage.userId}`);
    await fetch(url, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        id: Storage.userId,
        chatHistory: []
      })
    });
  }
}