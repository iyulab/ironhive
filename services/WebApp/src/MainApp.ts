import { LitElement, css, html } from "lit";
import { customElement } from "lit/decorators.js";
import { Router } from '@lit-labs/router';

import '@shoelace-style/shoelace';
import "./pages";

@customElement('main-app')
export class MainApp extends LitElement {
  private router = new Router(this, [
    { path: '/', 
      render: () => html`<home-page></home-page>`}
  ], {
    fallback: {
      render: () => html`<error-page></error-page>`
    }
  });

  render() {
    return html`
      <div class="main">
        ${this.router.outlet()}
      </div>
    `;
  }

  static styles = css`
    :host {
      display: block;
      width: 100vw;
      height: 100vh;
    }

    .main {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 100%;
      height: 100%;
    }
  `;
}
