namespace IronHive.Abstractions.Files;

public interface IFileStorageFactory
{
    IFileStorage Create(object config);
}
