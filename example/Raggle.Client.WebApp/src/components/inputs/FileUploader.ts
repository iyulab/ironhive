import { LitElement, css, html } from "lit";
import { customElement, property, state } from "lit/decorators.js";

@customElement('file-uploader')
export class FileUploader extends LitElement {

  @property({ type: Boolean, reflect: true }) open: boolean = false;
  @property({ type: Boolean, reflect: true }) loading: boolean = false;

  @state() files: File[] = [];

  render() {
    return html`
      <sl-dialog label="Upload Files"
        ?open=${this.open} 
        @sl-hide=${this.onHide}>
        <div class="drop-zone"
          @click=${this.onClick}
          @drop=${this.onDrop} 
          @dragover=${this.onDragOver}
          @dragleave=${this.onDragLeave}>
          Drop files here OR click to upload
        </div>
        <div class="files">
          ${this.files.map((file) => {
            return html`
              <div class="item">
                <div class="name">
                  ${file.name}
                </div>
                <sl-icon-button 
                  name="trash"
                  @click=${() => this.onDelete(file)}
                ></sl-icon-button>
              </div>
            `;
          })}
        </div>
        <div class="control" slot="footer">
          <sl-button size="small" 
            @click=${this.onHide}>
            Cancel
          </sl-button>
          <sl-button size="small" 
            variant="primary"
            @click=${this.onUpload}>
            Upload
          </sl-button>
        </div>
      </sl-dialog>
    `;
  }

  private onDragOver = (event: DragEvent) => {
    event.preventDefault();
  }

  private onDragLeave = (event: DragEvent) => {
    event.preventDefault();
  }

  private onDrop = (event: DragEvent) => {
    event.preventDefault();
    const files = Array.from(event.dataTransfer?.files || []);
    this.files = [...this.files, ...files];
  }

  private onDelete = async (file: File) => {
    this.files = this.files.filter(f => f !== file);
  }

  private onClick = async () => {
    const input = document.createElement('input');
    input.type = 'file';
    input.multiple = true;
    input.accept = '*/*';
    input.addEventListener('change', this.onSelect.bind(this));
    input.click();
  }

  private onSelect(event: Event) {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files || []);
    this.files = [...this.files, ...files];
  }

  private onHide = async () => {
    this.dispatchEvent(new CustomEvent('close'));
  }

  private onUpload = async () => {
    this.dispatchEvent(new CustomEvent('upload', { detail: this.files }));
  }

  static styles = css`
    sl-dialog::part(panel) {
      width: 500px;
      height: 500px;
    }
    sl-dialog::part(body) {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .drop-zone {
      width: 100%;
      height: 100px;
      border: 2px dashed var(--sl-color-gray-300);
      display: flex;
      align-items: center;
      justify-content: center;
      box-sizing: border-box;
      cursor: pointer;
    }
    
    .files {
      height: calc(100% - 100px);
      display: flex;
      flex-direction: column;
      border: 1px solid var(--sl-color-gray-300);
      font-size: 14px;
      overflow: auto;

      .item {
        display: flex;
        flex-direction: row;
        align-items: center;
        justify-content: space-between;
        gap: 4px;
        padding: 0px 8px;
        box-sizing: border-box;
      }
    }

    .control {
      display: flex;
      flex-direction: row;
      gap: 8px;
      justify-content: flex-end;
    }
  `;
}