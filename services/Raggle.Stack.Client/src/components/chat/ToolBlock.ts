import { LitElement, css, html, nothing } from "lit";
import { customElement, property } from "lit/decorators.js";
import type { ToolContent } from "../../models";

@customElement('tool-block')
export class ToolBlock extends LitElement {

  @property({ type: Object }) 
  value?: ToolContent;

  render() {
    if (!this.value) return nothing;
    
    return html`
      <div class="container">
        <h2>${this.value.name ?? 'Unknown tool'}</h2>
        ${this.value.arguments ? 
          html`<p><strong>Arguments:</strong> ${this.value.arguments}</p>` 
          : nothing}
        ${this.value.result ? 
          html`<pre><strong>Result:</strong>${JSON.stringify(this.value.result, null, 2)}</pre>` 
          : nothing}
      </div>
    `;
  }

  static styles = css`
    .container {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    h2 {
      margin: 0;
      font-size: 1.2rem;
      color: #333;
    }

    p {
      margin: 0;
      color: #555;
    }

    pre {
      background-color: #eee;
      padding: 0.5rem;
      border-radius: 4px;
      overflow: auto;
      margin: 0;
      white-space: pre-wrap;
    }
  `;
}
