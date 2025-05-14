import { AssistantMessageContent } from "@iyulab/chat-components";

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
  data?: AssistantMessageContent;
  tokenUsage?: TokenUsage;
  timestamp?: string;
}
