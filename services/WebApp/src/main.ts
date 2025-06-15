import { LitElement, css, html } from "lit";
import { state } from "lit/decorators.js";
import { Router } from '@lit-labs/router';

import "@iyulab/chat-components/components";
import "@iyulab/chat-components/styles/light.css";
import "@iyulab/chat-components/styles/dark.css";
import { type Theme, setTheme } from '@iyulab/chat-components/utilities/theme-functions.js';

import "./components";
import "./pages";

class Main extends LitElement {
  private router = new Router(this, [
    { path: '/', 
      render: () => html`<home-page></home-page>`},
    { path: '/test',
      render: () => html`<test-page></test-page>`},
  ], {
    fallback: {
      render: () => html`<error-page></error-page>`
    }
  });

  @state() theme: Theme = localStorage.getItem('theme') as Theme || 'light';

  connectedCallback(): void {
    super.connectedCallback();
    setTheme(this.theme);
  }

  render() {
    return html`
      <div class="header">
        <div class="title" @click=${() => window.location.href = '/'}>
          <uc-icon external name="logo"></uc-icon>
          <uc-icon external name="label"></uc-icon>
        </div>
        <div class="flex"></div>
        <div class="control">
          <uc-icon external name=${this.theme} 
            @click=${this.toggleTheme}
          ></uc-icon>
        </div>
      </div>

      <div class="main">
        <div class="view">
          ${this.router.outlet()}
        </div>
      </div>
    `;
  }

  private toggleTheme = () => {
    this.theme = this.theme === 'light' ? 'dark' : 'light';
    setTheme(this.theme);
    localStorage.setItem('theme', this.theme);
  }

  static styles = css`
    :host {
      position: relative;
      display: block;
      width: 100vw;
      height: 100vh;
    }
    :host * {
      box-sizing: border-box;
    }

    .header {
      position: relative;
      width: 100%;
      height: 48px;
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
      padding: 8px 12px 0px 12px;
      transition: color 0.3s;
    }

    .header .title {
      display: flex;
      flex-direction: row;
      align-items: center;
      gap: 4px;
      cursor: pointer;
    }
    .header .title:hover {
      opacity: 0.5;
    }
    .header .title uc-icon[name="logo"] {
      font-size: 32px;
    }
    .header .title uc-icon[name="label"]::part(svg) {
      width: 108px;
      height: 21px;
    }

    .header .flex {
      flex: 1;
    }

    .header .control {
      display: flex;
      flex-direction: row;
      align-items: center;
      gap: 8px;
    }
    .header .control uc-icon {
      font-size: 24px;
      cursor: pointer;
    }
    .header .control uc-icon:hover {
      opacity: 0.5;
    }

    .main {
      position: relative;
      width: 100%;
      height: calc(100% - 48px);
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .main .view {
      display: block;
      width: 100%;
      height: 100%;
    }
    .main .view.resize {
      width: 60%;
      height: 80%;
      border: 3px dashed red;
      overflow: auto;
      resize: both;
    }

    @media (max-width: 768px) {
      .header .title uc-icon[name="label"] {
        display: none;
      }
    }
  `;
}

customElements.define('main-entry', Main);
