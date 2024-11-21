import { LitElement, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";
import type { Assistant } from "../backend/Models";

@customElement('assistant-form')
export class AssistantForm extends LitElement {
  @state() assistant?: Assistant;

  render() {
    return html`
      <form @submit=${this.handleSubmit}>
        <div class="form-group">
          <label for="name">Name</label>
          <input 
            type="text" 
            id="name" 
            .value=${this.assistant?.name || ''} 
            @input=${this.updateField('name')}
          />
        </div>

        <div class="form-group">
          <label for="description">Description</label>
          <textarea 
            id="description" 
            .value=${this.assistant?.description || ''} 
            @input=${this.updateField('description')}
          ></textarea>
        </div>

        <div class="form-group">
          <label for="instruction">Instruction</label>
          <textarea 
            id="instruction" 
            .value=${this.assistant?.instruction || ''} 
            @input=${this.updateField('instruction')}
          ></textarea>
        </div>

        <div class="form-group">
          <label for="toolKit">ToolKit</label>
          <input 
            type="checkbox" 
            id="toolKit" 
            .checked=${this.assistant?.toolKit?.includes('Vector Search') || false} 
            @change=${this.updateToolKit('Vector Search')}
          /> Vector Search
          <input 
            type="checkbox" 
            id="toolKit" 
            .checked=${this.assistant?.toolKit?.includes('Web Search') || false} 
            @change=${this.updateToolKit('Web Search')}
          /> Web Search
          <input 
            type="checkbox" 
            id="toolKit" 
            .checked=${this.assistant?.toolKit?.includes('Time Tool') || false} 
            @change=${this.updateToolKit('Time Tool')}
          /> Time Tool
        </div>

        <button type="submit" class="submit-button">Submit</button>
      </form>
    `;
  }

  private updateField(field: keyof Assistant) {
    return (event: Event) => {
      const target = event.target as HTMLInputElement | HTMLTextAreaElement;
      this.assistant = {
        ...this.assistant,
        [field]: target.value
      };
    };
  }

  private updateToolKit(tool: string) {
    return (event: Event) => {
      const target = event.target as HTMLInputElement;
      const toolKit = this.assistant?.toolKit || [];
      if (target.checked) {
        toolKit.push(tool);
      } else {
        const index = toolKit.indexOf(tool);
        if (index > -1) {
          toolKit.splice(index, 1);
        }
      }
      this.assistant = {
        ...this.assistant,
        toolKit
      };
    };
  }

  private handleSubmit(event: Event) {
    event.preventDefault();
    // Handle form submission logic here
  }
}