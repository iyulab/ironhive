using IronHive.Connectors.OpenAI;
using IronHive.Core.Decoders;
using IronHive.Core.Handlers;
using IronHive.Core.Storages;
using WebApiSample.Data;
using WebApiSample.Services;
using WebApiSample.Settings;

namespace WebApiSample;

public static class Extensions
{
    /// <summary>
    /// 샘플 서비스를 사용합니다.
    /// </summary>
    public static WebApplicationBuilder UseSampleServices(this WebApplicationBuilder builder)
    {
        /*
         * 설정 파일을 등록합니다.
         * 1. 설정 파일을 확인하고, 없으면 기본 설정 파일을 생성합니다.
         * 2. JSON 파일로 등록하고, "Services" 섹션을 IOptions으로 등록합니다.
         */
        if (!File.Exists(AppConstants.SettingsFilePath))
        {
            new AppServicesSettings().SaveAsync(AppConstants.SettingsFilePath).Wait();
        }
        builder.Configuration.AddJsonFile(AppConstants.SettingsFilePath, false, true);
        var section = builder.Configuration.GetSection("Services");
        var settings = section.Get<AppServicesSettings>()
            ?? throw new InvalidOperationException("Failed to load settings.");
        builder.Services.Configure<AppServicesSettings>(section);

        /*
         * Hive 서비스를 등록 합니다.
         * 1. Connectors: AI 서비스 커넥터를 등록합니다.
         * 2. Storage: 파일 스토리지, 큐 스토리지, 벡터 스토리지를 등록합니다.
         * 3. Decoders: 파일 디코더를 등록합니다.
         * 4. Handlers: 파이프라인 핸들러를 등록합니다.
         */
        builder.Services.AddHiveServiceCore()
        // AI 서비스 커넥터 등록
        .AddOpenAIConnectors(AppConstants.OpenAIProvider, new OpenAIConfig
        {
            ApiKey = settings.Connectors.OpenAI.ApiKey,
            Organization = settings.Connectors.OpenAI.Organization,
            Project = settings.Connectors.OpenAI.Project,
        })
        .AddOpenAIConnectors(AppConstants.LMStudioProvider, new OpenAIConfig
        {
            BaseUrl = settings.Connectors.LMStudio.BaseUrl,
            ApiKey = settings.Connectors.LMStudio.ApiKey,
        })
        .AddOpenAIConnectors(AppConstants.GPUStackProvider, new OpenAIConfig
        {
            BaseUrl = settings.Connectors.GPUStack.BaseUrl,
            ApiKey = settings.Connectors.GPUStack.ApiKey
        })
        // 스토리지 등록
        .AddFileStorage<LocalFileStorage>(AppConstants.LocalFileStorage)
        .WithQueueStorage(new LocalQueueStorage(AppConstants.QueueDirectoryPath, TimeSpan.FromDays(1)))
        .WithVectorStorage(new LocalVectorStorage(AppConstants.VectorDatabasePath))
        // 파일 디코더 등록
        .AddFileDecoder(new TextDecoder())
        .AddFileDecoder(new PDFDecoder())
        .AddFileDecoder(new WordDecoder())
        .AddFileDecoder(new PPTDecoder())
        // 파이프라인 핸들러 등록
        .AddPipelineHandler<TextExtractionHandler>(AppConstants.ExtractStep)
        .AddPipelineHandler<TextChunkerHandler>(AppConstants.ChunkStep)
        .AddPipelineHandler<QnAExtractionHandler>(AppConstants.QnAStep)
        .AddPipelineHandler<VectorEmbeddingHandler>(AppConstants.EmbeddingStep)
        // 파이프라인 옵저버 등록
        .AddPipelineObserver<PipelineWorkerObserver>();

        /*
         * 메인 서비스 등록
         * 2. AppDbContext: DB 컨텍스트를 등록합니다.(싱글톤)
         * 3. AppService: 메인 서비스를 등록합니다.
         */
        builder.Services.AddDbContext<AppDbContext>(ServiceLifetime.Singleton, ServiceLifetime.Singleton);
        builder.Services.AddSingleton<AppService>();

        return builder;
    }
}
