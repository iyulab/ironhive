using Raggle.Tools.ModelSearch.Clients;
using Raggle.Tools.ModelSearch.Models;
using System.Text.Json;

var hf = new HuggingFaceClient();
var option = new JsonSerializerOptions
{
    WriteIndented = true
};
await foreach (var progress in hf.DownloadRepoAsync("nomic-ai/nomic-embed-text-v1.5", @"C:\Models"))
{
    Console.Clear();
    Console.WriteLine(JsonSerializer.Serialize(progress, option));
}