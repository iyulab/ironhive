import { LitElement, css, html } from "lit";
import { customElement, property, state, query } from "lit/decorators.js";
import type { Collection } from "../backend/Models";
import { API } from "../backend/ApiClient";

@customElement('storage-view')
export class StorageView extends LitElement {

  @query('#file-input') fileInput!: HTMLInputElement;
  @property({ type: Object }) collection?: Collection;
  @state() files: any[] = [];

  connectedCallback(): void {
    super.connectedCallback();
    this.loadCollection();
    this.loadFiles();
  }

  render() {
    return html`
      <input type="file" id="file-input" style="display: none" multiple />
      <div class="storage-view">
        <div class="header">
          <h2>${this.collection?.name || 'No Collection Selected'}</h2>
          <p>${this.collection?.description || ''}</p>
          <button @click=${this.uploadFile} class="upload-button">Upload</button>
        </div>
        <div class="file-list">
          ${this.files.map(file => html`
            <div class="file-item">
              <span>${file.name}</span>
            </div>
          `)}
        </div>
      </div>
    `;
  }

  private async loadCollection() {
    // Load the collection details
    // this.collection = await API.getCollection();
  }

  private async loadFiles() {
    // Load the files in the collection
    // this.files = await API.getFiles();
  }

  private uploadFile() {
    this.fileInput.click();
    this.fileInput.onchange = async () => {
      const files = this.fileInput.files;
      const file = files?.[0];
      if (files) {
        // Upload the file
        await API.uploadFile(this.collection?.id, file);
        this.loadFiles();
      }
    };
  }

  static styles = css`
    .storage-view {
      display: flex;
      flex-direction: column;
      height: 100%;
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