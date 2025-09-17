using IronHive.Abstractions.Queue;

namespace IronHive.Core.Storages;

/// <inheritdoc />
public sealed class LocalQueueConsumer<T> : IQueueConsumer
{
    private readonly LocalQueueStorage _storage;
    private readonly FileSystemWatcher _watcher;
    private readonly SemaphoreSlim _pumpGate = new(1, 1);

    private Task? _pumpTask;
    private Task? _pumpLoopTask;
    private CancellationTokenSource? _cts;

    public LocalQueueConsumer(LocalQueueStorage storage)
    {
        _storage = storage;
        _watcher = CreateFileSystemWatcher(storage.DirectoryPath);
    }

    /// <inheritdoc />
    public bool IsRunning { get; private set; }

    /// 폴링 간격 (파일 이벤트 누락/외부 생성 대비), 기본 30초
    public TimeSpan FallbackPollingInterval { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary> 메시지 수신 핸들러 (수동 ack가 기본) </summary>
    public required Func<IQueueMessage<T>, Task> OnReceived { get; init; }

    /// <inheritdoc />
    public void Dispose()
    {
        try { _watcher.Dispose(); } catch { }
        try { _cts?.Cancel(); } catch { }

        _pumpGate.Dispose();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (IsRunning) return Task.CompletedTask;
        IsRunning = true;

        _cts = new CancellationTokenSource();
        cancellationToken.ThrowIfCancellationRequested();

        // 초기 드레인 + 폴백 폴링 루프
        _pumpLoopTask = PumpLoopAsync(_cts.Token);

        // 파일 이벤트 구독 시작
        _watcher.EnableRaisingEvents = true;

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!IsRunning) return;
        IsRunning = false;

        _watcher.EnableRaisingEvents = false;

        _cts?.Cancel();

        var tasks = new List<Task>();
        if (_pumpTask != null)
            tasks.Add(_pumpTask);
        if (_pumpLoopTask != null)
            tasks.Add(_pumpLoopTask);

        // 최대 5초 대기 후 강제 종료
        if (tasks.Count > 0)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            await Task.WhenAny(
                Task.WhenAll(tasks),
                Task.Delay(TimeSpan.FromSeconds(5), timeoutCts.Token));
        }
        else
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        _cts?.Dispose();
        _cts = null;
        _pumpLoopTask = null;
        _pumpTask = null;
    }

    /// <summary> 폴백 폴링 + 이벤트 트리거 시 드레인 </summary>
    private async Task PumpLoopAsync(CancellationToken cancellationToken)
    {
        // 시작 시 한 번 드레인
        Pump();

        // 폴백 폴링: 이벤트 누락/외부 생성 등 대비
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(FallbackPollingInterval, cancellationToken);
                Pump(); // 주기적으로 한 번씩 시도
            }
            catch (OperationCanceledException)
            {
                // 정상 종료
                break;
            }
        }
    }

    /// <summary> 하나의 드레인 작업을 스케줄 (중복 실행 방지) </summary>
    private void Pump()
    {
        if (!IsRunning) return;
        // 락이 비어있을 때만 드레인 작업 시작
        if (_pumpGate.Wait(0))
        {
            _pumpTask = Task.Run(async () =>
            {
                try
                {
                    var token = _cts?.Token ?? CancellationToken.None;
                    while (!token.IsCancellationRequested && IsRunning)
                    {
                        var message = await _storage.DequeueAsync<T>(token);
                        if (message is null)
                            break;

                        try
                        {
                            if (OnReceived is not null)
                                await OnReceived(message);
                        }
                        catch (Exception ex)
                        {
                            // 임시 로그
                            Console.Error.WriteLine($"[LocalQueueConsumer] OnReceived handler error: {ex}");
                        }
                    }
                }
                finally
                {
                    _pumpGate.Release();
                }
            });
        }
        // 이미 드레인 중이면 건너뜀
    }

    /// <summary> 파일 시스템 이벤트 핸들러 구성 </summary>
    private FileSystemWatcher CreateFileSystemWatcher(string directoryPath)
    {
        var watcher = new FileSystemWatcher(directoryPath, $"*{LocalQueueStorage.MessageExtension}")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.Size | NotifyFilters.LastWrite,
            IncludeSubdirectories = false,
            InternalBufferSize = 64 * 1024, // 64KB
            EnableRaisingEvents = false,
        };

        // Created: 새 메시지 파일 생성시 트리거
        watcher.Created += (_, __) => Pump();

        // Renamed: 원자적 파일 이름 변경 시 트리거
        watcher.Renamed += (_, e) =>
        {
            // 새 이름이 .qmsg면 트리거
            if (Path.GetExtension(e.FullPath)
                .Equals(LocalQueueStorage.MessageExtension, StringComparison.OrdinalIgnoreCase))
            {
                Pump();
            }
        };

        // Changed: 특정 FS에서 타임스탬프/사이즈 변동만 오는 케이스의 대비책
        watcher.Changed += (_, __) => Pump();

        // 에러시 폴백
        watcher.Error += (_, __) => { /* 로그 */ };

        return watcher;
    }
}
