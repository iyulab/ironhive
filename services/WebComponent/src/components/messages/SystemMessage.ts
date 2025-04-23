import { LitElement, css, html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { format } from '../../internal';

@customElement('system-message')
export class SystemMessage extends LitElement {
  
  @property({ type: String }) type?: 'error' | 'warning' | 'info' = 'info';
  @property({ type: String }) timestamp?: string;
  @property({ type: String }) message?: string;

  render() {
    return html`
      <div class="container ${this.type}">
        <div class="body">
          <div class="icon">
            ${this.renderIcon()}
          </div>
          <div class="content">
            <slot></slot>
            ${this.message ? html`<p>${this.message}</p>` : ''}
          </div>
        </div>
        <div class="footer">
          <div class="flex"></div>
          <div class="timestamp">
            ${format(this.timestamp)}
          </div>
        </div>
      </div>
    `;
  }

  renderIcon() {
    switch (this.type) {
      case 'error':
        return html`<chat-icon name="alert-circle"></chat-icon>`;
      case 'warning':
        return html`<chat-icon name="alert-triangle"></chat-icon>`;
      default:
        return html`<chat-icon name="info"></chat-icon>`;
    }
  }

  static styles = css`
    .container {
      display: flex;
      flex-direction: column;
      gap: 4px;
      max-width: 80%;
      margin: 8px auto;
    }

    .body {
      display: flex;
      align-items: center;
      padding: 12px;
      box-sizing: border-box;
      border-radius: 8px;
      background-color: var(--hs-neutral-background-color, #f0f0f0);
      border: 1px solid var(--hs-neutral-border-color, #ddd);
    }

    .icon {
      margin-right: 12px;
      display: flex;
      align-items: center;
    }

    .content {
      flex: 1;
    }

    .footer {
      display: flex;
      flex-direction: row;
      gap: 8px;
    }

    .flex {
      flex: 1;
    }

    .timestamp {
      font-size: 0.7em;
      opacity: 0.7;
      text-align: center;
    }

    .error .icon {
      color: var(--hs-error-color, #ff3b30);
    }

    .warning .icon {
      color: var(--hs-warning-color, #ff9500);
    }

    .info .icon {
      color: var(--hs-info-color, #007aff);
    }

    .error .body {
      background-color: var(--hs-error-background-color, rgba(255, 59, 48, 0.1));
      border-color: var(--hs-error-border-color, rgba(255, 59, 48, 0.3));
    }

    .warning .body {
      background-color: var(--hs-warning-background-color, rgba(255, 149, 0, 0.1));
      border-color: var(--hs-warning-border-color, rgba(255, 149, 0, 0.3));
    }

    .info .body {
      background-color: var(--hs-info-background-color, rgba(0, 122, 255, 0.1));
      border-color: var(--hs-info-border-color, rgba(0, 122, 255, 0.3));
    }
  `;
}