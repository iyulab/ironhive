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

export interface HttpControllerConfig extends HttpOptions {
  baseUrl: string;
}

export interface HttpRequestConfig extends HttpOptions {
  params?: Record<string, any>;
  onReceive?: (data: any) => void;
  onProgress?: (progress: number) => void;
}

export interface HttpRequest extends HttpRequestConfig {
  method: HttpMethod;
  path: string;
  body?: any;
}
