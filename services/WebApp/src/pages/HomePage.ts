import { LitElement, PropertyValues, css, html, nothing } from "lit";
import { customElement, query, queryAll, state } from "lit/decorators.js";
import { repeat } from "lit/directives/repeat.js";

import type { Alert, BlockContent, TextBlock, ToolBlock, ToolBlockContent, Message as UcMessage } from "@iyulab/chat-components";
import { CanceledError, CancelToken } from '@iyulab/http-client';

import { Api, AssistantMessage, Message, MessageContent, MessageGenerationRequest, type ModelSummary } from "../services";

@customElement('home-page')
export class HomePage extends LitElement {
  private canceller = new CancelToken();

  @query('.message-area') messageAreaEl!: HTMLDivElement;
  @queryAll('uc-message') messageEls!: NodeListOf<UcMessage>;
  @query('.alert') alertEl!: Alert;
  @query('.input') inputEl!: TextBlock;

  @state() status: '대기중' | '생성준비완료' | '생성중' = '대기중';

  @state() models: ModelSummary[] = [];
  @state() model?: ModelSummary;
  @state() messages: Message[] = [];
  @state() usages: number = 0;

  protected firstUpdated(changedProperties: PropertyValues): void {
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
    this.scrollToBottom();
  }

  render() {
    return html`
      <!-- Select Area -->
      <model-select
        .models=${this.models}
        .selectedModel=${this.model}
        @select=${(e: any) => {this.model = e.detail; localStorage.setItem('model', JSON.stringify(this.model))}}
      ></model-select>

      <!-- Message Area -->
      <div class="message-area">
        <div class="message-box">
          ${repeat(this.messages, (msg: Message) => msg.id, (msg: Message) => {
              const content = this.messageToBlockContent(msg);
              return msg.role === 'user' ? html`
                <uc-message class="user-msg"
                  .content=${content}
                  .timestamp=${msg.timestamp}
                  @change=${this.handleChangeMessage}>
                </uc-message>`
              : msg.role === 'assistant' ? html`
                <uc-message class="bot-msg"
                  .content=${content}
                  .timestamp=${msg.timestamp}
                  @change=${this.handleChangeMessage}>
                </uc-message>`
              :nothing})}
            <!-- <div class="empty-msg"></div> -->
        </div>
      </div>

      <!-- Control Area -->
      <div class="control-area">
        <uc-alert
          class="alert"
        ></uc-alert>
        <div class="control-box">
          <uc-text-block 
            class="input"
            editable
            placeholder="메시지를 입력하세요..."
            maxRows="15"
            @input=${this.handleInputChange}
            @keydown=${this.handleInputKeydown}>
          </uc-text-block>
          
          <div class="buttons">
            <uc-attach-button
              multiple
              accept="image/*,text/plain,application/pdf"
              @select-files=${this.handleAttachChange}
            ></uc-attach-button>
            <div class="flex"></div>
            <uc-button tooltip="Clear All Messages"
              @click=${this.handleClearClick}>
              <uc-icon external name="eraser"></uc-icon>
            </uc-button>
            <uc-send-button
              mode=${this.status === '생성중' ? 'stop' : 'send'}
              ?disabled=${this.status === '대기중'}
              @click=${this.handleSendClick}>
            </uc-send-button>
          </div>
        </div>
      </div>
    `;
  }

  private messageToBlockContent = (msg: Message): BlockContent[] => {
    return msg.content.map((content: MessageContent) => {
      if (content.type === 'thinking') {
        return { type: 'thinking', value: content.value || '' };
      }

      if (content.type === 'text') {
        if (msg.role === 'user') {
          return { type: 'text', value: content.value || '' };
        }

        if (msg.role === 'assistant') {
          return { type: 'markdown', value: content.value || ''};
        }
      }

      if (content.type === 'tool') {
        return { type: 'tool', 
          status : content.isCompleted ? content.output?.isSuccess ? 'SUCCESS' : 'FAILED' :
          content.isApproved ? 'PENDING_APPROVAL' : 'WAITING',
          name: content.name, 
          input: content.input, 
          output: content.output  
        } as ToolBlockContent;
      }

      return { type: 'text', value: '' };
    });
  }

  private showAlert = (status: 'info' | 'warning' | 'danger', message: string) => {
    this.alertEl.status = status;
    this.alertEl.value = message;
    this.alertEl.open = true;
  }

  private handleAttachChange = (event: CustomEvent) => {
    const files = event.detail;
    this.showAlert('info', `현재 파일 첨부 기능은 지원하지 않습니다. 
      ${files.length}개의 파일이 선택되었습니다.`);
  }

  private handleInputChange = (event: InputEvent) => {
    const text = (event.target as TextBlock).value?.trim();
    if (this.status === '대기중' && text && text.length > 0) {
      this.status = '생성준비완료';
    } else if (this.status === '생성준비완료' && (!text || text.length === 0)) {
      this.status = '대기중';
    }
  }

