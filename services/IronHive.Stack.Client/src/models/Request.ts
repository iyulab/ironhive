import { Message } from "./Messages";

export interface ChatCompletionRequest {
  provider: string;
  model: string;
  system?: string;
  messages: Message[];
  tools?: Record<string, any>;
  maxTokens?: number;
  temperature?: number;
  topP?: number;
  topK?: number;
  stopSequences?: string[];
  stream: boolean;
}

export interface FileUploadRequest {
  files: File[];
  tags: string[];
}
