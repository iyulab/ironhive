export type ModelProvider = 'openai' | 'anthropic' | 'google' | 'xai' | 'iyulab';
export const ModelProviderList: ModelProvider[] = ['openai', 'anthropic', 'google', 'xai', 'iyulab'];

export interface ModelSummary {
  provider: ModelProvider;
  modelId: string;
  displayName: string;
  description: string;
  thinkable: boolean;
  inputPrice: number;
  outputPrice: number;
  contextLength: number;
}
