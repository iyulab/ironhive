import { LitElement, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";
import { Api } from "../services";

interface FileItem {
  name: string;
  size: number;
  type: string;
  uploadTime: string;
  id?: string;
  url?: string;
}

@customElement('file-page')
export class FilePage extends LitElement {
  @state() files: FileItem[] = [];
  @state() uploading: boolean = false;
  @state() message: string = '';
  @state() progress: number = 0;

  render() {
    return html` 
      <div class="container">
        <h2>File Upload & Download Test</h2>
        
        <div class="upload-section">
          <input 
            type="file" 
            id="fileInput" 
            @change=${this.handleFileChange} 
            ?disabled=${this.uploading}
            multiple
          />
          <button 
            @click=${this.uploadFiles} 
            ?disabled=${this.uploading}>
            ${this.uploading ? 'Uploading...' : 'Upload Files'}
          </button>
        </div>

        ${this.uploading ? html`
          <div class="progress-bar">
            <div class="progress" style="width: ${this.progress}%"></div>
          </div>
        ` : ''}
        
        ${this.message ? html`<div class="message">${this.message}</div>` : ''}
        
        <div class="file-list">
          <h3>Uploaded Files</h3>
          ${this.files.length === 0 
            ? html`<p>No files uploaded yet.</p>` 
            : html`
              <table>
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Type</th>
                    <th>Size</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  ${this.files.map(file => html`
                    <tr>
                      <td>${file.name}</td>
                      <td>${file.type}</td>
                      <td>${this.formatFileSize(file.size)}</td>
                      <td>
                        <button @click=${() => this.downloadFile(file)}>Download</button>
                        <button @click=${() => this.deleteFile(file)}>Delete</button>
                      </td>
                    </tr>
                  `)}
                </tbody>
              </table>
            `}
        </div>
      </div>
    `;
  }

  private handleFileChange(e: Event) {
    const input = e.target as HTMLInputElement;
    this.message = input.files ? `${input.files.length} file(s) selected` : '';
  }

  private async uploadFiles() {
    const input = this.shadowRoot?.querySelector('#fileInput') as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      this.message = 'Please select files to upload';
      return;
    }

    this.uploading = true;
    this.progress = 0;
    this.message = 'Uploading files...';

    try {
      const formData = new FormData();
      Array.from(input.files).forEach(file => {
        formData.append('files', file);
      });

      for await (const res of Api.upload(formData)) {
        if (res.type === 'progress') {
          this.progress = res.progress;
        } else if (res.type === 'success') {
          this.message = `Uploaded ${res.body} successfully`;
        } else if (res.type === 'failure') {
          this.message = `Upload failed: ${res.message}`;
        }
      }

      // Add uploaded files to the list
      const newFiles: FileItem[] = Array.from(input.files).map(file => ({
        name: file.name,
        size: file.size,
        type: file.type,
        uploadTime: new Date().toISOString(),
        id: Math.random().toString(36).substr(2, 9)
      }));

      this.files = [...this.files, ...newFiles];
      this.message = `${input.files.length} file(s) uploaded successfully`;
      
      // Reset input
      input.value = '';
    } catch (error) {
      this.message = `Upload failed: ${error instanceof Error ? error.message : 'Unknown error'}`;
    } finally {
      setTimeout(() => {
        this.uploading = false;
      }, 500);
    }
  }


  private async downloadFile(file: FileItem) {
    try {
      this.message = `Downloading ${file.name}...`;
      Api.download(file.name);
      this.message = `Downloaded ${file.name} successfully`;
    } catch (error) {
      this.message = `Download failed: ${error instanceof Error ? error.message : 'Unknown error'}`;
    }
  }

  private async deleteFile(file: FileItem) {
    try {
      this.message = `Deleting ${file.name}...`;
      
      // Replace with actual API call
      // await Api.deleteFile(file.id);
      
      // Simulate API call
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      // Remove file from list
      this.files = this.files.filter(f => f.id !== file.id);
      this.message = `Deleted ${file.name} successfully`;
    } catch (error) {
      this.message = `Delete failed: ${error instanceof Error ? error.message : 'Unknown error'}`;
    }
  }

  private formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  static styles = css`
    :host {
      width: 100%;
      height: 100%;
    }

    .container {
      position: relative;
      display: flex;
      flex-direction: column;
      width: 100%;
      height: 100%;
      min-width: 320px;
      min-height: 480px;
      color: var(--hs-text-color);
      background-color: var(--hs-background-color);
      overflow: hidden;
      padding: 20px;
      box-sizing: border-box;
    }

    h2, h3 {
      margin-top: 0;
      margin-bottom: 20px;
    }

    .upload-section {
      display: flex;
      margin-bottom: 20px;
      gap: 10px;
    }

    button {
      padding: 8px 16px;
      cursor: pointer;
      background-color: #4285f4;
      color: white;
      border: none;
      border-radius: 4px;
    }

    button:hover {
      background-color: #3367d6;
    }

    button:disabled {
      background-color: #cccccc;
      cursor: not-allowed;
    }

    .progress-bar {
      width: 100%;
      height: 10px;
      background-color: #e0e0e0;
      border-radius: 5px;
      margin-bottom: 20px;
      overflow: hidden;
    }

    .progress {
      height: 100%;
      background-color: #4285f4;
      transition: width 0.3s ease;
    }

    .message {
      margin-bottom: 20px;
      padding: 10px;
      border-radius: 4px;
      background-color: #f8f9fa;
      border-left: 4px solid #4285f4;
    }

    table {
      width: 100%;
      border-collapse: collapse;
    }

    th, td {
      padding: 8px 12px;
      text-align: left;
      border-bottom: 1px solid #e0e0e0;
    }

    th {
      background-color: #f8f9fa;
      font-weight: 500;
    }

    .file-list {
      flex: 1;
      overflow: auto;
    }
  `;
}