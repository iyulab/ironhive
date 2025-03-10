import { LitElement, css, html, nothing } from "lit";
import { customElement, property } from "lit/decorators.js";

import type { Message, MessageContent } from "../../models";

@customElement('chat-room')
export class ChatRoom extends LitElement {

  @property({ type: Boolean }) loading: boolean = false;
  @property({ type: Array }) messages: Message[] = [
    {
      "name": "bot",
      "role": "assistant",
      "timestamp": "2021-09-25T10:00:00Z",
      "content": [
        {
          "type": "text",
          "text": "Hello! How can I help you today?"
        }
      ]
    },
    {
      "name": "user",
      "role": "user",
      "timestamp": "2021-09-25T10:01:00Z",
      "content": [
        {
          "type": "text",
          "text": "I have a question about my order."
        }
      ]
    },
    {
      "name": "bot",
      "role": "assistant",
      "timestamp": "2021-09-25T10:02:00Z",
      "content": [
        {
          "type": "text",
          "text": "Sure! What is your order number?"
        }
      ],
    }
  ];

  render() {
    return html`
      <div class="container">
        <div class="message-area">
          ${this.messages.map(msg => html`
            <message-card
              .name=${msg.role}
              .avatar=${''}
              .timestamp=${msg.timestamp}>
                ${msg.content?.map(msg.role === 'user' 
                  ? this.renderUserContent 
                  : this.renderBotContent)}
              })}
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
      return html`<text-block .value="${content.text}"></text-block>`;
    } else {
      return nothing;
    }
  }

  private renderBotContent = (content: MessageContent) => {
    if (content.type === 'text') {
      return html`<marked-block .value="${content.text}"></marked-block>`;
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
      padding: 16px;
      box-sizing: border-box;
    }

    .message-area {
      width: 100%;
      display: flex;
      flex: 1;
      flex-direction: column;
      gap: 16px;
      padding: 8px 0px;
      box-sizing: border-box;
      overflow-y: auto;
    }

    .input-area {
      width: 100%;
    }
  `;
}
