// storage/app-storage.ts
import { Theme } from "@iyulab/chat-components";
import type { Message, ModelSummary } from "../services";

export class AppStorage {
  private static readonly PREFIX = 'ih-';

  public static get theme(): Theme {
    return this.getItem<Theme>('theme') || 'light';
  }
  public static set theme(value: Theme) {
    this.setItem('theme', value);
  }

  public static get model(): ModelSummary | undefined {
    return this.getItem<ModelSummary>('model');
  }
  public static set model(value: ModelSummary | undefined) {
    this.setItem('model', value);
  }

  public static get messages(): Message[] {
    return this.getItem<Message[]>('messages') || [];
  }
  public static set messages(value: Message[]) {
    this.setItem('messages', value);
  }

  public static get usages(): number {
    return this.getItem<number>('usages') || 0;
  }
  public static set usages(value: number) {
    this.setItem('usages', value);
  }

  public static get thinking(): 'low' | 'medium' | 'high' | 'none' {
    return this.getItem<'low' | 'medium' | 'high' | 'none'>('thinking') || 'none';
  }
  public static set thinking(value: 'low' | 'medium' | 'high' | 'none') {
    this.setItem('thinking', value);
  }

  /** 
   * 키에 해당하는 값을 저장합니다. JSON 형식의 데이터는 자동으로 문자열화합니다.
   */
  static setItem<T>(key: string, value: T): void {
    try {
      if (value === undefined || value === null) {
        localStorage.removeItem(`${this.PREFIX}${key}`);
        return;
      } else {
        const data = typeof value === 'object' ? JSON.stringify(value) : value.toString();
        localStorage.setItem(`${this.PREFIX}${key}`, data);
      }
    } catch (error) {
      console.error(`Failed to save ${key} to storage:`, error);
    }
  }

  /**
   * 키에 해당하는 값을 가져옵니다. JSON 형식의 데이터는 자동으로 파싱합니다.
   */ 
  static getItem<T>(key: string): T | undefined {
    try {
      const data = localStorage.getItem(`${this.PREFIX}${key}`);
      if (data?.startsWith('{') && data?.endsWith('}')) {
        return JSON.parse(data) as T;
      } else if (data?.startsWith('[') && data?.endsWith(']')) {
        return JSON.parse(data) as T;
      }
      return data as T || undefined;
    } catch {
      return undefined;
    }
  }
}