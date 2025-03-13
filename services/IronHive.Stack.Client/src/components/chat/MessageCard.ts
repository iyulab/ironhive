import { LitElement, css, html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { format } from '../../internal';

@customElement('message-card')
export class MessageCard extends LitElement {

  @property({ type: String }) name?: string;
  @property({ type: String }) avatar?: string;
  @property({ type: String }) timestamp?: string;

  render() {
    return html`
      <div class="container">
        <div class="side">
          ${this.avatar ? html`
            <img class="avatar" src="${this.avatar}" alt="Avatar"/>
          ` : html`
            <div class="avatar holder" aria-hidden="true"></div>
          `}
        </div>
        <div class="content">
          <div class="header">
            ${this.name}
          </div>
          <main class="body">
            <slot></slot>
          </main>
          <div class="footer">
            <div class="timestamp">
              ${format(this.timestamp)}
            </div>
          </div>
        </div>
      </div>
    `;
  }

  static styles = css`
    .container {
      display: flex;
      flex-direction: row;
      gap: 8px;
    }

    .side {
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .avatar {
      width: 35px;
      height: 35px;
      border-radius: 50%;
    }

    .avatar.holder {
      background-color: var(--hs-secondary-color);
    }

    .content {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .header {
      font-size: 0.9em;
      font-weight: 600;
      font-family: 'Roboto', sans-serif;
      line-height: 1.2;
    }

    .body {
      background-color: var(--hs-panel-background-color);
      padding: 8px;
      box-sizing: border-box;
      border-radius: 4px;
      font-size: 0.9em;
      line-height: 1.5;
      font-weight: 400;
    }

    .footer {
      display: flex;
      justify-content: flex-end;
    }

    .timestamp {
      font-size: 0.7em;
      opacity: 0.7;
    }
  `;
}
