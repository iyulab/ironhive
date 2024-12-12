import { LitElement, css, html } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import type { AssistantEntity } from "../../models";

@customElement('assistant-form')
export class AssistantForm extends LitElement {

  @property({ type: Object }) 
  assistant?: AssistantEntity;

  render() {
    return html`
      <form @submit=${this.handleSubmit}>
        <div class="form-group">
          <label for="name">Name</label>
          <input 
            type="text"
            id="name"
            .value=${this.assistant?.name || ''} 
            @change=${this.updateField('name')}
          />
        </div>

        <div class="form-group">
          <label for="description">Description</label>
          <textarea 
            id="description" 
            .value=${this.assistant?.description || ''} 
            @change=${this.updateField('description')}
          ></textarea>
        </div>

        <div class="form-group">
          <label for="instruction">Instruction</label>
          <textarea 
            id="instruction" 
            .value=${this.assistant?.instruction || ''} 
            @change=${this.updateField('instruction')}
          ></textarea>
        </div>

        <button type="submit" class="submit-button">Submit</button>
      </form>
    `;
  }

  private updateField(field: keyof AssistantEntity) {
    return (event: Event) => {
      const target = event.target as HTMLInputElement | HTMLTextAreaElement;
      this.assistant = {
        ...this.assistant,
        [field]: target.value
      };
    };
  }

  private handleSubmit(event: Event) {
    event.preventDefault();
    // Handle form submission logic here
  }
}