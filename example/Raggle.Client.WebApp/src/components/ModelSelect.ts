import { LitElement, PropertyValues, css, html } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import type { Models } from "../backend/Models";
import { API } from "../backend/ApiClient";

@customElement('model-select')
export class ModelSelect extends LitElement {
  // 모델 캐시: 타입별로 분리
  private static chatModels?: Models;
  private static embedModels?: Models;

  // 로딩 상태: 타입별로 분리
  @state() private loadingChat: boolean = false;
  @state() private loadingEmbed: boolean = false;

  // 현재 타입에 따른 모델
  @state() private models?: Models;

  @property({ type: String }) type: 'chat' | 'embed' = 'embed';

  protected async updated(_changedProperties: PropertyValues) {
    super.updated(_changedProperties);
    if (_changedProperties.has('type')) {
      // 타입이 변경될 때 해당 타입의 모델을 설정
      this.models = this.type === 'chat'
        ? ModelSelect.chatModels 
        : ModelSelect.embedModels;

      // 필요 시 모델 로드
      if (!this.models) {
        this.handleSelectFocus();
      }
    }
  }

  render() {
    const isLoading = this.isLoading();
    const models = this.models;

    return html`
      <div class="form-group">
        <label for="model-select">Model</label>
        <select 
          id="model-select" 
          @change="${this.handleModelChange}" 
          @focus="${this.handleSelectFocus}" 
          @click="${this.handleSelectFocus}"
          ?disabled="${isLoading || !models}"
        >
          ${isLoading ? html`
            <option>Loading models...</option>
          ` : html`
            <option value="">Select a model</option>
            ${models ? Object.entries(models).map(([provider, modelList]) => html`
              <optgroup label="${provider}">
                ${modelList.map(model => html`
                  <option value="${model}">${model}</option>
                `)}
              </optgroup>
            `) : html`
              <option disabled>No models available</option>
            `}
          `}
        </select>
      </div>
    `;
  }

  /**
   * 현재 타입에 맞는 로딩 상태를 반환합니다.
   */
  private isLoading(): boolean {
    return this.type === 'chat' ? this.loadingChat : this.loadingEmbed;
  }

  /**
   * 현재 타입에 맞는 로딩 상태를 설정합니다.
   */
  private setLoading(value: boolean): void {
    if (this.type === 'chat') {
      this.loadingChat = value;
    } else {
      this.loadingEmbed = value;
    }
  }

  /**
   * 현재 타입에 맞는 모델을 반환합니다.
   */
  private getCachedModels(): Models | undefined {
    return this.type === 'chat' ? ModelSelect.chatModels : ModelSelect.embedModels;
  }

  /**
   * Select 요소에 포커스나 클릭 이벤트가 발생했을 때 모델을 로드합니다.
   */
  private async handleSelectFocus() {
    const cachedModels = this.getCachedModels();
    if (!cachedModels && !this.isLoading()) {
      this.setLoading(true);
      try {
        if (this.type === 'chat') {
          ModelSelect.chatModels = await API.getChatModelsAsync();
        } else {
          ModelSelect.embedModels = await API.getEmbeddingModelsAsync();
        }
        this.models = this.getCachedModels();
      } catch (error) {
        console.error('Failed to load models:', error);
        // 사용자에게 에러 메시지를 표시하거나, 에러 상태를 추가할 수 있습니다.
      } finally {
        this.setLoading(false);
        this.requestUpdate();
      }
    }
  }

  /**
   * 모델 선택 시 발생하는 이벤트 핸들러
   */
  private handleModelChange(event: Event) {
    event.stopPropagation();
    const target = event.target as HTMLSelectElement;
    const selectedOption = target.selectedOptions[0];

    let provider = '';
    if (selectedOption.parentElement && selectedOption.parentElement.tagName === 'OPTGROUP') {
      provider = (selectedOption.parentElement as HTMLOptGroupElement).label;
    }
    const model = target.value;

    this.dispatchEvent(new CustomEvent('change', { 
      detail: { provider, model }
    }));
  }

  static styles = css`
    .form-group {
      margin-bottom: 1em;
    }
    label {
      display: block;
      margin-bottom: 0.5em;
      font-weight: bold;
    }
    select {
      width: 100%;
      padding: 0.5em;
      box-sizing: border-box;
    }
    .submit-button {
      padding: 0.75em 1.5em;
      background-color: #007BFF;
      color: white;
      border: none;
      cursor: pointer;
      border-radius: 4px;
    }
    .submit-button:hover {
      background-color: #0056b3;
    }
  `;
}
