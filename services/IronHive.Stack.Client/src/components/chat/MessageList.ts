import { LitElement, html, css, nothing, PropertyValues } from 'lit';
import { customElement, property, query, queryAll } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { Message } from '../../models';

@customElement('message-list')
export class MessageList extends LitElement {

  @queryAll('bot-message') botMsgEl!: NodeListOf<HTMLElement>;
  @query('.padding') padEl!: HTMLDivElement;
  @property({ type: Array }) messages: Message[] = [];

  protected async updated(changedProperties: PropertyValues) {
    super.updated(changedProperties);
    if (changedProperties.has('messages')) {
      this.focusOnNewMessage();
    }
  }

  render() {
    return html`
      <div class="container">
        ${repeat(this.messages, (msg) => msg.timestamp, (msg) => msg.role === 'user'
          ? html`
              <user-message .timestamp=${msg.timestamp}>
                ${msg.content?.map(content => content.type === 'text' 
                  ? html`<text-block .value=${content.value}></text-block>` 
                  : nothing)}
              </user-message>
            `
          : html`
              <bot-message
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
        <div class="padding"></div>
      </div>
    `;
  }
  
  private async focusOnNewMessage() {
    // 1. 컴포넌트 업데이트
    await this.updateComplete;

    // 2. 마지막 메시지 끝으로 스크롤
    if (this.botMsgEl.length > 0) {
      this.padEl.style.height = `600px`;
      await this.updateComplete;
      const last = this.botMsgEl[this.botMsgEl.length - 1];
      last.scrollIntoView({ 
        behavior: 'smooth', 
        block: 'start',
        inline: 'start' 
      });
      await this.updateComplete;
      this.padEl.style.height = '0px';
    }
  }

  static styles = css`
    :host {
      width: 100%;
      height: 100%;
      overflow-y: auto;
    }

    .container {
      width: 100%;
      display: flex;
      flex-direction: column;
      gap: 16px;
      padding: 32px;
      box-sizing: border-box;

      user-message {
        width: auto;
        height: auto;
        align-self: flex-end;
      }

      bot-message {
        width: 100%;
        height: auto;
        align-self: flex-start;
      }

      .padding {
        display: block;
        visibility: hidden;
        pointer-events: none;
      }
    }
  `;
}
