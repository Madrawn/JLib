using System.Diagnostics.CodeAnalysis;
using JLib.Exceptions;
using JLib.Helper;

namespace JLib;

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
            var pi = _owner.GetType().GetProperty(propertyName) ?? throw new InvalidSetupException($"property {propertyName} not found in type {_owner.GetType().Name}");

            if (pi.IsNullable() is false && value is null)
                throw new InvalidSetupException($"property {_owner.Name}.{propertyName} must not be null");

            return value;
        })).Value?.As<T>()!;

    internal void Materialize()
    {
        foreach (var _ in _lazies.Values.Select(value => value.Value)) { }
    }

}