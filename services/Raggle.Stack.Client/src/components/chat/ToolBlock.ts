import { LitElement, css, html, nothing } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import type { ToolContent } from "../../models";

@customElement('tool-block')
export class ToolBlock extends LitElement {

  @property({ type: Object }) value?: ToolContent;

  @state() status: 'success' | 'failed' | 'process' = 'process';
  @state() collapsed = true;

  render() {
    if (!this.value) return nothing;
    
    return html`
      <div class="container">
        <div class="header">
          <div class="status">
            ${this.status}
          </div>
          <div class="name">
            ${this.value.name}
          </div>
          <div class="collapse-or-expand-button"
            @click=${this.toggle}>
            ${this.collapsed ? '펼치기' : '접기'}
          </div>
        </div>
        <div class="body ${this.collapsed ? 'collapsed' : ''}">
          <marked-block
            .value=${`\`\`\`json\n${JSON.stringify(this.value, null, 2)}\n\`\`\``}
          ></marked-block>
        </div>
      </div>
    `;
  }

  private toggle() {
    this.collapsed = !this.collapsed
  }

  static styles = css`
    .container {
      border: 1px solid var(--hs-border-color);
      border-radius: 4px;
      padding: 4px;
      box-sizing: border-box;
    }

    .header {
      display: flex;
      flex-direction: row;
      align-items: center;
      gap: 8px;
      justify-content: space-between;
    }

    .status {
      width: 30px;
    }

    .name {
      flex: 1;
      font-weight: 600;
    }

    .collapse-or-expand-button {
      cursor: pointer;
    }

    .body {
      max-height: 1000px;
      transition: max-height 0.3s ease-out;
    }
    .body.collapsed {
      max-height: 0;
      overflow: hidden;
    }

  `;
}
