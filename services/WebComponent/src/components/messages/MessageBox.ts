import { LitElement, html, css, nothing, PropertyValues } from 'lit';
import { customElement, property, query } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import type { Message } from '../../models';

@customElement('message-box')
export class MessageBox extends LitElement {

  @query('.container') containerEl!: HTMLDivElement;  
  @property({ type: Array }) messages: Message[] = [];

  protected async updated(changedProperties: PropertyValues) {
    super.updated(changedProperties);
    if (changedProperties.has('messages')) {
      this.changedMessage();
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
      </div>
    `;
  }
  
  private async changedMessage() {
    // 스크롤을 계속 하단으로 내리기
    requestAnimationFrame(() => {
      this.scrollTo({
        top: this.scrollHeight,
        behavior: 'smooth'
      })
    });
    console.log('scrolled');
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
    }
  `;
}
