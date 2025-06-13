import { MessageContent } from "./Request";

export type StreamingMessageResponse = (
  StreamingMessageBeginResponse |
  StreamingMessageDoneResponse |
  StreamingMessageErrorResponse |
  StreamingContentAddedResponse |
  StreamingContentCompletedResponse |
  StreamingContentDeltaResponse |
  StreamingContentUpdatedResponse |
  StreamingContentInProgressResponse);

export interface StreamingMessageBeginResponse {
  type: "message.begin";
  id?: string;
}

export interface StreamingMessageDoneResponse {
  type: "message.done";
  id?: string;
  model?: string;
  doneReason?: "endTurn" | "toolCall" | "contentFilter";
  tokenUsage?: MessageTokenUsage;
  timestamp?: string;
}

export interface StreamingMessageErrorResponse {
  type: "message.error";
  code: number;
  message: string;
}

export interface StreamingContentAddedResponse {
  type: "message.content.added";
  index: number;
  content: MessageContent;
}

export interface StreamingContentDeltaResponse {
  type: "message.content.delta";
  index: number;
  delta: MessageDeltaContent;
}

export interface StreamingContentUpdatedResponse {
  type: "message.content.updated";
  index: number;
  updated: MessageUpdatedContent;
}

export interface StreamingContentInProgressResponse {
  type: "message.content.in_progress";
  index: number;
}

export interface StreamingContentCompletedResponse {
  type: "message.content.completed";
  index: number;
}

export type MessageDeltaContent = (
  TextMessageDeltaContent |
  ToolMessageDeltaContent |
  ThinkingMessageDeltaContent);

export interface TextMessageDeltaContent {
  type: "text"; 
  value: string;
}

export interface ToolMessageDeltaContent {
  type: "tool";
  input: string;
}

export interface ThinkingMessageDeltaContent {
  type: "thinking";
  data: string;
}

export type MessageUpdatedContent = (
  ToolMessageUpdatedResponse |
  ThinkingMessageUpdatedContent)

export interface ThinkingMessageUpdatedContent {
  type: "thinking";
  id: string;
}

export interface ToolMessageUpdatedResponse {
  type: "tool";
  output: { isSuccess: boolean; data: string; }
}

export interface MessageTokenUsage {
  inputTokens?: number;
  outputTokens?: number;
  totalTokens?: number;
}