export class Api {
  private static readonly controller: any;

  private static getBaseUrl(): string {
    const host: string = import.meta.env.DEV
      ? import.meta.env.VITE_API_HOST || window.location.origin
      : window.location.origin;
    const version: string = import.meta.env.VITE_API_VERSION || 'v1';
    return new URL(`${version}`, host).toString();
  }

  public static readonly System: any;

  public static readonly Assistant: any;

  public static readonly Memory: any;

  public static readonly Guest: any;

}
