import { Theme } from "../models";

export class AppStore {
  private static theme: Theme = localStorage.getItem('theme') as Theme || 'light';

  public static setTheme(theme: Theme) {
    this.theme = theme;
    localStorage.setItem('theme', theme);
  }
}
