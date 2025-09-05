# STRUCTURE

## Service Classification

### Services Container
- IProviderRegistry         => Singleton                                      => Required
  - IProvider[]
- IStorageRegistry          => Singleton                                      => Required
  - IStorage[]
- IToolCollection           => Singleton                                      => Required
  - ITool[]

### AI Services
- IModelCatalogService      => Singleton                                      => Required
  - IProviderRegistry
- IMessageService             => Singleton                                      => Required
  - IProviderRegistry
  - IToolCollection
- IEmbeddingService           => Singleton                                      => Required
  - IProviderRegistry

### File Services
- IFileStorageService               => Singleton                                      => Required
  - IStorageRegistry
- IFileExtractionService              => Singleton                                      => Required
  - IFileMediaTypeDetector
  - IFileDecoder[]
  
### Memory Services
- IMemoryService              => Singleton                          => Required
  - IStorageRegistry
  - IEmbeddingService
- IMemoryWorkerService
  - IStorageRegistry