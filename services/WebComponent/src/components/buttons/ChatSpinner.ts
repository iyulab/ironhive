import { LitElement, css, html } from "lit";
import { customElement } from "lit/decorators.js";

@customElement('chat-spinner')
export class ChatSpinner extends LitElement {
  render() {
    return html`
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
        <g class="spinner">
          <rect x="11" y="1" width="2" height="5" opacity=".14"/>
          <rect x="11" y="1" width="2" height="5" transform="rotate(30 12 12)" opacity=".29"/>
          <rect x="11" y="1" width="2" height="5" transform="rotate(60 12 12)" opacity=".43"/>
          <rect x="11" y="1" width="2" height="5" transform="rotate(90 12 12)" opacity=".57"/>
          <rect x="11" y="1" width="2" height="5" transform="rotate(120 12 12)" opacity=".71"/>
          <rect x="11" y="1" width="2" height="5" transform="rotate(150 12 12)" opacity=".86"/>
          <rect x="11" y="1" width="2" height="5" transform="rotate(180 12 12)"/>
        </g>
      </svg>
    `;
  }

  static styles = css`
    :host {
      display: inline-flex;
    }

    svg {
      width: 1em;
      height: 1em;
      fill: currentColor;
    }

    .spinner {
      transform-origin:center;
      animation:spinner_action .75s step-end infinite
    }
    
    @keyframes spinner_action {
      8.3%{transform:rotate(30deg)}
      16.6%{transform:rotate(60deg)}
      25%{transform:rotate(90deg)}
      33.3%{transform:rotate(120deg)}
      41.6%{transform:rotate(150deg)}
      50%{transform:rotate(180deg)}
      58.3%{transform:rotate(210deg)}
      66.6%{transform:rotate(240deg)}
      75%{transform:rotate(270deg)}
      83.3%{transform:rotate(300deg)}
      91.6%{transform:rotate(330deg)}
      100%{transform:rotate(360deg)}
    }
  `;
}
