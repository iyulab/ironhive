import { LitElement, PropertyValues, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";

import type { Message, ModelDescriptor } from "@iyulab/chat-components";
import { CancelToken } from '@iyulab/http-client';
import { Api, ChatCompletionRequest } from "../services";

@customElement('chat-page')
export class ChatPage extends LitElement {
  private canceller = new CancelToken();

  @state() loading: boolean = false;
  @state() models: ModelDescriptor[] = [];
  @state() selectedModel?: ModelDescriptor;
  @state() messages: Message[] = [];
  @state() tokenUsage: number = 0;
  @state() error?: string;

  protected firstUpdated(changedProperties: PropertyValues) {
    super.firstUpdated(changedProperties);
    
    fetch('/models.json')
      .then(res => res.text())
      .then(text => JSON.parse(text))
      .then((json: any) => {
        this.models = json.models;
        this.selectedModel = this.models.at(0) || undefined;
      });

    this.messages = JSON.parse(localStorage.getItem('messages') || '[]') as Message[];
  }

  render() {
    return html`
      <div class="container">
        <uc-bar-spinner
          ?open=${this.loading}
        ></uc-bar-spinner>

        <token-panel
          .value=${this.tokenUsage}
          .maxValue=${this.selectedModel?.contextWindow || 0}
        ></token-panel>

        <message-alert
          theme="danger"
          ?open=${this.error !== undefined}
          .value=${this.error}
        ></message-alert>

        <message-box
          .messages=${this.messages}
        ></message-box>

        <message-input
          .loading=${this.loading}
          placeholder="Type a message..."
          @send=${this.submit}
          @stop=${this.canceller.cancel.bind(this.canceller)}>
          <uc-model-select
            .models=${this.models}
            .selectedModel=${this.selectedModel}
            @select=${(e: any) => this.selectedModel = e.detail}
          ></uc-model-select>
          <div style="flex: 1"></div>
          <uc-clear-button
            @click=${this.clear}
          ></uc-clear-button>
        </message-input>
      </div>
    `;
  }

  private submit = async (e: any) => {
    try {
      this.error = undefined;
      this.loading = true;

      if (!this.selectedModel) {
        this.error = "모델을 선택해주세요.";
        return;
      }

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
      const request: ChatCompletionRequest = {
        provider: this.selectedModel.provider,
        model: this.selectedModel.modelId,
        messages: this.messages,
        instructions: "유저가 요구하지 않는 한 너무 길게 대답하지 마세요.",
      }

      for await (const res of Api.conversation(request, this.canceller)) { 
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
        } 
        
        if (res.timestamp) {
          last.timestamp = res.timestamp;
          this.messages = [...this.messages];
        } 
        
        if (res.tokenUsage) {
          this.tokenUsage = res.tokenUsage.totalTokens;
        }
      }
  
      localStorage.setItem('messages', JSON.stringify(this.messages));
    } catch (e: any) {
      this.error = e instanceof Error ? e.message : e.toString();
    } finally {
      this.loading = false;
      this.canceller = new CancelToken();
      this.canceller.register(() => {
        console.warn('Request cancelled');
      });
    }
  }

  private clear() {
    this.messages = [];
    localStorage.removeItem('messages');
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
      background-color: var(--uc-background-color-0);
      overflow: hidden;
    }

    uc-bar-spinner {
      position: absolute;
      z-index: 100;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      display: none;
    }
    uc-bar-spinner[open] {
      display: block;
    }

    token-panel {
      position: absolute;
      z-index: 100;
      width: 15%;
      top: 16px;
      left: 16px;
    }

    message-alert {
      position: absolute;
      z-index: 100;
      max-width: 60%;
      top: 16px;
      left: 50%;
      transform: translateX(-50%);
    }

    message-box {
      position: relative;
      width: 100%;
      height: 100%;

      --messages-padding: 10px 20% 160px 20%;
    }

    message-input {
      position: absolute;
      z-index: 100;
      width: 60%;
      bottom: 16px;
      left: 50%;
      transform: translateX(-50%);
    }
  `;
}
