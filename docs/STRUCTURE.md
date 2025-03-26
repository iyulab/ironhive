# STRUCTURE

## Class Map

- IChatCompletionService        => Singleton
- IEmbeddingService             => Singleton
- IHiveServiceStore             => Singleton
  - IChatCompletionConnector[]  => Instance
  - IEmbeddingConnector[]       => Instance
- IToolManager                  => Singleton
  - ITool[]                     => Singleton | Scoped | Transient

- IMemoryService                => Singleton
- IVectorStorage                => Singleton
- IQueueStorage                 => Singleton
- IFileStorageManger            => Singleton
  - IFileReader                 => Singleton
  - IFileStorage                => Scoped | Transient
- IPipelineOrchestrator         => Singleton
  - IPipelineHandler            => Singleton | Scoped | Transient
