import { LitElement, css, html } from "lit";
import { customElement, property } from "lit/decorators.js";

@customElement('chat-button')
export class ChatButton extends LitElement {

  @property({ type: Boolean, reflect: true }) disabled = false;
  @property({ type: Boolean, reflect: true }) loading = false;

  render() {
    return html`
      <div class="button"
        ?disabled=${this.disabled}>
        <slot></slot>
        <div class="button overlay">
          <chat-spinner></chat-spinner>
        </div>
      </div>
    `;
  }

  static styles = css`
    :host {
      display: inline-block;
      font-size: 16px;
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      user-select: none;
    }
    :host([loading]) .button.overlay {
      pointer-events: none;
      display: flex;
    }
    :host([disabled]) {
      pointer-events: none;
    }

    .button {
      position: relative;
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

    .overlay {
      display: none;
      position: absolute;
      z-index: 1;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
    }
  `;
}
