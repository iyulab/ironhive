import { LitElement, css, html, nothing } from "lit";
import { customElement, property } from "lit/decorators.js";

import DOMPurify from "dompurify";

@customElement('text-block')
export class TextBlock extends LitElement {

  @property({ type: String })
  value?: string;

  render() {
    if (!this.value) return nothing;
    return html`${DOMPurify.sanitize(this.value)}`;
  }

  static styles = css`
    :host {
      display: block;
      width: 100%;

      white-space: pre-wrap;
      word-wrap: break-word;

      font-size: 14px;
      line-height: 1.5;
    }
  `;
}
