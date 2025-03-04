import { LitElement, css, html } from "lit";
import { customElement, property } from "lit/decorators.js";
import { ifDefined } from 'lit/directives/if-defined.js';

@customElement('message-input')
export class MessageInput extends LitElement {

  @property({ type: String }) placeholder = '';
  @property({ type: Number }) rows = 1;
  @property({ type: Number }) maxlength?: number;
  @property({ type: Boolean }) disabled = false;
  @property({ type: String }) value = '';

  render() {
    return html`
      <div class="container">
        <!-- Input -->
        <div class="input">
          <textarea
            spellcheck="false"
            placeholder=${this.placeholder}
            rows=${this.rows}
            maxlength=${ifDefined(this.maxlength)}
            .value=${this.value}
            @input=${this.onInput}
            @keydown=${this.onKeydown}
          ></textarea>
        </div>

        <!-- Button Control -->
        <div class="control">
          <sl-icon-button
            name="paperclip"
          ></sl-icon-button>
          <div class="flex"></div>
          <sl-button
            size="small"
            variant="primary"
            ?circle=${true}
            ?disabled=${this.disabled || !this.value.trim()}
            @click=${this.onSend}>
            <sl-icon name="send"></sl-icon>
          </sl-button>
        </div>

      </div>
    `;
  }

  private onKeydown = (event: KeyboardEvent) => {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.onSend();
    }
  }

  private onInput = (event: Event) => {
    const textarea = event.target as HTMLTextAreaElement;
    textarea.style.height = 'auto';
    textarea.style.height = `${textarea.scrollHeight}px`;
    this.value = textarea.value;

    this.dispatchEvent(new CustomEvent('input', {
      detail: this.value,
    }));
  }

  private onSend = () => {
    const value = this.value.trim();
    if (!value) return;
    this.dispatchEvent(new CustomEvent('send', {
      detail: value,
    }));
    this.value = '';
  }

  static styles = css`
    :host {
      display: block;
      width: 100%;
    }

    .container {
      display: flex;
      flex-direction: column;
      gap: 4px;
      padding: 8px;
      background-color: var(--sl-panel-background-color);
      border: 1px solid var(--sl-panel-border-color);
      border-radius: 4px;
      box-sizing: border-box;
      overflow: hidden;
    }

    .input {
      display: flex;
      padding: 8px;
      box-sizing: border-box;
      max-height: 200px;
      overflow-y: auto;

      textarea {
        width: 100%;
        height: auto;
        border: none;
        resize: none;
        outline: none;
        background-color: transparent;
        font-size: 14px;
        font-family: inherit;
        overflow: hidden;
      }
    }

    .control {
      grid-area: control;
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
      gap: 8px;

      .flex {
        flex: 1;
      }
    }
  `;
}