import { LitElement, css, html } from "lit";
import { customElement, property } from "lit/decorators.js";

@customElement('main-layout')
export class MainLayout extends LitElement {

  @property({ type: String }) ratio: string = '1:2:2';

  protected updated(changedProperties: Map<string, any>) {
    if (changedProperties.has('ratio')) {
      this.parseRatio();
    }
  }

  render() {
    return html`
      <div class="left-panel">
        <slot name="left"></slot>
      </div>
      <div class="main-panel">
        <slot name="main"></slot>
      </div>
      <div class="right-panel">
        <slot name="right"></slot>
      </div>
    `;
  }

  private parseRatio() {
    const parts = this.ratio.split(':').map(part => parseInt(part.trim(), 10));
    if (parts.length === 3 && parts.every(num => !isNaN(num) && num > 0)) {
      const gridColumns = `${parts[0]}fr ${parts[1]}fr ${parts[2]}fr`;
      this.style.setProperty('--grid-template-columns', gridColumns);
    } else {
      console.warn(`Invalid ratio "${this.ratio}". Using default "1fr 2fr 2fr".`);
    }
  }

  static styles = css`
    :host {
      display: grid;
      --grid-template-columns: 1fr 2fr 2fr;
      grid-template-columns: var(--grid-template-columns, 1fr 2fr 2fr);
      grid-template-rows: 100%;
      width: 100%;
      height: 100%;
      border: 1px solid red;
      box-sizing: border-box;
    }

    .left-panel {
      border: 1px solid blue;
      box-sizing: border-box;
    }

    .main-panel {
      border: 1px solid green;
      box-sizing: border-box;
    }

    .right-panel {
      border: 1px solid yellow;
      box-sizing: border-box;
    }
  `;
}
