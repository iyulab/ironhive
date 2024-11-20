import { LitElement, css, html } from "lit";
import { customElement, property } from "lit/decorators.js";

@customElement('main-list')
export class MainList extends LitElement {

  @property({ type: String }) key: string = 'id';
  @property({ type: Array }) items: any[] = [];
  
  render() {
    return html`
      <div class="new-button" @click=${this.onCreate}>
        Create New +
      </div>
      <div class="list">
        ${this.items.map(item => html`
          <div class="list-item">
            <div class="item-name" @click=${() => this.onSelect(item)}>
              ${item.name}
            </div>
            <div class="delete-button" @click=${(e: Event) => this.onDelete(e, item)}>
              &times;
            </div>
          </div>
        `)}
      </div>
    `;
  }

  private onCreate = async () => {
    this.dispatchEvent(new CustomEvent('create'));
  };

  private onSelect = async (item: any) => {
    const key = item[this.key];
    this.dispatchEvent(new CustomEvent('select', { detail: key }));
  }

  private onDelete = async (event: Event, item: any) => {
    event.stopPropagation();
    const key = item[this.key];
    this.dispatchEvent(new CustomEvent('delete', { detail: key }));
  }

  static styles = css`
    :host {
      display: block;
      width: 100%;
      height: 100%;
      padding: 16px;
      box-sizing: border-box;
      font-family: Arial, sans-serif;
    }

    .new-button {
      display: flex;
      justify-content: center;
      align-items: center;
      background-color: #0078d4;
      color: white;
      width: 100%;
      height: 40px;
      border-radius: 4px;
      cursor: pointer;
      font-weight: bold;
      box-sizing: border-box;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
      transition: background-color 0.3s, box-shadow 0.3s;
    }
    .new-button:hover {
      background-color: #005a9e;
      box-shadow: 0 4px 8px rgba(0, 0, 0, 0.3);
    }
    .new-button:active {
      background-color: #004377;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
    }

    .list {
      margin-top: 16px;
      display: flex;
      flex-direction: column;
      box-sizing: border-box;
      gap: 0;
      width: 100%;
      height: calc(100% - 56px);
      overflow-y: auto;
      border: 1px solid #ddd;
      border-radius: 4px;
    }

    .list-item {
      position: relative;
      display: flex;
      flex-direction: row;
      justify-content: space-between;
      align-items: center;
      height: 50px;
      padding: 0 16px;
      cursor: pointer;
      background-color: white;
      transition: transform 0.2s, box-shadow 0.2s, background-color 0.2s;
    }
    .list-item + .list-item {
      box-sizing: border-box;
      border-top: 1px solid #ccc;
    }
    .list-item:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
      background-color: #f9f9f9;
    }
    .list-item:active {
      background-color: #e0e0e0;
      transform: translateY(0);
      box-shadow: none;
    }

    .item-name {
      flex: 1;
      font-size: 16px;
      color: #333;
      user-select: none;
    }

    .delete-button {
      width: 24px;
      height: 24px;
      display: flex;
      justify-content: center;
      align-items: center;
      background-color: #ff4d4f;
      color: white;
      border-radius: 50%;
      cursor: pointer;
      transition: background-color 0.3s, transform 0.2s;
      font-size: 16px;
      line-height: 1;
    }
    .delete-button:hover {
      background-color: #ff7875;
      transform: scale(1.1);
    }
    .delete-button:active {
      background-color: #d9363e;
      transform: scale(1);
    }
  `;
}
