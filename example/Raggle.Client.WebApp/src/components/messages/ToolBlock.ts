import { LitElement, css, html } from "lit";
import { customElement, property } from "lit/decorators.js";

@customElement('tool-block')
export class ToolBlock extends LitElement {

  @property({ type: String })
  name: string = '';

  @property({ type: Object })
  result: any;

  render() {
    return html`
      <div class="tool">
        <div class="name">Tool: ${this.name}</div>
        <div class="result">Result: ${this.result}</div>
      </div>
    `;
  }

  static styles = css`
    :host {
      display: block;
      width: 100%;
    }

    .tool {
      background-color: #f0f0f0;
      border-radius: 8px;
      padding: 8px;
      box-sizing: border-box;
    }

    .name {
      font-weight: bold;
      margin-bottom: 4px;
    }

    .result {
      color: #555;
    }
  `;
}
