import { LitElement, css, html } from "lit";
import { customElement, property } from "lit/decorators.js";
import { Message } from "../models";
import { API } from "../backend/ApiClient";

@customElement('main-chat')
export class MainChat extends LitElement {

  @property({ type: String })
  assistantId: string = '';

  @property({ type: Array })
  messages: Message[] = [];

  @property({ type: String })
  input: string = '';

  render() {
    return html`
      <div class="chat-container">
        <div class="messages">
          ${this.messages.map(msg => html`
            <message-block
              .message=${msg}
            ></message-block>
          `)}
        </div>
        <div class="input-container">
          <input 
            type="text" 
            .value=${this.input} 
            @input=${this.onInput} 
            placeholder="Type a message..."
          />
          <button @click=${this.sendMessage}>
            Send
          </button>
        </div>
      </div>
    `;
  }

  private onInput(event: Event) {
    const input = event.target as HTMLInputElement;
    this.input = input.value;
  }

  private async sendMessage() {
    if (this.input.trim()) {
      this.messages = [ 
        ...this.messages, 
        { role: 'user', content: [
          { type: 'text', index: 0,  text: this.input }
        ]}
      ];
      this.input = '';
      const message: Message = { role: 'assistant', content: []};
      
      var controller = API.chatAssistantAsync(this.assistantId, this.messages, (msg) => {
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
      display: block;
      width: 100%;
      height: 100%;
      box-sizing: border-box;
      font-family: Arial, sans-serif;
    }

    .chat-container {
      display: flex;
      flex-direction: column;
      height: 100%;
    }

    .messages {
      flex: 1;
      overflow-y: auto;
      padding: 16px;
      border-bottom: 1px solid #ddd;
      box-sizing: border-box;
    }

    .message {
      box-sizing: border-box;
      margin-bottom: 8px;
    }

    .user {
      font-weight: bold;
      box-sizing: border-box;
      margin-right: 8px;
    }

    .input-container {
      display: flex;
      padding: 8px;
      border-top: 1px solid #ddd;
      box-sizing: border-box;
    }

    input {
      flex: 1;
      padding: 8px;
      border: 1px solid #ddd;
      border-radius: 4px;
      margin-right: 8px;
      box-sizing: border-box;
    }

    button {
      padding: 8px 16px;
      background-color: #0078d4;
      color: white;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      transition: background-color 0.3s;
      box-sizing: border-box;
    }

    button:hover {
      background-color: #005a9e;
    }

    button:active {
      background-color: #004377;
    }
  `;
}