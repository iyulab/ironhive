import { AbortToken } from "./AbortToken";
import { HttpClientConfig, HttpRequest } from "./HttpModels";
import { HttpResponse } from "./HttpResponse";

export class HttpClient {
  private readonly baseUrl: string;
  private readonly headers?: HeadersInit;
  private readonly timeout?: number;
  private readonly credentials?: RequestCredentials;
  private readonly mode?: RequestMode;
  private readonly cache?: RequestCache;
  private readonly keepalive?: boolean;

  constructor(config: HttpClientConfig) {
    this.baseUrl = config.baseUrl;
    this.headers = config.headers;
    this.timeout = config.timeout;
    this.credentials = config.credentials;
    this.mode = config.mode;
    this.cache = config.cache;
    this.keepalive = config.keepalive;
  }

  public async send(request: HttpRequest, abortToken?: AbortToken): Promise<HttpResponse> {
    // 1. URL 생성
    const url = new URL(this.baseUrl.endsWith('/') && request.path.startsWith('/')
    ? this.baseUrl + request.path.slice(1)
    : this.baseUrl + request.path);
    if (request.query) {
      Object.entries(request.query).forEach(([key, value]) => {
        if (value !== null || value !== undefined) {
          (Array.isArray(value) ? value : [value]).forEach(val =>
            url.searchParams.append(key, val)
          );
        }
      });
    }

    // 2. Headers 설정
    const headers = new Headers(request.headers);
    if (this.headers) {
      Object.entries(this.headers).forEach(([key, value]) => {
        headers.append(key, value); // append to avoid overwriting
      });
    }

    // 3. Body 설정
    let body: BodyInit | undefined = request.body;
    if (typeof body === 'string') {
      headers.set("Content-Type", "text/plain;charset=UTF-8");
    } else if (typeof body === 'object') {
      if (body instanceof Blob) {
        headers.set("Content-Type", body.type || "application/octet-stream");
      } else if (body instanceof ArrayBuffer) {
        headers.set("Content-Type", "application/octet-stream");
      } else {
        headers.set("Content-Type", "application/json;charset=UTF-8");
        body = JSON.stringify(body);
      }
    }

    // 4. Abort 설정
    const token = abortToken || new AbortToken();
    const timeout = request.timeout ?? this.timeout;
    const timer = timeout
      ? setTimeout(() => token.cancel(), timeout)
      : null;

    try {
      // 5. Fetch 요청
      const res = await fetch(url.toString(), {
        method: request.method,
        headers: headers,
        body: body,
        cache: this.cache ?? request.cache,
        credentials: this.credentials ?? request.credentials,
        mode: this.mode ?? request.mode,
        keepalive: this.keepalive ?? request.keepalive,
        signal: token.signal,
      });

      // 6. 응답 처리
      return new HttpResponse(res);
    } finally {
      // 7. 타이머를 정리합니다.
      if (timer) {
        clearTimeout(timer);
      }
    }
  }

}
