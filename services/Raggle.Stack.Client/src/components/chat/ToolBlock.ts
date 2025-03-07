import { LitElement, css, html } from "lit";
import { customElement, property } from "lit/decorators.js";

@customElement('tool-block')
export class ToolBlock extends LitElement {

  @property({ type: String }) name: string = '';
  @property({ type: Object }) result: any;

  render() {
    console.log(this.result);
    return html`
      <div class="name">
        Tool: ${this.name}
      </div>
      <div class="result">
      ${this.result
        ? html`  
        ${this.result.result.map((item: any) => html`
          <div class="question">${item.payload.content.question}</div>
          <div class="answer">${item.payload.content.answer}</div>
        `)}`
        : html`
          <sl-spinner></sl-spinner>
        `
      }
      </div>
    `;
  }

  static styles = css`
    :host {
      display: flex;
      flex-direction: column;
      width: 100%;
      background-color: #f0f0f0;
      border-radius: 8px;
      padding: 8px;
      box-sizing: border-box;
    }

    .name {
      width: 100%;
      font-weight: 600;
      font-size: 18px;
    }

    .result {
      width: 100%;
      margin-top: 8px;
      display: flex;
      flex-direction: column;
      gap: 8px;

      .question {
        font-weight: 600;
        font-size: 16px;
      }

      .answer {
        font-size: 14px;
      }

      .question, .answer {
        padding: 8px;
        border-radius: 8px;
        background-color: #fff;
      }

      .question {
        background-color: #f0f0f0;
      }
    }
  `;
}
