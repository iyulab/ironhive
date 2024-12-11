import { MessageContent } from "./Messages";

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
  model?: string;
  content?: MessageContent;
  tokenUsage?: TokenUsage;
}
