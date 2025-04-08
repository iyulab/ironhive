using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Connectors.OpenAI;
using Microsoft.Extensions.Options;
using WebApiSample.Data;
using WebApiSample.Settings;

namespace WebApiSample.Services;

public class AppService
{
    private readonly IHiveMind _hive;
    private readonly AppDbContext _db;
    private readonly IOptionsMonitor<AppServicesSettings> _monitor;

    public AppService(IHiveMind hive, AppDbContext db, IOptionsMonitor<AppServicesSettings> monitor)
    {
        CurrentState = AppState.NotReady("Initializing...");
        CurrentSettings = monitor.CurrentValue;

        _hive = hive;
        _db = db;
        _db.Database.EnsureCreated();
        _monitor = monitor;
        _monitor.OnChange(OnChangedSettings);

        Memory = CreateMemoryService(CurrentSettings.Memory);
    }

    public AppState CurrentState { get; private set; } = AppState.NotReady("");

    public AppServicesSettings CurrentSettings { get; private set; }

    public MemoryService? Memory { get; private set; } = null;

    private void OnChangedSettings(AppServicesSettings settings)
    {
        AppState.Loading("settings changed");

        // 메모리 서비스 변경
        if (CurrentSettings.Memory != settings.Memory)
        {
            Memory = CreateMemoryService(settings.Memory);
        }

        // AI 서비스 변경
        if (CurrentSettings.Connectors.OpenAI != settings.Connectors.OpenAI)
        {
            var store = _hive.Services.GetRequiredService<IHiveServiceStore>();
            store.RemoveService<IChatCompletionConnector>(AppConstants.OpenAIProvider);
            store.RemoveService<IEmbeddingConnector>(AppConstants.OpenAIProvider);

            var config = new OpenAIConfig
            {
                ApiKey = settings.Connectors.OpenAI.ApiKey,
                Organization = settings.Connectors.OpenAI.Organization,
                Project = settings.Connectors.OpenAI.Project,
            };
            store.AddService<IChatCompletionConnector>(AppConstants.OpenAIProvider, new OpenAIChatCompletionConnector(config));
            store.AddService<IEmbeddingConnector>(AppConstants.OpenAIProvider, new OpenAIEmbeddingConnector(config));
        }

        // LM Studio 서비스 변경
        if (CurrentSettings.Connectors.LMStudio != settings.Connectors.LMStudio)
        {
            var store = _hive.Services.GetRequiredService<IHiveServiceStore>();
            store.RemoveService<IChatCompletionConnector>(AppConstants.LMStudioProvider);
            store.RemoveService<IEmbeddingConnector>(AppConstants.LMStudioProvider);

            var config = new OpenAIConfig
            {
                BaseUrl = settings.Connectors.LMStudio.BaseUrl,
                ApiKey = settings.Connectors.LMStudio.ApiKey,
            };
            store.AddService<IChatCompletionConnector>(AppConstants.LMStudioProvider, new OpenAIChatCompletionConnector(config));
            store.AddService<IEmbeddingConnector>(AppConstants.LMStudioProvider, new OpenAIEmbeddingConnector(config));
        }

        // GPU Stack 서비스 변경
        if (CurrentSettings.Connectors.GPUStack != settings.Connectors.GPUStack)
        {
            var store = _hive.Services.GetRequiredService<IHiveServiceStore>();
            store.RemoveService<IChatCompletionConnector>(AppConstants.GPUStackProvider);
            store.RemoveService<IEmbeddingConnector>(AppConstants.GPUStackProvider);

            var config = new OpenAIConfig
            {
                BaseUrl = settings.Connectors.GPUStack.BaseUrl,
                ApiKey = settings.Connectors.GPUStack.ApiKey,
            };
            store.AddService<IChatCompletionConnector>(AppConstants.GPUStackProvider, new OpenAIChatCompletionConnector(config));
            store.AddService<IEmbeddingConnector>(AppConstants.GPUStackProvider, new OpenAIEmbeddingConnector(config));
        }

        // 교환
        CurrentSettings = settings;
        AppState.Ready();
    }

    private MemoryService CreateMemoryService(MemorySettings settings)
    {
        var (provider, model) = AppUtility.ParseModelIdentifier(settings.EmbeddingModel);
        var config = new VectorMemoryConfig
        {
            EmbedProvider = provider,
            EmbedModel = model,
        };
        var memory = _hive.CreateVectorMemory(config);
        return new MemoryService(memory, _db, settings);
    }

    public enum AppStatus
    {
        NotReady,
        Ready,
        Loading,
    }

    public class AppState
    {
        public AppStatus Status { get; set; } = AppStatus.NotReady;

        public string Message { get; set; } = "";

        public AppState()
        { }

        public AppState(AppStatus status, string message)
        {
            Status = status;
            Message = message;
        }

        public static AppState Ready()
            => new(AppStatus.Ready, "");

        public static AppState Loading(string message)
            => new(AppStatus.Loading, message);

        public static AppState NotReady(string message)
            => new(AppStatus.NotReady, message);
    }
}
