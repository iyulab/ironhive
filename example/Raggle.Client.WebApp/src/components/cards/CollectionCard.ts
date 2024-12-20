import { css, html, LitElement, nothing } from "lit";
import { customElement, property } from "lit/decorators.js";

import type { CollectionEntity } from "../../models";
import { sinceEntity } from "../../services";

@customElement('collection-card')
export class CollectionCard extends LitElement {
  
  @property({ type: Object })
  collection?: CollectionEntity;

  render() {
    if (!this.collection) return nothing;

    return html`
      <div class="container"
        @click=${this.onSelect}>
        <div class="header">
          ${this.collection.name}
        </div>
        <div class="control">
          <sl-icon-button
            name="trash"
            @click=${this.onDelete}
          ></sl-icon-button>
        </div>
        <div class="description">
          ${this.collection.description}
        </div>
        <div class="meta">
          <strong>${this.collection.embedService}</strong>
          ${this.collection.embedModel}
        </div>
        <div class="date">
          ${sinceEntity(this.collection)}
        </div>
      </div>
    `;
  }

  private onSelect = async () => {
    this.dispatchEvent(new CustomEvent('select', { 
      detail: this.collection?.id
    }));
  }

  private onDelete = async (event: Event) => {
    event.stopPropagation();
    this.dispatchEvent(new CustomEvent('delete', { 
      detail: this.collection?.id
    }));
  }

  static styles = css`
    :host {
      display: contents;
    }

    .container {
      display: grid;
      grid-template-areas:
        "header control"
        "description description"
        "meta date";
      grid-template-columns: 70% 30%;
      grid-template-rows: 20% 60% 20%;
      padding: 16px;
      width: 400px;
      height: 200px;
      border: 1px solid var(--sl-color-gray-200);
      background-color: var(--sl-panel-background-color);
      border-radius: 8px;
      box-sizing: border-box;
      font-size: 18px;
      font-family: Arial, sans-serif;
      overflow: hidden;
      cursor: pointer;
    }
    .container:hover {
      scale: 1.05;
      background-color: var(--sl-color-gray-50);
    }
    .container:active {
      scale: 1.0;
    }

    .header {
      grid-area: header;
      align-content: center;
      font-size: 1em;
      font-weight: 600;
    }

    .control {
      grid-area: control;
      text-align: right;
    }

    .description {
      grid-area: description;
      font-size: 0.8em;
    }

    .meta {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      justify-content: flex-end;
      grid-area: meta;
      font-size: 0.8em;
    }

    .date {
      display: flex;
      align-items: flex-end;
      justify-content: flex-end;
      grid-area: date;
      font-size: 0.8em;
      text-align: right;
    }
  `;
}