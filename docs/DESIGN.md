- builder
	- providers
	- storages

	- AddModelCatalogProvider()
	- AddEmbeddingGenerator()
	- AddMessageGenerator()
	
	- AddFileStorage()
	- AddQueueStorage()
	- AddVectorStorage()
	
	- Agent: IAgentServiceBuilder
		- AddTool()
	- Files: IFileServiceBuilder
		- AddDecoder()
	- Memory: IMemoryServiceBuilder
		- SetPipeline()

- service
	- Providers
	- Storages
	
	- AgentService
		- Tools
	- FileService
		- Decoders
	- MemoryService
		- Pipeline

		- SetPipeline()
		- CreateWorkers()
	
	- MemoryWorkerService(기본 주입 or CreateWorker()로 생성?)
		- IMemoryService 주입
	
- HiveService
	- Services { get; }
	
	- Providers { get; }
	- Storages { get; }

	- Agents { get; }
	- Files { get; }
	- Memory { get; }
