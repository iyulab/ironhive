import { LitElement, css, html } from "lit";
import { customElement, property } from "lit/decorators.js";
import { unsafeHTML } from "lit/directives/unsafe-html.js";
import { until } from 'lit/directives/until.js';

import DOMPurify from "dompurify";
import { marked } from "marked";

@customElement('markdown-block')
export class MarkdownBlock extends LitElement {

  @property({ type: String })
  content: string = '';

  render() {
    return until(this.parse(this.content),
      html`<sl-skeleton></sl-skeleton>`);
  }

  private parse = async (content: string) => {
    content = content.replace(/^[\u200B\u200C\u200D\u200E\u200F\uFEFF]/,"");
    content = await marked.parse(content, {
      async: true,
      gfm: true,
    });
    content = DOMPurify.sanitize(content);
    return unsafeHTML(content);
  }

  static styles = css`
    :host {
      display: block;
      width: 100%;
    }
  `;
}