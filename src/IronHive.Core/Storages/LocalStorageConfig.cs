namespace IronHive.Core.Storages;

public static class LocalStorageConfig
{
    public static string DefaultDirectoryPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hivemind");

    public static string DefaultQueueStoragePath { get; } = Path.Combine(DefaultDirectoryPath, "queue");

    public static string DefaultVectorStoragePath { get; } = Path.Combine(DefaultDirectoryPath, "vector.db");

}
