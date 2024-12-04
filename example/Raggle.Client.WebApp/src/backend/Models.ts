export interface Models {
  [provider: string]: string[];
}

export interface Collection {
  collectionId?: string;
  name?: string;
  description?: string;
  embedServiceKey?: string;
  embedModelName?: string;
  createdAt?: string;
  lastUpdatedAt?: string;
  handlerOptions?: Record<string, any>;
}

export interface Document {
  documentId?: string;
  fileName?: string;
  fileSize?: number;
  contentType?: string;
  createdAt?: string;
  lastUpdatedAt?: string;
  tags?: string[];
  collectionId?: string;
}

export interface Assistant {
  assistantId?: string;
  name?: string;
  description?: string;
  instruction?: string;
  createdAt?: string;
  lastUpdatedAt?: string;
  memories?: string[];
  toolKits?: string[];
  toolkitOptions?: Record<string, any>;
}
