import { LitElement, css, html } from "lit";
import { customElement } from "lit/decorators.js";
import { Router } from '@lit-labs/router';
import { Theme } from "./models";

import '@shoelace-style/shoelace';
import "./components";
import "./pages";

@customElement('main-app')
export class MainApp extends LitElement {
  private theme: Theme = localStorage.getItem('theme') as Theme || 'light';
  private router = new Router(this, [
    { path: '/', 
      render: () => html`<home-page></home-page>`},
    { path: '/storages', 
      render: () => html`<storage-explorer></storage-explorer>`},
    { path: '/storages/:id',
        render: ({id}) => html`<storage-viewer key=${id!}></storage-viewer>`},
    { path: '/storage/new',
        render: () => html`<storage-editor></storage-editor>`},
    { path: '/assistants', 
      render: () => html`<assistant-explorer></assistant-explorer>`},
    { path: '/assistants/:id', 
        render: ({id}) => html`<assistant-editor key=${id!}></assistant-editor>`},
    { path: '/chat/:id',
      render: ({id}) => html`<chat-room key=${id!}></chat-room>`},
    { path: '/user', 
      render: () => html`<user-page></user-page>`},
  ], {
    fallback: {
      render: () => html`<error-page status="404"></error-page>`
    }
  });

  render() {
    return html`
      <div class="side-bar">
        <a class="home" href="/">
          <sl-icon name="logo-word"></sl-icon>
        </a>
        <a class="menu" href="/storages">
          <sl-icon name="archive"></sl-icon>
          <span>Storage</span>
        </a>
        <a class="menu" href="/assistants">
          <sl-icon name="robot"></sl-icon>
          <span>Assistant</span>
        </a>
        <a class="menu" href="/">
          <sl-icon name="chat-left-dots"></sl-icon>
          <span>Chat Room</span>
        </a>
        <div class="chat-list">
          ${Array.from({length: 5}).map((_, i) => html`
            <a class="sub-menu" href="/chat/${i}">
              <span>${i}th Chat Dummy</span>
            </a>
          `)}
        </div>
        <a class="menu" href="/user">
          <sl-icon name="person-gear"></sl-icon>
          <span>Profile</span>
        </a>
      </div>
      <div class="top-bar">
        <div class="left">
          <sl-icon-button 
            name="chevron-left"
            @click=${this.goback}
          ></sl-icon-button>
          <sl-icon-button
            name="chevron-right"
            @click=${this.goforward}
          ></sl-icon-button>
        </div>
        <div class="right">
          <sl-icon-button
            name="sun"
            @click=${this.toggleTheme}
          ></sl-icon-button>
        </div>
      </div>
      <div class="main">
        ${this.router.outlet()}
      </div>
    `;
  }

  private goback = async () => {
    window.history.back();
  }

  private goforward = async () => {
    window.history.forward();
  }

  private toggleTheme = async () => {
    const root = document.body.classList;
    root.toggle('sl-theme-dark');
    this.theme = root.contains('sl-theme-dark') ? 'dark' : 'light';
  }

  static styles = css`
    :host {
      display: grid;
      grid-template-columns: 260px 1fr;
      grid-template-rows: 60px 1fr;
      width: 100vw;
      height: 100vh;
      overflow: hidden;

      color: var(--sl-color-neutral-1000);
    }

    .side-bar {
      grid-row: 1 / 3;
      grid-column: 1 / 2;
      background-color: var(--sl-color-gray-100);
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 8px;
      gap: 8px;
      box-sizing: border-box;

      a {
        width: 100%;
        display: flex;
        align-items: center;
        text-decoration: none;
        box-sizing: border-box;
      }

      .home {
        text-decoration: none;
        color: var(--sl-color-red-700);
        margin: 12px 0px;
        justify-content: center;

        sl-icon {
          width: 80%;
          height: 80%;
        }

        &:hover {
          color: var(--sl-color-red-600);
        }
      }

      .menu {
        flex-direction: row;
        padding: 12px 16px;
        gap: 12px;
        font-size: 18px;
        line-height: 1;
        border-radius: 4px;
        color: var(--sl-color-gray-600);

        &:hover {
          background-color: var(--sl-color-gray-200);
        }
        &:active {
          transform: scale(0.95);
        }
      }

      .chat-list {
        width: 100%;
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: 4px;
        overflow-x: hidden;
        overflow-y: auto;
        text-overflow: ellipsis;
        white-space: nowrap;
        scrollbar-width: thin;
      }

      .sub-menu {
        padding: 8px 8px 8px 46px;
        font-size: 14px;
        color: var(--sl-color-gray-500);
        border-radius: 4px;

        &:hover {
          background-color: var(--sl-color-gray-200);
        }
        &:active {
          transform: scale(0.95);
        }
      }
    }

    .top-bar {
      grid-row: 1 / 2;
      grid-column: 2 / 3;
      background-color: var(--sl-color-gray-50);
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
      padding: 0 18px;
      box-sizing: border-box;

      sl-icon-button {
        font-size: 24px;
      }
    }

    .main {
      grid-row: 2 / 3;
      grid-column: 2 / 3;
      background-color: var(--sl-color-gray-50);
      display: block;
      box-sizing: border-box;
      overflow-y: auto;
    }

  `;
}