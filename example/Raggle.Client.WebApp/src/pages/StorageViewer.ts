import { LitElement, css, html } from "lit";
import { customElement, property, state } from "lit/decorators.js";

import type { CollectionEntity, DocumentEntity } from "../models";
import { Api, App } from "../services";

@customElement('storage-viewer')
export class StorageViewer extends LitElement {
  
  @property({ type: String })
  key: string = '';

  @state()
  collection?: CollectionEntity;

  @state()
  documents: DocumentEntity[] = [];

  @state()
  document?: DocumentEntity;

  @property({ type: Boolean, reflect: true })
  openPanel: boolean = false;

  @property({ type: Boolean, reflect: true })
  openUpload: boolean = true;

  connectedCallback(): void {
    super.connectedCallback();
    this.loadCollectionAsync(this.key).then(c => {
      this.collection = c;
    });
    this.loadDocumentAsync(this.key).then(d => {
      this.documents = d;
    });
  }

  render() {
    return html`
      <div class="header">
        <div class="name">
          ${this.collection?.name}
        </div>
        <div class="description">
          ${this.collection?.description}
        </div>
      </div>

      <sl-divider></sl-divider>

      <div class="control">
        <sl-button size="small"
          @click=${() => this.openUpload = true}>
          Upload
        </sl-button>
        <div class="flex"></div>
        <sl-input 
          type="search"
          placeholder="Search" 
          size="small"
        ></sl-input>
        <sl-button size="small"> 
          Delete
        </sl-button>
        <sl-button size="small"
          @click=${() => this.openPanel = !this.openPanel}>
          Panel
        </sl-button>
      </div>

      <div class="body" style="position: relative;">
        <div class="list">
          ${this.documents.map(d => html`
            <li>
              <span>${d.fileName}</span>
              <span>${d.fileSize} MB</span>
            </li>
          `)}
        </div>

        <div class="panel">
          Hello
        </div>
      </div>

      <file-uploader
        ?open=${this.openUpload}
        @upload=${this.onUpload}
        @close=${() => this.openUpload = false}
      ></file-uploader>
    `;
  }

  private loadCollectionAsync = async (key: string) => {
    return await Api.getCollectionAsync(key);
  }

  private loadDocumentAsync = async (key: string) => {
    return await Api.findDocumentsAsync(key);
  }

  private onUpload = async (e: CustomEvent<File[]>) => {
    const files = e.detail;
    if (!files || files.length === 0) {
      App.alert('File not selected', 'neutral');
    }
    await Api.uploadDocumentAsync(this.key, files);
  }

  static styles = css`
    :host {
      width: 100%;
      height: 100%;
      display: flex;
      flex-direction: column;
      padding: 32px 32px;
      box-sizing: border-box;
    }
    :host([openPanel]) .panel {
      display: block;
    }

    .header {
      display: flex;
      flex-direction: column;
      gap: 8px;
      height: 60px;
    }

    .control {
      display: flex;
      flex-direction: row;
      gap: 8px;
      height: 40px;

      .flex {
        flex: 1;
      }
    }

    .body {
      width: 100%;
      position: relative;
      display: flex;
      flex-direction: row;
      gap: 8px;
      height: calc(100% - 100px);

      .list {
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: 8px;
        overflow-y: auto;
        height: 100%;
        border: 1px solid var(--sl-panel-border-color);

        li {
          display: flex;
          flex-direction: row;
          justify-content: space-between;
          padding: 8px;
          background-color: var(--sl-panel-background-color);
          border-radius: var(--sl-border-radius-medium);
        }
      }

      .panel {
        width: 50%;
        display: none;
        position: absolute;
        top: 0;
        right: -50%;
        height: 100%;
        background-color: var(--sl-panel-background-color);
        border-left: 1px solid var(--sl-panel-border-color);
        box-shadow: -1px 0 0 0 var(--sl-panel-border-color);
      }
    }
  `;
}