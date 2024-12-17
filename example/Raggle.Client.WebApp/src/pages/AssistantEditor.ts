import { LitElement, css, html } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { until } from "lit/directives/until.js";

import type { AssistantEntity, CollectionEntity } from "../models";
import { Api } from "../services/ApiClient";
import { App } from "../services";

@customElement('assistant-editor')
export class AssistantEditor extends LitElement {
  private static readonly DefaultAssistant: AssistantEntity = {
    name: 'New Assistant',
    description: 'This is a new assistant.',
    instruction: 'You are helpful assistant.',
    options: {
      maxTokens: 2048,
      temperature: 0.7,
      topK: 50,
      topP: 0.9
    }
  }

  @property({ type: String }) key: string = '';

  @state() assistant: AssistantEntity = AssistantEditor.DefaultAssistant;
  @state() loading: boolean = false;

  connectedCallback(): void {
    super.connectedCallback();
    this.loadAsync(this.key).then(a => this.assistant = a);
  }

  render() {
    return html`
      <div class="header">
        <sl-icon name="robot"></sl-icon>
        <span>Assistant</span>
        <div class="flex"></div>
        <div class="control">
          <sl-button
            size="small"
            @click=${this.deleteAsync}>
            Delete
          </sl-button>
          <sl-button
            size="small"
            @click=${this.updateAsync}>
            Update
          </sl-button>
        </div>
      </div>
      <div class="form">
        <model-select
          type="chat"
          label="Model"
          required
          .value=${""}
        ></model-select>
        <sl-input
          label="Name"
          size="small"
          required
          .value=${this.assistant.name || ''}
          @sl-change=${this.updateAsync}
        ></sl-input>
        <sl-textarea
          label="Description"
          size="small"
          rows="2"
          .value=${this.assistant.description || ''}
          @sl-change=${this.updateAsync}
        ></sl-textarea>
        <sl-textarea
          label="Instruction"
          size="small"
          rows="4"
          .value=${this.assistant.instruction || ''}
          @sl-change=${this.updateAsync}
        ></sl-textarea>
        <div class="label">
          Tools
        </div>
        <checkbox-option
          label="Vector Search"
          ?checked=${true}>
          ${until(Api.findCollectionsAsync().then(collections => html`
            <sl-select
              size="small"
              ?hoist=${true}
              ?multiple=${true}
              ?clearable=${true}>
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
          required
          size="small"
          .value=${this.assistant.options.maxTokens.toString()}
          @sl-change=${this.updateAsync}
        ></sl-input>
        <sl-input
          label="Temperature"
          type="number"
          size="small"
          .value=${this.assistant.options.temperature.toString()}
          @sl-change=${this.updateAsync}
        ></sl-input>
        <sl-input
          label="Top K"
          type="number"
          size="small"
          .value=${this.assistant.options.topK.toString()}
          @sl-change=${this.updateAsync}
        ></sl-input>
        <sl-input
          label="Top P"
          type="number"
          size="small"
          .value=${this.assistant.options.topP.toString()}
          @sl-change=${this.updateAsync}
        ></sl-input>
      </div>
      <div class="preview">
        <chat-room 
          .assistant=${this.assistant}
        ></chat-room>
      </div>
    `;
  }

  private loadAsync = async (id: string) => {
    return await Api.getAssistantAsync(id);
  }

  private deleteAsync = async () => {
    if (!this.assistant.id) return;
    await Api.deleteAssistantAsync(this.assistant.id);
    window.history.pushState(null, '', '/assistants');
    window.dispatchEvent(new PopStateEvent('popstate'));
  }

  private updateAsync = async ()  => {
    if (!this.validate()) return;
    if (this.assistant) {
      this.assistant = await Api.upsertAssistantAsync(this.assistant);
    }
  }

  private validate = () => {
    console.log(this.assistant);
    if (!this.assistant.name) {
      App.alert('Name is required.');
      return false;
    } else if (!this.assistant.service || !this.assistant.model) {
      App.alert('Model is required.');
      return false;
    } else {
      return true;
    }
  }

  static styles = css`
    :host {
      width: 100%;
      height: 100%;
      display: grid;
      grid-template-areas:
        "header header"
        "form preview";
      grid-template-columns: 1fr 1fr;
      grid-template-rows: auto 1fr;
      overflow: hidden;
    }

    .header {
      grid-area: header;
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
      gap: 16px;
      padding: 8px;
      box-sizing: border-box;
      border-bottom: 1px solid var(--sl-color-gray-200);

      .flex {
        flex: 1;
      }
    }

    .form {
      grid-area: form;
      padding: 16px;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 16px;
      box-sizing: border-box;
      border-right: 1px solid var(--sl-color-gray-200);
      overflow-y: auto;

      .label {
        text-align: left;
        font-size: 14px;
      }

      & > * {
        width: 90%;
      }
    }

    .preview {
      grid-area: preview;
    }
  `;
}