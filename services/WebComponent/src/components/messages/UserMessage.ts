import { LitElement, css, html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { format } from '../../internal';

@customElement('user-message')
export class UserMessage extends LitElement {

  @property({ type: String }) timestamp?: string;

  render() {
    return html`
      <div class="container">
        <div class="body">
          <slot></slot>
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

  static styles = css`
    .container {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .body {
      background-color: var(--hs-panel-background-color);
      padding: 8px;
      box-sizing: border-box;
      border-radius: 8px;
    }

    .footer {
      display: flex;
      flex-direction: row;
      gap: 8px;

      .flex {
        flex: 1;
      }

      .timestamp {
        font-size: 0.7em;
        opacity: 0.7;
      }
    }
  `;
}