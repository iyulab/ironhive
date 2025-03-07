import { LitElement, css, html } from "lit";
import { customElement } from "lit/decorators.js";
import { Router } from '@lit-labs/router';

import '@shoelace-style/shoelace';
import "./components";
import "./pages";

@customElement('main-app')
export class MainApp extends LitElement {
  private router = new Router(this, [
    { path: '/', 
      render: () => html`<home-page></home-page>`},
    { path: '/storages', 
      render: () => html`<storage-explorer></storage-explorer>`},
    { path: '/storage',
      render: () => html`<storage-editor></storage-editor>`},
    { path: '/storage/:id',
      render: ({id}) => html`<storage-viewer .key=${id!}></storage-viewer>`},
    { path: '/assistants', 
      render: () => html`<assistant-explorer></assistant-explorer>`},
    { path: '/assistant/:id?', 
      render: ({id}) => html`<assistant-editor .key=${id}></assistant-editor>`},
    { path: '/chat/:id',
      render: ({id}) => html`<chat-room .key=${id!}></chat-room>`},
    { path: '/user', 
      render: () => html`<user-page></user-page>`}
  ], {
    fallback: {
      render: () => html`<error-page status="404"></error-page>`
    }
  });

  render() {
    if (window.location.pathname === '/test') {
      return html`<test-page></test-page>`;
    }

    return html`
      <main-layout>
        ${this.router.outlet()}
      </main-layout>
    `;
  }

  static styles = css`
    :host {
      display: block;
      width: 100vw;
      height: 100vh;
    }
  `;
}