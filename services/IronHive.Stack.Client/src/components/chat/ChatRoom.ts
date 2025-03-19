import { LitElement, PropertyValues, css, html } from "lit";
import { customElement, property, query } from "lit/decorators.js";

import type { Message, MessageContent } from "../../models";
import { HiveStack } from "../../services";
import { SubmitMessageEvent, StopMessageEvent } from "../events";
import { HttpResponse } from "../../internal";
import { MessageList } from "./MessageList";

@customElement('chat-room')
export class ChatRoom extends LitElement {
  private _client?: HiveStack;
  private _res?: HttpResponse;

  @query('message-list') messageEl!: MessageList;

  @property({ type: String }) baseUri: string = '';
  @property({ type: Boolean }) loading: boolean = false;
  @property({ type: Array }) messages: Message[] = [];

  protected async updated(_changedProperties: PropertyValues) {
    super.updated(_changedProperties);
    if (_changedProperties.has('baseUri')) {
      this._client = new HiveStack({ baseUrl: this.baseUri });
    }
  }

  render() {
    return html`
      <div class="container">
        <message-list
          .messages=${this.messages}
        ></message-list>
        
        <message-input
          placeholder="Type a message..."
          @submit=${this.handleSubmit}
          @stop=${this.handleStop}>
        ></message-input>
      </div>
    `;
  }

  private handleStop = (e: StopMessageEvent) => {
    this._res?.cancel();
  }

  private handleSubmit = async (e: SubmitMessageEvent) => {
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
    const anth = "anthropic/claude-3-5-haiku-latest";
    const open = "openai/gpt-4o-mini";
    const gemini = "gemini/gemini-2.0-flash";
    const iyulab = "iyulab/exaone-3.5";

    this._res = await this._client?.chatCompletionAsync({
      model: open,
      messages: this.messages,
      system: "you are a chatbot politely responding to user messages",
      stream: true
    }, (item) => {
      let last = this.messages[this.messages.length - 1];
      if (last.role !== 'assistant') {
        this.messages = [...this.messages, bot_msg];
        last = this.messages[this.messages.length - 1];
      }

      const data = item as MessageContent;
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
      this.messageEl.requestUpdate();
      // this.messageEl.scrollTo({
      //   top: this.messageEl.scrollHeight,
      //   behavior: 'instant'
      // });
    });
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
      overflow: hidden;
    }
  `;
}
