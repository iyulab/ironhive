export class StopMessageEvent extends CustomEvent<undefined> {
  constructor() {
    super('stop', {
      bubbles: false,
      composed: false
    });
  }
}

declare global {
  interface HTMLElementEventMap {
    'stop': StopMessageEvent;
  }
}
