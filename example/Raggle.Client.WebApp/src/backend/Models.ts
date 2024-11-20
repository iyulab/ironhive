export interface Assistant {
  id?: string;
  name: string;
  instruction?: string;
  description?: string;
  createdAt?: string;
  lastUpdatedAt?: string;
}

export interface Collection {
  id: string;
  name: string;
  description: string;

  embeddingProvider: string;
  embeddingModel: string;
  
  createdAt: string;
  lastUpdatedAt: string;
}

export interface Document {
  id: string;
  name: string;
  description: string;
  collectionId: string;
}
