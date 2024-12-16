import { AIServiceKey } from "./ServiceKeys";

export type AIServiceModels = {
  [provider in AIServiceKey]: string[];
};

export interface CompletionOptions {
  maxTokens?: number;
  temperature?: number;
  topK?: number;
  topP?: number;
  stopSequences?: string[];
}

export interface ModelOptions {
  serviceKey: AIServiceKey;
  modelName: string;
}

export interface ChunkOptions {
  maxTokens: number;
}

export interface HandlerOptions {
  chunk: ChunkOptions;
  summary?: ModelOptions;
  dialogue?: ModelOptions;
}

export interface ToolOptions {
  vector_search?: string[];
}