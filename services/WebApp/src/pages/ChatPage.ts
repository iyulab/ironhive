import { LitElement, PropertyValues, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";

import type { AssistantMessage, Message, ModelSummary } from "@iyulab/chat-components";
import { CanceledError, CancelToken } from '@iyulab/http-client';
import { Api, MessageGenerationRequest } from "../services";

@customElement('chat-page')
export class ChatPage extends LitElement {
  private canceller = new CancelToken();

  @state() loading: boolean = false;
  @state() error?: { status: "danger" | "warning" | "info", message: string };

  @state() models: ModelSummary[] = [];
  @state() model?: ModelSummary;
  @state() messages: Message[] = [];
  @state() usages: number = 0;

  protected firstUpdated(changedProperties: PropertyValues) {
    super.firstUpdated(changedProperties);
    
    fetch('/models.json')
      .then(res => res.text())
      .then(text => JSON.parse(text))
      .then((json: any) => {
        this.models = json.models;
        const model = localStorage.getItem('model');
        this.model = model ? JSON.parse(model) : this.models.at(0) || undefined;
      });

    this.messages = JSON.parse(localStorage.getItem('messages') || '[]');
    this.usages = parseInt(localStorage.getItem('usages') || '0', 0);
  }

  render() {
    return html`
      <div class="container">
        <uc-status-panel
          .status=${this.loading ? 'processing' : 'pending'}
        ></uc-status-panel>
        
        <uc-token-panel
          .value=${this.usages}
          .maxValue=${this.model?.contextLength || 0}
        ></uc-token-panel>

        <uc-message-alert
          ?open=${this.error !== undefined}
          timeout="5000"
          status=${this.error?.status || 'danger'}
          .value=${this.error?.message}
        ></uc-message-alert>

        <uc-message-box
          .messages=${this.messages}
          @tool-change=${this.change}
        ></uc-message-box>

        <uc-message-input
          .loading=${this.loading}
          placeholder="Type a message..."
          @send=${this.submit}
          @stop=${() => this.canceller.cancel()}>
          <uc-model-select
            .models=${this.models}
            .selectedModel=${this.model}
            @select=${(e: any) => {this.model = e.detail; localStorage.setItem('model', JSON.stringify(this.model))}}
          ></uc-model-select>
          <div style="flex: 1"></div>
          <uc-clear-button
            @click=${this.clear}
          ></uc-clear-button>
        </uc-message-input>
      </div>
    `;
  }

  private clear() {
    this.messages = [];
    this.usages = 0;
    localStorage.setItem('messages', JSON.stringify(this.messages));
    localStorage.setItem('usages', this.usages.toString());
    this.requestUpdate();
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
        if (content.type === 'tool' && 
          content.isCompleted === false &&
          content.isApproved === false) {
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

      if (!this.model) {
        this.error = { status: 'warning', message: '모델을 선택하세요.' };
        return;
      }

      const request: MessageGenerationRequest = {
        provider: this.model.provider,
        model: this.model.modelId,
        messages: this.messages,
        system: "유저가 요구하지 않는 한 너무 길게 대답하지 마세요.",
      }
      // console.debug('요청 메시지:', request);

      for await (const res of Api.conversation(request, this.canceller)) { 
        // console.debug(res);
        let last = this.messages[this.messages.length - 1];
        if (last.role !== 'assistant') {
          this.messages = [...this.messages, { role: 'assistant', content: [] }];
          last = this.messages[this.messages.length - 1] as AssistantMessage;
        }
  
        if (res.type === 'message.begin') {
          last.id = res.id;
          continue;
        } else if (res.type === 'message.content.added') {
          last.content.push(res.content);
        } else if (res.type === 'message.content.delta') {
          const content = last.content.at(res.index);
          if (content?.type === 'text' && res.delta.type === 'text') {
            content.value ||= '';
            content.value += res.delta.value || '';
          } else if (content?.type === 'thinking' && res.delta.type === 'thinking') {
            content.value ||= '';
            content.value += res.delta.data || '';
          } else if (content?.type === 'tool' && res.delta.type === 'tool') {
            content.input ||= '';
            content.input += res.delta.input || '';
          }
        } else if (res.type === 'message.content.updated') {
          const content = last.content.at(res.index);
          if (content?.type === 'thinking' && res.updated.type === 'thinking') {
            content.id = res.updated.id;
          } else if (content?.type === 'tool' && res.updated.type === 'tool') {
            content.output = res.updated.output;
            content.isCompleted = true;
          }
        } else if (res.type === 'message.content.in_progress') {
          const content = last.content.at(res.index);
          if (content?.type === 'tool') {
            continue;
          }
        } else if (res.type === 'message.content.completed') {
          const content = last.content.at(res.index);
          if (content?.type === 'thinking') {
            continue;
          } else if (content?.type === 'tool') {
            continue;
          }
        } else if (res.type === 'message.done') {
          last.id ||= res.id;
          last.timestamp = res.timestamp || new Date().toISOString();
          this.usages = res.tokenUsage?.totalTokens || 0;
          console.debug('토큰 사용량:', this.usages);
        }

        this.messages = [...this.messages];
      }

      localStorage.setItem('messages', JSON.stringify(this.messages));
      localStorage.setItem('usages', this.usages.toString());
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

    uc-status-panel {
      position: absolute;
      z-index: 100;
      top: 16px;
      left: 16px;
    }

    uc-token-panel {
      position: absolute;
      z-index: 100;
      top: 120px;
      left: 16px;
    }

    uc-alert {
      position: absolute;
      z-index: 100;
      max-width: 60%;
      top: 16px;
      left: 50%;
      transform: translateX(-50%);
    }

    uc-message-box {
      position: relative;
      width: 100%;
      height: 100%;

      --messages-padding: 10px 20% 160px 20%;
    }

    uc-message-input {
      position: absolute;
      z-index: 100;
      width: 60%;
      bottom: 16px;
      left: 50%;
      transform: translateX(-50%);
    }
  `;
}
