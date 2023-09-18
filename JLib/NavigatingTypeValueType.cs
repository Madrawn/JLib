using System.Runtime.CompilerServices;

using JLib.Exceptions;

namespace JLib;

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
            throw new TvtNavigationFailedException("cache has not been set yet");
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
            throw new TvtNavigationFailedException(
                $"{typeof(T).Name} {GetType().Name}.{propertyName}: The navigation failed due to an unhandled error: {e.Message}", e);
        }
    }
}