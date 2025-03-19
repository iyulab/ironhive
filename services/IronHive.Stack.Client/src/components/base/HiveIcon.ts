import { LitElement, css, nothing } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { unsafeHTML } from "lit/directives/unsafe-html.js";
import { sys_icons } from "./IconLib";

@customElement('hive-icon')
export class HiveIcon extends LitElement {
  public static basePath = '/assets/icons';
  public static lib: Map<string, string> = sys_icons;
  
  @state() data?: string;

  @property({ type: String }) type: 'code' | 'url' = 'code';
  @property({ type: String }) name?: string;

  protected async updated(_changedProperties: any) {
    super.updated(_changedProperties);

    if (_changedProperties.has('name') && this.name) {
      if (this.type === 'code') {
        this.data = await this.resolveData(this.name);
      } else {
        this.data = await this.fetchData(this.name);
      }
    }
  }

  render() {
    const data = this.data?.trim();
    return data?.startsWith('<svg') 
      ? unsafeHTML(data)
      : nothing;
  }

  private async resolveData (name: string) {
    const data = HiveIcon.lib.get(name);
    if (data) {
      return data;
    }
    return await this.fetchData(name);
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
