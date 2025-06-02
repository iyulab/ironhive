# STRUCTURE

## Service Classification

### Root Service
- IHiveMind

### 파생 서비스
- IHiveMemory
- IPipelineWorker

### AI Services
- IMessageGenerationService        => Singleton                                   => Required
  - IMessageGenerationProvider[]   => Singleton                                   => Optional
- IEmbeddingService                => Singleton                                   => Required
  - IEmbeddingProvider[]           => Singleton                                   => Optional

### File Services
- IFileStorageManger               => Singleton                                   => Required
  - IFileStorage[]                 => Singleton                                   => Optional
- IFileDecoderManager              => Singleton                                   => Required
  - IFileDecoder[]                 => Singleton                                   => Optional
  
### Memory Services
- IQueueStorage                 => Singleton                                      => Required
- IVectorStorage                => Singleton                                      => Required
- IEmbeddingService             => Singleton                                      => Required
- IPipelineObserver             => Singleton | Scoped | Transient                 => Optional
- IPipelineHandler[]            => Singleton | Scoped | Transient                 => Optional
