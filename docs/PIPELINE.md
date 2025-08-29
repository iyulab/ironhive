PipelineFactory
	- Create()

PipelineBuilder<TInput, TNext>
	- Start()
	- Add() or Next()
	- Use()
	- When().Then().Else()
	- Tap() // 체크포인트, 로그, 메트릭
	- WithTag()
	- WithTimeout()
	- Build()

IPipeline<TInput, TOutput>
	- Task<TOutput> InvokeAsync<TInput>(in, ct);

IPipelineMiddleware

