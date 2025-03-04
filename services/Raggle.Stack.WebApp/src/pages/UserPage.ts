import { LitElement, css, html } from "lit";
import { customElement, property } from "lit/decorators.js";

@customElement('user-page')
export class UserPage extends LitElement {

  // @property({ type: String }) 
  // key: string = '';

  render() {
    return html`
      <div class="container">
        <h2>User Page</h2>
        <p>Welcome to the user page!</p>
      </div>
    `;
  }

  static styles = css`
    :host {
      display: block;
      padding: 16px;
      font-family: Arial, sans-serif;
    }

    .container {
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 16px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    h2 {
      margin-top: 0;
    }

    p {
      margin: 0;
    }
  `;
}