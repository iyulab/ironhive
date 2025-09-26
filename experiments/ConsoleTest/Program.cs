using dotenv.net;
using IronHive.Providers.OpenAI;

DotEnv.Load(new DotEnvOptions(
    envFilePaths: [".env"],
    trimValues: true,
    overwriteExistingVars: false
));

return;
