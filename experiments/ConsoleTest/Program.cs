using System.Text.Json;
using IronHive.Abstractions.Tools;

namespace ConsoleTest;

internal class Program
{
    static async Task Main(string[] args)
    {
        // 1) 테스트용 OpenAPI (Swagger Petstore 축약본)
        //    - 서버: https://petstore3.swagger.io/api/v3
        //    - 오퍼레이션:
        //      a) GET /pet/{petId}   (operationId: getPetById)
        //      b) GET /pet/findByStatus (operationId: findPetsByStatus)
        var oas = /*lang=json*/ """
        {
          "openapi": "3.0.3",
          "info": { "title": "Mini Petstore", "version": "1.0.0" },
          "servers": [{ "url": "https://petstore3.swagger.io/api/v3" }],
          "paths": {
            "/pet/{petId}": {
              "get": {
                "operationId": "getPetById",
                "parameters": [
                  {
                    "name": "petId",
                    "in": "path",
                    "required": true,
                    "schema": { "type": "integer", "format": "int64" }
                  }
                ],
                "responses": {
                  "200": { "description": "pet found" },
                  "404": { "description": "pet not found" }
                }
              }
            },
            "/pet/findByStatus": {
              "get": {
                "operationId": "findPetsByStatus",
                "parameters": [
                  {
                    "name": "status",
                    "in": "query",
                    "required": true,
                    "schema": { "type": "string", "enum": ["available","pending","sold"] }
                  }
                ],
                "responses": {
                  "200": { "description": "ok" }
                }
              }
            }
          }
        }
        """;

        // 2) Tool 생성: operationId 기반
        var toolGetById = OpenApiTool.FromOperationId(
            openApiContent: oas,
            operationId: "getPetById",
            apiName: "petstore",
            source: "embedded"
        );

        var toolFindByStatus = OpenApiTool.FromOperationId(
            openApiContent: oas,
            operationId: "findPetsByStatus",
            apiName: "petstore",
            source: "embedded"
        );

        // 3) 입력 구성 (ToolInput는 최상위 Dictionary<string, object?> 여야 함)
        // 3-1) GET /pet/{petId}
        var inputGetById = new ToolInput(new Dictionary<string, object?>
        {
            ["path"] = new Dictionary<string, object?> { ["petId"] = 1L }, // long으로 맞춰도 되고, int도 대부분 OK
            // ["headers"] = new Dictionary<string, object?> { ["X-Debug"] = "1" }, // 필요하면
            // ["__serverUrl"] = "https://petstore3.swagger.io/api/v3" // 서버 오버라이드도 가능
        });

        // 3-2) GET /pet/findByStatus?status=available
        var inputFindByStatus = new ToolInput(new Dictionary<string, object?>
        {
            ["query"] = new Dictionary<string, object?> { ["status"] = "available" }
        });

        // 4) 실제 호출
        Console.WriteLine("=== 1) GET /pet/{petId} ===");
        var res1 = await toolGetById.InvokeAsync(inputGetById);
        DumpToolOutput(res1);

        Console.WriteLine();
        Console.WriteLine("=== 2) GET /pet/findByStatus?status=available ===");
        var res2 = await toolFindByStatus.InvokeAsync(inputFindByStatus);
        DumpToolOutput(res2);

        Console.WriteLine();
        Console.WriteLine("끝! 아무 키나 누르면 종료합니다.");
        Console.ReadKey();
    }

    private static void DumpToolOutput(ToolOutput output)
    {
        Console.WriteLine($"IsSuccess: {output.IsSuccess}");
        if (!string.IsNullOrWhiteSpace(output.Result))
        {
            try
            {
                // JSON이면 이쁘게 출력
                using var doc = JsonDocument.Parse(output.Result);
                Console.WriteLine(JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions
                {
                    WriteIndented = true
                }));
            }
            catch
            {
                // JSON이 아니면 그대로 출력
                Console.WriteLine(output.Result);
            }
        }
    }
}
