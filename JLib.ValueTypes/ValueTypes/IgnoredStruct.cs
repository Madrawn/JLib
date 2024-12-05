namespace JLib.ValueTypes;

/// <summary>
/// used to indicate when a TypeArgument is not used for reflection and can be ignored
/// </summary>
// ReSharper disable once ConvertToStaticClass
public struct IgnoredStruct
{
    /// <summary>
    /// throws an <see cref="InvalidOperationException"/> when called
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public IgnoredStruct()
    {
        throw new InvalidOperationException(nameof(IgnoredStruct) +
                                            " may be used for reflection only and must not be instantiated");
    }
}