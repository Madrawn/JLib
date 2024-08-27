using JLib.Exceptions;
using JLib.Helper;

namespace JLib.Reflection;
/// <summary>
/// Indicates, that a navigation from one <see cref="TypeValueType"/> to another via <see cref="NavigatingTypeValueType.Navigate{T}"/> has failed.
/// </summary>
public class TvtNavigationFailedException : InvalidSetupException
{
    /// <summary>
    /// The <see cref="TypeValueType"/> where the navigation failed.
    /// </summary>
    public NavigatingTypeValueType Type { get; }
    /// <summary>
    /// The <see cref="System.Type"/> of the <see cref="TypeValueType"/> the field tried to navigate to.
    /// </summary>
    public Type NavigationPropertyType { get; }

    internal TvtNavigationFailedException(NavigatingTypeValueType type, Type navigationPropertyType, string propertyName, Exception innerException)
        : base($"{navigationPropertyType.FullName} {type.Value.Name}.{propertyName}: The navigation failed due to an unhandled error: {innerException.Message}", innerException)
    {
        Type = type;
        Data[nameof(Type)] = type;
        NavigationPropertyType = navigationPropertyType;
        Data[nameof(NavigationPropertyType)] = navigationPropertyType;
    }
}
/// <summary>
/// indicates, that a <see cref="ValueType"/> member has been accessed before the <see cref="ITypeCache"/> Initialization has been completed.
/// </summary>
public class UninitializedTypeCacheException : InvalidSetupException
{
    internal UninitializedTypeCacheException(NavigatingTypeValueType detectedInType) : base(
        $"{detectedInType.Name}: the TypeCache has not been initialized yet")

    {
    }
    internal UninitializedTypeCacheException(NavigatingTypeValueType detectedInType, Type navigationPropertyType, string propertyName) : base(
        $"{navigationPropertyType.FullName()} {detectedInType.Name}.{propertyName}: the TypeCache has not been initialized yet")

    {
    }
}