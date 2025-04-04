# STRUCTURE

## Service Classification

### Root Service
- IHiveMind

### 파생 서비스
- IHiveSession
  - IHiveAgent[]
- IHiveMemory
- IPipelineWorker

### Common Services(Important)
- IHiveServiceStore             => Singleton!                                     => Required
  - IChatCompletionConnector[]  => Instance                                       => Optional
  - IEmbeddingConnector[]       => Instance                                       => Optional
  - IFileStorage[]              => Factory Instance                               => Optional

### AI Services
- IChatCompletionService        => Singleton!                                     => Required
  - IHiveServiceStore           => Singleton!                                     => Required
  - IToolHandlerManager         => Singleton!                                     => Required
    - IServiceProvider          => SYSTEM                                         => For Handler
    - IToolHandler[]            => Singleton | Scoped | Transient                 => Optional
- IEmbeddingService             => Singleton!                                     => Required
  - IHiveServiceStore           => Singleton!                                     => Required

### File Services
- IFileStorageManger            => Singleton!                                     => Required
  - IServiceProvider            => SYSTEM                                         => For Factory Store
  - IHiveServiceStore           => Singleton!                                     => Required
  - IFileDecoderResolver        => Singleton!                                     => Required
    - IFileDecoder[]            => Singleton!                                     => Optional
  
### Memory Services
- IQueueStorage               => Singleton!                                     => Required
- IVectorStorage              => Singleton!                                     => Required
- IEmbeddingService           => Singleton!                                     => Required
- IPipelineEventHandler       => Singleton!                                     => Required??
- IPipelineHandler[]          => Singleton | Scoped | Transient                 => Optional
