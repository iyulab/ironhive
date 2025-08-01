﻿using System.Runtime.CompilerServices;
using System.Threading.Channels;
using IronHive.Abstractions.Message;
using IronHive.Abstractions.Message.Content;
using IronHive.Abstractions.Message.Roles;
using IronHive.Abstractions.Tools;
using IronHive.Core.Utilities;

namespace IronHive.Core.Services;

public class MessageGenerationService : IMessageGenerationService
{
    private readonly Dictionary<string, IMessageGenerator> _providers;
    private readonly IToolPluginManager _plugins;

    public MessageGenerationService(IEnumerable<IMessageGenerator> providers, IToolPluginManager plugins)
    {
        _providers = providers.ToDictionary(p => p.ProviderName, p => p);
        _plugins = plugins;
    }

    /// <inheritdoc />
    public async Task<MessageResponse> GenerateMessageAsync(
        MessageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGetValue(request.Provider, out var provider))
            throw new KeyNotFoundException($"Service key '{request.Provider}' not found.");

        // 요청 준비
        MessageDoneReason? reason;
        MessageTokenUsage? usage;
        AssistantMessage? message;
        bool next;

        // 루프 시작
        var counter = new LimitedCounter(request.MaxLoopCount);
        do
        {
            // 마지막 메시지가 어시스턴트 메시지인 경우, 툴 사용을 확인하고, 메시지를 이어갑니다.
            if (request.Messages.LastOrDefault() is AssistantMessage last)
            {
                message = last;
                await foreach (var _ in ProcessToolContentAsync(message, cancellationToken))
                { }
            }
            else
            {
                message = new AssistantMessage { Id = Guid.NewGuid().ToString() };
            }

            var res = await provider.GenerateMessageAsync(request, cancellationToken);

            foreach (var content in res.Message?.Content ?? [])
            {
                // 툴 컨텐츠의 경우 승인 요구사항을 확인하고 반영합니다.
                if (content is ToolMessageContent tool)
                {
                    if (tool.RequiresApproval(request.Tools))
                        tool.ChangeToPaused();
                    else
                        tool.ChangeToApproved();
                }
                
                // 컨텐츠 추가
                message.Content.Add(content);
            }

            // 요청 메시지의 마지막이 어시스턴트가 아닌 경우, 메시지를 추가합니다.
            if (request.Messages.LastOrDefault() is not AssistantMessage)
            {
                request.Messages.Add(message);
            }
            reason = res.DoneReason; // 생성 종료 이유 업데이트
            usage = res.TokenUsage; // 토큰 사용량 업데이트

            counter.Increment();
            next = ShouldContinue(reason, message, counter, cancellationToken);
        }
        while (next);

        return new MessageResponse
        {
            DoneReason = reason,
            Message = message,
            TokenUsage = usage,
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageGenerationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGetValue(request.Provider, out var provider))
            throw new KeyNotFoundException($"Service key '{request.Provider}' not found.");

        string messageId = request.Messages.LastOrDefault() is AssistantMessage lastMessage
            ? lastMessage.Id
            : Guid.NewGuid().ToString();
        string model = request.Model;
        MessageDoneReason? reason = null;
        MessageTokenUsage? usage = null;

        // 메시지 생성 시작
        yield return new StreamingMessageBeginResponse
        {
            Id = messageId,
        };

        // 루프 시작
        bool next;
        var counter = new LimitedCounter(request.MaxLoopCount);
        do
        {
            // 마지막 메시지가 어시스턴트 메시지인 경우, 툴 사용을 확인하고, 메시지를 이어갑니다.
            if (request.Messages.LastOrDefault() is AssistantMessage message)
            {
                await foreach (var res in ProcessToolContentAsync(message, cancellationToken))
                {
                    yield return res;
                }
            }
            // 마지막 메시지가 어시스턴트 메시지가 아닌 경우, 새로운 메시지를 생성합니다.
            else
            {
                message = new AssistantMessage { Id = messageId };
            }

            // 현재 루프에서 생성된 컨텐츠를 저장할 메시지 컨텐츠 스택 객체
            var stack = new List<MessageContent>();
            await foreach (var res in provider.GenerateStreamingMessageAsync(request, cancellationToken))
            {
                if (res is StreamingMessageBeginResponse mbr)
                {
                    continue;
                }
                else if (res is StreamingContentAddedResponse car)
                {
                    car.Index = message.Content.Count + car.Index;
                    stack.Add(car.Content);
                    yield return car;
                }
                else if (res is StreamingContentDeltaResponse cdr)
                {
                    var content = stack.ElementAt(cdr.Index);
                    content.Merge(cdr.Delta);
                    cdr.Index = message.Content.Count + cdr.Index;
                    yield return cdr;
                }
                else if (res is StreamingContentUpdatedResponse cur)
                {
                    var content = stack.ElementAt(cur.Index);
                    content.Update(cur.Updated);
                    cur.Index = message.Content.Count + cur.Index;
                    yield return cur;
                }
                else if (res is StreamingContentCompletedResponse ccr)
                {
                    ccr.Index = message.Content.Count + ccr.Index;
                    yield return ccr;
                }
                else if (res is StreamingMessageDoneResponse mdr)
                {
                    // 스트리밍 메시지 종료 응답인 경우, 종료 이유와 토큰 사용량을 업데이트합니다.
                    reason = mdr.DoneReason;
                    usage = mdr.TokenUsage;
                    model = mdr.Model;
                }
                else if (res is StreamingMessageErrorResponse mer)
                {
                    yield return mer;
                }
                else
                {
                    throw new InvalidOperationException("Unexpected response type.");
                }
            }

            // 스택에 있는 컨텐츠를 메시지에 추가합니다.
            foreach (var content in stack)
            {
                if (content is ToolMessageContent tool)
                {
                    // 툴 컨텐츠의 경우 승인 요구사항을 확인하고 반영합니다.
                    if (tool.RequiresApproval(request.Tools))
                        tool.ChangeToPaused();
                    else
                        tool.ChangeToApproved();

                    yield return new StreamingContentUpdatedResponse
                    {
                        Index = message.Content.Count,
                        Updated = new ToolUpdatedContent
                        {
                            Status = tool.Status,
                            Output = tool.Output
                        }
                    };
                }
                message.Content.Add(content);
            }

            // 요청 메시지의 마지막이 어시스턴트가 아닌 경우, 메시지를 추가합니다.
            if (request.Messages.LastOrDefault() is not AssistantMessage)
            {
                request.Messages.Add(message);
            }

            // 다음 루프를 계속 진행할지 여부를 결정합니다.
            counter.Increment();
            next = ShouldContinue(reason, message, counter, cancellationToken);
        }
        while (next);

