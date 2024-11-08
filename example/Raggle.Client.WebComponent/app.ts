import { LitElement, css, html } from "lit";
import { customElement, query, state } from "lit/decorators.js";

@customElement('the-app')
export class TheApp extends LitElement {

  connectedCallback() {
    super.connectedCallback();
  }

  render() {
    return html`
      Hello World
    `;
  }

  static styles = css`
    :host {
      width: 100%;
      height: 100%;
    }
  `;
}
