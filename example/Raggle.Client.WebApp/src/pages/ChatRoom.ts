import { LitElement, css, html, nothing } from "lit";
import { customElement, property } from "lit/decorators.js";

import type { Message } from "../models";
import { Api } from "../services/ApiClient";

@customElement('chat-room')
export class ChatRoom extends LitElement {

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
        ]}
      ];
      const message: Message = { role: 'assistant', content: []};
      
      var controller = Api.chatAssistantAsync(this.assistantId, this.messages, (msg) => {
        if (msg.content) {
          console.log(msg.content);
          if (msg.content?.type === 'text') {
            console.log('text');
            const content = message.content?.at(msg.content.index);
            if (content) {
              console.log('content');
              content.text = msg.content.text;
            } else {
              console.log('push');
              message.content?.push(msg.content);
            }
          }
        }
      });

      // await new Promise(resolve => setTimeout(resolve, 1000));

      // console.log('aborting');
      // controller.abort();
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
      gap: 8px;
    }

    .input {
      width: 100%;
    }
  `;
}