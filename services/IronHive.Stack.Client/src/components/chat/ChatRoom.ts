import { LitElement, css, html, nothing } from "lit";
import { customElement, property } from "lit/decorators.js";

import type { Message, MessageContent } from "../../models";

@customElement('chat-room')
export class ChatRoom extends LitElement {

  @property({ type: Boolean }) loading: boolean = false;
  @property({ type: Array }) messages: Message[] = [];

  render() {
    return html`
      <div class="container">
        <div class="message-area">
          ${this.messages.map(msg => html`
            <message-card
              .name=${msg.role}
              .avatar=${'/assets/images/user-avatar.png'}
              .timestamp=${msg.timestamp}>
                ${msg.content?.map(msg.role === 'user' 
                  ? this.renderUserContent 
                  : this.renderBotContent)}
            </message-card>
          `)}
        </div>

        <div class="input-area">
          <message-input
            placeholder="Type a message...">
          ></message-input>
        </div>
      </div>
    `;
  }

  public updateMessages(messages: Message[]) {
    this.messages = messages;
  }

  public updateMessage(message: Message) {
    this.messages = [...this.messages, message];
  }

  private renderUserContent = (content: MessageContent) => {
    if (content.type === 'text') {
      return html`<text-block .value="${content.value}"></text-block>`;
    } else {
      return nothing;
    }
  }

  private renderBotContent = (content: MessageContent) => {
    if (content.type === 'text') {
      return html`<marked-block .value="${content.value}"></marked-block>`;
    } else if (content.type === 'tool') {
      return html`<tool-block .value="${content}"></tool-block>`;
    } else {
      return nothing;
    }
  }

  static styles = css`
    .container {
      position: relative;
      display: flex;
      flex-direction: column;
      width: 100%;
      height: 100%;
      min-width: 320px;
      min-height: 480px;
      color: var(--hs-text-color);
      background-color: var(--hs-background-color);
    }

    .message-area {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 16px;
      padding: 16px;
      box-sizing: border-box;
      overflow-y: auto;
    }

    .input-area {
      width: 100%;
      height: auto;
    }
  `;
}
