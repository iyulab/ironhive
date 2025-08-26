- builder
	- Providers: IKeyedCollection<IProvider>
		- AddModelCatalogProvider()
		- AddMessageGenerator()
		- AddEmbeddingGenerator()
	- Files: IFileServiceBuilder
		- SetDetector()
		- AddStorage()
		- AddDecoder()
	- Memory: IMemoryServiceBuilder
		- SetVectorStorage()
		- SetQueueStorage()

		- Workers
			- AddPipelineHandler()
			- ConfigWorkers()

- service
	- AgentService
		- Generators<Message>
		- Tools
	- FileService
		- Detector or Resolver or Analyzer
		- Storages
		- Decoders

		- SetDetector()
	- MemoryService
		- Generators<Embedding>
		- Vector
		- Queue
		- Workers
			- Pipelines

		- SetVectorStorage()
		- SetQueueStorage()