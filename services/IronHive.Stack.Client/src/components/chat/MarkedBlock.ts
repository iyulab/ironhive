import { LitElement, css, html, nothing } from "lit";
import { customElement, property } from "lit/decorators.js";
import { unsafeHTML } from "lit/directives/unsafe-html.js";
import { until } from 'lit/directives/until.js';

import DOMPurify from "dompurify";
import { marked } from "marked";

@customElement('marked-block')
export class MarkedBlock extends LitElement {

  @property({ type: String })
  value?: string;

  render() {
    if (!this.value) return nothing;

    return until(
      this.parse(this.value),
      html`<div>...</div>`);
  }

  private parse = async (value: string) => {
    value = value.replace(/^[\u200B\u200C\u200D\u200E\u200F\uFEFF]/,"");
    value = await marked.parse(value, {
      async: true,
      gfm: true,
    });
    value = DOMPurify.sanitize(value);
    return unsafeHTML(value);
  }

  static styles = css`
    :host {
      display: block;
      width: 100%;
    }
  `;
}
