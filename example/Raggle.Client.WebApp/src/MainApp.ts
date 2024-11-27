import { LitElement, css, html } from "lit";
import { customElement } from "lit/decorators.js";
import { Router } from '@lit-labs/router';

import '@shoelace-style/shoelace';
import "./components";
import "./pages";

@customElement('main-app')
export class MainApp extends LitElement {
  private _routes = new Router(this, [
    {path: '/', render: () => html`<session-page></session-page>`},
    {path: '/memory', render: () => html`<memory-page></memory-page>`},
    {path: '/assistant', render: () => html`<assistant-page></assistant-page>`},
  ]);

  render() {
    return html`
      <header>
        <a href="/">Raggle</a>
        <a href="/assistant">Assistant</a>
        <a href="/memory">Memory</a>
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

      a {
        color: white;
        text-decoration: none;
        cursor: pointer;
      }

      a:hover {
        text-decoration: underline;
      }
      a:active {
        color: #ccc;
      }
    }

    main {
      display: block;
      width: 100%;
      height: calc(100% - 48px);
      background-color: #f0f0f0;
      box-sizing: border-box;
    }

  `;
}