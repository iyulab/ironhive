import { css, html, LitElement, nothing, PropertyValues } from 'lit';
import { customElement, property, query } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';

import { computePosition, offset, shift, flip, autoPlacement } from '@floating-ui/dom';

import type { ModelSummary } from '../services/types/ModelSummary.js';

@customElement('model-select')
export class ModelSelect extends LitElement {

  @query('.selecter') selecterEl!: HTMLElement;
  @query('.list') listEl!: HTMLElement;

  @property({ type: Boolean, reflect: true }) open: boolean = false;
  @property({ type: String }) placeholder: string = "Choose a model";
  @property({ type: Array }) models: ModelSummary[] = [];
  @property({ type: Object }) selectedModel?: ModelSummary;

  connectedCallback(): void {
    super.connectedCallback();
    this.addEventListener('focusout', () => this.open = false);
  }

  disconnectedCallback(): void {
    this.removeEventListener('focusout', () => this.open = false);
    super.disconnectedCallback();
  }

  protected updated(changedProperties: PropertyValues) {
    super.updated(changedProperties);

    if (changedProperties.has('open')) {
      if (this.open) {
        this.show();
      } else {
        this.hide();
      }
    }
  }

  render() {
    return html`
      <div class="selecter" tabindex="0" @click=${() => this.open = !this.open}>
        <div class="value">
          ${this.selectedModel?.displayName || this.placeholder}
        </div>
        <uc-icon class="icon"
          name=${this.open ? 'chevron-up' : 'chevron-down'}
        ></uc-icon>
      </div>
      <div class="list scroll" tabindex="0">
        ${repeat(this.models, (i) => i.modelId, (i) => {
          const selected = this.selectedModel?.modelId === i.modelId;
          return html`
            <div class="item" ?selected=${selected} @click=${() => this.select(i)}>
              <div class="display">
                ${i.displayName}
                ${selected ? html`<uc-icon name="check"></uc-icon>` : nothing}
              </div>
              <div class="description">
                ${i.description}
              </div>
            </div>
          `})}
      </div>
    `;
  }

  private select = (model: ModelSummary) => {
    this.selectedModel = model;
    this.dispatchEvent(new CustomEvent('select', { 
      detail: this.selectedModel,
      bubbles: true, composed: true 
    }));
    this.open = false;
  }

  private show = async () => {
    if (!this.open) return;
    const { x, y } = await computePosition(this, this.listEl, {
      middleware: [
        offset(),
        shift(),
        flip(),
        autoPlacement({
          allowedPlacements: ['top-start', 'bottom-start'],
        }),
      ],
    });

    Object.assign(this.listEl.style, {
      left: `${x}px`,
      top: `${y}px`,
    });
    this.listEl.classList.add('open');
  }

  private hide = async () => {
    if (this.open) return;
    this.listEl.classList.remove('open');

    this.dispatchEvent(new CustomEvent('popup', {
      bubbles: true,
      composed: true,
    }));
  }

  static styles = css`
    :host {
      position: relative;
      display: block;
      background-color: var(--uc-background-color-0);
      border: 1px solid var(--uc-border-color-low);
      border-radius: 8px;
      padding: 8px 12px;
    }

    .selecter {
      position: relative;
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
      cursor: pointer;
      font-size: 14px;
      line-height: 16px;
      gap: 8px;
    }

    /* 리스트 스타일 */
    .list {
      position: absolute;
      width: max-content;
      top: 0;
      left: 0;

      display: flex;
      flex-direction: column;
      visibility: hidden;
      opacity: 0;

      border-radius: 8px;
      border: 1px solid var(--uc-border-color-low);
      background-color: var(--uc-background-color-0);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
      z-index: 1000;

      max-height: 280px;
      overflow: auto;
    }
    .list.open {
      visibility: visible;
      opacity: 1;
    }

    .item {
      position: relative;
      padding: 6px 12px;
      display: flex;
      flex-direction: column;
      transition: background-color 0.2s, color 0.2s;
      cursor: pointer;
      
      .display {
        display: flex;
        flex-direction: row;
        align-items: center;
        justify-content: space-between;
        font-size: 12px;
        line-height: 20px;
        font-weight: 600;
      }

      .description {
        font-size: 12px;
        line-height: 20px;
        font-weight: 300;
        opacity: 0.6;
      }
    }
    .item[selected] {
      color: var(--uc-blue-color-500);
    }
    .item:hover {
      background-color: var(--uc-background-color-300);
    }
  `;
}
