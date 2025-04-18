import { LitElement, css, html } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { ifDefined } from 'lit/directives/if-defined.js';

import { SubmitMessageEvent } from "../../models";

@customElement('message-input')
export class MessageInput extends LitElement {

  @state() disabled: boolean = true;

  @property({ type: String, reflect: true }) mode: 'input' | 'control' = 'input';
  @property({ type: String, reflect: true }) placeholder?: string;
  @property({ type: Number, reflect: true }) rows?: number;
  @property({ type: String, reflect: true }) value = '';

  render() {
    return html`
      <div class="container">
        <!-- Input -->
        <div class="input-area">
          <textarea
            spellcheck="false"
            placeholder=${ifDefined(this.placeholder)}
            rows=${ifDefined(this.rows)}
            .value=${this.value}
            @input=${this.handleInput}
            @keydown=${this.handleKeydown}
          ></textarea>
          <div class="filler">${this.value}</div>
        </div>

        <!-- Button Control -->
        <div class="control-area">
          <slot name="control"></slot>
          <div class="flex"></div>
          
          <chat-button
            ?disabled=${this.disabled}
            @click=${this.invokeSendEvent}>
            <hive-icon 
              name=${this.mode === 'input' ? 'send' : 'stop'}
            ></hive-icon>
          </chat-button>
        </div>
      </div>
    `;
  }

  private handleInput = (event: InputEvent) => {
    const target = event.target as HTMLTextAreaElement;
    this.value = target.value;
    this.disabled = !target.value.trim();
  }

  private handleKeydown = (event: KeyboardEvent) => {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.invokeSendEvent();
    }
  }

  private invokeSendEvent = () => {
    const value = this.value.trim();
    if (!value) return;
    this.dispatchEvent(new SubmitMessageEvent(value));
    this.value = '';
    this.disabled = !this.value.trim();
  }

  static styles = css`
    :host {
      display: block;
      font-size: 16px;
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      line-height: 28px;
    }

    .container {
      display: flex;
      flex-direction: column;
      gap: 4px;
      padding: 8px 16px;
      border: 1px solid var(--hs-border-color);
      border-radius: 4px;
      box-sizing: border-box;
    }

    .input-area {
      position: relative;
      box-sizing: border-box;
      max-height: 240px;

      textarea {
        position: absolute;
        width: 100%;
        top: 0;
        left: 0;
        bottom: 0;
        right: 0;
        padding: 0;
        margin: 0;
        border: none;
        resize: none;
        outline: none;
        color: currentColor;
        background-color: transparent;
        font-size: inherit;
        line-height: inherit;
        font-family: inherit;
        overflow: auto;
      }

      .filler {
        min-height: 56px;
        display: block;
        visibility: hidden;
        pointer-events: none;
        font-size: inherit;
        line-height: inherit;
        word-break: break-word;
        white-space: pre-wrap;
      }
    }

    .control-area {
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