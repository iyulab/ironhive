import { LitElement, html, css, nothing, PropertyValues } from 'lit';
import { customElement, property, query } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { Message } from '../../models';

@customElement('message-list')
export class MessageList extends LitElement {

  @query('.container') container!: HTMLElement;
  @query('.padding') padding!: HTMLElement;

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
  
  private focusOnNewMessage() {
    // 패딩 요소 직전의 마지막 메시지로 스크롤
    this.padding.style.height = '800px';
    const lastMessage = this.container.querySelector('user-message:last-of-type, bot-message:last-of-type');
    
    if (lastMessage) {
      lastMessage.scrollIntoView({ 
        behavior: 'smooth', 
        block: 'center',
      });
    }
  }

  static styles = css`
    :host {
      width: 100%;
      height: 100%;
      display: block;
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
        height: 100%;
        display: block;
        visibility: hidden;
        pointer-events: none;
      }
    }
  `;
}
