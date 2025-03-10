import { LitElement, css, html } from "lit";
import { customElement } from "lit/decorators.js";

import "@iyulab/hive-stack/components/chat";

@customElement('test-page')
export class TestPage extends LitElement {

  render() {
    return html`
      <div class="flexible-box">
        <chat-room></chat-room>
      </div>
    `;
  }

  static styles = css`
    :host {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 100%;
      height: 100%;
    }

    .flexible-box {
      display: block;
      width: 400px;
      height: 90%;
      border: 1px solid var(--sl-color-red-500);
      resize: both;
      overflow: auto;
      box-sizing: border-box;
    }
  `;
}
