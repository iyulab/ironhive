import type { HttpRequest, HttpMethod, HttpRequestConfig, HttpControllerConfig } from './HttpRequest';
import type { HttpResponse } from "./HttpResponse";
import { FetchResponse } from './FetchResponse';
import { XhrResponse } from './XhrResponse';

export class HttpController {
  private readonly baseUrl: string;
  private readonly headers?: HeadersInit;
  private readonly timeout?: number;
  private readonly credentials?: RequestCredentials;
  private readonly mode?: RequestMode;
  private readonly cache?: RequestCache;
  private readonly keepalive?: boolean;

  constructor(config: HttpControllerConfig) {
    this.baseUrl = config.baseUrl;
    this.headers = config.headers;
    this.timeout = config.timeout;
    this.credentials = config.credentials;
    this.mode = config.mode;
    this.cache = config.cache;
    this.keepalive = config.keepalive;
  }

  // HTTP Methods
  public get(path: string, config?: HttpRequestConfig): Promise<HttpResponse> {
    const request = this.buildRequest("GET", path, undefined, config);
    return this.send(request);
  }

  public post(path: string, body?: any, config?: HttpRequestConfig): Promise<HttpResponse> {
    const request = this.buildRequest("POST", path, body, config);
    return this.send(request);
  }

  public put(path: string, body?: any, config?: HttpRequestConfig): Promise<HttpResponse> {
    const request = this.buildRequest("PUT", path, body, config);
    return this.send(request);
  }

  public patch(path: string, body?: any, config?: HttpRequestConfig): Promise<HttpResponse> {
    const request = this.buildRequest("PATCH", path, body, config);
    return this.send(request);
  }

  public delete(path: string, config?: HttpRequestConfig): Promise<HttpResponse> {
    const request = this.buildRequest("DELETE", path, undefined, config);
    return this.send(request);
  }

  public async send(request: HttpRequest): Promise<HttpResponse> {
    if (request.body instanceof FormData) {
      return await this.sendXhr(request);
    } else {
      return await this.sendFetch(request);
    }
  }

  // Fetch Implementation
  private async sendFetch(request: HttpRequest): Promise<HttpResponse> {
    const url = this.buildUrl(request.path, request.params);
    const headers = new Headers(request.headers);

    let body: BodyInit | undefined = request.body;

    if (body && !(body instanceof FormData) && typeof body === 'object' && !(body instanceof Blob)) {
      if (body instanceof URLSearchParams) {
        headers.set("Content-Type", "application/x-www-form-urlencoded");
        body = body.toString();
      } else {
        headers.set("Content-Type", "application/json");
        body = JSON.stringify(body);
      }
    }

    const controller = new AbortController();
    const signal = controller.signal;

    const timeoutId = request.timeout
      ? setTimeout(() => controller.abort(), request.timeout)
      : null;

    try {
      const response = await fetch(url.toString(), {
        method: request.method,
        headers: headers,
        body: body,
        cache: request.cache,
        credentials: request.credentials,
        mode: request.mode,
        keepalive: request.keepalive,
        signal: signal,
      });

      // Handle onReceive if provided
      if (request.onReceive && response.body) {
        const reader = response.body.getReader();
        const decoder = new TextDecoder('utf-8');
        let buffer = '';

        const read = async () => {
          while (true) {
            const { done, value } = await reader.read();
            if (done) break;

            buffer += decoder.decode(value, { stream: true });
            let lines = buffer.split('\n');

            buffer = lines.pop() || '';

            for (const line of lines) {
              if (line.trim()) {
                try {
                  const data = JSON.parse(line);
                  request.onReceive!(data);
                } catch (e) {
                  console.error('Failed to parse JSON:', e);
                }
              }
            }
          }

          // Handle any remaining data in the buffer
          if (buffer.trim()) {
            try {
              const data = JSON.parse(buffer);
              request.onReceive!(data);
            } catch (e) {
              console.error('Failed to parse JSON:', e);
            }
          }
        };

        read();
      }

      return new FetchResponse(response, controller);
    } catch (error) {
      throw error;
    } finally {
      if (timeoutId) clearTimeout(timeoutId);
    }
  }

  // XHR Implementation
  private sendXhr(request: HttpRequest): Promise<HttpResponse> {
    return new Promise<HttpResponse>((resolve, reject) => {
      const url = this.buildUrl(request.path, request.params);
      const xhr = new XMLHttpRequest();

      xhr.open(request.method, url.toString(), true);
      if (request.body) {
        if (request.body instanceof FormData) {
          // No need to set content type for FormData
        } else if (typeof request.body === 'object') {
          xhr.setRequestHeader('Content-Type', 'application/json');
          request.body = JSON.stringify(request.body);
        }
      }

      // Set up request options
      if (request.headers) {
        for (const [key, value] of Object.entries(request.headers)) {
          xhr.setRequestHeader(key, value);
        }
      }
      if (request.credentials) {
        xhr.withCredentials = request.credentials !== "omit";
      }
      if (request.timeout) {
        xhr.timeout = request.timeout;
      }

      // Handle upload progress if onProgress is provided
      if (request.onProgress && xhr.upload) {
        xhr.upload.onprogress = (event: ProgressEvent) => {
          if (event.lengthComputable) {
            const progress = (event.loaded / event.total);
            request.onProgress!(progress);
          } else {
            request.onProgress!(-1);
          }
        };
      }

      // Handle response streaming if onReceive is provided
      if (request.onReceive && "responseType" in xhr) {
        xhr.responseType = 'text';
        let lastLength = 0;

        xhr.onprogress = () => {
          const chunk = xhr.responseText.slice(lastLength);
          lastLength = xhr.responseText.length;
          let lines = chunk.split('\n');

          lines.forEach(line => {
            if (line.trim()) {
              try {
                const data = JSON.parse(line);
                request.onReceive!(data);
              } catch (e) {
                console.error('Failed to parse JSON:', e);
              }
            }
          });
        };
      }

      // Set up event listeners
      xhr.onload = () => {
        resolve(new XhrResponse(xhr));
      };
      xhr.onerror = () => {
        reject(new Error('Network error'));
      };
      xhr.ontimeout = () => {
        reject(new Error('Request timed out'));
      };

      // Send the request
      try {
        xhr.send(request.body);
      } catch (error) {
        reject(error);
      }
    });
  }

  // Helper Methods
  private buildRequest(
    method: HttpMethod,
    path: string,
    body?: any,
    config?: HttpRequestConfig
): HttpRequest {
    return {
      method,
      path,
      body: body,
      headers: config?.headers || this.headers,
      params: config?.params,
      credentials: config?.credentials || this.credentials,
      cache: config?.cache || this.cache,
      mode: config?.mode || this.mode,
      keepalive: config?.keepalive || this.keepalive,
      timeout: config?.timeout || this.timeout,
      onReceive: config?.onReceive,
      onProgress: config?.onProgress,
    };
  }

  // Helper method to build a URL with query parameters
  private buildUrl(
    path: string, 
    params?: Record<string, any>
  ): URL {
    let fullPath = this.baseUrl.endsWith('/') && path.startsWith('/')
      ? this.baseUrl + path.slice(1)
      : this.baseUrl + path;

    const url = new URL(fullPath);

    if (params) {
      Object.keys(params).forEach(key => {
        const value = params[key];
        if (Array.isArray(value)) {
          value.forEach(val => url.searchParams.append(key, String(val)));
        } else if (value !== undefined && value !== null) {
          url.searchParams.append(key, String(value));
        }
      });
    }

    return url;
  }
}