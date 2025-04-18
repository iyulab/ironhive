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
export interface HttpClientConfig extends HttpOptions {
  /**
   * Base URL for the HTTP requests.
   */
  baseUrl: string;
}

/**
 * Represents an HTTP request.
 */
export interface HttpRequest extends HttpOptions {
  /**
   * HTTP method to use for the request.
   */
  method: HttpMethod;

  /**
   * Path of the request relative to the base URL.
   */
  path: string;

  /**
   * Parameters to include in the request URL.
   */
  query?: Record<string, any>;

  /**
   * Body of the request.
   */
  body?: any;
}
