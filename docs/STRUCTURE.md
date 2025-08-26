# STRUCTURE

## Service Classification

### AI Services
- IMessageService             => Singleton                                      => Required
  - IMessageGenerator[]
  - IToolPlugin[]
- IEmbeddingService           => Singleton                                      => Required
  - IEmbeddingGenerator[]

### File Services
- IFileStorageManger               => Singleton                                      => Required
  - IFileStorage[]
- IFileDecoderManager              => Singleton                                      => Required
  - IFileDecoder[]
  
### Memory Services
- IMemoryService              => Singleton                          => Required
  - IQueueStorage
  - IVectorStorage
  - IPipelineHandler[]        => Singleton | Scoped | Transient     => Optional
  - IMemoryWorkerManager
    - IMemoryWorker