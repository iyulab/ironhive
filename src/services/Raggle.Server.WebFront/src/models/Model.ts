export interface User {
  id: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface Assistant {
  id: string;
  userID: string;
  name: string;
  instruction: string;
  knowledges: string[];
  connections: string[];
  openAPIs: string[];
  chatHistory: any;
}

export interface Knowledge {
  id: string;
  userID: string;
  name: string;
  description: string;
  files: any[];
}

export interface Connection {
  id: string;
  userID: string;
  type: string;
  name: string;
  description: string;
  connectionString: string;
}

export interface OpenAPI {
  id: string;
  userID: string;
  schemeType: string;
  schema: string;
}

export interface ChatHistory {
  role: 'user' | 'bot';
  message: string;
}
