export interface CollectionEntity {
  id?: string;
  name?: string;
  description?: string;
  embedService?: string;
  embedModel?: string;
  handlerOptions: any;
  createdAt?: string;
  lastUpdatedAt?: string;
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
  name: string;
  description?: string;
  instruction?: string;
  options: any;
  tools: string[];
  toolOptions?: any;
  createdAt?: string;
  lastUpdatedAt?: string;
}

export interface UserEntity {  
  userName: string;
  createdAt?: string;
  lastUpdatedAt?: string;
}