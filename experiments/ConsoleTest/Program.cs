using dotenv.net;
using IronHive.Abstractions.Messages;
using IronHive.Providers.OpenAI;
using System.Text.Json;
using System.Text.Json.Schema;

DotEnv.Load(new DotEnvOptions(
    envFilePaths: [".env"],
    trimValues: true,
    overwriteExistingVars: false
));

//await OpenApiTest.RunAsync();

var key = Environment.GetEnvironmentVariable("OPENAI");
//await TestRunner.RunAsync(key);
await TestRunner.RunStreamAsync(key);

//var options = JsonSerializerOptions.Default;
//var schema = options.GetJsonSchemaAsNode(typeof(MessageContent), new JsonSchemaExporterOptions
//{
//    TreatNullObliviousAsNonNullable = true,
//});

//Console.WriteLine(schema.ToString());
