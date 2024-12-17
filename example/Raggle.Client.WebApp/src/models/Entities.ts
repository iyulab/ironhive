import { ExecuteOptions, HandlerOptions, ToolOptions } from "./Options";

export interface CollectionEntity {
  id?: string;
  name?: string;
  description?: string;
  embedService?: string;
  embedModel?: string;
  createdAt?: string;
  lastUpdatedAt?: string;
  handlerOptions: HandlerOptions;
}

export interface DocumentEntity {
  id?: string;
  fileName?: string;
  fileSize?: number;
  contentType?: string;
  tags?: string[];
  createdAt?: string;
  lastUpdatedAt?: string;
  collectionId?: string;
}

export interface AssistantEntity {
  id?: string;
  service?: string;
  model?: string;
  name?: string;
  description?: string;
  instruction?: string;
  options: ExecuteOptions;
  tools?: string[];
  toolOptions?: ToolOptions;
  createdAt?: string;
  lastUpdatedAt?: string;
}
