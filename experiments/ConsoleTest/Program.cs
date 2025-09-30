using dotenv.net;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using System.Text.Json;

DotEnv.Load(new DotEnvOptions(
    envFilePaths: [".env"],
    trimValues: true,
    overwriteExistingVars: false
));
var key = Environment.GetEnvironmentVariable("GOOGLE") 
    ?? throw new Exception("GOOGLE_API_KEY is not set in .env file");

await TestRunner.RunAsync(key);

return;
