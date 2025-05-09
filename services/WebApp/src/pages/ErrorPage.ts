import { LitElement, html, css } from 'lit-element';
import { customElement, property } from 'lit/decorators.js';

@customElement('error-page')
export class ErrorPage extends LitElement {

  @property({ type: Number }) status: number = 404;
  @property({ type: String }) message: string = 'Page Not Found';

  render() {
    return html`
      <div class="container">
        <h1>Error ${this.status}</h1>
        <p>${this.message}</p>
      </div>
    `;
  }

  static styles = css`
    .container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 100%;
      text-align: center;
      color: #bbbbbb;
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
