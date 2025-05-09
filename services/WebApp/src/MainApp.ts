import { LitElement, css, html } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { Router } from '@lit-labs/router';

import "@iyulab/chat-components";
import "@iyulab/chat-components/styles/chat-styles.css";
import "@iyulab/chat-components/styles/github-markdown.css";
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

  @property({ type: String, reflect: true }) 
  mode: 'flexible' | 'fixed' = 'fixed';

  render() {
    return html`
      <div class="flexible-box">
        ${this.router.outlet()}
      </div>
      <div class="toggler theme" @click=${this.toggle}>T</div>
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
    :host([mode="fixed"]) .flexible-box {
      width: 100%;
      height: 100%;
      resize: none;
    }

    .flexible-box {
      position: relative;
      display: block;
      width: 60%;
      height: 80%;
      padding: 20px;
      border: 1px dashed gray;
      resize: both;
      overflow: auto;
      box-sizing: border-box;
    }

    .toggler {
      position: absolute;
      width: 24px;
      height: 24px;
      background-color: gray;
      color: #fff;
      text-align: center;
      line-height: 24px;
      user-select: none;
      cursor: pointer;
    }
    .toggler.theme {
      top: 10px;
      right: 10px;
    }
    .toggler:hover {
      background-color: #888;
    }
    .toggler:active {
      background-color: #555;
    }
  `;
}
