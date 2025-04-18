export class SubmitMessageEvent extends CustomEvent<string> {
  constructor(value: string) {
    super('submit', {
      bubbles: false,
      composed: false,
      detail: value
    });
  }
}

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
    'submit': SubmitMessageEvent;
  }
}

declare global {
  interface HTMLElementEventMap {
    'stop': StopMessageEvent;
  }
}