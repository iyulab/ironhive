namespace IronHive.Abstractions.Exceptions;

/// <summary>
/// Base type for IronHive domain exceptions. Providers and pipeline components raise
/// subtypes of this exception so consumers can write typed recovery logic instead of
/// parsing provider-specific error strings.
/// </summary>
public class HiveException : Exception
{
    public HiveException()
    { }

    public HiveException(string message)
        : base(message)
    { }

    public HiveException(string message, Exception? innerException)
        : base(message, innerException)
    { }
}
