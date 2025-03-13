import { Message } from "./Messages";

export interface ChatCompletionRequest {
  model: string;
  system?: string;
  maxTokens?: number;
  temperature?: number;
  topP?: number;
  topK?: number;
  stopSequences?: string[];
  messages: Message[];
  stream: boolean;
}

export interface FileUploadRequest {
  files: File[];
  tags: string[];
}
