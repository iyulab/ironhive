import { LitElement, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";

import "@iyulab/ironhive/components/chat";
import "@iyulab/ironhive/components/styles/hivestack-light.css";
import "@iyulab/ironhive/components/styles/hivestack-dark.css";

@customElement('test-page')
export class TestPage extends LitElement {

  render() {
    return html`
      <div class="flexible-box">
        <chat-room
          baseUri="http://172.30.1.26:5075/v1/"
        ></chat-room>
      </div>
      <div @click=${this.toggle} class="toggler">T</div>
    `;
  }

  private toggle = () => {
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
      width: 100%;
      height: 100%;
    }

    .flexible-box {
      display: block;
      width: 800px;
      height: 80%;
      border: 0.5px solid gray;
      resize: both;
      overflow: auto;
      box-sizing: border-box;
    }
    .flexible-box::-webkit-scrollbar {
      width: 0px;
    }

    .toggler {
      position: absolute;
      top: 5px;
      right: 10px;
      width: 24px;
      height: 24px;
      background-color: #000;
      color: #fff;
      text-align: center;
      line-height: 24px;
      cursor: pointer;
    }
    .toggler:hover {
      background-color: #333;
    }
    .toggler:active {
      background-color: #666;
    }

  `;
}
