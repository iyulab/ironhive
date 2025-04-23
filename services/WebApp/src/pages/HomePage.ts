import { LitElement, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";

@customElement('home-page')
export class HomePage extends LitElement {

  @state() loading: boolean = false;
  @state() disabled: boolean = false;

  render() {
    return html` 
      <div class="container">
        <chat-button 
          ?loading=${this.loading} 
          ?disabled=${this.disabled} 
          @click=${this.clicked}>
          Hello
        </chat-button>
      </div>
    `;
  }

  private clicked = async (e: any) => {
    console.log(e);
    this.loading = true;
  }

  static styles = css`
    :host {
      width: 100%;
      height: 100%;
    }

    .container {
      position: relative;
      display: flex;
      width: 10%;
      height: 5%;
      align-items: center;
      justify-content: center;
      border: 1px dashed red;
      box-sizing: border-box;
    }
  `;
}