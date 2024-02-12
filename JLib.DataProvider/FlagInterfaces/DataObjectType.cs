using JLib.Reflection;

namespace JLib.DataProvider;

public abstract record DataObjectType(Type Value) : NavigatingTypeValueType(Value), IDataObjectType
{
    public const int NextPriority = 10_000;
}