export class HttpResponse {
  private readonly response: Response;
  private readonly controller: AbortController;
  private reader?: ReadableStreamDefaultReader<Uint8Array>;

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

  constructor(response: Response, controller: AbortController) {
    this.response = response;
    this.controller = controller;
  }

  public text = (): Promise<string> => this.response.text();

  public bytes = async (): Promise<Uint8Array> => this.response.bytes();

  public blob = async (): Promise<Blob> => this.response.blob();

  public arrayBuffer = async (): Promise<ArrayBuffer> => this.response.arrayBuffer();

  public formData = async (): Promise<FormData> => this.response.formData();

  public async json<T>(): Promise<T> {
    return this.response.json();
  }

  public async onReceive<T>(callback: (data: T) => void): Promise<void> {
    this.reader = this.response.body?.getReader();
    if (!this.reader) {
      throw new Error('Response body is not readable');
    }

    const decoder = new TextDecoder('utf-8');
    let buffer = '';

    try {
      while (true) {
        const { done, value } = await this.reader.read();
        if (done) break;
  
        // Decode the incoming value and append it to the buffer
        buffer += decoder.decode(value, { stream: true });
        const lines = buffer.split('\n');

        // Keep the incomplete line in the buffer
        buffer = lines.pop() || ''; 
  
        for (const line of lines) {
          this.parseAndCallback(line, callback);
        }
      }
  
      // Handle any remaining data in the buffer
      this.parseAndCallback(buffer, callback);
      
    } finally {
      // Ensure the reader is closed properly
      this.reader.cancel();
      this.reader = undefined;
    }
  }

  public cancel(): void {
    this.controller.abort();
    if (this.reader) {
      this.reader.cancel();
    }
  }

  private parseAndCallback<T>(line: string, callback: (data: T) => void): void {
    if (!line.trim()) return;

    try {
      const parsed = JSON.parse(line);
      callback(parsed);
    } catch (error) {
      console.error('Failed to parse JSON:', error);
    }
  }
}
