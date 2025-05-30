import { LitElement, PropertyValues, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";

import type { AssistantMessage, Message, ModelDescriptor } from "@iyulab/chat-components";
import { CanceledError, CancelToken } from '@iyulab/http-client';
import { Api, ChatCompletionRequest } from "../services";

@customElement('chat-page')
export class ChatPage extends LitElement {
  private canceller = new CancelToken();

  @state() loading: boolean = false;
  @state() models: ModelDescriptor[] = [];
  @state() selectedModel?: ModelDescriptor;
  @state() messages: Message[] = [];
  @state() tokenUsage: number = 0;
  @state() error?: { status: "danger" | "warning" | "info", message: string };

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
        <status-panel
          .status=${this.loading ? 'processing' : 'pending'}
        ></status-panel>
        
        <token-panel
          .value=${this.tokenUsage}
          .maxValue=${this.selectedModel?.contextWindow || 0}
        ></token-panel>

        <message-alert
          ?open=${this.error !== undefined}
          timeout="5000"
          status=${this.error?.status || 'danger'}
          .value=${this.error?.message}
        ></message-alert>

        <message-box
          .messages=${this.messages}
          @tool-change=${this.change}
        ></message-box>

        <message-input
          .loading=${this.loading}
          placeholder="Type a message..."
          @send=${this.submit}
          @stop=${() => this.canceller.cancel()}>
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

  private clear() {
    this.messages = [];
    localStorage.removeItem('messages');
  }

  private submit = async (e: any) => {
    const value = e.detail;
    const user_msg: Message = {
      role: 'user',
      content: [{ type: 'text', value: value }],
      timestamp: new Date().toISOString()
    }
    this.messages = [...this.messages, user_msg];
    await this.generate();
  }

  private change = async (e: any) => {
    this.messages = e.detail;
    const last = this.messages[this.messages.length - 1];
    if (last.role === 'assistant') {
      for (const content of last.content || []) {
        if (content.type === 'tool' && content.approvalStatus === 'requires') {
          return;
        }
      }

      this.generate();
    }
  }

  private generate = async () => {
    try {
      this.error = undefined;
      this.loading = true;

      if (!this.selectedModel) {
        this.error = { status: 'warning', message: '모델을 선택하세요.' };
        return;
      }

      const request: ChatCompletionRequest = {
        provider: this.selectedModel.provider,
        model: this.selectedModel.modelId,
        messages: this.messages,
        system: "유저가 요구하지 않는 한 너무 길게 대답하지 마세요.",
      }

      for await (const res of Api.conversation(request, this.canceller)) { 
        console.debug(res);
        let last = this.messages[this.messages.length - 1];
        if (last.role !== 'assistant') {
          this.messages = [...this.messages, { role: 'assistant', content: [] }];
          last = this.messages[this.messages.length - 1] as AssistantMessage;
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
              content.id ||= data.id;
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
      if (e instanceof CanceledError) {
        this.error = { status: 'info', message: '요청이 취소되었습니다.' };
      } else {
        this.error = e instanceof Error 
          ? { status: 'danger', message: e.message } 
          : { status: 'danger', message: '알 수 없는 오류가 발생했습니다.' };
      }
    } finally {
      this.loading = false;
      this.canceller = new CancelToken();
      this.canceller.register(() => {
        console.warn('등록된 취소 요청');
      });
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
      background-color: var(--uc-background-color-0);
      overflow: hidden;
    }

    status-panel {
      position: absolute;
      z-index: 100;
      top: 16px;
      left: 16px;
    }

    token-panel {
      position: absolute;
      z-index: 100;
      top: 120px;
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
