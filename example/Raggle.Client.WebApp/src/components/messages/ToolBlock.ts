import { LitElement, css, html } from "lit";
import { customElement, property } from "lit/decorators.js";

@customElement('tool-block')
export class ToolBlock extends LitElement {

  @property({ type: String }) name: string = '';
  @property({ type: Object }) result: any;

  render() {
    return html`
      <div class="name">
        Tool: ${this.name}
      </div>
      <div class="result">
        ${this.result ? html`
          <pre>${JSON.stringify(this.result, null, 2)}</pre>
        ` : html`
          <sl-skeleton></sl-skeleton>
        `}
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
    }
  `;
}
