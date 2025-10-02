using dotenv.net;
using IronHive.Abstractions;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Tools;
using IronHive.Core;
using IronHive.Core.Services;
using IronHive.Core.Tools;
using IronHive.Providers.GoogleAI;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Text.Json;

DotEnv.Load(new DotEnvOptions(
    envFilePaths: [".env"],
    trimValues: true,
    overwriteExistingVars: false
));
var key = Environment.GetEnvironmentVariable("GOOGLE") 
    ?? throw new Exception("GOOGLE_API_KEY is not set in .env file");


return;