        // 마지막 메시지를 전달 합니다.
        yield return new StreamingMessageDoneResponse
        {
            DoneReason = reason,
            TokenUsage = usage,
            Id = messageId,
            Model = model,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 도구 컨텐츠를 처리합니다.
    /// </summary>
    private async IAsyncEnumerable<StreamingMessageResponse> ProcessToolContentAsync(
        AssistantMessage message,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var maxConcurrent = 3; // 동시 실행할 최대 도구 호출 수
        var semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
        var channel = Channel.CreateBounded<StreamingMessageResponse>(new BoundedChannelOptions(maxConcurrent * 4)
        {
            FullMode = BoundedChannelFullMode.Wait
        });

        // 1) 각 ToolMessageContent별로 별도의 Task를 생성
        var tasks = new List<Task>();
        foreach (var (item, idx) in message.Content.Select((x, i) => (x, i)))
        {
            // 툴이 아닌 아이템 건너뛰기
            if (item is not ToolMessageContent tool)
                continue;
            // 이미 완료된 툴 건너뛰기
            if (tool.IsCompleted)
                continue;
            // 승인되지 않은 툴 건너뛰기
            if (tool.Status != ToolContentStatus.Approved)
                continue;
            
            var task = Task.Run(async () =>
            {
                await semaphore.WaitAsync(cancellationToken);
                
                try
                {
                    tool.ChangeToInProgress();
                    await channel.Writer.WriteAsync(new StreamingContentUpdatedResponse
                    {
                        Index = idx,
                        Updated = new ToolUpdatedContent
                        {
                            Status = tool.Status,
                            Output = tool.Output
                        }
                    }, cancellationToken);

                    var input = new ToolInput(tool.Input);
                    var output = await _plugins.InvokeAsync(tool.Name, input, cancellationToken);
                    tool.CompleteExecution(output);
                    await channel.Writer.WriteAsync(new StreamingContentUpdatedResponse
                    { 
                        Index = idx,
                        Updated = new ToolUpdatedContent 
                        { 
                            Status = tool.Status,
                            Output = tool.Output
                        }
                    }, cancellationToken);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error processing tool content for {tool.Name} at index {idx}.", ex);
                }
                finally
                {
                    // 항상 Semaphore를 릴리즈
                    semaphore.Release();
                }
            }, cancellationToken);
            tasks.Add(task);
        }

        // 2) 태스크들을 동시 실행
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while processing tool content.", ex);
            }
            finally
            {
                channel.Writer.Complete(); // 모든 태스크가 끝나면 완료 표시
            }
        }, CancellationToken.None);

        // 3) 채널에서 남은 출력 읽어서 yield, Complete()가 호출될 때까지 계속 읽기
        await foreach (var res in channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return res;
        }
    }

    /// <summary>
    /// 루프를 계속 진행할지 여부를 결정합니다.
    /// </summary>
    private static bool ShouldContinue(MessageDoneReason? endReason, AssistantMessage message, LimitedCounter counter, CancellationToken token)
    {
        // 취소 요청이 있는 경우 종료 Throw
        if (token.IsCancellationRequested)
            throw new OperationCanceledException(token);
        // 최대 루프 카운트 초과할 경우 종료 Throw
        if (counter.ReachedMax)
            throw new InvalidOperationException("Max loop count exceeded.");

        // 정상 종료일 경우 종료
        if (endReason == MessageDoneReason.EndTurn)
            return false;
        // 정지 시퀀스 생성시 종료
        if (endReason == MessageDoneReason.StopSequence)
            return false;
        // 도구 승인 요청이 있는 경우 종료
        if (message.RequiresApproval)
            return false;

        return true;
    }
}
