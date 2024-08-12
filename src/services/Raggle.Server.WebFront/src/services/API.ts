import { Storage } from "./Storage";

export class API {
  public static async getUser() {
    const url = new URL(`${Storage.host}/user/${Storage.userId}`);
    const res = await fetch(url, {
      method: 'GET',
    });
    if (res.status === 404) {
      return undefined;
    } else {
      const user = await res.json();
      return user;
    }
  }

  public static async createUser(user: any) {
    const url = new URL(`${Storage.host}/user`);
    await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(user)
    });
  }

  public static async clearHistory() {
    const url = new URL(`${Storage.host}/user/${Storage.userId}`);
    await fetch(url, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        chatHistory: []
      })
    });
  }

  public static async getSources(userId: string) {
    const url = new URL(`${Storage.host}/sources/${userId}`);
    const res = await fetch(url, {
      method: 'GET',
    });
    const sources = await res.json();
    return sources;
  }

  public static async getSource(sourceId: string) {
    const url = new URL(`${Storage.host}/source/${sourceId}`);
    const res = await fetch(url, {
      method: 'GET',
    });
    const source = await res.json();
    return source;
  }

  public static async createSource(source: any) {
    const url = new URL(`${Storage.host}/source`);
    const res = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(source)
    });
    const createdSource = await res.json();
    return createdSource;
  }

  public static async updateSource(source: any) {
    const url = new URL(`${Storage.host}/source/${source.id}`);
    const res = await fetch(url, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(source)
    });
    const updatedSource = await res.json();
    return updatedSource;
  }

  public static async deleteSource(sourceId: string) {
    const url = new URL(`${Storage.host}/source/${sourceId}`);
    await fetch(url, {
      method: 'DELETE',
    });
  }

  public static async getSourceStatus(sourceId: string) {
    const url = new URL(`${Storage.host}/source/${sourceId}/status`);
    const res = await fetch(url, {
      method: 'GET',
    });
    const status = await res.json();
    return status;
  }

  public static async CheckFile(filename: string) {
    const url = new URL(`${Storage.host}/file/${filename}`);
    const res = await fetch(url, {
      method: 'GET',
    });
    return res.ok;
  }

}