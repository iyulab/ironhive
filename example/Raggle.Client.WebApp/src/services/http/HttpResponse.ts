export interface HttpResponse {
  ok: boolean;
  status: number;
  headers: Headers;
  url: string;
  redirected: boolean;

  text(): Promise<string>;
  json<T>(): Promise<T>;
  bytes(): Promise<Uint8Array>;
  arrayBuffer(): Promise<ArrayBuffer>;
  formData(): Promise<FormData>;
  blob(): Promise<Blob>;
  
  cancel(): void;
}

export class XhrResponse implements HttpResponse {
  private readonly xhr: XMLHttpRequest;

  constructor(xhr: XMLHttpRequest) {
    this.xhr = xhr;
  }

  public get ok(): boolean {
    return this.status >= 200 && this.status < 300;
  }

  public get status(): number {
    return this.xhr.status;
  }

  public get headers(): Headers {
    const headers = new Headers();
    const headerLines = this.xhr.getAllResponseHeaders().trim().split(/[\r\n]+/);
    headerLines.forEach((line) => {
      const parts = line.split(': ');
      const header = parts.shift();
      const value = parts.join(': ');
      if (header && value) {
        headers.append(header, value);
      }
    });
    return headers;
  }

  public get url(): string {
    return this.xhr.responseURL;
  }

  public get redirected(): boolean {
    // XHR does not provide redirect info directly
    return false;
  }

  public text(): Promise<string> {
    return Promise.resolve(this.xhr.responseText);
  }

  public bytes(): Promise<Uint8Array> {
    return Promise.resolve(new Uint8Array(this.xhr.response as ArrayBuffer));
  }

  public blob(): Promise<Blob> {
    return Promise.resolve(this.xhr.response as Blob);
  }

  public arrayBuffer(): Promise<ArrayBuffer> {
    return Promise.resolve(this.xhr.response as ArrayBuffer);
  }

  public formData(): Promise<FormData> {
    return Promise.resolve(this.xhr.response as FormData);
  }

  public json<T>(): Promise<T> {
    return Promise.resolve(JSON.parse(this.xhr.responseText));
  }

  public cancel(): void {
    this.xhr.abort();
  }
}

export class FetchResponse implements HttpResponse {
  private readonly response: Response;
  private readonly controller: AbortController;

  constructor(response: Response, controller: AbortController) {
    this.response = response;
    this.controller = controller;
  }

  public get ok(): boolean {
    return this.response.ok;
  }

  public get status(): number {
    return this.response.status;
  }

  public get headers(): Headers {
    return this.response.headers;
  }

  public get url(): string {
    return this.response.url;
  }

  public get redirected(): boolean {
    return this.response.redirected;
  }

  public text(): Promise<string> {
    return this.response.text();
  }

  public bytes(): Promise<Uint8Array> {
    return this.response.bytes();
  }

  public blob(): Promise<Blob> {
    return this.response.blob();
  }

  public arrayBuffer(): Promise<ArrayBuffer> {
    return this.response.arrayBuffer();
  }

  public formData(): Promise<FormData> {
    return this.response.formData();
  }

  public json<T>(): Promise<T> {
    return this.response.json();
  }

  public cancel(): void {
    this.controller.abort();
  }
}
