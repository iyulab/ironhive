import { LitElement, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";

import type { CollectionEntity } from "../models";
import { Api } from "../services/ApiClient";

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
      <div class="container">
        <div class="header">
          <sl-icon name="archive"></sl-icon>
          <span>Storage</span>
          <div class="flex"></div>
          <sl-icon name="question-circle"></sl-icon>
        </div>

        <sl-divider></sl-divider>

        <div class="control">
          <div class="flex"></div>
          <sl-button href="/storage/new">
            Create New
            <sl-icon slot="suffix" name="plus-lg"></sl-icon>
          </sl-button>
        </div>

        <div class="list">
          ${this.collections?.map(c => html`
            <collection-card
              .collection=${c}
              @select=${this.onSelect}
              @delete=${this.onDelete}
            ></collection-card>
          `)}
        </div>
      </div>
    `;
  }

  private loadAsync = async () => {
    return await Api.findCollectionsAsync();
  }

  private onDelete =  async (e: CustomEvent<string>) => {
    const id = e.detail;
    await Api.deleteCollectionAsync(id);
    this.collections = this.collections?.filter(c => c.id !== id);
  }

  private onSelect = async (e: CustomEvent<string>) => {
    const id = e.detail;
    window.location.href = `/storage/${id}`;
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

    .list {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }
  `;
}
