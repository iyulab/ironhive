import { DataSource } from "@/models/DataSource";

export type Theme = "light" | "dark";

export class Storage {
  private static readonly userIdKey = "raggle-userId";
  private static readonly themeKey = "raggle-theme";
  private static readonly tempSourceKey = "raggle-tempsource";
  private static _host: string = import.meta.env.VITE_HOST || location.origin;
  private static _userId: string | null = localStorage.getItem(this.userIdKey);
  private static _theme: Theme = localStorage.getItem(this.themeKey) as Theme;

  public static get host(): string {
    return this._host;
  }

  public static get userId(): string {
    if (!this._userId) {
      this._userId = this.getRandomUUID();
      localStorage.setItem(this.userIdKey, this._userId);
    }
    return this._userId;
  }

  public static get theme(): Theme {
    if (!this._theme) {
      this._theme = 'light';
      localStorage.setItem(this.themeKey, this._theme);
    }
    return this._theme;
  }

  public static getTempSource(id: string): DataSource | undefined {
    const source = localStorage.getItem(this.tempSourceKey);
    const tempSource = JSON.parse(source || "{}");
    if (tempSource?.id === id) {
      return tempSource;
    }
    return undefined;
  }

  public static setTempSource(source: DataSource): void {
    const sourceStr = JSON.stringify(source);
    localStorage.setItem(this.tempSourceKey, sourceStr);
  }

  public static clearTempSource(): void {
    localStorage.removeItem(this.tempSourceKey);
  }

  public static getRandomUUID(): string {
    return window.isSecureContext
      ? window.crypto.randomUUID()
      : window.crypto.getRandomValues(new Uint8Array(16)).join("-");
  }
}