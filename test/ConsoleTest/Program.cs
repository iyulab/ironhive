using IronHive.Abstractions;
using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Connectors.Anthropic;
using IronHive.Connectors.OpenAI;
using IronHive.Core;
using IronHive.Storages.LiteDB;
using IronHive.Storages.Qdrant;
using Microsoft.Extensions.DependencyInjection;
using Qdrant.Client.Grpc;

var text = "Hello, World!";
Console.WriteLine(text);

var hive = Create();
var memory = hive.Services.GetRequiredService<IHiveMemory>();
var embed = hive.Services.GetRequiredService<IEmbeddingService>();

#region 컬렉션 이름 테스트
//소문자
//await memory.CreateCollectionAsync("test", "openai", "text-embedding-3-large");
////대문자
//await memory.CreateCollectionAsync("TEST", "openai", "text-embedding-3-large");
////숫자
//await memory.CreateCollectionAsync("123", "openai", "text-embedding-3-large");
////특수문자
//await memory.CreateCollectionAsync("test!", "openai", "text-embedding-3-large");
//await memory.CreateCollectionAsync("test@", "openai", "text-embedding-3-large");
//await memory.CreateCollectionAsync("test#", "openai", "text-embedding-3-large");
//await memory.CreateCollectionAsync("test$", "openai", "text-embedding-3-large");
//await memory.CreateCollectionAsync("test%", "openai", "text-embedding-3-large");
//await memory.CreateCollectionAsync("test&", "openai", "text-embedding-3-large");
//await memory.CreateCollectionAsync("test*", "openai", "text-embedding-3-large");

//await memory.CreateCollectionAsync("test(", "openai", "text-embedding-3-large");
//await memory.CreateCollectionAsync("test)", "openai", "text-embedding-3-large");
//await memory.CreateCollectionAsync("test<", "openai", "text-embedding-3-large");
//await memory.CreateCollectionAsync("test>", "openai", "text-embedding-3-large");
//await memory.CreateCollectionAsync("test{", "openai", "text-embedding-3-large");
//await memory.CreateCollectionAsync("test}", "openai", "text-embedding-3-large");
//await memory.CreateCollectionAsync("test[", "openai", "text-embedding-3-large");
//await memory.CreateCollectionAsync("test]", "openai", "text-embedding-3-large");

//await memory.CreateCollectionAsync("test.", "openai", "text-embedding-3-large");
//await memory.CreateCollectionAsync("test-", "openai", "text-embedding-3-large");
//await memory.CreateCollectionAsync("test_", "openai", "text-embedding-3-large");
//await memory.CreateCollectionAsync("test/", "openai", "text-embedding-3-large");
//await memory.CreateCollectionAsync("test:", "openai", "text-embedding-3-large");
////공백
//await memory.CreateCollectionAsync("test 123", "openai", "text-embedding-3-large");
////한글
//await memory.CreateCollectionAsync("안녕하세요", "openai", "text-embedding-3-large");
////길이
//var str = new string('a', 1024);
//await memory.CreateCollectionAsync(str, "openai", "text-embedding-3-large");
////null
//await memory.CreateCollectionAsync(null, "openai", "text-embedding-3-large");
#endregion

#region 주입 테스트
//await memory.CreateCollectionAsync("test", "openai", "text-embedding-3-large");

