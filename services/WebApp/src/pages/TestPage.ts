import { LitElement, PropertyValues, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";

@customElement('test-page')
export class TestPage extends LitElement {
  
  @state() open: boolean = false;
  @state() loading: boolean = false;
  @state() disabled: boolean = false;

  connectedCallback(): void {
    super.connectedCallback();
  }

  updated(changedProperties: PropertyValues): void {
    super.updated(changedProperties);
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
        <uc-alert status="info" ?open=${this.open} timeout="5000">
          요고 요고 요고 요고 요고 요고 요고 요고 요고 요고 요고
          요고 요고 요고 요고 요고 요고 요고 요고 요고 요고 요고
          요고 요고 요고 요고 요고 요고 요고 요고 요고 요고 요고
          요고 요고 요고 요고 요고 요고 요고 요고 요고 요고 요고
          요고 요고 요고 요고 요고 요고 요고 요고 요고 요고 요고
          요고 요고 요고 요고 요고 요고 요고 요고 요고 요고 요고
        </uc-alert>
        <uc-dot-bounce-loader></uc-dot-bounce-loader>
        <uc-dot-rotate-loader></uc-dot-rotate-loader>
        <uc-bar-bounce-loader></uc-bar-bounce-loader>
        <uc-bar-rotate-loader></uc-bar-rotate-loader>
        <uc-ring-stretch-loader></uc-ring-stretch-loader>
        <uc-ring-rotate-loader></uc-ring-rotate-loader>
        <uc-pulse-loader></uc-pulse-loader>
      </div>
    `;
  }

  private clicked = async () => {
    if (this.loading) return;
    
    this.loading = true;
    this.open = true;
    await new Promise(resolve => setTimeout(resolve, 1000));
    this.loading = false;
  }

  static styles = css`
    :host {
      display: flex;
      justify-content: center;
      align-items: center;
      width: 100%;
      height: 100%;
    }

    .container {
      position: relative;
      display: flex;
      width: 60%;
      height: 80%;
      align-items: center;
      justify-content: center;
      border: 1px dashed gray;
      box-sizing: border-box;
      overflow: auto;
      resize: both;
    }

    uc-alert {
      position: absolute;
      top: 0;
      left: 50%;
      transform: translateX(-50%);
      z-index: 10;
    }
  `;
}