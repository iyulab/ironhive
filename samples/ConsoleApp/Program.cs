using ConsoleApp;
using dotenv.net;

DotEnv.Load(new DotEnvOptions(
    envFilePaths: [".env"],
    trimValues: true,
    overwriteExistingVars: false
));

// === Message Generation 샘플 ===
await MessageSample.Run();
return;

// === Image Generation & Edit 샘플 ===
// await ImageSample.Run();
// return;

// === Audio Processing (TTS/STT) 샘플 ===
// await AudioSample.Run();
// return;

// === Video Generation 샘플 ===
//await VideoSample.Run();
//return;

