import { LitElement, PropertyValues, css, html, nothing } from "lit";
import { customElement, query, queryAll, state } from "lit/decorators.js";
import { repeat } from "lit/directives/repeat.js";

import type { Alert, BlockItem, TextBlock, ToolBlockItem, Message as UcMessage } from "@iyulab/chat-components";
import { CanceledError, CancelToken } from '@iyulab/http-client';

import { Api, AssistantMessage, Message, MessageContent, MessageGenerationRequest, ToolMessageContent, type ModelSummary } from "../services";
import { AppStorage } from "../services/AppStorage";

@customElement('home-page')
export class HomePage extends LitElement {
  private canceller = new CancelToken();
  private pausedCount: number = 0;

  @queryAll('uc-message') messageEls!: NodeListOf<UcMessage>;
  @query('.message-area') messageAreaEl!: HTMLDivElement;
  @query('uc-alert') alertEl!: Alert;
  @query('uc-text-block') inputEl!: TextBlock;

  @state() status: 'wait' | 'ready' | 'busy' | 'pause' = 'wait';
  @state() scrollPosition: 'top' | 'center' | 'bottom' = 'bottom';
  @state() model?: ModelSummary;
  @state() messages: Message[] = [];
  @state() usages: number = 0;
  @state() thinking: 'low' | 'medium' | 'high' | 'none' = 'none';

  protected firstUpdated(changedProperties: PropertyValues): void {
    super.firstUpdated(changedProperties);
    this.model = AppStorage.model;
    this.messages = AppStorage.messages;
    this.usages = AppStorage.usages;
    this.thinking = AppStorage.thinking;
    this.scrollToBottom();
  }

  render() {
    return html`
      <!-- Header Area -->
      <div class="header-area">
        <model-select
          .model=${this.model}
          @select-model=${this.handleSelectModel}
        ></model-select>
        <uc-token-indicator class="header-btn"
          type="button"
          .value=${this.usages}
          .maxValue=${this.model?.contextLength || 0}
        ></uc-token-indicator>
        <uc-button class="header-btn" tooltip="Customize Model Parameters">
          <uc-icon name="sidebar-right"></uc-icon>
        </uc-button>
        <uc-alert></uc-alert>
      </div>

      <!-- Message Area -->
      <div class="message-area" @scroll=${this.updateScrollPosition}>
        <div class="message-box">
          ${repeat(this.messages, (msg: Message) => msg.id, (msg: Message) => {
              const items = this.messageToBlockItem(msg);
              return msg.role === 'user' ? html`
                <uc-message class="user-msg"
                  .items=${items}
                  .timestamp=${msg.timestamp}
                  @tool-approval=${this.handleToolUpdate}
                ></uc-message>`
              : msg.role === 'assistant' ? html`
                <uc-message class="bot-msg"
                  .items=${items}
                  .timestamp=${msg.timestamp}
                  @tool-approval=${this.handleToolUpdate}
                ></uc-message>`
              :nothing})}
        </div>
      </div>

      <!-- Control Area -->
      <div class="control-area">
        <div class="control-box">
          <uc-icon class="scroll-btn"
            name="chevron-down"
            ?visible=${this.scrollPosition !== 'bottom'}
            @click=${this.scrollToBottom}
          ></uc-icon>

          <uc-text-block 
            editable
            placeholder="메시지를 입력하세요..."
            maxRows="15"
            @input=${this.handleChangeInput}
            @keydown=${this.handleKeydownInput}
          ></uc-text-block>
          
          <div class="buttons">
            <uc-attach-button
              multiple
              accept="image/*,text/plain,application/pdf"
              @select-files=${this.handleSelectFiles}
            ></uc-attach-button>
            <uc-think-button
              ?disabled=${this.model?.thinkable === false}
              .value=${this.model?.thinkable ? this.thinking : 'none'}
              @change-think=${this.handleChangeThink}
            ></uc-think-button>
            <div class="flex"></div>
            <uc-button tooltip="Clear Messages"
              @click=${this.handleClearClick}>
              <uc-icon external name="eraser"></uc-icon>
            </uc-button>
            <uc-send-button
              mode=${this.status === 'busy' || this.status === 'pause' ? 'stop' : 'send'}
              ?disabled=${this.status === 'wait' || this.status === 'pause'}
              @click=${this.handleSendClick}
            ></uc-send-button>
          </div>
        </div>
      </div>
    `;
  }

