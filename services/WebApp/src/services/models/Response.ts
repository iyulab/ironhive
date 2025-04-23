import { MessageContent } from "@iyulab/chat-component";

export type EndReason = 
  | "endTurn"
  | "stopSequence"
  | "contentFilter"
  | "maxTokens";

export interface TokenUsage {
  inputTokens: number;
  outputTokens: number;
  totalTokens: number;
}

export interface StreamingResponse {
  endReason?: EndReason;
  data?: MessageContent;
  tokenUsage?: TokenUsage;
}
