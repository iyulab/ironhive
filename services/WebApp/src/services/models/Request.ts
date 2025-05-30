import { Message } from "@iyulab/chat-components";

export interface ChatCompletionRequest {
  provider: string;
  model: string;
  system?: string;
  messages: Message[];
  parameters?: {
    [key: string]: any;
  }
}
