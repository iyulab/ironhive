import { LitElement, PropertyValues, css, html } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { until } from "lit/directives/until.js";

import type { AssistantEntity } from "../models";
import { Api, App, goTo } from "../services";
import { format } from "@iyulab/hive-stack/internal";

@customElement('assistant-editor')
export class AssistantEditor extends LitElement {
  private debouncer?: NodeJS.Timeout;
  private static readonly DefaultAssistant: AssistantEntity = {
    name: 'New Assistant',
    description: 'This is a new assistant.',
    instruction: 'You are helpful assistant.',
    tools: [],
    options: {
      maxTokens: 2048,
      temperature: 0.7,
      topK: 50,
      topP: 0.9
    },
  }

  @property({ type: String }) key?: string;

  @state() assistant: AssistantEntity = AssistantEditor.DefaultAssistant;
  @state() status: 'draft' | 'updating' | 'updated' | 'error' = 'draft';
  @state() error: string = '';

  protected async updated(_changedProperties: PropertyValues) {
    super.updated(_changedProperties);
    await this.updateComplete;

    if (_changedProperties.has('key')) {
      await this.loadAsync();
    }
  }

  render() {
    return html`
      <!-- Preview -->
      <div class="preview">
        <chat-room 
          .assistantId=${this.assistant.id || ''}
        ></chat-room>
      </div>

      <sl-divider vertical></sl-divider>

      <!-- Form -->
      <div class="form">

        <!-- Form-Header -->
        <div class="header">
          <div class="status">
          ${this.status === 'updating'
            ? html`
              <sl-spinner></sl-spinner>
              <span>Updating...</span>`
            : this.status === 'updated'
            ? html`<sl-icon name="check-lg"></sl-icon>
              <span>Updated ${format(this.assistant.lastUpdatedAt)}</span>`
            : this.status === 'error'
            ? html`<sl-icon name="exclamation-octagon"></sl-icon>
              <span>${this.error}</span>`
            : html`
              <sl-icon name="plus-circle"></sl-icon>
              <span>New Assistant</span>`}
          </div>
          <div class="flex"></div>
          <div class="control">
            <sl-button
              size="small"
              ?disabled=${this.status === 'updating'}
              @click=${this.onUpdate}>
              Update
            </sl-button>
          </div>
        </div>
        
        <!-- Form-Body -->
        <div class="body">
          <model-select
            type="chat"
            label="Model"
            name="service-model"
            required
            .value=${`${this.assistant.service}/${this.assistant.model}`}
            @change=${this.onChange}
          ></model-select>
          <sl-input
            label="Name"
            name="name"
            size="small"
            required
            .value=${this.assistant.name || ''}
            @sl-change=${this.onChange}
          ></sl-input>
          <sl-textarea
            label="Description"
            name="description"
            size="small"
            rows="2"
            .value=${this.assistant.description || ''}
            @sl-change=${this.onChange}
          ></sl-textarea>
          <sl-textarea
            label="Instruction"
            name="instruction"
            size="small"
            rows="4"
            .value=${this.assistant.instruction || ''}
            @sl-change=${this.onChange}
          ></sl-textarea>
          <div class="label">
            Tools
          </div>
          <checkbox-option
            label="Vector Search"
            name="tools.vector_search"
            ?checked=${this.assistant.tools.includes('vector_search')}
            @change=${this.onChange}>
            ${until(Api.Memory.findCollections().then(collections => html`
              <sl-select
                size="small"
                ?hoist=${true}
                ?multiple=${true}
                ?clearable=${true}
                name="toolOptions.vector_search"
                value=${this.assistant.toolOptions?.vector_search?.join(' ') || ''}
                @sl-change=${this.onChange}>
                ${collections.map(c => html`
                  <sl-option value=${c.id!}>
                    ${c.name}
                  </sl-option>
                `)}
              </sl-select>
            `), html`
              <sl-skeleton 
                effect="pulse"
                style="width: 100%; height: 30px;"
              ></sl-skeleton>
            `)}
          </checkbox-option>
          <div class="label">
            Options
          </div>
          <sl-input
            type="number"
            label="Max Tokens"
            name="options.maxTokens"
            size="small"
            .value=${this.assistant.options.maxTokens.toString()}
            @sl-change=${this.onChange}
          ></sl-input>
          <sl-input
            type="number"  
            label="Temperature"
            name="options.temperature"
            size="small"
            .value=${this.assistant.options.temperature.toString()}
            @sl-change=${this.onChange}
          ></sl-input>
          <sl-input
            type="number"
            label="Top K"
            name="options.topK"
            size="small"
            .value=${this.assistant.options.topK.toString()}
            @sl-change=${this.onChange}
          ></sl-input>
          <sl-input
            type="number"
            label="Top P"
            name="options.topP"
            size="small"
            .value=${this.assistant.options.topP.toString()}
            @sl-change=${this.onChange}
          ></sl-input>
        </div>
      </div>
    `;
  }

