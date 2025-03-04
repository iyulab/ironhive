import { LitElement, css, html } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('loading-overlay')
export class LoadingOverlay extends LitElement {

  @property({ type: Number }) value: number = 100;
  @property({ type: String }) label: string = '업로드 중';

  // 프로그레스 링의 크기와 두께를 설정합니다.
  private readonly size: number = 200;
  private readonly strokeWidth: number = 10;
  private readonly radius: number = (this.size - this.strokeWidth) / 2;
  private readonly circumference: number = 2 * Math.PI * this.radius;

  render() {
    const normalizedValue = Math.min(Math.max(this.value, 0), 100);
    const offset = this.circumference - (normalizedValue / 100) * this.circumference;

    return html`
      <div class="progress-container">
        <svg
          class="progress-ring"
          width="${this.size}"
          height="${this.size}"
        >
          <circle
            class="progress-ring__background"
            stroke="var(--sl-color-gray-300)"
            stroke-width="${this.strokeWidth}"
            fill="transparent"
            r="${this.radius}"
            cx="${this.size / 2}"
            cy="${this.size / 2}"
          ></circle>
          <circle
            class="progress-ring__progress"
            stroke="var(--sl-color-primary-500)"
            stroke-width="${this.strokeWidth}"
            fill="transparent"
            r="${this.radius}"
            cx="${this.size / 2}"
            cy="${this.size / 2}"
            stroke-dasharray="${this.circumference}"
            stroke-dashoffset="${offset}"
            style="transition: stroke-dashoffset 0.35s; transform: rotate(-90deg); transform-origin: center;"
          ></circle>
          <text
            x="50%"
            y="50%"
            dominant-baseline="middle"
            text-anchor="middle"
            font-size="20"
            fill="#fff"
          >
            ${Math.floor(normalizedValue)}%
          </text>
        </svg>
        <p class="label">
          ${this.label}
        </p>
      </div>
    `;
  }

  static styles = css`
    :host {
      position: fixed;
      top: 0;
      left: 0;
      width: 100vw;
      height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background-color: rgba(0, 0, 0, 0.5);
      z-index: 9999;
    }

    .progress-container {
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .progress-ring {
      transform: rotate(0deg);
    }

    .progress-ring__background {
      /* 배경 원의 스타일 */
      background-color: transparent;
    }

    .progress-ring__progress {
      /* 프로그레스 원의 스타일 */
      transition: stroke-dashoffset 0.35s;
      transform: rotate(0deg);
      transform-origin: center;
    }

    .label {
      margin-top: 16px;
      color: #fff;
      font-size: 1.2em;
      text-align: center;
    }
  `;
}
