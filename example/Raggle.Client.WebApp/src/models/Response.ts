export interface IStreamingChatResponse {
  status: string; // Discriminator property
  timeStamp: string;
}

export interface StreamingStopResponse extends IStreamingChatResponse {
  status: 'stop';
}

export interface StreamingFilterResponse extends IStreamingChatResponse {
  status: 'filter';
}

export interface StreamingLimitResponse extends IStreamingChatResponse {
  status: 'limit';
}

export interface StreamingErrorResponse extends IStreamingChatResponse {
  status: 'error';
  message?: string;
}

export interface StreamingTextResponse extends IStreamingChatResponse {
  status: 'text_gen';
  text?: string;
}

export interface StreamingToolCallResponse extends IStreamingChatResponse {
  status: 'tool_call';
  name?: string;
  argument?: string;
}

export interface StreamingToolUseResponse extends IStreamingChatResponse {
  status: 'tool_use';
  name?: string;
  argument?: string;
}

export interface StreamingToolResultResponse extends IStreamingChatResponse {
  status: 'tool_result';
  name?: string;
  result?: any;
}

export type StreamingChatResponse =
  | StreamingStopResponse
  | StreamingFilterResponse
  | StreamingLimitResponse
  | StreamingErrorResponse
  | StreamingTextResponse
  | StreamingToolCallResponse
  | StreamingToolUseResponse
  | StreamingToolResultResponse;

