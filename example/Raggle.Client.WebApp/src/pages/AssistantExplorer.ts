import { LitElement, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";

import type { AssistantEntity } from "../models";
import { Api } from "../services/ApiClient";

@customElement('assistant-explorer')
export class AssistantExplorer extends LitElement {

  @state() 
  assistants: AssistantEntity[] = [];

  connectedCallback(): void {
    super.connectedCallback();
    this.loadAsync().then(a => this.assistants = a);
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
        <sl-button href="/storage/new">
          Create New
          <sl-icon slot="suffix" name="plus-lg"></sl-icon>
        </sl-button>
      </div>
      <div class="list">
        ${this.assistants?.map(a => html`
          <sl-details open>
            <span slot="summary">${a.name}</span>
            <span>${a.createdAt}</span>
            <span>${a.lastUpdatedAt}</span>
            <sl-button size="small" @click=${() => this.deleteAsync(a.id)}>Delete</sl-button>
          </sl-details>
        `)}
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
      display: flex;
      flex-direction: column;
      padding: 8px 48px;
    }

    .header {
      display: flex;
      flex-direction: row;
      align-items: flex-end;
      font-size: 24px;
      padding: 0px 16px;
      line-height: 32px;
      gap: 14px;

      span {
        font-size: 32px;
        font-weight: 600;
        color: var(--sl-color-gray-800);
      }

      .flex {
        flex: 1;
      }
    }
  `;
}