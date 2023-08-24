using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using JLib.Exceptions;
using JLib.Helper;

namespace JLib;

public interface IPostInitValidatedType
{
    void PostInitValidation(ITypeCache cache);
}

public abstract record TypeValueType(Type Value) : ValueType<Type>(Value)
{
    public string Name => Value.Name;
}

internal class TvtNavigationManager
{
    private readonly NavigatingTypeValueType _owner;
    private readonly ITypeCache _cache;
    internal TvtNavigationManager(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
        NavigatingTypeValueType owner,
        ITypeCache cache)
    {
        _owner = owner;
        _cache = cache;
    }

    private readonly Dictionary<string, Lazy<TypeValueType?>> _lazies = new();

    internal T Navigate<T>(Func<ITypeCache, T> resolver, string propertyName)
        where T : TypeValueType?
        => _lazies.GetValueOrAdd(propertyName, () => new(() =>
        {
            var value = resolver(_cache);
            var pi = _owner.Value.GetProperty(propertyName) ?? throw new InvalidSetupException($"property {propertyName} not found in type {_owner.Name}");

            if (pi.IsNullable() is false && value is null)
                throw new InvalidSetupException($"property {_owner.Name}.{propertyName} must not be null");

            return value;
        })).Value?.As<T>()!;

    internal void Materialize()
    {
        foreach (var _ in _lazies.Values.Select(value => value.Value)) { }
    }
}

public abstract record NavigatingTypeValueType(Type Value) : TypeValueType(Value)
{

    private TvtNavigationManager? _navigationManager;

    internal void SetCache(ITypeCache typeCache)
    {
        if (_navigationManager is not null)
            throw new InvalidSetupException("you must not set the cache twice");

        _navigationManager = new(this, typeCache);
    }
    internal void MaterializeNavigation()
    {
        if (_navigationManager is null)
            throw new InvalidSetupException("cache has not been set yet");
        _navigationManager.Materialize();
    }

    protected T Navigate<T>(Func<ITypeCache, T> resolver, [CallerMemberName] string propertyName = "")
        where T : TypeValueType?
    {
        if (_navigationManager is null)
            throw new InvalidSetupException("cache has not been set yet");
        return _navigationManager.Navigate(resolver, propertyName);
    }
}

public static class Types
{
}


public record NavigationPropertyName(string Value) : StringValueType(Value)
{
    public static implicit operator NavigationPropertyName(string value)
        => new(value);

}
