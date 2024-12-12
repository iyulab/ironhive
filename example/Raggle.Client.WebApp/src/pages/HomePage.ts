import { LitElement, css, html } from "lit";
import { customElement } from "lit/decorators.js";

@customElement('home-page')
export class HomePage extends LitElement {

  render() {
    return html`
      <div class="user-input">
        <textarea 
          placeholder="Enter your message here..." 
          rows="4"
        ></textarea>
        <button @click=${this.sendMessage}>Send</button>
      </div>
    `;
  }

  private sendMessage() {
    console.log('Send message');
  }

  static styles = css`
    :host {
      width: 100%;
      height: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
      box-sizing: border-box;
    }

    .user-input {
      width: 60%;
      display: flex;
      flex-direction: row;
      gap: 8px;
    }

    textarea {
      width: 90%;
      padding: 8px;
      font-size: 16px;
      border-radius: 8px;
      border: 1px solid #ddd;
    }

    button {
      padding: 8px 16px;
      font-size: 16px;
      background-color: #0078d4;
      color: white;
      border: none;
      border-radius: 8px;
      cursor: pointer;
    }
  `;
}
