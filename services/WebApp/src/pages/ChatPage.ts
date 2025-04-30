import { LitElement, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";

import { Message } from "@iyulab/chat-components";
import { CancelToken } from '@iyulab/http-client';
import { Api, ChatCompletionRequest } from "../services";

@customElement('chat-page')
export class ChatPage extends LitElement {
  private token = new CancelToken();

  @state() loading: boolean = false;
  @state() messages: Message[] = [];

  render() {
    return html`
      <div class="container">
        <message-box
          .messages=${this.messages}
        ></message-box>
        
        <message-input
          .loading=${this.loading}
          placeholder="Type a message..."
          @send=${this.submit}
          @stop=${this.token.cancel}
        ></message-input>
      </div>
    `;
  }

  private submit = async (e: any) => {
    try {
      this.loading = true;
      this.token = new CancelToken();
      this.token.register(() => {
        console.warn('Request cancelled');
      });

      const value = e.detail;
      const user_msg: Message = {
        role: 'user',
        content: [{ type: 'text', value: value }],
        timestamp: new Date().toISOString()
      }
      const bot_msg: Message = {
        role: 'assistant',
        content: [],
      }

      this.messages = [...this.messages, user_msg];
      const anth = { provider: "anthropic", model: "claude-3-5-haiku-latest" };
      const open = { provider: "openai", model: "gpt-4.1-mini" };
      const gemini = { provider: "gemini", model: "gemini-2.0-flash-lite" };
      const iyulab = { provider: "iyulab", model: "exaone-3.5" };
      const xai = { provider: "xai", model: "grok-3-mini-fast" };
      const request: ChatCompletionRequest = {
        provider: open.provider,
        model: open.model,
        messages: this.messages,
        instructions: "유저의 질문에 성실히 대답하세요."
      }

      for await (const res of Api.conversation(request, this.token)) { 
        let last = this.messages[this.messages.length - 1];
        if (last.role !== 'assistant') {
          this.messages = [...this.messages, bot_msg];
          last = this.messages[this.messages.length - 1];
        }
  
        const data = res.data;
        if (data) {
          const index = data.index || 0;
          last.content ||= [];
          const content = last.content?.at(index);
  
          if (content) {
            if (content.type === 'text' && data.type === 'text') {
              content.value ||= '';
              content.value += data.value || '';
            } else if (content.type === 'thinking' && data.type === 'thinking') {
              content.value ||= '';
              content.value += data.value || '';
            } else {
              last.content[index] = data;
            }
          } else {
            last.content?.push(data);
          }
  
          this.messages = [...this.messages];
        } else if (res.timestamp) {
          last.timestamp = res.timestamp;
          this.messages = [...this.messages];
        }
      }
  
      this.loading = false;
    } catch (e) {
      console.info('Error:', e);
    }
  }

  static styles = css`
    :host {
      width: 100%;
      height: 100%;
    }

    .container {
      position: relative;
      width: 100%;
      height: 100%;
      min-width: 320px;
      min-height: 480px;
      color: var(--hs-text-color);
      background-color: var(--hs-background-color);
      overflow: hidden;
    }

    message-box {
      position: relative;
      width: 100%;
      height: 100%;

      --messages-padding: 10px 20% 160px 20%;
    }

    message-input {
      position: absolute;
      bottom: 16px;
      left: 50%;
      transform: translateX(-50%);
      width: 60%;
    }
  `;
}
