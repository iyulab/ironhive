export class SendMessageEvent extends CustomEvent<string> {
  constructor(value: string) {
    super('send', {
      bubbles: true,
      composed: true,
      detail: value
    });
  }
}

declare global {
  interface HTMLElementEventMap {
    'send': SendMessageEvent;
  }
}
