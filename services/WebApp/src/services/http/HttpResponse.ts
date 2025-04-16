/**
 * Represents an HTTP response.
 */
export interface HttpResponse {
  /**
   * Indicates whether the response was successful (status in the range 200-299).
   */
  ok: boolean;

  /**
   * The status code of the response.
   */
  status: number;

  /**
   * The headers of the response.
   */
  headers: Headers;

  /**
   * The URL of the response.
   */
  url: string;

  /**
   * Indicates whether the response was redirected.
   */
  redirected: boolean;

  /**
   * Returns a promise that resolves with the response body as a string.
   */
  text(): Promise<string>;

  /**
   * Returns a promise that resolves with the response body parsed as JSON.
   * @template T The type of the parsed JSON.
   */
  json<T>(): Promise<T>;

  /**
   * Returns a promise that resolves with the response body as a Uint8Array.
   */
  bytes(): Promise<Uint8Array>;

  /**
   * Returns a promise that resolves with the response body as an ArrayBuffer.
   */
  arrayBuffer(): Promise<ArrayBuffer>;

  /**
   * Returns a promise that resolves with the response body as FormData.
   */
  formData(): Promise<FormData>;

  /**
   * Returns a promise that resolves with the response body as a Blob.
   */
  blob(): Promise<Blob>;

  /**
   * Cancels the response.
   */
  cancel(): void;
}
