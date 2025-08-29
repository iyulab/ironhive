using System.Collections.Concurrent;
using IronHive.Abstractions.Memory;

namespace IronHive.Core.Memory;

/// <inheritdoc />
public class MemoryWorkerService : IMemoryWorkerService
{
    private readonly IMemoryService _memory;
    private readonly ConcurrentBag<IMemoryWorker> _workers = new();
    private readonly object _lock = new();

    private int _state = 0; // 0: 정지됨, 1: 실행 중
    private int _minWorkers = 1;
    private int _maxWorkers = 10;

    public MemoryWorkerService(IMemoryService memory)
    {
        _memory = memory;
    }

    /// <inheritdoc />
    public bool IsRunning => Volatile.Read(ref _state) == 1;

    /// <inheritdoc />
    public int CurrentWorkers => _workers.Count;

    /// <inheritdoc />
    public int MinWorkers 
    { 
        get => Volatile.Read(ref _minWorkers);
        set
        {
            // 최소 워커 수가 1 이상이어야 하며, 현재 워커 수보다 작아야 함
            if (value < 1 || value > MaxWorkers)
                throw new ArgumentOutOfRangeException(nameof(MinWorkers), "MinWorkers must be between 1 and MaxWorkers.");
            Interlocked.Exchange(ref _minWorkers, value);
        }
    }

    /// <inheritdoc />
    public int MaxWorkers
    {
        get => Volatile.Read(ref _maxWorkers);
        set
        {
            // 최대 워커 수가 최소 워커 수 이상이어야 함
            if (value < MinWorkers)
                throw new ArgumentOutOfRangeException(nameof(MaxWorkers), "MaxWorkers must be greater than or equal to MinWorkers.");
            Interlocked.Exchange(ref _maxWorkers, value);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Task.Run(() => StopAsync(true)).Wait();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public Task StartAsync()
    {
        if (Interlocked.CompareExchange(ref _state, 1, 0) != 0)
            return Task.CompletedTask;

        // 초기(최소) 워커 생성 및 시작
        for (int i = 0; i < MinWorkers; i++)
        {
            ScaleUp();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(bool force = false)
    {
        // 모든 워커가 중지될 때까지 대기
        var tasks = new List<Task>();
        while (!_workers.IsEmpty)
        { 
            if (_workers.TryTake(out var worker))
            {
                worker.StateChanged -= OnWorkerStateChanged;
                tasks.Add(worker.StopAsync(false).ContinueWith(t =>
                {
                    worker.Dispose();
                }));
            }
        }
        await Task.WhenAll(tasks);

        Interlocked.Exchange(ref _state, 0);
        var list = new List<string>();
    }

    /// <summary>
    /// 하나의 워커를 추가하고 시작합니다. (Scale Up)
    /// </summary>
    private void ScaleUp()
    {
        // 새로운 워커 인스턴스 생성
        var worker = new MemoryWorker(_memory);
        worker.StateChanged += OnWorkerStateChanged;

        // 워커를 백그라운드에서 실행하고 관리 목록에 추가
        Task.Run(worker.StartAsync);
        _workers.Add(worker);
    }

    /// <summary>
    /// 하나의 워커를 중지하고 제거합니다. (Scale Down)
    /// </summary>
    private void ScaleDown()
    {
        // 워커를 관리 목록에서 제거 시도
        if (_workers.TryTake(out var worker))
        {
            // 이벤트 구독 해제 및 워커 중지/폐기
            worker.StateChanged -= OnWorkerStateChanged;
            _ = worker.StopAsync(false).ContinueWith(t =>
            {
                worker.Dispose();
            });
        }
    }

    /// <summary>
    /// 워커의 상태 변경을 처리하여 스케일링을 수행합니다.
    /// </summary>
    private void OnWorkerStateChanged(object? sender, MemoryWorkerState state)
    {
        if (sender is not IMemoryWorker worker)
            return;

        switch (state)
        {
            // 워커가 메시지 처리 시작 -> 워커 추가 (Scale Up)
            case MemoryWorkerState.Processing:
                lock (_lock)
                {
                    if (_workers.Count < MaxWorkers)
                    {
                        ScaleUp();
                    }
                }
                break;

            // 워커가 유휴 상태 진입 -> 워커 제거 (Scale Down)
            case MemoryWorkerState.Idle:
                lock (_lock)
                {
                    if (_workers.Count > MinWorkers)
                    {
                        ScaleDown();
                    }
                }
                break;

            // 워커가 중지됨 -> 워커 재시작
            case MemoryWorkerState.Stopped:
                lock (_lock)
                {
                    Task.Run(worker.StartAsync);
                }
                break;
        }
    }
}
