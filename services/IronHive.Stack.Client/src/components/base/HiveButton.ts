import { LitElement, css, html } from "lit";
import { customElement, property } from "lit/decorators.js";

@customElement('hive-button')
export class HiveButton extends LitElement {

  @property({ type: Boolean, reflect: true }) disabled = false;
  @property({ type: Boolean, reflect: true }) loading = false;

  render() {
    return html`
      <div class="button"
        ?disabled=${this.disabled}>
        ${this.loading
          ? html`<hive-spinner></hive-spinner>`
          : html`<slot></slot>`}
      </div>
    `;
  }

  static styles = css`
    :host {
      display: inline-block;
      font-size: 16px;
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    }

    .button {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 8px;
      
      border: none;
      border-radius: 4px;
      background-color: var(--hs-primary-color);
      box-sizing: border-box;
      cursor: pointer;
      transition: background-color 0.3s ease;  
    }

    .button:active {
      opacity: 0.8;
    }

    .button[disabled] {
      opacity: 0.5; 
      cursor: not-allowed;
    }
  `;
}
