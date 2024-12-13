import { Theme } from "../models";

export class App {
  private static theme: Theme = localStorage.getItem('theme') as Theme || 'light';

  public static setTheme(theme: Theme) {
    this.theme = theme;
    localStorage.setItem('theme', theme);
  }

  public static alert(message: string, variant: 'primary' | 'success' | 'neutral' | 'warning' | 'danger' = 'danger') {
    const alert = Object.assign(document.createElement('sl-alert'), {
      variant,
      closable: true,
      duration: 3000,
      innerHTML: `<span>${message}</span>`
    });

    document.body.append(alert);
    return alert.toast();
  }
}
