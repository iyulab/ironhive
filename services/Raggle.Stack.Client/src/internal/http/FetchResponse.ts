import { HttpResponse } from "./HttpResponse";

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
