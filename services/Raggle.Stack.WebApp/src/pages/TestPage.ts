import { LitElement, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";

import "@iyulab/hive-stack/components/chat";
import "@iyulab/hive-stack/components/styles/hivestack-light.css";
import "@iyulab/hive-stack/components/styles/hivestack-dark.css";
import { SendMessageEvent } from '@iyulab/hive-stack/components';
import { HiveStack, Message } from '@iyulab/hive-stack';

@customElement('test-page')
export class TestPage extends LitElement {
  private _client: HiveStack = new HiveStack({
    baseUrl: 'http://172.30.1.26:5075/v1/'
  });

  @state() messages: Message[] = [];

  render() {
    return html`
      <div class="flexible-box">
        <chat-room
          .messages=${this.messages}
          @send=${this.send}
        ></chat-room>
      </div>
      <div @click=${this.toggle} class="toggler">T</div>
    `;
  }

  private toggle = () => {
    const html = document.documentElement;
    if (html.hasAttribute('theme')) {
      html.removeAttribute('theme');
    } else {
      html.setAttribute('theme', 'dark');
    }
  }

  private send = async (e: SendMessageEvent) => {
    const value = e.detail;
    const user_msg: Message = {
      role: 'user',
      content: [{ type: 'text', value: value }]
    }
    this.messages = [...this.messages, user_msg];
    const res = await this._client.chatCompletionAsync({
      model: 'openai/gpt-4o-mini',
      messages: this.messages,
      system: "you are the best assistant ever",
      stream: false
    });
    const bot_msg = (await res.json() as any).data as Message;
    this.messages = [...this.messages, bot_msg];
    console.log(this.messages);
  }

  static styles = css`
    :host {
      position: relative;
      display: flex;
      align-items: flex-start;
      justify-content: center;
      width: 100%;
      height: 100%;
    }

    .flexible-box {
      display: block;
      width: 400px;
      height: 90%;
      border: 0.5px solid gray;
      resize: both;
      overflow: auto;
      box-sizing: border-box;
    }
    .flexible-box::-webkit-scrollbar {
      width: 0px;
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
