import { AIKey } from "./ServiceKeys";

export type ServiceModels = {
  [provider in AIKey]: string[];
};

export interface CompletionOptions {
  maxTokens?: number;
  temperature?: number;
  topK?: number;
  topP?: number;
  stopSequences?: string[];
}

export interface ModelOptions {
  provider: AIKey;
  model: string;
}

export interface ChunkOptions {
  maxTokens: number;
}

export interface ToolOptions {
  vector_search?: string[];
}

export interface HandlerOptions {
  chunk: ChunkOptions;
  summary?: ModelOptions;
  dialogue?: ModelOptions;
  embeddings: ModelOptions;
}

const options: HandlerOptions = {
  summary: {
    provider: 'openai',
    model: 'gpt-4o-mini',
  },
  chunk: {
    maxTokens: 100,
  },
  dialogue: {
    provider: 'openai',
    model: 'gpt-4o-mini',
  },
  embeddings: {
    provider: 'openai',
    model: 'text-embedding-ada-002',
  },
}