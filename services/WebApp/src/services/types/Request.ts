export interface MessageGenerationRequest {
  provider: string;
  model: string;
  system?: string;
  messages: Message[];
  tools?: any;
  thinkingEffort: "none" | "low" | "medium" | "high";
  maxTokens?: number;
  temperature?: number;
  topK?: number;
  topP?: number;
  stopSequences?: string[];
}

export interface UserMessage {
  role: "user";
  id?: string;
  content: MessageContent[];
  timestamp?: string;
}

export interface AssistantMessage {
  role: "assistant";
  id?: string;
  name?: string;
  content: MessageContent[];
  timestamp?: string;
}

export type Message = UserMessage | AssistantMessage;

export interface TextMessageContent {
  type: "text";
  value?: string;
}

export interface ThinkingMessageContent {
  type: "thinking";
  signature?: string;
  format?: "detail" | "summary" | "secure";
  value?: string;
}

export interface ToolMessageContent {
  type: "tool";
  isCompleted?: boolean;
  isApproved?: boolean;
  id?: string;
  name?: string;
  input?: string;
  output?: { isSuccess: boolean; result: string };
}

export type MessageContent = (
  TextMessageContent |
  ThinkingMessageContent |
  ToolMessageContent);
