using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Raggle.Abstractions;
using Raggle.Abstractions.Messages;

namespace Raggle.Server.WebApi.Controllers;

public class GuestRequest
{
    public required string Service { get; set; }
    public required string Model { get; set; }
    public required MessageCollection Messages { get; set; }
}

[ApiController]
[Route("/v1/guest")]
public class GuestController : ControllerBase
{
    private readonly IRaggle _raggle;
    private readonly JsonOptions _jsonOptions;

    public GuestController(IRaggle raggle, IOptions<JsonOptions> jsonOptions)
    {
        _raggle = raggle;
        _jsonOptions = jsonOptions.Value;
    }

    //[HttpPost]
    //public async Task StreamingAsync(
    //    [FromBody] GuestRequest request,
    //    CancellationToken cancellationToken)
    //{
    //    var tools = new ToolCollection();
    //    var instruction = """
    //        you are helpful assistant to company employees.
    //        you can help them with kindness and respect.

    //        you have access to database connection, and the connection details are:
    //        - type: ms_sql_server
    //        - data: current company useful information
    //        """;

    //    var dbTool = FunctionToolFactory.CreateFromObject<DatabaseTool>(_raggle.Services);
    //    tools.AddRange(dbTool);
    //    //var pyTool = FunctionToolFactory.CreateFromObject<PythonTool>();
    //    //tools.AddRange(pyTool);

    //    var assistant = _raggle.CreateAssistant(
    //        service: request.Service,
    //        model: request.Model,
    //        id: Guid.NewGuid().ToString(),
    //        name: "The Test",
    //        description: "The Test",
    //        instruction: instruction,
    //        options: null,
    //        tools: tools);

    //    try
    //    {
    //        Response.ContentType = "application/stream+json";

    //        await foreach (var response in assistant.StreamingInvokeAsync(request.Messages, cancellationToken))
    //        {
    //            var json = JsonSerializer.Serialize(response, _jsonOptions.JsonSerializerOptions);

    //            var data = Encoding.UTF8.GetBytes(json + "\n");
    //            await Response.Body.WriteAsync(data, cancellationToken);

    //            await Response.Body.FlushAsync(cancellationToken);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine(ex.Message);
    //    }
    //}
}
