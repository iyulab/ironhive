import { HttpResponse } from "./HttpResponse";

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
