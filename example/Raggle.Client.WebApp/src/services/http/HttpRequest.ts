export type HttpMethod =
  | 'GET'
  | 'HEAD'
  | 'OPTIONS'
  | 'TRACE'
  | 'PUT'
  | 'POST'
  | 'PATCH'
  | 'DELETE'
  | 'CONNECT';

export interface HttpOptions {
  headers?: HeadersInit;
  timeout?: number;
  credentials?: RequestCredentials;
  mode?: RequestMode;
  cache?: RequestCache;
  keepalive?: boolean;
}

export type HttpControllerConfig = HttpOptions & {
  baseUrl: string;
}

export type HttpRequestConfig = HttpOptions & {
  params?: any;
}

export type HttpRequest = HttpRequestConfig & {
  method: HttpMethod;
  path: string;
  body?: BodyInit;
}
