import { LitElement, css, html } from "lit";
import { customElement, query, state } from "lit/decorators.js";

import type { SlButton } from "@shoelace-style/shoelace";
import type { CollectionEntity } from "../models";
import type { ModelValue } from "../components";
import { Api, App } from "../services";

@customElement('storage-editor')
export class StorageEditor extends LitElement {

  @query('form')
  form!: HTMLDivElement;

  @state()
  collection: CollectionEntity = {
    name: '',
    description: '',
    handlerOptions: {
      chunk: {
        maxTokens: 2048
      }
    }
  };

  render() {
    return html`
      <div class="form">
        <sl-input
          label="Name"
          size="small"
          required
          .value=${this.collection.name || ''}
          @sl-change=${this.changeName}
        ></sl-input>
        <sl-textarea
          label="Description"
          size="small"
          required
          rows="2"
          help-text="Provide a clear and concise description of the storage to help AI assistants understand its purpose."
          .value=${this.collection.description || ''}
          @sl-change=${this.changeDescription}
        ></sl-textarea>
        <model-select
          label="Embedding Model"
          size="small"
          required
          .value=${this.getModelValue(this.collection)}
          @model-change=${this.changeEmbedModel}
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
            label="Max Chunk Token Size"
            size="small"
            value=${this.collection.handlerOptions?.chunk.maxTokens || 0}
            required
            @sl-change=${this.changeChunkOption}
          ></sl-input>
        </checkbox-option>
        <checkbox-option
          label="Generate Summary"
          ?checked=${this.collection.handlerOptions?.summary !== undefined}
          help-text="Generate a summary for each chunk data"
          @change=${this.changeSummayCheck}>
          <model-select
            type="chat"
            label="Text Model"
            size="small"
            required
            .value=${this.getModelValue(this.collection.handlerOptions?.summary)}
            @model-change=${this.changeSummaryOption}
          ></model-select>
        </checkbox-option>
        <checkbox-option
          label="Generate QnA"
          ?checked=${this.collection.handlerOptions?.dialogue !== undefined}
          help-text="Generate QnA pairs for each chunk data"
          @change=${this.changeDialogueCheck}>
          <model-select
            type="chat"
            label="Text Model"
            size="small"
            required
            .value=${this.getModelValue(this.collection.handlerOptions?.dialogue)}
            @model-change=${this.changeDialogueOption}
          ></model-select>
        </checkbox-option>
        <div class="control">
          <sl-button 
            @click=${this.cancel}>
            Cancel
          </sl-button>
          <sl-button 
            variant="primary" 
            @click=${this.submit}>
            Confirm
          </sl-button>
        </div>
      </div>
    `;
  }

  private cancel = async () => {
    window.history.back();
  }

  private submit = async (event: Event) => {
    const target = event.target as SlButton;
    
    try {
      target.loading = true;
      if(!this.validate()) return;  
      const coll = await Api.upsertCollectionAsync(this.collection);
      window.location.href = `/storages/${coll.id}`;
    } catch (error: any) {
      App.alert(error);
    } finally {
      target.loading = false;
    }
  }

  private changeName = (event: Event) => {
    const target = event.target as HTMLInputElement;
    this.collection.name = target.value;
  }

  private changeDescription = (event: Event) => {
    const target = event.target as HTMLInputElement;
    this.collection.description = target.value;
  }

  private changeEmbedModel = (event: CustomEvent<ModelValue>) => {
    const value = event.detail;
    this.collection.embedService = value.provider;
    this.collection.embedModel = value.model;
  }

  private changeChunkOption = (event: Event) => {
    const target = event.target as HTMLInputElement;
    this.collection.handlerOptions.chunk.maxTokens = parseInt(target.value);
  }

  private changeSummayCheck = (event: CustomEvent<boolean>) => {
    const value = event.detail;
    if (value === false) {
      delete this.collection.handlerOptions.dialogue;
    }
  }

  private changeSummaryOption = (event: CustomEvent<ModelValue>) => {
    const value = event.detail;
    this.collection.handlerOptions.summary = {
      serviceKey: value.provider,
      modelName: value.model
    }
  }

  private changeDialogueCheck = (event: CustomEvent<boolean>) => {
    const value = event.detail;
    if (value === false) {
      delete this.collection.handlerOptions.dialogue;
    }
  }

  private changeDialogueOption = (event: CustomEvent<ModelValue>) => {
    const value = event.detail;
    this.collection.handlerOptions.dialogue = {
      serviceKey: value.provider,
      modelName: value.model
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
    }
    return true;
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