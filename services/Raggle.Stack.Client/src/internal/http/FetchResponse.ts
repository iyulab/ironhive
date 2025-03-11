import { HttpResponse } from "./HttpResponse";

export class FetchResponse implements HttpResponse {
  private readonly _response: Response;
  private readonly _abort: AbortController;

  constructor(response: Response, abort: AbortController) {
    this._response = response;
    this._abort = abort;
  }

  public get ok(): boolean {
    return this._response.ok;
  }

  public get status(): number {
    return this._response.status;
  }

  public get headers(): Headers {
    return this._response.headers;
  }

  public get url(): string {
    return this._response.url;
  }

  public get redirected(): boolean {
    return this._response.redirected;
  }

  public text(): Promise<string> {
    return this._response.text();
  }

  public bytes(): Promise<Uint8Array> {
    return this._response.bytes();
  }

  public blob(): Promise<Blob> {
    return this._response.blob();
  }

  public arrayBuffer(): Promise<ArrayBuffer> {
    return this._response.arrayBuffer();
  }

  public formData(): Promise<FormData> {
    return this._response.formData();
  }

  public json<T>(): Promise<T> {
    return this._response.json();
  }

  public cancel(): void {
    this._abort.abort();
  }
}
