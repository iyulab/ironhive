import { Message } from "@iyulab/chat-components";

export interface MessageGenerationRequest {
  provider: string;
  model: string;
  system?: string;
  messages: Message[];
  tools?: any;
  parameters?: MessageGenerationParameters;
}

export interface MessageGenerationParameters {
  maxTokens?: number;
  temperature?: number;
  topK?: number;
  topP?: number;
  stopSequences?: string[];
}
