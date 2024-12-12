import { LitElement, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";
import type { AssistantEntity } from "../models";
import { Api } from "../services/ApiClient";

@customElement('assistant-explorer')
export class AssistantExplorer extends LitElement {

  @state() 
  assistants: AssistantEntity[] = [];

  protected async firstUpdated(_changedProperties: any): Promise<void> {
    this.assistants = await this.loadAsync();
  }

  render() {
    return html`
      <div class="header">
        <sl-icon name="archive"></sl-icon>
        <span>Assistant</span>
        <div class="flex"></div>
        <sl-icon name="question-circle"></sl-icon>
      </div>
      <sl-divider></sl-divider>
      <div class="control">
        <sl-button
          size="small" variant="primary"
        >Create New</sl-button>
      </div>
      <div class="list">

      </div>
    `;
  }

  private async loadAsync() {
    return await Api.getAssistantsAsync();
  }

  private async deleteAsync(id: string) {
    await Api.deleteAssistantAsync(id);
    this.assistants = this.assistants.filter(a => a.id !== id);
  }

  private async createAsync() {
    const assistant = await Api.upsertAssistantAsync({
      provider: 'anthropic',
      model: 'claude-3-5-sonnet-20241022',
      name: 'Default Assistant',
      description: 'This assistant is created by default',
      instruction: 'You are useful assistant',
    });
    window.location.href = `/assistants/${assistant.id}`;
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