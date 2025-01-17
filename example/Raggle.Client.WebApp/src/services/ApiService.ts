import { 
  AssistantApi,
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
    const isDev = import.meta.env.DEV || false;
    let host: string;
    let version: string;
    if (isDev) {
      host = import.meta.env.VITE_API_HOST || window.location.origin;
      version = import.meta.env.VITE_API_VERSION || 'v1';
    } else {
      host = window.location.origin;
      version = 'v1';
    }
    console.log('API Host:', host);
    console.log('API Version:', version);
    console.log('API Origin:', window.location.origin);
    return new URL(`${version}`, host).toString();
  }

  public static readonly System = new SystemApi(Api.controller);

  public static readonly Assistant = new AssistantApi(Api.controller);

  public static readonly Memory = new MemoryApi(Api.controller);

}
