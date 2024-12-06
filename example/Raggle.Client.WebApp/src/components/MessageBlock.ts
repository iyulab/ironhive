import { LitElement, css, html, nothing } from "lit";
import { customElement, property } from "lit/decorators.js";
import { ifDefined } from "lit/directives/if-defined.js";
import { until } from "lit/directives/until.js";
import { unsafeHTML } from "lit/directives/unsafe-html.js";

import DOMPurify from "dompurify";
import { marked } from "marked";

import type { Message, MessageContent } from "../models";

@customElement('message-block')
export class MessageBlock extends LitElement {

  @property({ type: Object })
  message?: Message;

  render() {
    if (!this.message?.contents) return nothing;

    return html`
      <div class="container">
        <div class="avatar">
          ${this.message.role}:
        </div>
        <div class="content">
          ${this.message.contents?.map(content => html`
              ${until(this.renderContent(content), "Loading...")}
          `)}
        </div>
      </div>
    `;
  }

  private async parse(text: string) {
    text = text.replace(/^[\u200B\u200C\u200D\u200E\u200F\uFEFF]/,"");
    text = await marked.parse(text, {
      async: true,
      gfm: true,
    });
    text = DOMPurify.sanitize(text);
    return text;
  }

  private async renderContent(content: MessageContent) {
    if (content.type === 'text') {
      return unsafeHTML(`${await this.parse(content.text || '')}`);
    } else if (content.type === 'image') {
      return html`<img src="${ifDefined(content.url)}" alt="Image content" />`;
    } else if (content.type === 'tool') {
      return html`<span>Tool: ${content.name}</span>`;
    } else {
      return html`<span>Unsupported content type</span>`;
    }
  }

  static styles = css`
    :host {
      width: 100%;
      display: block;
    }

    .container {
      display: flex;
      flex-direction: row;
      padding: 8px;
      box-sizing: border-box;
    }

    .avatar {
      font-weight: bold;
      margin-right: 8px;
    }

    .content {
      display: flex;
      flex-direction: column;
      background-color: #f9f9f9;
      border-radius: 8px;
      padding: 8px;
      box-sizing: border-box;
    }
  `;
}