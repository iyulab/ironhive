import { LitElement, css, nothing } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { unsafeHTML } from "lit/directives/unsafe-html.js";
import { icons } from "./IconLibrary";

@customElement('chat-icon')
export class ChatIcon extends LitElement {
  
  @state() data?: string;
  @property({ type: String }) name?: string;

  protected async updated(_changedProperties: any) {
    super.updated(_changedProperties);
    await this.updateComplete;

    if (_changedProperties.has('name') && this.name) {
      this.data = icons.get(this.name);
    }
  }

  render() {
    const data = this.data?.trim();
    return data?.startsWith('<svg') 
      ? unsafeHTML(data)
      : nothing;
  }
  
  static styles = css`
    :host {
      display: inline-flex;
      font-size: 16px;
      color: inherit;
    }

    svg {
      width: 1em;
      height: 1em;
      fill: currentColor;
    }
  `;
}
