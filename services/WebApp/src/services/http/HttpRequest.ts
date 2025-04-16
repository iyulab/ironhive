/**
 * Represents the HTTP methods that can be used in an HTTP request.
 */
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

/**
 * Options for configuring an HTTP request.
 */
export interface HttpOptions {
  /**
   * Headers to include in the request.
   */
  headers?: HeadersInit;

  /**
   * Timeout duration for the request in milliseconds.
   */
  timeout?: number;

  /**
   * Credentials to include in the request.
   */
  credentials?: RequestCredentials;

  /**
   * Mode of the request (e.g., 'cors', 'no-cors', 'same-origin').
   */
  mode?: RequestMode;

  /**
   * Cache mode of the request (e.g., 'default', 'no-store').
   */
  cache?: RequestCache;

  /**
   * Whether to keep the connection alive.
   */
  keepalive?: boolean;
}

/**
 * Configuration options for an HTTP controller.
 */
export interface HttpControllerConfig extends HttpOptions {
  /**
   * Base URL for the HTTP requests.
   */
  baseUrl: string;
}

/**
 * Configuration options for an HTTP request.
 */
export interface HttpRequestConfig extends HttpOptions {
  /**
   * Parameters to include in the request URL.
   */
  params?: Record<string, any>;

  /**
   * Callback function to handle the received data.
   */
  onReceive?: (data: any) => void;

  /**
   * Callback function to handle the progress of the request.
   */
  onProgress?: (progress: number) => void;
}

/**
 * Represents an HTTP request.
 */
export interface HttpRequest extends HttpRequestConfig {
  /**
   * HTTP method to use for the request.
   */
  method: HttpMethod;

  /**
   * Path of the request relative to the base URL.
   */
  path: string;

  /**
   * Body of the request.
   */
  body?: any;
}
