using MessagePack;
using MessagePack.Resolvers;

namespace IronHive.Core.Utilities;

public static class MessageSerializer
{
    private static readonly MessagePackSerializerOptions _options = ContractlessStandardResolver.Options
        .WithCompression(MessagePackCompression.Lz4Block);

    /// <summary>
    /// Serialize the object to bytes using MessagePack
    /// </summary>
    public static byte[]? Serialize<T>(
        T value,
        MessagePackSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (value == null)
        {
            return null;
        }
        return MessagePackSerializer.Serialize(value, options ?? _options, cancellationToken);
    }

    /// <summary>
    /// Deserialize the bytes to an object using MessagePack
    /// </summary>
    public static T? Deserialize<T>(
        byte[] buffer, 
        MessagePackSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (buffer == null || buffer.Length == 0)
        {
            return default;
        }
        return MessagePackSerializer.Deserialize<T>(buffer, options ?? _options, cancellationToken);
    }
}
