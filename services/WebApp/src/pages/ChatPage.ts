import { LitElement, PropertyValues, css, html } from "lit";
import { customElement, query, state } from "lit/decorators.js";

import type { AssistantMessage, Message, ModelSummary } from "@iyulab/chat-components";
import { CanceledError, CancelToken } from '@iyulab/http-client';
import { Api, MessageGenerationRequest } from "../services";

@customElement('chat-page')
export class ChatPage extends LitElement {
  private canceller = new CancelToken();

  @query('message-box') messageBoxEl!: LitElement;

  @state() loading: boolean = false;
  @state() error?: { status: "danger" | "warning" | "info", message: string };

  @state() models: ModelSummary[] = [];
  @state() context: { model?: ModelSummary; messages: Message[]; usages: number; } = {
    model: undefined, messages: [], usages: 0 
  };

  protected firstUpdated(changedProperties: PropertyValues) {
    super.firstUpdated(changedProperties);
    
    fetch('/models.json')
      .then(res => res.text())
      .then(text => JSON.parse(text))
      .then((json: any) => {
        this.models = json.models;
        this.context.model = this.models.at(0) || undefined;
      });

    this.context = JSON.parse(localStorage.getItem('history') || '{"model": null, "messages": [], "usages": 0}');
  }

  render() {
    return html`
      <div class="container">
        <status-panel
          .status=${this.loading ? 'processing' : 'pending'}
        ></status-panel>
        
        <token-panel
          .value=${this.context.usages}
          .maxValue=${this.context.model?.contextLength || 0}
        ></token-panel>

        <message-alert
          ?open=${this.error !== undefined}
          timeout="5000"
          status=${this.error?.status || 'danger'}
          .value=${this.error?.message}
        ></message-alert>

        <message-box
          .messages=${this.context.messages}
          @tool-change=${this.change}
        ></message-box>

        <message-input
          .loading=${this.loading}
          placeholder="Type a message..."
          @send=${this.submit}
          @stop=${() => this.canceller.cancel()}>
          <uc-model-select
            .models=${this.models}
            .selectedModel=${this.context.model}
            @select=${(e: any) => this.context.model = e.detail}
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
    this.context.messages = [];
    this.context.usages = 0;
    localStorage.setItem('history', JSON.stringify(this.context));
    this.requestUpdate();
  }

  private submit = async (e: any) => {
    const value = e.detail;
    const user_msg: Message = {
      role: 'user',
      content: [{ type: 'text', value: value }],
      timestamp: new Date().toISOString()
    }
    this.context.messages = [...this.context.messages, user_msg];
    await this.generate();
  }

  private change = async (e: any) => {
    this.context.messages = e.detail;
    const last = this.context.messages[this.context.messages.length - 1];
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

      if (!this.context.model) {
        this.error = { status: 'warning', message: '모델을 선택하세요.' };
        return;
      }

      const request: MessageGenerationRequest = {
        provider: this.context.model.provider,
        model: this.context.model.modelId,
        messages: this.context.messages,
        system: "유저가 요구하지 않는 한 너무 길게 대답하지 마세요.",
      }
      console.debug('요청 메시지:', request);

      for await (const res of Api.conversation(request, this.canceller)) { 
        console.debug(res);
        let last = this.context.messages[this.context.messages.length - 1];
        if (last.role !== 'assistant') {
          this.context.messages = [...this.context.messages, { role: 'assistant', content: [] }];
          this.messageBoxEl.requestUpdate();
          last = this.context.messages[this.context.messages.length - 1] as AssistantMessage;
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
          this.context.usages = res.tokenUsage?.TotalTokens || 0;
        }

        this.messageBoxEl.requestUpdate();
      }

      localStorage.setItem('history', JSON.stringify(this.context));
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
