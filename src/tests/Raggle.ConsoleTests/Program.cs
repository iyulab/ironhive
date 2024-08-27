using Raggle.Tools.HuggingFace;

var hf = new HuggingFaceClient();
await foreach(var ss in hf.DownloadRepoAsync("nomic-ai/nomic-embed-text-v1", @"C:\Models"))
{ 
    Console.WriteLine(ss.TotalProgress);
};