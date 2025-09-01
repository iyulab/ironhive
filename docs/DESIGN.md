- builder
	- AddModelCatalogProvider()
	- AddEmbeddingGenerator()
	- AddMessageGenerator()
	- AddMessageTool()

	- Files: IFileServiceBuilder
		- AddStorage()
		- AddDecoder()
	- Memory: IMemoryServiceBuilder
		- AddVectorStorage()
		- AddQueueStorage()
		- SetPipeline()

- service
	- AgentService
		- Generators<Message>
		- Tools
	- FileService
		- Detector or Resolver or Analyzer
		- Storages<File>
		- Decoders
	- MemoryService
		- Generators<Embedding>
		- Storages<Queue>
		- Storages<Vector>
		- Pipeline

		- SetPipeline()
	
	- MemoryWorkerService(기본 주입 or CreateWorker()로 생성?)
		- IMemoryService 주입
	
- HiveService
	- Files { get; }
	- Memory { get; }

	- CreateAgent()
