import { LitElement, css, html } from "lit";
import { customElement, property } from "lit/decorators.js";
import { ifDefined } from 'lit/directives/if-defined.js';

import { send } from "../IconData";
import { SendMessageEvent } from "../../events";

@customElement('message-input')
export class MessageInput extends LitElement {

  @property({ type: String }) placeholder?: string;
  @property({ type: Number }) rows?: number;
  @property({ type: Number }) maxlength?: number;
  @property({ type: Boolean }) disabled = true;
  @property({ type: String }) value = '';

  render() {
    return html`
      <div class="container">
        <!-- Input -->
        <div class="input-area">
          <textarea
            spellcheck="false"
            placeholder=${ifDefined(this.placeholder)}
            rows=${ifDefined(this.rows)}
            maxlength=${ifDefined(this.maxlength)}
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
          <div class="send-button"
            ?disabled=${this.disabled}
            @click=${this.invokeSendEvent}>
            <hive-icon 
              .data=${send}
            ></hive-icon>
          </div>
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
    this.dispatchEvent(new SendMessageEvent(value));
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
      border: 1px solid hsl(240 5.9% 90%);
      border-radius: 4px;
      box-sizing: border-box;
    }

    .input-area {
      position: relative;
      box-sizing: border-box;
      max-height: 200px;

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
        background-color: transparent;
        font-size: inherit;
        line-height: inherit;
        font-family: inherit;
        overflow: auto;
      }

      .filler {
        visibility: hidden;
        pointer-events: none;
        min-height: 56px;
        font-size: inherit;
        line-height: inherit;
        word-break: break-word;
        white-space: pre-wrap;
        display: block;
        overflow: auto;
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

      .send-button {
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: 4px;
        padding: 4px;
        box-sizing: border-box;
        background-color: hsl(240 5.9% 90%);
        width: 40px;
        height: 30px;
        cursor: pointer;
      }

      .send-button:active {
        background-color: hsl(240 5.9% 80%);
      }
      .send-button[disabled] {
        background-color: hsl(240 5.9% 95%);
        cursor: not-allowed;
      }
      .send-button[disabled]:active {
        background-color: hsl(240 5.9% 95%);
      }
    }
  `;
}