import { LitElement, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";
import type { CollectionEntity } from "../models";
import { Api } from "../services/ApiClient";
import { until } from "lit/directives/until.js";

@customElement('storage-explorer')
export class StorageExplorer extends LitElement {

  @state() 
  collections?: CollectionEntity[] = [];

  connectedCallback(): void {
    super.connectedCallback();
    this.loadAsync().then(c => this.collections = c);
  }

  render() {
    return html`
      <div class="header">
        <sl-icon name="archive"></sl-icon>
        <span>Storage</span>
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
        ${this.collections?.map(c => html`
          <sl-details open>
            <span slot="summary">${c.name}</span>
            <span>${c.createdAt}</span>
            <span>${c.updatedAt}</span>
            <sl-button size="small" @click=${() => this.deleteAsync(c.id)}>Delete</sl-button>
          </sl-details>
        `)}
      </div>
    `;
  }

  private loadAsync = async () => {
    return await Api.findCollectionsAsync();
  }

  private deleteAsync =  async (id: string) => {
    await Api.deleteCollectionAsync(id);
    this.collections = this.collections?.filter(c => c.id !== id);
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
