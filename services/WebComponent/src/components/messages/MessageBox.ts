import { LitElement, html, css, nothing, PropertyValues } from 'lit';
import { customElement, property, query } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import type { Message } from '../../models';

@customElement('message-box')
export class MessageBox extends LitElement {
  private observer: ResizeObserver = new ResizeObserver(this.updateFillHeight.bind(this));
  private updateScrollButtonBound = this.updateScrollButton.bind(this);
  private lastCount: number = 0;

  @query('.scroll.top') scrollTopBtn!: HTMLDivElement;
  @query('.scroll.bottom') scrollBottomBtn!: HTMLDivElement;

  @property({ type: Array }) messages: Message[] = [];

  connectedCallback(): void {
    super.connectedCallback();
    this.addEventListener('scroll', this.updateScrollButtonBound);
    this.observer.observe(this);
    this.updateFillHeight();
  }

  disconnectedCallback(): void {
    super.disconnectedCallback();
    this.removeEventListener('scroll', this.updateScrollButtonBound);
    this.observer.disconnect();
  }

  protected async updated(changedProperties: PropertyValues) {
    super.updated(changedProperties);
    await this.updateComplete;

    if (changedProperties.has('messages')) {
      // 새로운 메시지가 추가되었을 때 스크롤을 하단으로 이동합니다.
      if(this.lastCount === this.messages.length) return;
      this.lastCount = this.messages.length;
      await this.scrollToBottom();
    }
  }

  render() {
    return html`
      <chat-icon class="scroll top hidden"
        name="arrow-up"
        @click=${this.scrollToTop}
      ></chat-icon>

      <div class="container">
        ${repeat(this.messages, (msg) => msg.timestamp, (msg) => msg.role === 'user'
          ? html`
              <user-message class="message user"
                .timestamp=${msg.timestamp}>
                ${msg.content?.map(content => content.type === 'text' 
                  ? html`<text-block .value=${content.value}></text-block>` 
                  : nothing)}
              </user-message>
            `
          : html`
              <bot-message class="message bot"
                .name=${msg.name}
                .avatar=${'/assets/images/user-avatar.png'}
                .timestamp=${msg.timestamp}>
                ${msg.content?.map(content => content.type === 'text'
                  ? html`<marked-block .value=${content.value}></marked-block>`
                  : content.type === 'tool'
                  ? html`<tool-block .value=${content}></tool-block>`
                  : nothing)}
              </bot-message>
          `)}
      </div>

      <chat-icon class="scroll bottom hidden"
        name="arrow-down"
        @click=${this.scrollToBottom}
      ></chat-icon>
    `;
  }

  // 마지막 메시지의 높이를 계산하여 업데이트 합니다.
  private async updateFillHeight() {
    const rect = this.getBoundingClientRect();
    this.style.setProperty('--fill-height', `${rect.height}px`);
  }

  // 스크롤 버튼의 가시성을 업데이트 합니다.
  private async updateScrollButton() {
    const sensitivity = 64;
    const scrollTop = this.scrollTop;
    const scrollHeight = this.scrollHeight;
    const clientHeight = this.clientHeight;

    // 상단 스크롤 버튼의 가시성 조정
    if (scrollTop > sensitivity) {
      this.scrollTopBtn.classList.remove('hidden');
    } else {
      this.scrollTopBtn.classList.add('hidden');
    }

    // 하단 스크롤 버튼의 가시성 조정
    if ((scrollTop + clientHeight) >= (scrollHeight - sensitivity)) {
      this.scrollBottomBtn.classList.add('hidden');
    } else {
      this.scrollBottomBtn.classList.remove('hidden');
    }
  }

  // 스크롤을 상단으로 이동합니다.
  private async scrollToTop() {
    this.scrollTo({ top: 0, behavior: 'smooth' });
  }
  
  // 스크롤을 하단으로 이동합니다.
  private async scrollToBottom() {
    this.scrollTo({ top: this.scrollHeight, behavior: 'smooth' });
  }

  static styles = css`
    :host {
      position: relative;
      width: 100%;
      height: 100%;
      overflow-y: auto;

      --scroll-btn-size: 32px;
      --box-vertical-padding: 32px;
      --box-horizontal-padding: 64px;
      --message-gap: 16px;
      --fill-height: 100%;
    }

    .container {
      width: 100%;
      height: auto;
      display: flex;
      flex-direction: column;
      gap: var(--message-gap);
      padding: var(--box-vertical-padding) var(--box-horizontal-padding);
      box-sizing: border-box;
    }
    .container > :last-child {
      min-height: calc(var(--fill-height) - var(--box-vertical-padding) - var(--message-gap) - var(--scroll-btn-size) - 10px);
    }

    .message {
      height: auto;
    }
    .message.user {
      width: auto;
      align-self: flex-end;
    }
    .message.bot {
      width: 100%;
      align-self: flex-start;
    }

    .scroll {
      position: sticky;
      z-index: 100;
      left: 50%;
      width: var(--scroll-btn-size);
      height: var(--scroll-btn-size);
      display: flex;
      justify-content: center;
      align-items: center;
      opacity: 0.5;
      color: var(--hs-text-color);
      background-color: var(--hs-background-color);
      border: 1px solid var(--hs-border-color);
      border-radius: 50%;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
      box-sizing: border-box;
      transition: opacity 0.3s ease;
      cursor: pointer;
    }
    .scroll.top {
      top: 10px;
    }
    .scroll.bottom {
      bottom: 10px;
    }
    .scroll.hidden {
      opacity: 0;
    }
    .scroll:hover {
      opacity: 1;
    }
    .scroll:active {
      transform: scale(0.9);
    }
  `;
}
