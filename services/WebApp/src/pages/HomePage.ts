import { LitElement, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";

@customElement('home-page')
export class HomePage extends LitElement {
  
  @state() open: boolean = false;
  @state() loading: boolean = false;
  @state() disabled: boolean = false;

  connectedCallback(): void {
    super.connectedCallback();
  }

  render() {

    return html` 
      <div class="container">
        <uc-button 
          ?loading=${this.loading} 
          ?disabled=${this.disabled} 
          @click=${this.clicked}>
          Yama
        </uc-button>
        <uc-copy-button></uc-copy-button>
      </div>
    `;
  }

  private clicked = async (e: any) => {
    this.loading = !this.loading;
    this.open = !this.open;
  }

  static styles = css`
    :host {
      width: 100%;
      height: 100%;
    }

    .container {
      position: relative;
      display: flex;
      width: 50%;
      height: 50%;
      align-items: center;
      justify-content: center;
      border: 1px dashed red;
      box-sizing: border-box;
    }

    .container > div {
      overflow-wrap: anywhere;
    }
  `;
}