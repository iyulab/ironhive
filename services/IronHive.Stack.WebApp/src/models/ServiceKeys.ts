export type AIServiceKey = 
  | "openai" 
  | "anthropic" 
  | "ollama";

export type HandlerServiceKey = 
  | "extract"
  | "chunk" 
  | "summary" 
  | "dialogue" 
  | "embeddings";

export type ToolServiceKey = 
  | "vector_search";
