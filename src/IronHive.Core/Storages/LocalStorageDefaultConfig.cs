namespace IronHive.Core.Storages;

public static class LocalStorageDefaultConfig
{
    public static string BaseDirectoryPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hivemind");

    public static string QueueStoragePath { get; } = Path.Combine(BaseDirectoryPath, "queue");

    public static string VectorStoragePath { get; } = Path.Combine(BaseDirectoryPath, "vector.db");

}