  private loadAsync = async () => {
    try {
      if (this.key) {
        this.assistant = await Api.Assistant.get(this.key);
        this.status = 'updated';
      } else {
        this.assistant = AssistantEditor.DefaultAssistant;
        this.status = 'draft';
      }
    } catch (error: any) {
      this.error = error;
      this.status = 'error';
    }
  }

  private onChange = (event: Event) => {
    const target = event.target as any;
    const names = target.name.split('.');

    if (names[0] === 'service-model') {
      const [service, model] = target.value.split('/');
      this.assistant.service = service;
      this.assistant.model = model;
    } else if (names[0] === 'tools') {
      const checked = target.checked;
      this.assistant.tools = checked
        ? [...this.assistant.tools, names[1]]
        : this.assistant.tools.filter(t => t !== names[1]);
    } else if (names[0] === 'toolOptions') {
      (this.assistant as any)[names[0]] = {
        ...this.assistant.toolOptions,
        [names[1]]: target.value
      };
    } else if (names[0] === 'options') {
      (this.assistant as any)[names[0]] = {
        ...this.assistant.options,
        [names[1]]: Number(target.value)
      };
    } else {
      (this.assistant as any)[names[0]] = target.value;
    }

    if (this.debouncer) {
      clearTimeout(this.debouncer);
    } else {
      this.debouncer = setTimeout(this.onUpdate, 5_000);
    }
  }

  private onUpdate = async () => {
    if (!this.validate()) return;

    try {
      this.status = 'updating';
      this.assistant = await Api.Assistant.upsert(this.assistant);
      if (this.key) {
        this.status = 'updated';
      } else {
        goTo(`/assistant/${this.assistant.id}`);
      }
      this.debouncer = undefined;
    } catch (error: any) {
      App.alert(`Update Error: ${error}`);
    }
  }

  private validate = () => {
    if (!this.assistant.name) {
      this.error = 'Name is required.';
      this.status = 'error';
      return false;
    } else if (!this.assistant.service || !this.assistant.model) {
      this.error = 'Model is required.';
      this.status = 'error';
      return false;
    } else {
      return true;
    }
  }

  static styles = css`
    :host {
      width: 100%;
      height: 100%;
      display: flex;
      flex-direction: row;
      overflow: hidden;
    }

    .preview {
      width: 50%;
      height: 100%;
    }

    sl-divider {
      margin: 0;
    }

    .form {
      width: 50%;
      height: 100%;
      
      .header {
        width: 100%;
        height: 40px;
        display: flex;
        flex-direction: row;
        align-items: center;
        justify-content: space-between;
        gap: 16px;
        padding: 4px 48px;
        box-sizing: border-box;
        font-size: 14px;

        .status {
          display: flex;
          gap: 8px;
          align-items: center;
        }

        .flex {
          flex: 1;
        }
      }

      .body {
        width: 100%;
        height: calc(100% - 40px);
        display: flex;
        flex-direction: column;
        align-items: center;
        padding: 16px;
        gap: 8px;
        box-sizing: border-box;
        overflow-y: auto;

        .label {
          text-align: left;
          font-size: 14px;
        }

        & > * {
          width: 90%;
        }
      }
    }
  `;
}