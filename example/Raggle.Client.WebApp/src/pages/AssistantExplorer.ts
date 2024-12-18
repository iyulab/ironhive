import { LitElement, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";

import type { AssistantEntity } from "../models";
import { Api } from "../services/ApiClient";
import { goTo } from "../services/AppUtility";

@customElement('assistant-explorer')
export class AssistantExplorer extends LitElement {

  @state() assistants: AssistantEntity[] = [];

  connectedCallback(): void {
    super.connectedCallback();
    this.loadAsync().then(a => this.assistants = a);
  }

  render() {
    return html`
      <div class="container">
        <div class="header">
          <sl-icon name="robot"></sl-icon>
          <span>Assistant</span>
          <div class="flex"></div>
          <sl-icon name="question-circle"></sl-icon>
        </div>

        <sl-divider></sl-divider>

        <div class="control">
          <div class="flex"></div>
          <sl-button size="small" @click=${this.createAsync}>
            Create New
            <sl-icon slot="suffix" name="plus-lg"></sl-icon>
          </sl-button>
        </div>

        <div class="grid">
          ${this.assistants?.map(a => html`
            <assistant-card
              .assistant=${a}
              @delete=${this.deleteAsync}
              @select=${this.selectAsync}
            ></assistant-card>
          `)}
        </div>
      </div>
    `;
  }

  private async loadAsync() {
    return await Api.getAssistantsAsync();
  }

  private async deleteAsync(event: CustomEvent<string>) {
    const id = event.detail;
    await Api.deleteAssistantAsync(id);
    this.assistants = this.assistants.filter(a => a.id !== id);
  }

  private async selectAsync(event: CustomEvent<string>) {
    const id = event.detail;
    window.location.href = `/assistant/${id}`;
  }

  private async createAsync() {
    goTo(`/assistant`);
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

    .container {
      min-width: 700px;
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .header {
      width: 100%;
      display: flex;
      flex-direction: row;
      align-items: flex-end;
      justify-content: space-between;
      font-size: 28px;
      padding: 0px 16px;
      line-height: 32px;
      gap: 14px;
      box-sizing: border-box;

      span {
        font-size: 32px;
        font-weight: 600;
        color: var(--sl-color-gray-800);
      }

      .flex {
        flex: 1;
      }
    }

    .control {
      width: 100%;
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
      gap: 16px;

      .flex {
        flex: 1;
      }
    }

    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 16px;
    }
  `;
}