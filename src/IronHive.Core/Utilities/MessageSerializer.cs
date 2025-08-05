using MessagePack;
using MessagePack.Resolvers;

namespace IronHive.Core.Utilities;

/// <summary>
/// MessagePack을 이용한 직렬화 유틸리티 클래스입니다.
/// </summary>
public static class MessageSerializer
{
    private static readonly MessagePackSerializerOptions _options = ContractlessStandardResolver.Options
        .WithCompression(MessagePackCompression.Lz4Block);

    /// <summary>
    /// 객체를 MessagePack 형식으로 직렬화합니다.
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
    /// MessagePack 형식의 바이트 배열을 객체로 역직렬화합니다.
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
