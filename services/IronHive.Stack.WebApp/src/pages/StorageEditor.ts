import { LitElement, css, html } from "lit";
import { customElement, property, state } from "lit/decorators.js";

import type { SlButton } from "@shoelace-style/shoelace";
import type { CollectionEntity } from "../models";
import { Api, App, goTo } from "../services";

@customElement('storage-editor')
export class StorageEditor extends LitElement {
  private static readonly DefaultCollection: CollectionEntity = {
    name: '',
    description: '',
    handlerOptions: {
      chunk: {
        maxTokens: 2048
      }
    }
  }

  @property({ type: String }) key: string = '';

  @state() collection: CollectionEntity = StorageEditor.DefaultCollection;

  render() {
    return html`
      <div class="form">
        <sl-input
          label="Name"
          name="name"
          size="small"
          required
          .value=${this.collection.name || ''}
          @sl-change=${this.onChange}
        ></sl-input>
        <sl-textarea
          label="Description"
          name="description"
          size="small"
          required
          rows="2"
          help-text="Provide a clear and concise description of the storage to help AI assistants understand its purpose."
          .value=${this.collection.description || ''}
          @sl-change=${this.onChange}
        ></sl-textarea>
        <model-select
          label="Embedding Model"
          name="service-model"
          size="small"
          required
          .value=${this.getModelValue(this.collection)}
          @change=${this.onChange}
        ></model-select>
        <div class="label">
          Options
        </div>
        <checkbox-option
          label="Chunk Data"
          ?checked=${true}
          ?disabled=${true}
          help-text="Chunk data into smaller pieces by token count.">
          <sl-input
            type="number"
            name="handlerOptions.max_tokens"
            label="Max Chunk Token Size"
            size="small"
            value=${this.collection.handlerOptions.chunk.maxTokens || 0}
            required
            @sl-change=${this.onChange}
          ></sl-input>
        </checkbox-option>
        <checkbox-option
          label="Generate Summary"
          name="handlers.summary"
          ?checked=${this.collection.handlerOptions.summary !== undefined}
          help-text="Generate a summary for each chunk data"
          @change=${this.onChange}>
          <model-select
            type="chat"
            name="handlerOptions.summary"
            label="Text Model"
            size="small"
            required
            .value=${this.getModelValue(this.collection.handlerOptions.summary)}
            @change=${this.onChange}
          ></model-select>
        </checkbox-option>
        <checkbox-option
          label="Generate QnA"
          name="handlers.dialogue"
          ?checked=${this.collection.handlerOptions.dialogue !== undefined}
          help-text="Generate QnA pairs for each chunk data"
          @change=${this.onChange}>
          <model-select
            type="chat"
            name="handlerOptions.dialogue"
            label="Text Model"
            size="small"
            required
            .value=${this.getModelValue(this.collection.handlerOptions.dialogue)}
            @change=${this.onChange}
          ></model-select>
        </checkbox-option>
        <div class="control">
          <sl-button 
            @click=${this.onCancel}>
            Cancel
          </sl-button>
          <sl-button 
            variant="primary" 
            @click=${this.onSubmit}>
            Confirm
          </sl-button>
        </div>
      </div>
    `;
  }

  private onChange = (event: Event) => {
    const target = event.target as any;
    const names = target.name.split('.');

    if (names[0] === 'service-model') {
      const [provider, model] = target.value.split('/');
      this.collection.embedService = provider;
      this.collection.embedModel = model;
    } else if (names[0] === 'handlers') {
      if (target.checked) {
        (this.collection.handlerOptions as any)[names[1]] = {};
      } else {
        delete (this.collection.handlerOptions as any)[names[1]];
      }
    } else if (names[0] === 'handlerOptions') {
      if (names[1] === 'summary' || names[1] === 'dialogue') {
        const [provider, model] = target.value.split('/');
        (this.collection.handlerOptions as any)[names[1]] = {
          serviceKey: provider,
          modelName: model
        }
      } else if (names[1] === 'max_tokens') {
        (this.collection.handlerOptions.chunk as any)[names[1]] = parseInt(target.value);
      } else {
        (this.collection.handlerOptions as any)[names[1]] = target.value;
      }
    } else {
      (this.collection as any)[names[0]] = target.value;
    }
  }

  private onCancel = async () => {
    window.history.back();
  }

  private onSubmit = async (event: Event) => {
    const target = event.target as SlButton;
    if (target.loading) return;
    
    try {
      target.loading = true;
      if(!this.validate()) return;  
      const coll = await Api.Memory.upsertCollection(this.collection);
      goTo(`/storage/${coll.id}`);
    } catch (error: any) {
      App.alert(error);
    } finally {
      target.loading = false;
    }
  }

  private validate() {
    if (!this.collection.name) {
      App.alert('Name is required', 'warning');
      return false;
    } else if (!this.collection.description) {
      App.alert('Description is required', 'warning');
      return false;
    } else if (!this.collection.embedService || !this.collection.embedModel) {
      App.alert('Embedding Model is required', 'warning');
      return false;
    } else if (!this.collection.handlerOptions.chunk.maxTokens) {
      App.alert('Max Chunk Token Size is required', 'warning');
      return false;
    } else if (this.collection.handlerOptions.summary && (!this.collection.handlerOptions.summary.serviceKey || !this.collection.handlerOptions.summary.modelName)) {
      App.alert('Summary Model is required', 'warning');
      return false;
    } else if (this.collection.handlerOptions.dialogue && (!this.collection.handlerOptions.dialogue.serviceKey || !this.collection.handlerOptions.dialogue.modelName)) {
      App.alert('Dialogue Model is required', 'warning');
      return false;
    } else {
      return true;
    }
  }

  private getModelValue(data: any) {
    if (!data) 
      return '';
    if (data.embedService && data.embedModel) 
      return `${data.embedService}/${data.embedModel}`;    
    if (data.serviceKey && data.modelName) 
      return `${data.serviceKey}/${data.modelName}`;
    return '';
  }

  static styles = css`
    :host {
      width: 100%;
      display: flex;
      padding: 64px 16px;
      justify-content: center;
      align-items: center;
      box-sizing: border-box;
    }

    .form {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .label {
      display: contents;
      font-size: 14px;
    }

    .control {
      width: 100%;
      display: flex;
      flex-direction: row;
      gap: 16px;

      sl-button {
        width: 100%;
      }
    }
  `;
}