  private handleInputKeydown = (event: KeyboardEvent) => {
    event.stopPropagation();
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      if (this.status !== '생성준비완료') return;
      
      this.handleSendClick();
    }
  }

  private handleClearClick = () => {
    this.messages = [];
    this.usages = 0;
    localStorage.setItem('messages', JSON.stringify(this.messages));
    localStorage.setItem('usages', this.usages.toString());
    this.status = '대기중';
    this.requestUpdate();
  }

  private handleSendClick = () => {
    if (this.status === '생성준비완료') {
      const user_msg: Message = {
        role: 'user',
        content: [{ type: 'text', value: this.inputEl.value?.trim() || '' }],
        timestamp: new Date().toISOString()
      }
      this.messages = [...this.messages, user_msg];
      this.scrollToBottom();
      this.inputEl.value = '';
      this.generate();
    } else if (this.status === '생성중') {
      this.canceller.cancel('사용자가 요청을 중단했습니다.');
      this.status = '대기중';
      this.requestUpdate();
    }
  }

  private handleChangeMessage = async (e: any) => {
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

  private scrollToBottom = () => {
    requestAnimationFrame(() => {
      this.messageAreaEl.scrollTo({
        top: this.messageAreaEl.scrollHeight,
        behavior: 'smooth'
      });
    });
  }

  private generate = async () => {
    try {
      this.status = '생성중';

      if (!this.model) {
        this.showAlert('warning', '모델이 선택되지 않았습니다. 모델을 선택해주세요.');
        this.status = '생성준비완료';
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
        // console.log(res);
        let last = this.messages[this.messages.length - 1];
        if (last.role !== 'assistant') {
          this.messages = [...this.messages, { role: 'assistant', content: [] }];
          last = this.messages[this.messages.length - 1] as AssistantMessage;
          this.scrollToBottom();
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
            const block = this.messageEls[this.messages.length - 1].content?.at(res.index) as ToolBlockContent;
            block.status = content.output.isSuccess ? 'SUCCESS' : 'FAILED';
          }
        } else if (res.type === 'message.content.in_progress') {
          const content = last.content.at(res.index);
          if (content?.type === 'tool') {
            const block = this.messageEls[this.messages.length - 1].content?.at(res.index) as ToolBlockContent;
            block.status = 'EXECUTING';
          }
        } else if (res.type === 'message.content.completed') {
          const content = last.content.at(res.index);
          if (content?.type === 'thinking') {
            continue;
          } else if (content?.type === 'tool') {
            if (content.isCompleted === false && content.isApproved === false) {
              const block = this.messageEls[this.messages.length - 1].content?.at(res.index) as ToolBlockContent;
              block.status = 'PENDING_APPROVAL';
            }
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
      const last = this.messages[this.messages.length - 1];
      if (last && last.role === 'assistant' && last.content.length === 0) {
        // 마지막 메시지제거
        this.messages = this.messages.slice(0, -1);
      }
      if (e instanceof CanceledError) {
        this.showAlert('info', '요청이 취소되었습니다.');
      } else {
        this.showAlert('danger', `메시지 생성 중 오류가 발생했습니다: ${e.message || e}`);
      }
    } finally {
      this.canceller = new CancelToken();
      this.canceller.register(() => {
        console.warn('등록된 취소 요청');
      });
      if (this.inputEl.value) {
        this.status = '생성준비완료';
      } else {
        this.status = '대기중';
      }
    }
  }

  static styles = css`
    :host {
      position: relative;
      display: flex;
      flex-direction: column;
      width: 100%;
      height: 100%;
    }
    :host * {
      box-sizing: border-box;
    }

    /* Select Area */
    model-select {
      position: absolute;
      top: -40px;
      left: 50%;
      transform: translateX(-50%);
      z-index: 100;
      min-width: 200px;
      max-width: 300px;
    }

    /* Message Area */
    .message-area {
      position: relative;
      display: flex;
      align-items: baseline;
      justify-content: center;
      width: 100%;
      height: 100%;
      overflow: auto;
      scrollbar-width: thin;
      scrollbar-color: var(--uc-scrollbar-color) transparent;
      scrollbar-gutter: stable both-edges;
    }

    .message-box {
      position: relative;
      display: flex;
      flex-direction: column;
      justify-content: flex-start;
      gap: 8px;
      width: 90%;
      max-width: 720px;
    }
    .message-box .user-msg {
      align-self: flex-end;
      max-width: 80%;
    }
    .message-box .bot-msg {
      align-self: flex-start;
      width: 100%;
    }
    .message-box .bot-msg::part(body) {
      background-color: var(--uc-background-color-0);
    }
    /* 마지막 메시지 크기 주의 */
    .message-box > :nth-last-child(1) {
      min-height: calc(100vh - 180px - 48px); /* 180px for bottom + 48px for header */
      margin-bottom: 180px;
    }

    /* Control Area */
    .control-area {
      position: absolute;
      z-index: 100;
      bottom: 24px;
      left: 50%;
      transform: translateX(-50%);
      width: 90%;
      max-width: 720px;
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    uc-alert {
      position: relative;
      width: 100%;
    }

    .control-box {
      position: relative;
      width: 100%;

      display: flex;
      flex-direction: column;
      gap: 4px;
      padding: 12px;
      border: 1px solid var(--uc-border-color-low);
      border-radius: 16px;
      box-shadow: 0 4px 8px var(--uc-shadow-color-mid);
      background-color: var(--uc-background-color-0);
    }
    .control-box .buttons {
      display: flex;
      flex-direction: row;
      align-items: center;
      gap: 8px;
    }
    .control-box .buttons .flex {
      flex: 1;
    }
  `;
}
