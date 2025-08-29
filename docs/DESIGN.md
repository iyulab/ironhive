- builder
	- AddModelCatalogProvider()
	- AddMessageGenerator()
	- AddMessageTool()
	- AddEmbeddingGenerator()

	- Files: IFileServiceBuilder
		- SetDetector()
		- AddStorage()
		- AddDecoder()
	- Memory: IMemoryServiceBuilder
		- SetVectorStorage()
		- SetQueueStorage()
		- SetMemoryPipeline()

- service
	- AgentService
		- Generators<Message>
		- Tools
	- FileService
		- Detector or Resolver or Analyzer
		- Storages
		- Decoders
	- MemoryService
		- Generators<Embedding>
		- Vector
		- Queue
		- Pipeline

		- SetVectorStorage()
		- SetQueueStorage()
		- SetMemoryPipeline()
	
	- MemoryWorkerService(기본 주입 or CreateWorker()로 생성?)
		- IMemoryService 주입	
	
- HiveService
	- File { get; }
	- Memory { get; }

	- CreateAgent()
	- CreateMemoryWorker()
