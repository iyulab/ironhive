export class Storage {
  private static readonly userIdKey = "raggle-userId";
  private static readonly _host: string = import.meta.env.VITE_HOST || location.origin;
  private static _userId: string | null = localStorage.getItem(this.userIdKey);

  public static get host(): string {
    return this._host;
  }

  public static get userId(): string | null {
    return this._userId;
  }

  public static set userId(value: string) {
    localStorage.setItem(this.userIdKey, value);
    this._userId = value;
  }

  public static getRandomUUID(): string {
    return window.isSecureContext
      ? window.crypto.randomUUID()
      : window.crypto.getRandomValues(new Uint8Array(16)).join("-");
  }
  
}