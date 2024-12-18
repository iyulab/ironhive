import { LitElement, css, html, nothing } from "lit";
import { customElement, property } from "lit/decorators.js";

import type { Message } from "../../models";

@customElement('user-message')
export class UserMessage extends LitElement {

  @property({ type: Object })
  message?: Message;

  render() {
    return html`
      <div class="container">
        <img class="avatar"
          src="/assets/images/user-avatar.png" />
        <div class="name">
          ${this.message?.name || 'User'}
        </div>
        <div class="content">
          ${this.message?.content?.map(item => {
            if (item.type === 'text') {
              return html`
                <text-block
                  .content=${item.text || ''}
                ></text-block>
              `;
            } else {
              return nothing;
            }
          })}
        </div>
      </div>
    `;
  }

  static styles = css`
    :host {
      width: 100%;
      display: block;
    }

    .container {
      display: grid;
      grid-template-areas: 
        "avatar name"
        "avatar content";
      grid-template-columns: auto 1fr;
      grid-template-rows: auto 1fr;
      gap: 12px;
    }

    .avatar {
      grid-area: avatar;
      width: 40px;
      height: 40px;
      border-radius: 50%;
    }

    .name {
      grid-area: name;
      font-size: 12px;
      font-weight: 600;
      color: var(--sl-color-gray-600);
    }

    .content {
      grid-area: content;
      display: flex;
      flex-direction: column;
      gap: 4px;
      border-radius: 4px;
      padding: 8px;
      background-color: var(--sl-color-gray-100);
      border: 1px solid var(--sl-color-gray-200);
      box-sizing: border-box;
    }
  `;
}