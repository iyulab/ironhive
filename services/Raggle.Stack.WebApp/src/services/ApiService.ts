import { 
  AssistantApi,
  GuestApi,
  MemoryApi,
  SystemApi
} from './api';
import { 
  HttpController 
} from './http';

export class Api {
  private static readonly controller: HttpController = new HttpController({
    baseUrl: Api.getBaseUrl(),
  });

  private static getBaseUrl(): string {
    const host: string = import.meta.env.DEV
      ? import.meta.env.VITE_API_HOST || window.location.origin
      : window.location.origin;
    const version: string = import.meta.env.VITE_API_VERSION || 'v1';
    return new URL(`${version}`, host).toString();
  }

  public static readonly System = new SystemApi(Api.controller);

  public static readonly Assistant = new AssistantApi(Api.controller);

  public static readonly Memory = new MemoryApi(Api.controller);

  public static readonly Guest = new GuestApi(Api.controller);

}
