import { LitElement, css, html } from "lit";
import { customElement } from "lit/decorators.js";
import { Router } from '@lit-labs/router';

@customElement('main-app')
export class MainApp extends LitElement {
  private _routes = new Router(this, [
    {path: '/', render: () => html`<h1>Home</h1>`},
    {path: '/projects', render: () => html`<h1>Projects</h1>`},
    {path: '/about', render: () => html`<h1>About</h1>`},
  ]);

  render() {
    return html`
      <header>
        <div @click=${() => this._routes.goto('/')}>Raggle</div>
        <div @click=${() => this._routes.goto('/projects')}>Memory</div>
        <div @click=${() => this._routes.goto('/about')}>Assistant</div>
      </header>
      <main>
        ${this._routes.outlet()}
      </main>
    `;
  }

  static styles = css`
    :host {
      display: block;
      width: 100vw;
      height: 100vh;
      overflow: hidden;
    }

    header {
      height: 48px;
      display: flex;
      flex-direction: row;
      align-items: center;
      gap: 48px;
      background-color: #333;
      color: white;
      box-sizing: border-box;
      padding: 24px;

      div {
        cursor: pointer;
      }

      div:hover {
        text-decoration: underline;
      }
      div:active {
        color: #f0f0f0;
      }
    }

    main {
      display: block;
      width: 100%;
      height: calc(100% - 48px);
      background-color: #f0f0f0;
      box-sizing: border-box;
      overflow: auto;
    }
  `;
}