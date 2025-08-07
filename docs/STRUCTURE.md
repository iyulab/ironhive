# STRUCTURE

## Service Classification

### Root Service
- IHiveMind

### AI Services
- IMessageGenerationService        => Singleton                                   => Required
  - IMessageGenerator[]
- IEmbeddingGenerationService      => Singleton                                   => Required
  - IEmbeddingGenerator[]

### File Services
- IFileStorageManger               => Singleton                                   => Required
  - IFileStorage[]
- IFileDecoderManager              => Singleton                                   => Required
  - IFileDecoder[]
  
### Memory Services
- IMemoryService
- IMemoryWorker
- IQueueStorage                 => Singleton                                      => Required
- IVectorStorage                => Singleton                                      => Required
- IPipelineHandler[]            => Singleton | Scoped | Transient                 => Optional
