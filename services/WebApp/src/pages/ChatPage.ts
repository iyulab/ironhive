import { LitElement, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";

import { Message, SubmitMessageEvent } from "@iyulab/chat-component";
import { Api, ChatCompletionRequest } from "../services";

@customElement('chat-page')
export class ChatPage extends LitElement {
  @state() messages: Message[] = [];
  @state() count: number = 0;

  render() {
    return html` 
      <div class="container">
        <message-box
          .messages=${this.messages}
        ></message-box>
        
        <message-input
          placeholder="Type a message..."
          @submit=${this.submit}>
        ></message-input>
      </div>
    `;
  }

  private submit = async (e: SubmitMessageEvent) => {
    const value = e.detail;
    const user_msg: Message = {
      role: 'user',
      content: [{ type: 'text', value: value }],
      timestamp: new Date().toISOString()
    }
    const bot_msg: Message = {
      role: 'assistant',
      content: [],
      timestamp: new Date().toISOString()
    }

    this.messages = [...this.messages, user_msg];
    const anth = { provider: "anthropic", model: "claude-3-5-haiku-latest" };
    const open = { provider: "openai", model: "gpt-4.1-nano" };
    const gemini = { provider: "gemini", model: "gemini-2.0-flash-lite" };
    const iyulab = { provider: "iyulab", model: "exaone-3.5" };
    const xai = { provider: "xai", model: "grok-3-mini-fast" };
    const request: ChatCompletionRequest = {
      provider: open.provider,
      model: open.model,
      messages: this.messages,
      instructions: "유저의 질문에 성실히 대답하세요."
    }

    for await (const res of Api.conversation(request)) {  
      let last = this.messages[this.messages.length - 1];
      if (last.role !== 'assistant') {
        this.messages = [...this.messages, bot_msg];
        last = this.messages[this.messages.length - 1];
      }

      const data = res.data;
      if (!data) continue;
      
      const index = data.index || 0;
      last.content ||= [];
      const content = last.content?.at(index);

      if (content) {
        if (content.type === 'text' && data.type === 'text') {
          content.value ||= '';
          content.value += data.value;
        } else {
          last.content[index] = data;
        }
      } else {
        last.content?.push(data);
      }

      this.messages = [...this.messages];
    }
  }

  static styles = css`
    :host {
      width: 100%;
      height: 100%;
    }

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
      overflow: hidden;
    }

    message-box {
      position: relative;
      top: 0;
      left: 0;
      width: 100%;
      height: 80%;
    }

    message-input {
      position: absolute;
      bottom: 10px;
      left: 10%;
      width: 80%;
      height: 20%;
    }
  `;
}
