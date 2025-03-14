export class SubmitMessageEvent extends CustomEvent<string> {
  constructor(value: string) {
    super('submit', {
      bubbles: false,
      composed: false,
      detail: value
    });
  }
}

declare global {
  interface HTMLElementEventMap {
    'submit': SubmitMessageEvent;
  }
}
