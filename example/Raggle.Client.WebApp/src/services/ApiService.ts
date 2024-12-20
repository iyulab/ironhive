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
    const version = import.meta.env.VITE_API_VERSION || '';
    const host = import.meta.env.VITE_API_HOST || window.location.origin;
    return new URL(`${version}`, host).toString();
  }

  public static readonly System = new SystemApi(Api.controller);

  public static readonly Assistant = new AssistantApi(Api.controller);

  public static readonly Memory = new MemoryApi(Api.controller);

}
