export class HttpController {
  private baseUrl: string;
  private headers: Record<string, string>;
  private readonly timeout: number;
  private readonly withCredentials: boolean;
  private readonly responseType: "json" | "text" | "blob";

  constructor(
    baseUrl: string,
    options: {
      headers?: Record<string, string>;
      timeout?: number;
      withCredentials?: boolean;
      responseType?: "json" | "text" | "blob";
    } = {}
  ) {
    this.baseUrl = baseUrl;
    this.headers = options.headers || {};
    this.timeout = options.timeout || 0;
    this.withCredentials = options.withCredentials || false;
    this.responseType = options.responseType || "json";
  }

  public async get<T>(endpoint: string): Promise<{ data: T; status: number }> {
    const init: RequestInit = {
      method: "GET",
      headers: this.headers,
      credentials: this.withCredentials ? "include" : "same-origin",
    };

    try {
      const response = await this.fetchWithTimeout(`${this.baseUrl}${endpoint}`, init);

      if (!response.ok) {
        throw new Error(`HTTP error! Status: ${response.status}`);
      }

      const data = await this.parseResponse(response);
      return { data, status: response.status };
    } catch (error) {
      if ((error as any).name === "AbortError") {
        console.error("Request aborted");
      } else {
        console.error("Fetch error:", error);
      }
      throw error;
    }
  }

  public async post<T>(
    endpoint: string,
    body: any
  ): Promise<{ data: T; status: number }> {
    const init: RequestInit = {
      method: "POST",
      headers: { ...this.headers, "Content-Type": "application/json" },
      credentials: this.withCredentials ? "include" : "same-origin",
      body: JSON.stringify(body),
    };

    try {
      const response = await this.fetchWithTimeout(`${this.baseUrl}${endpoint}`, init);

      if (!response.ok) {
        throw new Error(`HTTP error! Status: ${response.status}`);
      }

      const data = await this.parseResponse(response);
      return { data, status: response.status };
    } catch (error) {
      if ((error as any).name === "AbortError") {
        console.error("Request aborted");
      } else {
        console.error("Fetch error:", error);
      }
      throw error;
    }
  }

  private createAbortController(): AbortController {
    return new AbortController();
  }

  private async fetchWithTimeout(
    input: RequestInfo,
    init?: RequestInit
  ): Promise<Response> {
    const controller = this.createAbortController();
    const signal = controller.signal;

    const timeoutId = this.timeout
      ? setTimeout(() => controller.abort(), this.timeout)
      : null;

    try {
      const response = await fetch(input, { ...init, signal });
      return response;
    } finally {
      if (timeoutId) clearTimeout(timeoutId);
    }
  }

  private async parseResponse(response: Response): Promise<any> {
    if (this.responseType === "json") {
      return await response.json();
    } else if (this.responseType === "text") {
      return await response.text();
    } else if (this.responseType === "blob") {
      return await response.blob();
    } else {
      throw new Error("Unsupported response type");
    }
  }
}
