import { LitElement, css, html, nothing } from "lit";
import { customElement, query, queryAll, state } from "lit/decorators.js";
import { Api } from "../services";
import { AIServiceModels, Message } from "../models";
import { AssistantMessage, UserMessage } from "../components";

@customElement('guest-page')
export class GuestPage extends LitElement {
  private readonly models: AIServiceModels = {
    "openai": ["o3-mini", "gpt-4o-mini", "gpt-4o"],
    "anthropic": ["claude-3-5-sonnet-latest", "claude-3-5-haiku-latest", "claude-3-opus-latest"],
    "ollama": []
  };

  @query('.messages') messagesEl!: HTMLDivElement;
  @queryAll('user-message') userMsgs!: NodeListOf<UserMessage>;
  @queryAll('assistant-message') assistantMsgs!: NodeListOf<AssistantMessage>;

  @state() service: string = 'openai';
  @state() model: string = 'gpt-4o-mini';
  @state() messages: Message[] = [];

  render() {
    return html`
      <div class="header">
        <sl-select
          size="small"
          value=${`${this.service}/${this.model}`}
          .hoist=${true}
          @sl-change=${this.onChange}>
          ${Object.entries(this.models).map(([k,v]) => html`
            <small>${k.toUpperCase()}</small>
            ${v.map(m => html`
              <sl-option value=${`${k}/${m}`}>${m}</sl-option>
            `)}
          `)}
        </sl-select>
      </div>
      <div class="container">
        <div class="messages">
          ${this.messages.map(msg => {
            if (msg.role === 'user') {
              return html`
                <user-message
                  .message=${msg}
                ></user-message>
              `;
            } else if (msg.role === 'assistant') {
              return html`
                <assistant-message
                  .message=${msg}
                ></assistant-message>
              `;
            } else {
              return nothing;
            }
          })}
        </div>
        <div class="input">
          <message-input
            placeholder="Type a message..."
            rows="1"
            @send=${this.onSend}
          ></message-input>
        </div>
      </div>
    `;
  }

  private onSend = async (event: CustomEvent<string>) => {
    const value = event.detail;
    if (value) {
      this.messages = [ 
        ...this.messages, 
        { role: 'user', content: [
          { type: 'text', index: 0,  text: value }
        ]},
        { role: 'assistant', content: [] }
      ];
      await this.updateComplete;
      this.messagesEl.scrollTop = this.messagesEl.scrollHeight;
      
      await Api.Guest.message(
        this.service,
        this.model,
        this.messages, 
        async (data) => {
          if (!data.content) return;
          this.assistantMsgs[this.assistantMsgs.length - 1].appendContent(data.content);
          this.messagesEl.scrollTop = this.messagesEl.scrollHeight;
        });
    }
  }

  private onChange = (event: Event) => {
    const [service, model] = (event.target! as HTMLInputElement).value.split('/');
    this.service = service;
    this.model = model;
  }

  static styles = css`
    :host {
      display: block;
      width: 100vw;
      height: 100vh;
      overflow: hidden;

      --header-height: 48px;
    }

    .header {
      position: relative;
      display: flex;
      align-items: center;
      justify-content: center;
      width: 100%;
      height: var(--header-height);
      padding: 16px;
      box-sizing: border-box;
    }

    sl-option::part(label) {
      font-size: var(--sl-font-size-small);
    }

    .container {
      position: relative;
      display: flex;
      flex-direction: column;
      width: 100%;
      height: calc(100% - var(--header-height));
      padding: 16px;
      box-sizing: border-box;
    }

    .messages {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 16px;
      overflow-y: auto;
      padding: 32px 16px;
      box-sizing: border-box;
    }

    .input {
      width: 100%;
    }
  `;
}