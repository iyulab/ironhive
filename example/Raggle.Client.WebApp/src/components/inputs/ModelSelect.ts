import { LitElement, css, html, nothing } from "lit";
import { customElement, property } from "lit/decorators.js";
import { until } from "lit/directives/until.js";

import type { AIServiceModels, AIServiceKey } from "../../models";
import { Api } from "../../services/ApiClient";

export interface ModelValue {
  provider: AIServiceKey;
  model: string;
}

@customElement('model-select')
export class ModelSelect extends LitElement {
  // Chache the models to avoid fetching them multiple times
  private static cmodels?: AIServiceModels;
  private static emodels?: AIServiceModels;

  @property({ type: String }) type: 'chat' | 'embed' = 'embed';
  @property({ type: String }) label: string = '';
  @property({ type: String }) name: string = '';
  @property({ type: String }) size: "small" | "medium" | "large" = 'small';
  @property({ type: String }) placeholder: string = '';
  @property({ type: Boolean }) required = false;
  @property({ type: Boolean }) disabled = false;
  @property({ type: String }) value: string = '';

  render() {
    return until(this.getModels().then((models) => {
      if (!models) return nothing;
      return html`
        <sl-select
          label=${this.label}
          name=${this.name}
          size=${this.size}
          placeholder=${this.placeholder}
          ?required=${this.required}
          ?disabled=${this.disabled}
          value=${this.value}
          .hoist=${true}
          @sl-change=${this.onChange}
        >
          ${Object.entries(models).map(([k,v]) => html`
            <small>${k}</small>
            ${v.map(m => html`
              <sl-option value=${`${k}/${m}`}>${m}</sl-option>
            `)}
          `)}
        </sl-select>
      `;
    })
    , html`
      <sl-skeleton 
        effect="pulse"
        style="width: 100%; height: 30px;"
      ></sl-skeleton>
    `);
  }

  private getModels = async () => {
    if (this.type === 'chat') {
      ModelSelect.cmodels ??= await Api.getChatModelsAsync();
      return ModelSelect.cmodels;
    } else {
      ModelSelect.emodels ??= await Api.getEmbeddingModelsAsync();
      return ModelSelect.emodels;
    }
  }

  private onChange = async (event: Event) => {
    event.preventDefault();
    event.stopPropagation();
    this.value = (event.target as HTMLSelectElement).value as string;
    const [provider, model] = this.value.split('/');
    const value: ModelValue = { provider, model } as ModelValue;
    this.dispatchEvent(new CustomEvent('change', {
      detail: value,
    }));
  }

  static styles = css`
    :host {
      width: 100%;
    }
  `;
}
