import { LitElement, css, html } from "lit";
import { customElement } from "lit/decorators.js";
import { Router } from '@lit-labs/router';

import "@iyulab/chat-components";
import "@iyulab/chat-components/styles/lc-light.css";
import "@iyulab/chat-components/styles/lc-dark.css";
import '@shoelace-style/shoelace';
import "./pages";

@customElement('main-app')
export class MainApp extends LitElement {
  private router = new Router(this, [
    { path: '/', 
      render: () => html`<home-page></home-page>`},
    { path: '/chat',
      render: () => html`<chat-page></chat-page>`},
    { path: '/file',
      render: () => html`<file-page></file-page>`},
  ], {
    fallback: {
      render: () => html`<error-page></error-page>`
    }
  });

  render() {
    return html`
      <div class="flexible-box">
        ${this.router.outlet()}
      </div>
      <div class="toggler" @click=${this.toggle}>T</div>
    `;
  }

  private toggle = async () => {
    const html = document.documentElement;
    if (html.hasAttribute('theme')) {
      html.removeAttribute('theme');
    } else {
      html.setAttribute('theme', 'dark');
    }
  }

  static styles = css`
    :host {
      position: relative;
      display: flex;
      align-items: center;
      justify-content: center;
      width: 100vw;
      height: 100vh;
    }

    .flexible-box {
      position: relative;
      display: block;
      width: 60%;
      height: 80%;
      border: 1px dashed gray;
      resize: both;
      overflow: auto;
      box-sizing: border-box;
    }

    .toggler {
      position: absolute;
      top: 10px;
      right: 10px;
      width: 24px;
      height: 24px;
      background-color: gray;
      color: #fff;
      text-align: center;
      line-height: 24px;
      user-select: none;
      cursor: pointer;
    }
    .toggler:hover {
      background-color: #888;
    }
    .toggler:active {
      background-color: #555;
    }
  `;
}
