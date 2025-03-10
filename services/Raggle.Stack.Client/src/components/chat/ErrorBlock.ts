import { LitElement, css, html } from "lit";
import { customElement, property } from "lit/decorators.js";

@customElement('error-block')
export class ErrorBlock extends LitElement {

  @property({ type: String }) type: string = '';
  @property({ type: String }) error: string = '';

  render() {
    return html`
      <div class="error">
        <strong>${this.type}</strong>
        <p>${this.error}</p>
      </div>
    `;
  }

  static styles = css`
    .error {
      color: red;
      background-color: #ffe6e6;
      border: 1px solid red;
      padding: 16px;
      border-radius: 4px;
      margin: 16px 0;
    }
  `;
}