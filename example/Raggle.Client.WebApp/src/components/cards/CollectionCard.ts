import { css, html, LitElement, nothing } from "lit";
import { customElement, property } from "lit/decorators.js";

import type { CollectionEntity } from "../../models";

@customElement('collection-card')
export class CollectionCard extends LitElement {
  
  @property({ type: Object })
  collection?: CollectionEntity;

  render() {
    if (!this.collection) return nothing;

    return html`
      <div class="content"
        @click=${() => this.onSelect(this.collection?.id || '')}>
        <div class="name">
          ${this.collection.name}
        </div>
        <div class="id">
          ${this.collection.id}
        </div>
        <p class="description">
          ${this.collection.description}
        </p>
      </div>
      <div class="meta">
        <div class="service">
          ${this.collection.embedService}/${this.collection.embedModel}
        </div>
        <div class="flex"></div>
        <div class="date">
          ${this.getDate()}
        </div>
        <sl-icon-button
          name="trash"
          @click=${() => this.onDelete(this.collection?.id || '')}
        ></sl-icon-button>
      </div>
    `;
  }

  private getDate() {
    if (!this.collection) return '';
    if (this.collection.lastUpdatedAt)
      return new Date(this.collection.lastUpdatedAt).toLocaleDateString();
    if (this.collection.createdAt)
      return new Date(this.collection.createdAt).toLocaleDateString();
    return '';
  }

  private onSelect = async (id: string) => {
    this.dispatchEvent(new CustomEvent('select', { detail: id }));
  }

  private onDelete = async (id: string) => {
    this.dispatchEvent(new CustomEvent('delete', { detail: id }));
  }

  static styles = css`
    :host {
      min-width: 340px;
      max-width: 700px;
      display: flex;
      flex-direction: column;
      padding: 16px 24px;
      background-color: var(--sl-color-neutral-0);
      border: 1px solid var(--sl-color-gray-200);
      border-radius: 8px;
      font-family: Arial, sans-serif;
      box-sizing: border-box;
      text-overflow: ellipsis;
      overflow: hidden;
      white-space: nowrap;
    }

    .content {
      display: flex;
      height: 120px;
      flex-direction: column;
      gap: 4px;
      color: var(--sl-color-gray-800);
      cursor: pointer;

      .name {
        font-size: 18px;
        font-weight: 600;
      }

      .id {
        font-size: 12px;
        color: var(--sl-color-gray-400);
      }

      .description {
        margin: 0;
        font-size: 16px;
        display: -webkit-box;
        -webkit-line-clamp: 3;
        -webkit-box-orient: vertical;
        white-space: normal;
        overflow: hidden;
      }
    }
    .content:hover {
      color: var(--sl-color-primary-500);
    }

    .meta {
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
      font-size: 14px;
      color: var(--sl-color-gray-400);

      .flex {
        flex: 1;
      }
    }
`;
}