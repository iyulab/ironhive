import { LitElement, css, html, nothing } from "lit";
import { customElement, property, query, queryAll } from "lit/decorators.js";

import type { Message } from "../models";
import type { AssistantMessage, UserMessage } from "../components";
import { Api } from "../services/ApiClient";

@customElement('chat-room')
export class ChatRoom extends LitElement {

  @query('.messages') messagesEl!: HTMLDivElement;
  @queryAll('user-message') userMsgs!: NodeListOf<UserMessage>;
  @queryAll('assistant-message') assistantMsgs!: NodeListOf<AssistantMessage>;

  @property({ type: String }) key: string = '';
  @property({ type: String }) assistantId: string = '';
  @property({ type: Array }) messages: Message[] = [];

  render() {
    return html`
      <div class="messages">
        ${this.messages.map(msg => {
          if (msg.role === 'user') {
            return html`
              <user-message
                .message=${msg}
              ></user-message>
            `;
          } else if (msg.role === 'assistant') {
            return html`
              <assistant-message
                .message=${msg}
              ></assistant-message>
            `;
          } else {
            return nothing;
          }
        })}
      </div>
      <div class="input">
        <message-input
          placeholder="Type a message..."
          rows="1"
          @send=${this.onSend}
        ></message-input>
      </div>
    `;
  }

  private onSend = async (event: CustomEvent<string>) => {
    const value = event.detail;

    if (value) {
      this.messages = [ 
        ...this.messages, 
        { role: 'user', content: [
          { type: 'text', index: 0,  text: value }
        ]},
        { role: 'assistant', content: [] }
      ];
      this.messagesEl.scrollTop = this.messagesEl.scrollHeight;
      
      var res = await Api.chatAssistantAsync(this.assistantId, this.messages);
      res.onReceive<any>(async (data) => {
        if (!data.content) return;
        this.assistantMsgs[this.assistantMsgs.length - 1].appendContent(data.content);
        this.messagesEl.scrollTop = this.messagesEl.scrollHeight;
      });
    }
  }

  static styles = css`
    :host {
      position: relative;
      display: flex;
      flex-direction: column;
      width: 100%;
      height: 100%;
      padding: 16px;
      box-sizing: border-box;
    }

    .messages {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 16px;
      overflow-y: auto;
      padding: 32px 16px;
      box-sizing: border-box;
    }

    .input {
      width: 100%;
    }
  `;
}