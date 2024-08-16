export interface EntityBase {
  id: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface UserEntity extends EntityBase {
  userID?: string;
}

export interface User extends EntityBase {
  device: string;
}

export interface Assistant extends UserEntity {
  name: string;
  instruction?: string;
  knowledges: string[];
  connections: string[];
  openAPIs: string[];
  chatHistory: any;
}

export interface Knowledge extends UserEntity {
  name: string;
  description?: string;
  files: KnowledgeFile[];
}

export interface KnowledgeFile {
  type: string;
  name: string;
  size: number;
}

export interface Connection extends UserEntity {
  type: string;
  name: string;
  description?: string;
  connectionString: string;
}

export interface OpenAPI extends UserEntity {
  schemeType: string;
  schema: string;
}

export interface ChatHistory {
  role: 'user' | 'bot';
  message: string;
}
