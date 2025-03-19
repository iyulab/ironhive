using IronHive.Core;
using IronHive.Storages.AmazonS3;
using IronHive.Storages.LocalDisk;

var storage = new LocalFileStorage();

var files = await storage.ListAsync("/");
foreach (var file in files)
{
    Console.WriteLine(file);
}

return;
