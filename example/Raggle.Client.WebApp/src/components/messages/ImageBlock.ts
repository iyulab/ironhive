import { LitElement, css, html } from "lit";
import { customElement, property } from "lit/decorators.js";

@customElement('image-block')
export class ImageBlock extends LitElement {

  @property({ type: String })
  data: string = '';

  @property({ type: String })
  alt: string = '';

  render() {
    return html`
      <div class="container">
        <img src="${this.data}" alt="${this.alt}" />
      </div>
    `;
  }

  static styles = css`
    :host {
      display: block;
      width: 100%;
    }

    .container {
      display: flex;
      justify-content: center;
      align-items: center;
      background-color: #f0f0f0;
      border-radius: 8px;
      padding: 8px;
      box-sizing: border-box;
    }

    img {
      width: 300px;
      height: 300px;
      border-radius: 8px;
    }
  `;
}
