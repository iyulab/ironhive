export interface MessageGenerationRequest {
  provider: string;
  model: string;
  system?: string;
  messages: Message[];
  tools?: any;
  thinkingEffort?: "low" | "medium" | "high";
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
  id?: string;
  format?: "detail" | "summary" | "secure";
  value?: string;
}

export type ToolContentStauts = (
  "waiting" | 
  "paused" | 
  "approved" |
  "rejected" |
  "inProgress" |
  "success" | 
  "failure");

export interface ToolMessageContent {
  type: "tool";
  status: ToolContentStauts;
  id?: string;
  name?: string;
  input?: string;
  output?: string;
}

export type MessageContent = (
  TextMessageContent |
  ThinkingMessageContent |
  ToolMessageContent);
