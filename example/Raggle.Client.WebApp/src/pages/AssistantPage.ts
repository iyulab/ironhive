import { LitElement, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";
import type { AssistantEntity } from "../models";
import { API } from "../backend/ApiClient";

@customElement('assistant-page')
export class AssistantPage extends LitElement {

  @state() assistants?: AssistantEntity[] = undefined;
  @state() assistant?: AssistantEntity = undefined;

  connectedCallback(): void {
    super.connectedCallback();
    this.loadAssistants();
  }

  render() {
    return html`
      <main-layout ratio="1:2:2">
        <main-list
          create-label="Create Assistant"
          slot="left"
          key="assistantId"
          .items=${this.assistants || []}
          @create=${this.createAssistant}
          @select=${(e: CustomEvent) => this.loadAssistant(e.detail)}
          @delete=${(e: CustomEvent) => this.deleteAssistant(e.detail)}
        ></main-list>
        <main-chat 
          slot="right"
          .assistantId=${this.assistant?.assistantId || ''}
        ></main-chat>
        <div class="assistant-form" slot="main">
          <div class="info">Created: ${this.assistant?.createdAt}</div>
          <div class="info">Updated: ${this.assistant?.lastUpdatedAt}</div>
          <div class="input-group">
            <label for="name">Name</label>
            <input id="name" type="text" 
              .value=${this.assistant?.name || ''} 
              @change=${(e: Event) => this.changeAssistant(e)} />
          </div>
          <div class="input-group">
            <label for="description">Description</label>
            <textarea id="description" 
              .value=${this.assistant?.description || ''} 
              @change=${(e: Event) => this.changeAssistant(e)}
            ></textarea>
          </div>
          <div class="input-group">
            <label for="instruction">Instruction</label>
            <textarea id="instruction" 
              .value=${this.assistant?.instruction || ''} 
              @change=${(e: Event) => this.changeAssistant(e)}
            ></textarea>
          </div>
        </div>
      </main-layout>
    `;
  }

  private async loadAssistants() {
    this.assistants = await API.getAssistantsAsync();
    if (this.assistants.length > 0)
      this.assistant = this.assistants[0];
    else
      this.createAssistant();
  }

  private async loadAssistant(id: string) {
    this.assistant = await API.getAssistantAsync(id);
  }

  private async deleteAssistant(id: string) {
    await API.deleteAssistantAsync(id);
    if (this.assistant?.assistantId === id) {
      this.assistant = undefined;
    }
    await this.loadAssistants();
  }

  private async createAssistant() {
    this.assistant = await API.upsertAssistantAsync({
      name: 'Default Assistant',
      description: 'This assistant is created by default',
      instruction: 'You are useful assistant',
      settings: {
        provider: 'anthropic',
        model: 'claude-3-5-sonnet-20241022'
      }
    });
    await this.loadAssistants();
  }

  private changeAssistant = async (e: Event)  => {
    const target = e.target as HTMLElement;
    const propKey = target.id;
    const propValue = (target as any).value;
    if (this.assistant) {
      const updated = { ...this.assistant, [propKey]: propValue };
      this.assistant = await API.upsertAssistantAsync(updated);
      await this.loadAssistants();
    }
  }

  static styles = css`
    :host {
      display: block;
      width: 100%;
      height: 100%;
    }

    .assistant-form {
      display: flex;
      flex-direction: column;
      gap: 16px;
      padding: 24px;
    }

    .input-group {
      display: flex;
      flex-direction: column;
    }

    .input-group label {
      margin-bottom: 8px;
      font-weight: bold;
      color: #333333;
      cursor: pointer;
    }

    .input-group input,
    .input-group textarea {
      padding: 12px;
      border: 1px solid #cccccc;
      border-radius: 4px;
      font-size: 16px;
      resize: vertical;
      transition: border-color 0.3s;
    }

    .input-group input:focus,
    .input-group textarea:focus {
      border-color: #007bff;
      outline: none;
    }
  `;
}