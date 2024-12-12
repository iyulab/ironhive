import { LitElement, css, html } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import type { CollectionEntity, DocumentEntity } from "../models";

@customElement('storage-viewer')
export class StorageViewer extends LitElement {
  
  @property({ type: String })
  key: string = '';

  @state()
  collection?: CollectionEntity;

  @state()
  documents: DocumentEntity[] = [];

  protected async firstUpdated(_changedProperties: any): Promise<void> {
    await this.firstUpdated(_changedProperties);
    
  }

  render() {
    return html`
      <div class="container">
        <h2>${this.collection?.name}</h2>
        <ul>
          ${this.documents.map(d => html`
            <li>
              <span>${d.fileName}</span>
              <span>${d.fileSize} MB</span>
            </li>
          `)}
        </ul>
      </div>
    `;
  }

  static styles = css`
    :host {
      display: block;
      padding: 16px;
      font-family: Arial, sans-serif;
    }

    .storage-container {
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 16px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    h2 {
      margin-top: 0;
    }

    ul {
      list-style: none;
      padding: 0;
    }

    li {
      display: flex;
      justify-content: space-between;
      padding: 8px 0;
      border-bottom: 1px solid #eee;
    }

    li:last-child {
      border-bottom: none;
    }
  `;
}