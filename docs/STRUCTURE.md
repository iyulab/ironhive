# STRUCTURE

## Service Classification

### Root Service
- IHiveMind

### AI Services
- IMessageGenerationService        => Singleton                                      => Required
  - IMessageGenerator[]
  - IToolPlugin[]
- IEmbeddingGenerationService      => Singleton                                      => Required
  - IEmbeddingGenerator[]

### File Services
- IFileStorageManger               => Singleton                                      => Required
  - IFileStorage[]
- IFileDecoderManager              => Singleton                                      => Required
  - IFileDecoder[]
  
### Memory Services
- IMemoryService
  - IMemoryWorkerManager
    - IMemoryWorker
  - IQueueStorage                  => Singleton                                      => Required
  - IVectorStorage                 => Singleton                                      => Required
  - IPipelineHandler[]             => Singleton | Scoped | Transient                 => Optional
