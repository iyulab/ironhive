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
                    ${selected ? html`<uc-icon name="check"></uc-icon>` : nothing}
                    ${i.displayName}
                  </div>
                  <div class="abilities">
                    ${i.thinkable ? html`💡` : nothing}
                  </div>
                  <div class="description">
                    ${i.description}
                  </div>
                  <uc-tooltip placement="right-start">
                    <div class="tooltip-section">
                      <div class="ts-title">💲 Price (1M Tokens)</div>
                      <div class="ts-value">
                        <span class="io-badge in">in</span>
                        <span>$ ${i.inputPrice || '0'}</span>
                        <span class="io-badge out">out</span>
                        <span>$ ${i.outputPrice || '0'}</span>
                      </div>
                    </div>
                    
                    <div class="tooltip-section">
                      <div class="ts-title">📊 Token Limit</div>
                      <div class="ts-value">
                        <span class="io-badge in">in</span>
                        <span>${this.formatTokens(i.contextLength)}</span>
                        <span class="io-badge out">out</span>
                        <span>${i.maxOutput ? this.formatTokens(i.maxOutput) : '?'}</span>
                      </div>
                    </div>
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

  private formatTokens(tokens: number): string {
    if (tokens >= 1000000) {
      return `${(tokens / 1000000).toFixed(1)}M`;
    } else if (tokens >= 1000) {
      return `${(tokens / 1000).toFixed(0)}K`;
    }
    return tokens.toString();
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
      width: 210px;

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
      display: grid;
      grid-template-columns: 1fr auto;
      grid-template-areas:
        "dp ab"
        "ds ds";
      cursor: pointer;
    }
    .models .item:hover {
      background-color: var(--uc-background-color-200);
    }
    .models .item[selected] {
      background-color: var(--uc-background-color-300);
    }
    .models .item .display {
      grid-area: dp;
      display: flex;
      flex-direction: row;
      align-items: center;
      gap: 8px;
      font-size: 14px;
      line-height: 1.5;
      font-weight: 400;
    }
    .models .item .abilities {
      grid-area: ab;
      display: flex;
      flex-direction: row;
      align-items: center;
      gap: 4px;
      font-size: 14px;
    }
    .models .item .description {
      grid-area: ds;
      font-size: 12px;
      line-height: 1.5;
      font-weight: 300;
      opacity: 0.6;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    .models .item uc-tooltip {
      display: flex;
      flex-direction: column;
      gap: 8px;
      padding: 4px;
    }

    .tooltip-section {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }
    .ts-title {
      font-weight: 600;
      font-size: 12px;
    }
    .ts-value {
      display: grid;
      grid-template-columns: auto 1fr auto 1fr;
      align-items: center;
      gap: 8px;
      font-size: 12px;
      padding: 4px;
    }
    .io-badge {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      padding: 2px 4px;
      border-radius: 4px;
      font-size: 10px;
      font-weight: 600;
      font-style: italic;
      font-family: 'Georgia', serif;
    }
    .io-badge.in {
      background: #dbeafe;
      color: #1e40af;
    }
    .io-badge.out {
      background: #d1fae5;
      color: #065f46;
    }
  `;
}
