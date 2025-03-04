import { LitElement, html, css } from 'lit-element';
import { customElement, state } from 'lit/decorators';

@customElement('sign-up-page')
export class SignUpPage extends LitElement {
  @state() email = '';
  @state() password = '';
  @state() confirmPassword = '';

  render() {
    return html`
      <div>
        <h1>Sign Up</h1>
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
          <div>
            <label for="confirmPassword">Confirm Password:</label>
            <input
              type="password"
              id="confirmPassword"
              .value="${this.confirmPassword}"
              @input="${(e: Event) => this.confirmPassword = (e.target as HTMLInputElement).value}"
              required
            />
          </div>
          <button type="submit">Sign Up</button>
        </form>
      </div>
    `;
  }

  private onSubmit = async (event: Event) => {
    event.preventDefault();
    if (this.password !== this.confirmPassword) {
      console.error('Passwords do not match');
      return;
    }
    // Handle sign-up logic here
    console.log('Email:', this.email);
    console.log('Password:', this.password);
  }

  static styles = css`
    /* Add your styles here */
  `;
}