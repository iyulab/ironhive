import { LitElement, css, html } from "lit";
import { customElement, property } from "lit/decorators.js";

import DOMPurify from "dompurify";

@customElement('text-block')
export class TextBlock extends LitElement {

  @property({ type: String })
  content: string = '';

  render() {
    return html`
      <pre class="text">${DOMPurify.sanitize(this.content)}</pre>
    `;
  }

  static styles = css`
    :host {
      display: block;
      width: 100%;
    }

    .text {
      margin: 0;
      padding: 0;
      font-size: 14px;
      font-style: inherit;
      line-height: 1.5;
    }
  `;
}
