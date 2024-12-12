import { LitElement, css, html } from "lit";
import { customElement, property, query, state } from "lit/decorators.js";
import type { CollectionEntity } from "../models";
import { serialize } from '@shoelace-style/shoelace/dist/utilities/form.js';

@customElement('storage-editor')
export class StorageEditor extends LitElement {

  @query('form')
  form!: HTMLFormElement;

  @state()
  collection?: CollectionEntity;

  render() {
    return html`
      <form class="form">
        <sl-input
          label="Name"
          name="name"
          required
          .value=${this.collection?.name || ''}
        ></sl-input>
        <sl-textarea
          label="Description"
          name="description"
          required
          .value=${this.collection?.description || ''}
        ></sl-textarea>
        <sl-select
          label="Embedding Model"
          name="embeddingModel"
          required
          @sl-change=${this.change}
        >
          <small>Section 1</small>
          <sl-option value="option-1">Option 1</sl-option>
          <sl-option value="option-2">Option 2</sl-option>
          <sl-option value="option-3">Option 3</sl-option>
          <sl-divider></sl-divider>
          <small>Section 2</small>
          <sl-option value="option-4">Option 4</sl-option>
          <sl-option value="option-5">Option 5</sl-option>
          <sl-option value="option-6">Option 6</sl-option>
        </sl-select>
        <sl-details summary="Options" open>
          <sl-checkbox name="option-1">Option 1</sl-checkbox>
          <sl-checkbox name="option-2">Option 2</sl-checkbox>
          <sl-checkbox name="option-3">Option 3</sl-checkbox>
        </sl-details>
        <div class="control">
          <sl-button @click=${this.submit}>
            Cancel
          </sl-button>
          <sl-button variant="primary" @click=${this.submit}>
            Confirm
          </sl-button>
        </div>
      </form>
    `;
  }

  private submit = async () => {
    const formData = serialize(this.form);
    console.log(formData);
  }

  private change(event: Event) {
    const target = event.target as HTMLSelectElement;
    console.log(target.value);
  }

  static styles = css`
    :host {
      width: 100%;
      display: flex;
      justify-content: center;
      align-items: center;
    }

    .form {
      min-width: 500px;
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .control {
      width: 100%;
      display: flex;
      flex-direction: row;
      gap: 16px;

      sl-button {
        width: 100%;
      }
    }
  `;
}