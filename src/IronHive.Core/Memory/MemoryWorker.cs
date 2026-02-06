using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Queue;
using IronHive.Abstractions.Workflow;
using System.Collections.Concurrent;

namespace IronHive.Core.Memory;

/// <inheritdoc />
public class MemoryWorker : IMemoryWorker
{
    private readonly IQueueStorage _queue;
    private readonly IWorkflow<MemoryContext> _pipeline;

    private readonly ConcurrentDictionary<Task, byte> _tasks = new();
    private readonly CancellationTokenSource _cts = new(); // 전체 수명 토큰

    private SemaphoreSlim? _gate;
    private IQueueConsumer? _consumer;
    private int _flag = 0; // 0: 정지됨, 1: 실행 중

    public MemoryWorker(IQueueStorage queue, IWorkflow<MemoryContext> pipeline)
    {
        _queue = queue;
        _pipeline = pipeline;
        _pipeline.Progressed += OnPipelineProgressed;
    }

    /// <inheritdoc />
    public bool IsRunning => Volatile.Read(ref _flag) == 1;

    /// <inheritdoc />
    public int RunningTaskCount => _tasks.Count;

    /// <inheritdoc />
    public int MaxConcurrentTasks { get; set; } = 5;

    /// <inheritdoc />
    public event EventHandler<WorkflowEventArgs<MemoryContext>>? Progressed;

    /// <inheritdoc />
    public void Dispose()
    {
        StopAsync(force: true).GetAwaiter().GetResult();
        _pipeline.Progressed -= OnPipelineProgressed;
        _cts.Dispose();
        Progressed = null;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task StartAsync()
    {
        if (Interlocked.CompareExchange(ref _flag, 1, 0) != 0)
            return;

        _gate?.Dispose();
        _gate = new SemaphoreSlim(MaxConcurrentTasks, MaxConcurrentTasks);

        _consumer = await _queue.CreateConsumerAsync<MemoryContext>(
            onReceived: OnReceivedMessageAsync,
            cancellationToken: default).ConfigureAwait(false);

        await _consumer.StartAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task StopAsync(bool force = false)
    {
        // 큐 소비자를 중지하고 해제합니다.
        if (_consumer is not null)
        {
            await _consumer.StopAsync().ConfigureAwait(false);
            _consumer.Dispose();
            _consumer = null;
        }

        // 강제 중지: 전체 취소 토큰을 취소합니다.
        if (force)
        {
            _cts.Cancel();
        }

        // 실행중인 작업이 있으면 대기합니다.
        if (!force && !_tasks.IsEmpty)
        {
            Task[] running = _tasks.Keys.ToArray();
            try { await Task.WhenAll(running).ConfigureAwait(false); } catch { /* 개별 실패 무시 */ }
        }

        // 게이트웨이 해제
        if (_gate is not null)
        {
            _gate.Dispose();
            _gate = null;
        }

        Interlocked.Exchange(ref _flag, 0);
    }

    /// <summary>
    /// 큐에서 메시지를 수신했을 때 호출되는 비동기 메서드입니다.
    /// </summary>
    private async Task OnReceivedMessageAsync(IQueueMessage<MemoryContext> msg)
    {
        if (_gate is null)
        {
            await msg.DeadAsync("Worker is Stunning Down").ConfigureAwait(false);
            return;
        }

        //await _gate.WaitAsync(_cts.Token);
        await _gate.WaitAsync().ConfigureAwait(false);

        var task = Task.Run(async () =>
        {
            try
            {
                await _pipeline.RunAsync(msg.Body, _cts.Token).ConfigureAwait(false);
                await msg.CompleteAsync().ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (_cts.IsCancellationRequested)
            {
                await msg.RequeueAsync().ConfigureAwait(false); // 워커가 중지 중이므로 재큐
            }
            catch (Exception ex)
            {
                await msg.DeadAsync(ex.ToString()).ConfigureAwait(false); // 스택 포함
            }
            finally
            {
                _gate.Release();
            }
        }, _cts.Token);

        _tasks.TryAdd(task, 0);
        _ = task.ContinueWith(
            t => _tasks.TryRemove(t, out _),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    /// <summary>
    /// 파이프라인이 진행될 때 호출되는 메서드입니다.
    /// </summary>
    private void OnPipelineProgressed(object? sender, WorkflowEventArgs<MemoryContext> e)
    {
        Progressed?.Invoke(this, e);
    }
}
