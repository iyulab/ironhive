export interface ServiceModels {
  [provider: string]: string[];
}

export interface CollectionEntity {
  collectionId?: string;
  name?: string;
  description?: string;
  embedServiceKey?: string;
  embedModelName?: string;
  createdAt?: string;
  lastUpdatedAt?: string;
  handlerOptions?: Record<string, any>;
}

export interface DocumentEntity {
  documentId?: string;
  fileName?: string;
  fileSize?: number;
  contentType?: string;
  createdAt?: string;
  lastUpdatedAt?: string;
  tags?: string[];
  collectionId?: string;
}

export interface AssistantEntity {
  assistantId?: string;
  name?: string;
  description?: string;
  instruction?: string;
  settings: ServiceSettings;
  memories?: string[];
  toolKits?: string[];
  toolkitOptions?: Record<string, any>;
  createdAt?: Date;
  lastUpdatedAt?: Date;
}

export interface ServiceSettings {
  provider?: string;
  model?: string;
  maxTokens?: number;
  temperature?: number;
  topK?: number;
  topP?: number;
  stopSequences?: string[];
}
