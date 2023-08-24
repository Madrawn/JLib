using System.Runtime.CompilerServices;

using JLib.Exceptions;

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

    internal void RegisterNavigation<T>(NavigationPropertyName propertyName, Func<ITypeCache, T> factory)
    {
        materializer.Add(() =>
        {

        });
        _lazies.Add<T>(propertyName, factory);
    }
    internal void RegisterNavigation<T>(Func<ITypeCache, T?> factory)
    {

        _lazies.Add<T>(factory);
    }

    internal T GetNavigation<T>(NavigationPropertyName propertyName) where T : TypeValueType?
    {
        if (_navigationLocked)
            throw new InvalidSetupException("tried to access a navigation property before it could be resolved. " +
                                            "This hint towards a circular dependency in their Initialization. " +
                                            "Try resolving one of them manually without using navigation properties");
        return _lazies.Get<T>(propertyName);
    }
    internal T GetNavigation<T>() where T : TypeValueType?
    {
        if (_navigationLocked)
            throw new InvalidSetupException("tried to access a navigation property before it could be resolved. " +
                                            "This hint towards a circular dependency in their Initialization. " +
                                            "Try resolving one of them manually without using navigation properties");
        return _lazies.Get<T>();
    }
}
public abstract record NavigatingTypeValueType(Type Value) : TypeValueType(Value)
{

    private TvtNavigationManager _navigationManager = new();
    private bool _navigationLocked = true;
    internal void UnlockNavigation() => _navigationLocked = false;
    internal void LockNavigation() => _navigationLocked = true;

    protected T Navigate<T>(Func<ITypeCache, T> resolver, [CallerMemberName] string propertyName = "") where T : TypeValueType?
    {
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