import { LitElement, css, html } from "lit";
import { customElement, property, state, query } from "lit/decorators.js";
import type { Collection, Document } from "../backend/Models";
import { API } from "../backend/ApiClient";

@customElement('storage-view')
export class StorageView extends LitElement {

  @query('#query-input') queryInput!: HTMLInputElement;
  @query('#file-input') fileInput!: HTMLInputElement;
  @property({ type: Object }) collection?: Collection;
  @state() documents: Document[] = [];

  connectedCallback(): void {
    super.connectedCallback();
    this.loadDocuments();
  }

  render() {
    return html`
      <input type="file" id="file-input" style="display: none" multiple />
      <div class="storage-view">
        <div class="query">
          <input id="query-input" type="text" placeholder="Search" />
          <sl-button @click=${this.queryDocuments}>Search</sl-button>
        </div>
        <div class="header">
          <h2>${this.collection?.name || 'No Collection Selected'}</h2>
          <p>${this.collection?.description || ''}</p>
          <button @click=${this.uploadFile} class="upload-button">Upload</button>
        </div>
        <div class="file-list">
          ${this.documents.map(document => html`
            <div class="file-item">
              <span>${document.fileName}</span>
              <span>${document.fileSize} bytes</span>
              <sl-button
                @click=${() => this.deleteDocument(document)}
              >Delete</sl-button>
            </div>
          `)}
        </div>
      </div>
    `;
  }

  private async queryDocuments() {
    if (this.collection?.collectionId) {
      const query = this.queryInput.value;
      const response = await API.searchCollectionAsync(this.collection.collectionId, query);
      this.queryInput.value = '';
      console.log(response);
    }
  }

  private async loadDocuments() {
    console.log(this.collection?.collectionId);
    this.documents = await API.findDocumentsAsync(this.collection?.collectionId);
  }

  private uploadFile() {
    this.fileInput.click();
    this.fileInput.onchange = async () => {
      const file = this.fileInput.files?.[0];
      if (file && this.collection?.collectionId) {
        await API.uploadDocumentAsync(this.collection.collectionId, file);
        this.loadDocuments();
      }
    };
  }

  private async deleteDocument(document: Document) {
    await API.deleteDocumentAsync(this.collection?.collectionId, document.documentId);
    this.loadDocuments();
  }

  static styles = css`
    .storage-view {
      display: flex;
      flex-direction: column;
      height: 100%;
    }

    .query {
      display: flex;
      padding: 1em;
      gap: 1em;
    }

    .query input {
      flex: 1;
      padding: 0.5em;
      border: 1px solid #ddd;
      border-radius: 4px;
    }

    .query sl-button {
      padding: 0.5em 1em;
      background-color: #007BFF;
      color: white;
      border: none;
      cursor: pointer;
      border-radius: 4px;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1em;
      background-color: #f5f5f5;
      border-bottom: 1px solid #ddd;
    }
    .header h2 {
      margin: 0;
    }
    .header p {
      margin: 0;
      color: #666;
    }
    .upload-button {
      padding: 0.5em 1em;
      background-color: #007BFF;
      color: white;
      border: none;
      cursor: pointer;
      border-radius: 4px;
    }
    .upload-button:hover {
      background-color: #0056b3;
    }
    .file-list {
      flex: 1;
      overflow-y: auto;
      padding: 1em;
    }
    .file-item {
      padding: 0.5em 0;
      border-bottom: 1px solid #ddd;
    }
  `;
}