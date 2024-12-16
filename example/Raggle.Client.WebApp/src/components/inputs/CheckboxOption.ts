import { LitElement, css, html } from "lit";
import { customElement, property } from "lit/decorators.js";

@customElement('checkbox-option')
export class CheckboxOption extends LitElement {

  @property({ type: String }) name: string = '';
  @property({ type: String }) label: string = '';
  @property({ type: String, attribute: 'help-text' }) helpText: string = '';
  @property({ type: Boolean, reflect: true }) disabled: boolean = false;
  @property({ type: Boolean, reflect: true }) checked: boolean = false;
  
  render() {
    return html`
      <label>
        <input type="checkbox"
          name=${this.name}
          ?checked=${this.checked}
          ?disabled=${this.disabled}
          @change=${this.onChange}/>
        ${this.label}
      </label>
      <div class="help-text">${this.helpText}</div>
      <div class="body">
        <slot></slot>
      </div>
    `;
  }

  private onChange = async (e: Event) => {
    this.checked = (e.target as HTMLInputElement).checked;
    this.dispatchEvent(new CustomEvent('change', { detail: this.checked }));
  }

  static styles = css`
    :host {
      display: flex;
      flex-direction: column;
      gap: 4px;
      padding: 16px;
      border: var(--sl-input-border-width) solid var(--sl-input-border-color);
      border-radius: var(--sl-input-border-radius-small);
      background-color: var(--sl-input-background-color);
      box-sizing: border-box;
    }
    :host([checked]) .body {
      max-height: 500px;
      overflow: auto;
    }

    label {
      display: flex;
      flex-direction: row;
      align-items: center;
      font-size: 14px;
      line-height: 1.5;
      gap: 8px;
      cursor: pointer;

      input {
        width: 1.2em;
        height: 1.2em;
        margin: 0;
      }
    }

    .help-text {
      font-size: var(--sl-input-help-text-font-size-small);
      color: var(--sl-input-help-text-color);
    }

    .body {
      max-height: 0;
      overflow: hidden;
      transition: max-height 0.3s ease;
    }
  `;
}