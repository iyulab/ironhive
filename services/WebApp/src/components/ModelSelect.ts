import { css, html, LitElement, nothing, PropertyValues } from 'lit';
import { customElement, property, query, state } from 'lit/decorators.js';
import { until } from 'lit/directives/until.js';
import { repeat } from 'lit/directives/repeat.js';

import type { ModelSummary, ModelProvider } from '../services/types/ModelSummary.js';
import { ModelProviderList } from '../services/types/ModelSummary.js';

@customElement('model-select')
export class ModelSelect extends LitElement {

  @query('.selecter') selecterEl!: HTMLElement;
  @query('.popup') popupEl!: HTMLElement;

  @state() provider: ModelProvider = 'openai';
  
  @property({ type: Boolean, reflect: true }) open: boolean = false;
  @property({ type: String }) placeholder: string = "Choose a model";
  @property({ type: Object }) model?: ModelSummary;

  connectedCallback(): void {
    super.connectedCallback();
    this.tabIndex = 0;
    this.addEventListener('focusout', () => this.open = false);
  }

  disconnectedCallback(): void {
    this.removeEventListener('focusout', () => this.open = false);
    super.disconnectedCallback();
  }

  protected willUpdate(changedProperties: PropertyValues): void {
    super.willUpdate(changedProperties);

    if (changedProperties.has('open') && this.open && this.model) {
      this.provider = this.model.provider;
    }
  }

  render() {
    return html`
      <div class="selecter" tabindex="0" @click=${() => this.open = !this.open}>
        <uc-icon class="logo"
          external 
          name=${this.model?.provider || 'question-circle'}
        ></uc-icon>
        <div class="display">
          ${this.model?.displayName || this.placeholder}
        </div>
        <uc-icon class="caret"
          name=${this.open ? 'chevron-up' : 'chevron-down'}
        ></uc-icon>
      </div>

      <div class="popup" ?visible=${this.open} tabindex="0">
        <div class="providers">
          ${repeat(ModelProviderList, (p) => p, (p) => {
            const selected = this.provider === p;
            return html`
              <uc-icon external name=${p} ?selected=${selected}
                @click=${() => this.provider = p}
              ></uc-icon>`;
          })}
        </div>
        <div class="models">
          ${until(fetch(`/models/${this.provider}.json`).then(async res => {
            const models = (await res.json()).data as ModelSummary[];
            return repeat(models, (i) => i.modelId, (i) => {
              const selected = this.model?.modelId === i.modelId;
              return html`
                <div class="item" ?selected=${selected} @click=${() => this.select(i)}>
                  <div class="display">
                    ${i.displayName}
                    ${i.thinkable ? html`<uc-icon name="lightbulb-fill"></uc-icon>` : nothing}
                  </div>
                  <div class="description">
                    ${i.description}
                  </div>
                  <uc-tooltip placement="right-start">
                    <strong>Thinkable:</strong><span>${i.thinkable ? '✅' : '❌'}</span>
                    <strong>Input Price:</strong><span>${i.inputPrice ? `$${i.inputPrice} / 1M tokens` : 'Free'}</span>
                    <strong>Output Price:</strong><span>${i.outputPrice ? `$${i.outputPrice} / 1M tokens` : 'Free'}</span>
                    <strong>Context Length:</strong><span>${i.contextLength} tokens</span>
                    <strong>Max Output:</strong><span>${i.maxOutput ? `${i.maxOutput} tokens` : 'Unknown'}</span>
                  </uc-tooltip>
                </div>`});
            }), html`<uc-dot-rotate-loader class="loader"></uc-dot-rotate-loader>`)}
        </div>
      </div>
    `;
  }

  private select = (model: ModelSummary) => {
    this.dispatchEvent(new CustomEvent('select-model', { 
      detail: model,
      bubbles: true, 
      composed: true
    }));
    this.open = false;
  }

  static styles = css`
    :host {
      position: relative;
      display: block;
      background-color: var(--uc-background-color-0);
      border: 1px solid var(--uc-border-color-low);
      border-radius: 8px;
      user-select: none;
    }

    .selecter {
      position: relative;
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
      cursor: pointer;
      min-width: 240px;
    }
    .selecter .logo {
      font-size: 18px;
      padding: 8px 12px;
    }
    .selecter .display {
      flex: 1;
      font-size: 14px;
      line-height: 18px;
      font-weight: 600;
      padding: 8px 4px;
    }
    .selecter .caret {
      font-size: 18px;
      padding: 8px;
    }

    /* 리스트 스타일 */
    .popup {
      position: absolute;
      z-index: 1000;
      top: 100%;
      left: 0;
      display: flex;
      flex-direction: row;
      
      border-radius: 8px;
      border: 1px solid var(--uc-border-color-low);
      background-color: var(--uc-background-color-0);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);

      opacity: 0;
      pointer-events: none;
    }
    .popup[visible] {
      opacity: 1;
      pointer-events: auto;
    }

    .providers {
      display: flex;
      flex-direction: column;
      background-color: var(--uc-background-color-100);
    }
    .providers uc-icon {
      padding: 12px;
      font-size: 18px;
      cursor: pointer;
    }
    .providers uc-icon:hover {
      background-color: var(--uc-background-color-200);
    }
    .providers uc-icon[selected] {
      background-color: var(--uc-background-color-300);
    }

    .models {
      position: relative;
      display: flex;
      flex-direction: column;
      min-width: 210px;

      border-left: 1px solid var(--uc-border-color-low);
    }
    .models .loader {
      width: 100%;
      height: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .models .item {
      position: relative;
      padding: 4px 8px;
      display: flex;
      flex-direction: column;
      cursor: pointer;
    }
    .models .item:hover {
      background-color: var(--uc-background-color-200);
    }
    .models .item[selected] {
      background-color: var(--uc-background-color-300);
    }
    .models .item .display {
      display: flex;
      flex-direction: row;
      align-items: center;
      gap: 8px;
      font-size: 14px;
      line-height: 1.5;
      font-weight: 400;
    }
    .models .item .display uc-icon {
      color: var(--uc-yellow-color-500);
    }
    .models .item .description {
      font-size: 12px;
      line-height: 1.5;
      font-weight: 300;
      opacity: 0.6;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    .models .item uc-tooltip {
      display: grid;
      grid-template-columns: auto 1fr;
      gap: 4px;
    }
    
  `;
}
