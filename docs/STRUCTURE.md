# STRUCTURE

## Service Classification

### Root Service
- IHiveMind

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
- IMemoryService                => Singleton!                                     => Required
  - IQueueStorage               => Singleton!                                     => Required
  - IPipelineStorage            => Singleton!                                     => Required
  - IVectorStorage              => Singleton!                                     => Required
  - IEmbeddingService           => Singleton!                                     => Required

### Memory Worker Services
- IPipelineWorker               => Instance
  - IServiceProvider            => SYSTEM                                         => For Handler
  - IPipelineHandler[]          => Singleton | Scoped | Transient                 => Optional
  - IQueueStorage               => Singleton!                                     => Required
  - IPipelineStorage            => Singleton!                                     => Required
  - IPipelineEventHandler       => Singleton!                                     => Required
