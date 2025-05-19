import { Message } from "@iyulab/chat-components";

export interface ChatCompletionRequest {
  provider: string;
  model: string;
  instructions?: string;
  messages: Message[];
  maxTokens?: number;
  temperature?: number;
  topP?: number;
  topK?: number;
  stopSequences?: string[];
}
