import { LitElement, css, html } from "lit";
import { customElement, state } from "lit/decorators.js";

@customElement('memory-page')
export class MemoryPage extends LitElement {
  @state()
  item: any = {};

  render() {
    return html`
      <main-layout ratio="1:2:2">
        <main-list 
          slot="left"
          key="id"
          .items=${[
            
          ]}
          @create=${(e: CustomEvent) => {

          }}
        ></main-list>
        <div slot="main">
          
        </div>
        <div slot="right">
          
        </div>
      </main-layout>
    `;
  }

  static styles = css`
    :host {
      display: block;
      width: 100%;
      height: 100%;
    }
  `;
}