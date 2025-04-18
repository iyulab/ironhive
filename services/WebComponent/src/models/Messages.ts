interface MessageContentBase {
  id?: string;
  index?: number;
}

interface TextContent extends MessageContentBase {
  type: "text";
  value?: string;
}

interface ImageContent extends MessageContentBase {
  type: "image";
  data?: string;
}

type ToolStatus = "pending" | "running" | "completed" | "failed";

interface ToolContent extends MessageContentBase {
  type: "tool";
  status?: ToolStatus;
  name?: string;
  arguments?: any;
  result?: any;
}

type MessageContent = TextContent | ImageContent | ToolContent;

interface Message {
  role: "user" | "assistant";
  avatar?: string;
  name?: string;
  content?: MessageContent[];
  timestamp?: string;
}

export type { 
  MessageContentBase, 
  TextContent, 
  ImageContent, 
  ToolStatus, 
  ToolContent, 
  MessageContent, 
  Message 
};
