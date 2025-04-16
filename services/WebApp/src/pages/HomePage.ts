import { LitElement, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";

import "@iyulab/chat-component";
import "@iyulab/chat-component/styles/hivestack-light.css";
import "@iyulab/chat-component/styles/hivestack-dark.css";

import { Message, MessageContent, StopMessageEvent, SubmitMessageEvent } from "@iyulab/chat-component";
import { HttpResponse } from "../services/http";
import { Api } from "../services";

@customElement('home-page')
export class HomePage extends LitElement {
  private _res?: HttpResponse;
  
  @state() messages: Message[] = [];

  render() {
    return html`
      <div class="flexible-box">
        <div class="container">
          <message-box
            .messages=${this.messages}
          ></message-box>
          
          <message-input
            placeholder="Type a message..."
            @submit=${this.submit}
            @stop=${this.stop}>
          ></message-input>
        </div>
      </div>
      <div @click=${this.toggle} class="toggler">T</div>
    `;
  }

  private stop = (e: StopMessageEvent) => {
    this._res?.cancel();
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
    const open = { provider: "openai", model: "gpt-4o-mini" };
    const gemini = { provider: "gemini", model: "gemini-2.0-flash-lite" };
    const iyulab = { provider: "iyulab", model: "exaone-3.5" };

    this._res = await Api.chatAsync({
      provider: open.provider,
      model: open.model,
      messages: this.messages,
      instructions: "유저의 질문에 성실히 대답하세요."
    }, (item) => {
      let last = this.messages[this.messages.length - 1];
      if (last.role !== 'assistant') {
        this.messages = [...this.messages, bot_msg];
        last = this.messages[this.messages.length - 1];
      }

      const data = item.data as MessageContent;
      const index = data.index || 0;
      last.content ||= [];
      const content = last.content?.at(index);
      console.log("Data: ", data);

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
    });
  }

  private toggle = () => {
    const html = document.documentElement;
    if (html.hasAttribute('theme')) {
      html.removeAttribute('theme');
    } else {
      html.setAttribute('theme', 'dark');
    }
  }

  static styles = css`
    :host {
      position: relative;
      display: flex;
      align-items: center;
      justify-content: center;
      width: 100%;
      height: 100%;
    }

    .flexible-box {
      display: block;
      width: 800px;
      height: 80%;
      border: 0.5px solid gray;
      resize: both;
      overflow: auto;
      box-sizing: border-box;
    }
    .flexible-box::-webkit-scrollbar {
      width: 0px;
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

    .toggler {
      position: absolute;
      top: 5px;
      right: 10px;
      width: 24px;
      height: 24px;
      background-color: #000;
      color: #fff;
      text-align: center;
      line-height: 24px;
      cursor: pointer;
    }
    .toggler:hover {
      background-color: #333;
    }
    .toggler:active {
      background-color: #666;
    }

  `;
}