//var dummies = new string[]
//{
//    "물 섭취의 중요성: 하루 최소 8잔의 물을 마시면 신진대사 활성화에 도움이 됩니다.",
//    "규칙적인 운동: 주 3회 이상 운동하면 심혈관 건강이 증진됩니다.",
//    "독서의 혜택: 매일 30분씩 책을 읽으면 두뇌 활성화와 창의력 향상에 도움이 됩니다.",
//    "재테크 팁: 지출을 기록하면 소비 습관을 파악하고 저축을 늘릴 수 있습니다.",
//    "코딩 학습: 프로그래밍 언어를 배우면 문제 해결 능력과 창의력을 키울 수 있습니다.",
//    "여행 준비: 여행 전 필수 앱과 정보를 미리 준비하면 안전한 여행이 가능합니다.",
//    "환경 보호: 재활용과 에너지 절약을 통해 환경 오염을 줄일 수 있습니다.",
//    "건강한 식습관: 다양한 채소와 과일을 섭취하면 비타민과 미네랄 보충에 도움이 됩니다.",
//    "스트레스 관리: 명상과 심호흡 연습은 정신 건강 유지에 효과적입니다.",
//    "외국어 학습: 매일 단어 암기와 회화 연습은 언어 능력 향상에 큰 도움이 됩니다.",
//    "시간 관리: 할 일 목록을 작성하면 업무 효율성을 높일 수 있습니다.",
//    "재택근무 팁: 일정한 루틴과 규칙적인 휴식은 생산성 유지에 중요합니다.",
//    "인터넷 보안: 정기적인 비밀번호 변경과 이중 인증 설정은 계정 안전에 필수입니다.",
//    "자기계발: 새로운 취미를 시도하면 창의력과 자기 만족감을 높일 수 있습니다.",
//    "경제 동향: 주기적인 경제 뉴스 파악은 투자 결정에 유용합니다.",
//    "사회적 네트워킹: 다양한 사람들과의 만남은 정보와 기회의 창을 열어줍니다.",
//    "음악 감상: 다양한 장르의 음악은 스트레스 해소와 감성 자극에 좋습니다.",
//    "예술 감상: 미술 전시회 방문은 문화적 감수성과 창의력 향상에 기여합니다.",
//    "역사 탐구: 과거 사건들을 공부하면 현재 문제를 더 깊이 이해할 수 있습니다.",
//    "자연 관찰: 주말에 가까운 산이나 공원을 방문하면 심리 안정에 도움이 됩니다."
//};

//var sources = new List<TextMemorySource>();
//foreach (var dummy in dummies)
//{
//    sources.Add(new TextMemorySource
//    {
//        Id = Guid.NewGuid().ToString(),
//        Text = dummy,
//    });
//}

//foreach (var source in sources)
//{
//    var vector = await embed.EmbedAsync("openai", "text-embedding-3-large", source.Text);
//    await memory.Storage.UpsertVectorsAsync("test", new VectorRecord[]
//    {
//        new VectorRecord
//        {
//            Id = Guid.NewGuid().ToString(),
//            Source = source,
//            Vectors = vector,
//            Payload = source.Text,
//            LastUpdatedAt = DateTime.Now,
//        },
//    });
//}
#endregion

#region 찾기 테스트
//var filter = new VectorRecordFilter();
//filter.AddSourceId("96e1ddd9-ca23-4d9c-9d39-b5d1cfdb14f0");
//filter.AddVectorId("1a852546-07fc-4a3c-8c6d-4d11b6dd7aeb");
//var points = await memory.Storage.FindVectorsAsync("test", limit: 6, filter: filter);
//var list = points.ToList();

//var query = "요즘 날씨가 좀 쌀쌀허네";
//var qVector = await embed.EmbedAsync("openai", "text-embedding-3-large", query);
//var scored = await memory.Storage.SearchVectorsAsync("test", qVector, 0.0f, 5, filter);
//var list = scored.ToList();
#endregion

#region 정리
//await memory.DeleteCollectionAsync("test");
#endregion

return;

IHiveMind Create()
{
    var o_config = new OpenAIConfig
    {
        ApiKey = "",
    };
    var a_config = new AnthropicConfig
    {
        ApiKey = "",
    };
    var g_config = new OpenAIConfig
    {
        BaseUrl = "https://generativelanguage.googleapis.com/v1beta/openai/",
        ApiKey = ""
    };
    var l_config = new OpenAIConfig
    {
        BaseUrl = "http://172.30.1.53:8080/v1-openai/",
        ApiKey = ""
    };

    var mind = new HiveServiceBuilder()
        .AddChatCompletionConnector("openai", new OpenAIChatCompletionConnector(o_config))
        .AddEmbeddingConnector("openai", new OpenAIEmbeddingConnector(o_config))
        .AddChatCompletionConnector("anthropic", new AnthropicChatCompletionConnector(a_config))
        .AddChatCompletionConnector("gemini", new OpenAIChatCompletionConnector(g_config))
        .AddChatCompletionConnector("iyulab", new OpenAIChatCompletionConnector(l_config))
        //.WithVectorStorage(new LiteDBVectorStorage(new LiteDBConfig
        //{
        //    DatabasePath = "c:\\temp\\vector.db"
        //}))
        .WithVectorStorage(new QdrantVectorStorage(new QdrantConfig()))
        .BuildHiveMind();

    return mind;
}
