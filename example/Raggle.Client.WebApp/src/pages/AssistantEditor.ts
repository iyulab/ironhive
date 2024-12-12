import { LitElement, css, html } from "lit";
import { customElement, property, state } from "lit/decorators.js";

import { Api } from "../services/ApiClient";
import type { AssistantEntity } from "../models";

@customElement('assistant-editor')
export class AssistantEditor extends LitElement {

  @property({ type: String })
  key: string = '';

  @state()
  assistant?: AssistantEntity;

  protected async firstUpdated(_changedProperties: any): Promise<void> {
    await this.firstUpdated(_changedProperties);
    this.assistant = await this.loadAsync(this.key);
  }

  render() {
    return html`
      <div class="chat">
        <main-chat 
          .assistantId=${this.assistant?.id || ''}
        ></main-chat>
      </div>
      <div class="form">
        <div class="info">Created: ${this.assistant?.createdAt}</div>
        <div class="info">Updated: ${this.assistant?.lastUpdatedAt}</div>
        <div class="input-group">
          <label for="name">Name</label>
          <input id="name" type="text" 
            .value=${this.assistant?.name || ''} 
            @change=${(e: Event) => this.updateAsync(e)} />
        </div>
        <div class="input-group">
          <label for="description">Description</label>
          <textarea id="description" 
            .value=${this.assistant?.description || ''} 
            @change=${(e: Event) => this.updateAsync(e)}
          ></textarea>
        </div>
        <div class="input-group">
          <label for="instruction">Instruction</label>
          <textarea id="instruction" 
            .value=${this.assistant?.instruction || ''} 
            @change=${(e: Event) => this.updateAsync(e)}
          ></textarea>
        </div>
      </div>
    `;
  }

  private loadAsync = async (id: string) => {
    return await Api.getAssistantAsync(id);
  }

  private deleteAsync = async (id: string) => {
    await Api.deleteAssistantAsync(id);
    window.history.back();
  }

  private updateAsync = async (e: Event)  => {
    const target = e.target as HTMLElement;
    const propKey = target.id;
    const propValue = (target as any).value;
    if (this.assistant) {
      const updated = { ...this.assistant, [propKey]: propValue };
      this.assistant = await Api.upsertAssistantAsync(updated);
    }
  }

  static styles = css`
    .page-container {
      padding: 1em;
    }
    h1 {
      font-size: 2em;
      margin-bottom: 1em;
    }
  `;
}