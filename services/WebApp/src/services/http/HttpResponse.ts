export class HttpResponse {
  private readonly _response: Response;

  constructor(response: Response) {
    this._response = response;
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
    return this._response.arrayBuffer().then(buffer => new Uint8Array(buffer));
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

  /**
   * 스트리밍 응답을 처리하는 제너레이터 함수입니다.
   */
  public async *stream(delimiter = "\n"): AsyncGenerator<string> {
    const reader = this._response.body?.getReader();
    const decoder = new TextDecoder("utf-8");
  
    if (!reader) {
      throw new Error("No response body available.");
    }
  
    try {
      let done = false;
      let buffer = ""; // 이 변수는 디코딩된 텍스트를 누적할 곳입니다.
  
      while (!done) {
        const { value, done: isDone } = await reader.read();
        done = isDone;
  
        if (value) {
          buffer += decoder.decode(value, { stream: true });

          const lines = buffer.split(delimiter); // 구분자로 텍스트를 나눕니다.
          if (lines.length > 1) {
            for (let i = 0; i < lines.length - 1; i++) {
              yield lines[i]; // 마지막 줄을 제외한 나머지 줄을 반환합니다.
            }
            buffer = lines[lines.length - 1]; // 마지막 줄은 누적된 텍스트로 남겨둡니다.
          }
        }
      }
  
      // 남아있는 누적된 텍스트를 반환
      if (buffer) {
        yield buffer;
      }
    } finally {
      reader.releaseLock();
      this._response.body?.cancel(); // 스트림을 안전하게 취소
    }
  }

}
