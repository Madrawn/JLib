using System.Runtime.CompilerServices;
using JLib.Exceptions;

namespace JLib.Reflection;

/// <summary>
/// A base class for <see cref="TypeValueType"/>s which allows them to include navigation properties to other <see cref="TypeValueType"/>s.
/// </summary>
/// <param name="Value"></param>
public abstract record NavigatingTypeValueType(Type Value) : TypeValueType(Value)
{
    private TvtNavigationManager? _navigationManager;

    internal void SetCache(ITypeCache typeCache)
    {
        if (_navigationManager is not null)
            throw new("you must not set the cache twice");

        _navigationManager = new(this, typeCache);
    }

    private string CacheExceptionText([CallerMemberName] string callerMemberName = "")
        => $"{GetType().Name}.{callerMemberName} ({Value.Name}) cache has not been set yet";

    internal void MaterializeNavigation()
    {
        if (_navigationManager is null)
            throw new UninitializedTypeCacheException(this);
        _navigationManager.Materialize();
    }
    /// <summary>
    /// returns a materialized instance of <typeparamref name="T"/> using the given <paramref name="resolver"/>
    /// </summary>
    /// <typeparam name="T">the <see cref="TypeValueType"/> of the associated <see cref="Type"/></typeparam>
    /// <param name="resolver">provides a <see cref="ITypeCache"/> to pull the to be referenced <typeparamref name="T"/> from.</param>
    /// <param name="propertyName">the name of the Property</param>
    /// <returns>the instance of <typeparamref name="T"/> which has been returned by the <paramref name="resolver"/></returns>
    /// <exception cref="TvtNavigationFailedException"></exception>
    protected T Navigate<T>(Func<ITypeCache, T> resolver, [CallerMemberName] string propertyName = "")
        where T : TypeValueType?
    {
        if (_navigationManager is null)
            throw new UninitializedTypeCacheException(this, typeof(T), propertyName);
        try
        {
            return _navigationManager.Navigate(resolver, propertyName);
        }
        catch (TvtNavigationFailedException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new TvtNavigationFailedException(this, typeof(T), propertyName, e);
        }
    }
}