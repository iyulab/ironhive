import { HttpRequest, HttpMethod, HttpRequestConfig, HttpControllerConfig } from './HttpRequest';
import { HttpResponse } from "./HttpResponse";
import { XMLHttpResponse } from './XMLHttpResponse';

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
    this.headers = config?.headers;
    this.timeout = config?.timeout;
    this.credentials = config?.credentials;
    this.mode = config?.mode;
    this.cache = config?.cache;
    this.keepalive = config?.keepalive;
  }

  public async get(
    path: string, 
    config?: HttpRequestConfig
  ): Promise<HttpResponse> {
    const request = this.buildRequest("GET", path, undefined, config);
    return await this.fetch(request);
  }

  public async post(
    path: string, 
    body?: any, 
    config?: HttpRequestConfig
  ): Promise<HttpResponse> {
    const request = this.buildRequest("POST", path, body, config);
    return await this.fetch(request);
  }

  public async put(
    path: string, 
    body?: any, 
    config?: HttpRequestConfig
  ): Promise<HttpResponse> {
    const request = this.buildRequest("PUT", path, body, config);
    return await this.fetch(request);
  }

  public async patch(
    path: string, 
    body?: any, 
    config?: HttpRequestConfig
  ): Promise<HttpResponse> {
    const request = this.buildRequest("PATCH", path, body, config);
    return await this.fetch(request);
  }

  public async delete(
    path: string, 
    config?: HttpRequestConfig
  ): Promise<HttpResponse> {
    const request = this.buildRequest("DELETE", path, undefined, config);
    return await this.fetch(request);
  }

  public upload(
    path: string,
    form: FormData,
    progress?: (event: ProgressEvent) => void,
    complete?: (data?: any) => void,
    error?: (event: Event) => void,
    config?: HttpRequestConfig
  ): XMLHttpRequest {
    const url = this.buildUrl(path, config?.params);
    const xhr = new XMLHttpRequest();

    xhr.open("POST", url.toString(), true);

    const headers = config?.headers || this.headers;
    if (headers) {
      for (const [key, value] of Object.entries(headers)) {
        // FormData의 Content-Type은 브라우저가 자동 설정하므로 제외
        if (key.toLowerCase() === "content-type") continue;
        xhr.setRequestHeader(key, value);
      }
    }
    
    const credentials = config?.credentials || this.credentials;
    if (credentials) {
      xhr.withCredentials = credentials !== "omit";
    }
    
    const timeout = config?.timeout || this.timeout;
    if (timeout) {
      xhr.timeout = timeout;
    }

    if (progress && xhr.upload) {
      xhr.upload.onprogress = progress;
    }

    if (error) {
      xhr.onerror = error;
    }

    xhr.onreadystatechange = () => {
      if (xhr.readyState !== XMLHttpRequest.DONE) return;
      if (xhr.status >= 300) return;
      if (!complete) return;
      
      complete(JSON.parse(xhr.response));
    };

    xhr.send(form);
    return xhr;
  }

  public async fetch(request: HttpRequest): Promise<HttpResponse> {
    const url = this.buildUrl(request.path, request.params);
    const controller = new AbortController();
    const timer = this.timeout
      ? setTimeout(() => controller.abort(), this.timeout)
      : null;

    try {
      const response = await fetch(url, {
        method: request.method,
        headers: request.headers,
        body: request.body,
        cache: request.cache,
        credentials: request.credentials,
        keepalive: request.keepalive,
        mode: request.mode,
        signal: controller.signal,

        integrity: "",
        priority: "auto",
        redirect: "follow",
      });

      return new HttpResponse(response, controller);
    } catch (error) {
      throw error;
    } finally {
      if (timer) {
        clearTimeout(timer);
      }
    }
  }

  private buildRequest(
    method: HttpMethod, 
    path: string,
    body?: any,
    config?: HttpRequestConfig
  ): HttpRequest {
    
    const headers = config?.headers 
      ? new Headers(config.headers) 
      : new Headers(this.headers);

    if (body instanceof FormData) {
      headers.delete("Content-Type");
    } else if (body instanceof Blob) {
      headers.set("Content-Type", body.type || "application/octet-stream");
    } else if (body instanceof ArrayBuffer || ArrayBuffer.isView(body)) {
      headers.set("Content-Type", "application/octet-stream");
    } else if (body instanceof URLSearchParams) {
      headers.set("Content-Type", "application/x-www-form-urlencoded");
      body = body.toString();
    } else if (typeof body === "string") {
      headers.set("Content-Type", "text/plain");
    } else if (typeof body === "object") {
      const contentType = headers.get("Content-Type");
      if (contentType === "application/xml") {
        body = new XMLSerializer().serializeToString(body);
      } else {
        body = JSON.stringify(body);
        headers.set("Content-Type", "application/json");
      }
    }

    return {
      path: path,
      method: method,
      body: body,
      headers: headers,
      params: config?.params,
      credentials: config?.credentials || this.credentials,
      cache: config?.cache || this.cache,
      mode: config?.mode || this.mode,
      keepalive: config?.keepalive || this.keepalive,
      timeout: config?.timeout || this.timeout,
    }
  }

  private buildUrl(path: string, params?: any): URL {
    if (this.baseUrl.endsWith('/') && path.startsWith('/')) {
      path = path.slice(1);
    }

    if (params) {
      const query = new URLSearchParams(params).toString();
      return new URL(`${this.baseUrl}${path}?${query}`);
    } else {
      return new URL(`${this.baseUrl}${path}`);
    }
  }
  
}
