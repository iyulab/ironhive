import { LitElement, css, html } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import type { Collection } from "../backend/Models";
import { API } from "../backend/ApiClient";

@customElement('collection-form')
export class CollectionForm extends LitElement {

  @state() 
  collection: Collection = {
    name: '',
    description: '',
    embedServiceKey: '',
    embedModelName: '',
    handlerOptions: {
      "GenerateQA": {
        "ServiceKey": "OpenAI",
        "ModelName": "gpt-4o-mini",
      },
      "Embeddings": {
        "ServiceKey": "OpenAI",
        "ModelName": "text-embedding-ada-002",
      }
    },
  };

  render() {
    return html`
      <form @submit=${this.handleSubmit}>
        <div class="input-group">
          <label for="name">Name</label>
          <input
            type="text" 
            id="name" 
            .value=${this.collection.name || ''} 
            @input=${this.updateField('name')}
          />
        </div>

        <div class="input-group">
          <label for="description">Description</label>
          <textarea 
            id="description" 
            .value=${this.collection.description || ''} 
            @input=${this.updateField('description')}
          ></textarea>
        </div>

        <model-select
          type="embed"
          @change=${this.updateModelField}
        ></model-select>

        <!-- 숨겨진 필드 -->
        <input type="hidden" name="id" .value=${this.collection.collectionId || ''} />
        <input type="hidden" name="createdAt" .value=${this.collection.createdAt || ''} />
        <input type="hidden" name="lastUpdatedAt" .value=${this.collection.lastUpdatedAt || ''} />

        <button type="submit" class="submit-button">Submit</button>
      </form>
    `;
  }

  private updateField(field: keyof Collection) {
    return (event: Event) => {
      const target = event.target as HTMLInputElement | HTMLTextAreaElement;
      this.collection = {
        ...this.collection,
        [field]: target.value
      };
    };
  }

  private updateModelField(event: CustomEvent) {    
    this.collection = {
      ...this.collection,
      embedServiceKey: event.detail.provider,
      embedModelName: event.detail.model
    };
  }

  private async handleSubmit(event: Event) {
    event.preventDefault();
    await API.upsertCollectionAsync(this.collection);
    this.dispatchEvent(new CustomEvent('submit'));
  }

  static styles = css`
    .form-group {
      margin-bottom: 1em;
    }
    label {
      display: block;
      margin-bottom: 0.5em;
      font-weight: bold;
    }
    input, textarea, select {
      width: 100%;
      padding: 0.5em;
      box-sizing: border-box;
    }
    .submit-button {
      padding: 0.75em 1.5em;
      background-color: #007BFF;
      color: white;
      border: none;
      cursor: pointer;
      border-radius: 4px;
    }
    .submit-button:hover {
      background-color: #0056b3;
    }
  `;
}
