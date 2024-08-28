using LLama.Native;

namespace Raggle.Extensions.LLamaSharp;

public static class LLamaSharpNative
{
    public static void Initialize()
    {
        // Configure logging. Change this to `true` to see log messages from llama.cpp
        var showLLamaCppLogs = false;
        NativeLibraryConfig
           .All
           .WithLogCallback((level, message) =>
           {
               if (showLLamaCppLogs)
                   Console.WriteLine($"[llama {level}]: {message.TrimEnd('\n')}");
           });

        // Configure native library to use. This must be done before any other llama.cpp methods are called!
        NativeLibraryConfig
           .All
           .WithCuda()
           //.WithAutoDownload() // An experimental feature
           .DryRun(out var loadedllamaLibrary, out var loadedLLavaLibrary);

        // Calling this method forces loading to occur now.
        NativeApi.llama_empty_call();
    }
}
