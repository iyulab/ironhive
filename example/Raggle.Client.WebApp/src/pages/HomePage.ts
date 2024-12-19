import { LitElement, css, html } from "lit";
import { customElement } from "lit/decorators.js";

@customElement('home-page')
export class HomePage extends LitElement {

  render() {
    return html`
      <div class="intro">
        <p>Welcome</p>
        <p>Raggle is an app that combines the Rag system with a chatbot.</p>
      </div>
      <div class="input">
        <message-input
          
        ></message-input>
      </div>
    `;
  }

  static styles = css`
    :host {
      width: 100%;
      height: 100%;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 32px;
      padding: 64px 16px;
      box-sizing: border-box;
    }

    .intro {
      width: 60%;
      display: flex;
      flex-direction: column;
      gap: 16px;
      align-items: center;

      p {
        margin: 0;
        font-size: 18px;
        font-weight: 600;
        color: var(--sl-color-gray-600);
      }
    }

    .input {
      width: 60%;
      display: flex;
      flex-direction: column;
      gap: 16px;
      align-items: center;
    }
  `;
}
