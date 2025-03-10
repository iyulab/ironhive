import { LitElement, css, nothing } from "lit";
import { customElement, property, } from "lit/decorators.js";
import { unsafeHTML } from "lit/directives/unsafe-html.js";

@customElement('hive-icon')
export class HiveIcon extends LitElement {
  public static basePath = '/assets/icons';

  @property({ type: String }) name?: string;
  @property({ type: String }) data?: string;

  protected async updated(_changedProperties: any) {
    if (_changedProperties.has('name') && this.name) {
      this.data = await this.fetchData(this.name);
    }
  }

  render() {
    const data = this.data?.trim();
    return data?.startsWith('<svg') 
      ? unsafeHTML(data)
      : nothing;
  }

  private async fetchData (name: string) {
    const path = HiveIcon.basePath.endsWith('/')
      ? `${HiveIcon.basePath}${name}.svg`
      : `${HiveIcon.basePath}/${name}.svg`;
    const res = await fetch(path);
    return await res.text();
  }
  
  static styles = css`
    :host {
      display: inline-flex;
      font-size: 16px;
      color: inherit;
    }

    svg {
      width: 1em;
      height: 1em;
      fill: currentColor;
    }
  `;
}
