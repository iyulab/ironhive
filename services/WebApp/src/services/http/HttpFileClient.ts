export class HttpFileClient {

  constructor(config) {
  }

  public async uploadAsync(request: HttpRequest, cancellationToken?: CancellationToken): Promise<HttpResponse> {
    // return new Promise<HttpResponse>((resolve, reject) => {
    //   const url = this.buildUrl(request.path, request.params);
    //   const xhr = new XMLHttpRequest();

    //   xhr.open(request.method, url.toString(), true);
    //   if (request.body) {
    //     if (request.body instanceof FormData) {
    //       // No need to set content type for FormData
    //     } else if (typeof request.body === 'object') {
    //       xhr.setRequestHeader('Content-Type', 'application/json');
    //       request.body = JSON.stringify(request.body);
    //     }
    //   }

    //   if (request.headers) {
    //     for (const [key, value] of Object.entries(request.headers)) {
    //       xhr.setRequestHeader(key, value);
    //     }
    //   }

    //   if (request.credentials) {
    //     xhr.withCredentials = request.credentials !== "omit";
    //   }

    //   if (request.timeout) {
    //     xhr.timeout = request.timeout;
    //   }

    //   // CancellationToken 사용
    //   if (cancellationToken) {
    //     cancellationToken.registerCancellationCallback(() => xhr.abort());
    //   }

    //   xhr.onload = () => resolve(new XhrResponse(xhr));
    //   xhr.onerror = () => reject(new Error('Network error'));
    //   xhr.ontimeout = () => reject(new Error('Request timed out'));

    //   xhr.send(request.body);
    // });
  }

  public async downloadAsync(request: HttpRequest, cancellationToken?: CancellationToken): Promise<HttpResponse> {

  }
}