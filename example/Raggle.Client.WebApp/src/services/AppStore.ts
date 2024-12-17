import { Theme } from "../models";

export class App {
  public static setTheme(theme: Theme) {
    localStorage.setItem('app-theme', theme);
    if (theme === 'light') 
      document.body.classList.remove('sl-theme-dark');
    if (theme === 'dark' && !document.body.classList.contains('sl-theme-dark'))
      document.body.classList.add('sl-theme-dark');
  }

  public static getTheme() {
    const theme = localStorage.getItem('app-theme');
    if (theme) {
      return theme as Theme;
    } else {
      this.setTheme('light');
      return 'light';
    }
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
