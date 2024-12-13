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
          size="small"
          name="name"
          required
          .value=${this.collection?.name || ''}
          @sl-change=${this.change}
        ></sl-input>
        <sl-textarea
          label="Description"
          size="small"
          name="description"
          required
          .value=${this.collection?.description || ''}
        ></sl-textarea>
        <model-select
          label="Embedding Model"
          size="small"
          required
          .value=${`${this.collection?.provider}/${this.collection?.model}` || ''}
          @model-change=${this.change}
        ></model-select>
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
    console.log(target);
    console.log(target.name);
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