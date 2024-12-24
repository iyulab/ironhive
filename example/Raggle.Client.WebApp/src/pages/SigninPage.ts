import { LitElement, html, css } from 'lit-element';
import { customElement, state } from 'lit/decorators';

@customElement('sign-in-page')
export class SignInPage extends LitElement {
  @state() email = '';
  @state() password = '';

  render() {
    return html`
      <div>
        <h1>Sign In</h1>
        <form @submit="${this.onSubmit}">
          <div>
            <label for="email">Email:</label>
            <input
              type="email"
              id="email"
              .value="${this.email}"
              @input="${(e: Event) => this.email = (e.target as HTMLInputElement).value}"
              required
            />
          </div>
          <div>
            <label for="password">Password:</label>
            <input
              type="password"
              id="password"
              .value="${this.password}"
              @input="${(e: Event) => this.password = (e.target as HTMLInputElement).value}"
              required
            />
          </div>
          <button type="submit">Log In</button>
        </form>
      </div>
    `;
  }

  private onSubmit = async (event: Event) => {
    event.preventDefault();
    // Handle sign-in logic here
    console.log('Email:', this.email);
    console.log('Password:', this.password);
  }

  static styles = css`
    /* Add your styles here */
  `;
}