import { LitElement, html, css } from 'lit-element';
import { customElement, property } from 'lit/decorators.js';

@customElement('error-page')
export class ErrorPage extends LitElement {
  @property({ type: Number }) 
  status: number = 404;

  render() {
    return html`
      <div class="error-container">
        <h1>Error ${this.status}</h1>
        <p>${this.getErrorMessage(this.status)}</p>
      </div>
    `;
  }

  private getErrorMessage(statusCode: number): string {
    switch (statusCode) {
      case 400:
        return 'Bad Request';
      case 401:
        return 'Unauthorized';
      case 403:
        return 'Forbidden';
      case 404:
        return 'Page Not Found';
      case 500:
        return 'Internal Server Error';
      default:
        return 'An unexpected error occurred';
    }
  }

  static styles = css`
    .error-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 100%;
      text-align: center;
      color: var(--sl-color-gray-700);
    }

    h1 {
      font-size: 48px;
      margin: 0;
    }

    p {
      font-size: 24px;
      margin: 16px 0 0;
    }
  `;
}