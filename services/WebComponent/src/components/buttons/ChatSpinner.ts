import { LitElement, css, html } from "lit";
import { customElement } from "lit/decorators.js";

@customElement('chat-spinner')
export class ChatSpinner extends LitElement {
  render() {
    return html`
      <div class="spinner"></div>
    `;
  }

  static styles = css`
    :host {
      display: inline-block;
    }

    .spinner {
      border: 4px solid var(--hs-spinner-bg-color, #f3f3f3);
      border-top: 4px solid var(--hs-spinner-color, #3498db);
      border-radius: 50%;
      width: 24px;
      height: 24px;
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
  `;
}
