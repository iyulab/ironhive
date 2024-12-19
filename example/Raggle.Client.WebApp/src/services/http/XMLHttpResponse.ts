// XMLHttpResponse.ts
export class XMLHttpResponse {
  public status: number | null = null;
  public url: string | null = null;
  public headers: Headers | null = null;
  public data: any = null;

  private xhr: XMLHttpRequest;
  private resolve!: (value: XMLHttpResponse) => void;
  private reject!: (reason?: any) => void;
  public promise: Promise<XMLHttpResponse>;

  constructor(xhr: XMLHttpRequest) {
    this.xhr = xhr;

    this.promise = new Promise<XMLHttpResponse>((resolve, reject) => {
      this.resolve = resolve;
      this.reject = reject;

      // Ready state change 이벤트 처리
      xhr.onreadystatechange = () => {
        if (xhr.readyState === XMLHttpRequest.DONE) {
          this.status = xhr.status;
          this.url = xhr.responseURL;
          this.headers = this.parseHeaders(xhr.getAllResponseHeaders());
          this.data = this.parseBody(xhr.response, xhr.getResponseHeader("Content-Type"));

          if (xhr.status >= 200 && xhr.status < 300) {
            resolve(this);
          } else {
            reject(new Error(`Request failed with status ${xhr.status}`));
          }
        }
      };

      // 에러 처리
      xhr.onerror = () => {
        reject(new Error("Network error occurred during the request."));
      };

      xhr.ontimeout = () => {
        reject(new Error("The request timed out."));
      };

      xhr.onabort = () => {
        reject(new Error("The request was aborted."));
      };
    });
  }

  public onProgress(callback: (evnet: ProgressEvent) => void): void {
    this.xhr.upload.onprogress = (event: ProgressEvent) => {
      callback(event);
    };
  }

  public cancel(): void {
    this.xhr.abort();
  }

  /**
   * 응답 헤더 문자열을 Headers 객체로 변환
   * @param headerStr 응답 헤더 문자열
   * @returns Headers 객체
   */
  private parseHeaders(headerStr: string): Headers {
    const headers = new Headers();
    const headerPairs = headerStr.split("\u000d\u000a");
    for (const header of headerPairs) {
      const index = header.indexOf("\u003a\u0020");
      if (index > 0) {
        const key = header.substring(0, index);
        const val = header.substring(index + 2);
        headers.append(key, val);
      }
    }
    return headers;
  }

  /**
   * 응답 본문을 적절한 형식으로 파싱
   * @param response 응답 본문
   * @param contentType 응답의 Content-Type 헤더 값
   * @returns 파싱된 데이터
   */
  private parseBody(response: any, contentType: string | null): any {
    if (!contentType) return response;
    if (contentType.includes("application/json")) {
      try {
        return JSON.parse(response);
      } catch {
        return response;
      }
    }
    // 필요에 따라 다른 Content-Type 처리 추가 가능
    return response;
  }
}