  private handleChangeThink = (event: CustomEvent) => {
    const value = event.detail;
    this.thinking = value || 'none';
    AppStorage.thinking = this.thinking;
  }

  private handleSelectModel = (event: CustomEvent) => {
    this.model = event.detail;
    AppStorage.model = this.model;
  }

  private handleSelectFiles = (event: CustomEvent) => {
    const files = event.detail;
    this.info(`첨부된 파일: ${files.length}개, 현재 첨부 기능은 지원하지 않습니다.`);
  }

  private handleChangeInput = (event: InputEvent) => {
    const text = (event.target as TextBlock).value?.trim();
    if (this.status === 'wait' && text && text.length > 0) {
      this.status = 'ready';
    } else if (this.status === 'ready' && (!text || text.length === 0)) {
      this.status = 'wait';
    }
  }

  private handleKeydownInput = (event: KeyboardEvent) => {
    event.stopPropagation();
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      if (this.status !== 'ready') return;
      
      this.handleSendClick();
    }
  }

  private handleClearClick = () => {
    this.messages = [];
    this.usages = 0;
    AppStorage.messages = this.messages;
    AppStorage.usages = this.usages;
    this.status = 'wait';
    this.requestUpdate();
  }

  private handleSendClick = () => {
    if (this.status === 'ready') {
      const user_msg: Message = {
        role: 'user',
        content: [{ type: 'text', value: this.inputEl.value?.trim() || '' }],
        timestamp: new Date().toISOString()
      }
      this.messages = [...this.messages, user_msg];
      this.scrollToBottom();
      this.inputEl.value = '';
      this.generate();
    } else if (this.status === 'busy') {
      this.canceller.cancel('사용자가 요청을 중단했습니다.');
    }
  }

  private handleToolUpdate = async (e: CustomEvent) => {
    const { index, isApproved } = e.detail;
    const last = this.messages.at(-1);
    const content = last?.content.at(index) as ToolMessageContent;
    if (!content) return;
    content.status = isApproved ? 'approved' : 'rejected';
    this.pausedCount --;

    if (this.pausedCount <= 0) {
      this.generate();
    }
  }

  private updateScrollPosition = () => {
    const scrollTop = this.messageAreaEl.scrollTop;
    const scrollHeight = this.messageAreaEl.scrollHeight;
    const clientHeight = this.messageAreaEl.clientHeight;

    // 스크롤 위치에 따라 상태 업데이트
    if (scrollTop === 0) {
      this.scrollPosition = 'top';
    } else if (scrollTop + clientHeight >= scrollHeight - 1) {
      this.scrollPosition = 'bottom';
    } else {
      this.scrollPosition = 'center';
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

  private info = (message: string) => 
    this.alertEl.show({ status: 'info', value: message, timeout: 3000 });

  private warn = (message: string) =>
    this.alertEl.show({ status: 'warning', value: message, timeout: 3000 });

  private error = (message: string) =>
    this.alertEl.show({ status: 'danger', value: message });

  private messageToBlockItem = (msg: Message): BlockItem[] => {
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
        return { 
          type: 'tool', 
          status : content.status,
          name: content.name, 
          input: content.input, 
          output: content.output
        } as ToolBlockItem;
      }

      return { type: 'text', value: '' };
    });
  }

  private generate = async () => {
    try {
      this.status = 'busy';

      if (!this.model) {
        this.warn('모델이 선택되지 않았습니다. 모델을 선택해주세요.');
        this.status = 'ready';
        return;
      }

      const request: MessageGenerationRequest = {
        provider: this.model.provider,
        model: this.model.modelId,
        messages: this.messages,
        system: "유저가 요구하지 않는 한 너무 길게 대답하지 마세요.",
        thinkingEffort: this.model.thinkable ? this.thinking !== 'none' ? this.thinking : undefined : undefined,
        maxTokens: this.model.maxOutput,
      }
      // console.debug('요청 메시지:', request);

      for await (const res of Api.conversation(request, this.canceller)) { 
        console.log(res);
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
            content.status = res.updated.status;
            content.output = res.updated.output;
            if (content.status === 'paused') {
              this.pausedCount++;
            }
          }
        } else if (res.type === 'message.content.in_progress') {
          continue;
        } else if (res.type === 'message.content.completed') {
          continue;
        } else if (res.type === 'message.done') {
          last.id ||= res.id;
          last.timestamp = res.timestamp || new Date().toISOString();
          this.usages = res.tokenUsage?.totalTokens || 0;
          if (res.doneReason === 'toolCall') {
            this.status = 'pause';
            this.info('사용자의 승인을 요구하는 도구 호출이 있습니다. 도구를 승인해주세요.');
          } else {
            AppStorage.messages = this.messages;
            AppStorage.usages = this.usages;
            if (this.inputEl.value) {
              this.status = 'ready';
            } else {
              this.status = 'wait';
            }
          }
        }

        this.messages = [...this.messages];
      }
    } catch (e: any) {
      const last = this.messages[this.messages.length - 1];
      if (last && last.role === 'assistant' && last.content.length === 0) {
        // 마지막 메시지제거
        this.messages = this.messages.slice(0, -1);
      }
      if (e instanceof CanceledError) {
        this.info('요청이 취소되었습니다.');
      } else if (e instanceof Error) {
        this.error(`메시지 생성 중 오류가 발생했습니다:<br>${e.message}`);
      } else {
        this.error(`메시지 생성 중 오류가 발생했습니다: 알 수 없는 오류`);
      }
    } finally {
      this.canceller = new CancelToken();
      this.canceller.register(() => {
        console.warn('등록된 취소 요청');
      });
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

    /* Header Area */
    .header-area {
      position: absolute;
      top: -48px;
      left: 50%;
      transform: translateX(-50%);
      z-index: 100;
      width: 720px;
      height: 48px;
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: center;
      gap: 4px;
    }
    .header-btn {
      border: none;
      font-size: 18px;
    }

    uc-alert {
      position: absolute;
      z-index: 100;
      top: 110%;
      left: 50%;
      min-width: 320px;
      max-width: 100%;
      transform: translateX(-50%) translateY(-24px);
    }
    uc-alert[open] {
      transform: translateX(-50%) translateY(0px);
    }

    /* Message Area */
    .message-area {
      position: relative;
      display: flex;
      justify-content: center;
      width: 100%;
      height: calc(100% - 106px);
      overflow: auto;
      scrollbar-width: thin;
      scrollbar-color: var(--uc-scrollbar-color) transparent;
      scrollbar-gutter: stable both-edges;
    }

    .message-box {
      position: relative;
      display: flex;
      flex-direction: column;
      gap: 8px;
      width: 90%;
      height: 100%;
      max-width: 720px;
    }
    .message-box .user-msg {
      max-width: 100%;
      align-self: flex-end;
    }
    .message-box .bot-msg {
      width: 100%;
      align-self: flex-start;
    }
    .message-box .bot-msg::part(body) {
      background-color: var(--uc-background-color-0);
    }
    .message-box > :nth-last-child(1) {
      min-height: 100%;
    }
    .message-box > :nth-last-child(1)::part(footer) {
      padding-bottom: 32px;
    }

    /* Control Area */
    .control-area {
      position: relative;
      width: 100%;
      height: 106px;
    }

    .control-box {
      position: absolute;
      z-index: 100;
      bottom: 16px;
      left: 50%;
      transform: translateX(-50%);
      width: 90%;
      max-width: 720px;
      display: flex;
      flex-direction: column;
      gap: 4px;
      padding: 12px;
      border: 1px solid var(--uc-border-color-low);
      border-radius: 16px;
      box-shadow: 0 4px 8px var(--uc-shadow-color-mid);
      background-color: var(--uc-background-color-0);
    }
    .control-box .scroll-btn {
      position: absolute;
      z-index: 100;
      top: -42px;
      right: 0;
      font-size: 16px;
      padding: 8px;
      border: 1px solid var(--uc-border-color-low);
      border-radius: 50%;
      background-color: var(--uc-background-color-0);
      box-shadow: 0 2px 4px var(--uc-shadow-color-mid);
      pointer-events: none;
      scale: 0;
      opacity: 0;
      transform: translateY(42px);
      transition: transform 0.3s ease, opacity 0.3s ease, scale 0.3s ease;
      cursor: pointer;
    }
    .control-box .scroll-btn[visible] {
      pointer-events: auto;
      scale: 1;
      opacity: 1;
      transform: translateY(0);
    }
    .control-box .scroll-btn:hover {
      background-color: var(--uc-background-color-100);
    }
    .control-box .scroll-btn:active {
      background-color: var(--uc-background-color-200);
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
