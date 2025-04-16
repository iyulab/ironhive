import { LitElement, css, html, nothing } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import type { ToolContent } from "../models";

@customElement('tool-block')
export class ToolBlock extends LitElement {

  @property({ type: Object }) value?: ToolContent;
  @state() collapsed = true;

  render() {
    console.log('ToolBlock.render', this.value);
    if (!this.value) return nothing;
    
    return html`
      <div class="container">
        <div class="header" @click=${() => this.collapsed = !this.collapsed}>
          <hive-icon
            ?rotate=${this.value.status === 'running' || this.value.status === 'pending'}
            name=${this.value.status === 'pending' ? 'sandglass'
              : this.value.status === 'running' ? 'spinner'
              : this.value.status === 'completed' ? 'check'
              : this.value.status === 'failed' ? 'error'
              : 'question'}
          ></hive-icon>
          <div class="name">
            ${this.value.name}
          </div>
          <hive-icon
            name=${this.collapsed ? 'expand' : 'collapse'}
          ></hive-icon>
        </div>
        <div class="body ${this.collapsed ? 'collapsed' : ''}">
          <marked-block
            .value=${`\`\`\`json\n${JSON.stringify(this.value, null, 2)}\n\`\`\``}
          ></marked-block>
        </div>
      </div>
    `;
  }

  static styles = css`
    .container {
      border: 1px solid var(--hs-border-color);
      border-radius: 4px;
      margin: 8px 0px;
      box-sizing: border-box;
    }

    .header {
      display: flex;
      flex-direction: row;
      align-items: center;
      padding: 4px;
      box-sizing: border-box;
      gap: 8px;
      justify-content: space-between;
      cursor: pointer;

      hive-icon {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 24px;
        height: 24px;
      }
      hive-icon[rotate] {
        animation: rotate 1.5s linear infinite;
      }

      .name {
        flex: 1;
        font-weight: 600;
        line-height: 24px;
      }
    }

    .body {
      width: 100%;
      max-height: 340px;
      transition: max-height 0.3s ease-out;
      overflow: auto;
    }
    .body.collapsed {
      max-height: 0;
      overflow: hidden;
    }
    
    @keyframes rotate {
      0% {
        transform: rotate(0deg);
      }
      66.67% { /* 약 1초 동안 360도 회전 */
        transform: rotate(180deg);
      }
      100% { /* 0.5초 동안 정지 */
        transform: rotate(180deg);
      }
    }
  `;
}
