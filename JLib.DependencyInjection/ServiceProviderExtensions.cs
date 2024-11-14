using Microsoft.Extensions.DependencyInjection;

namespace JLib.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceProvider"/>.
/// </summary>
public static partial class ServiceProviderExtensions
{
    /// <summary>
    /// retrieves the <typeparamref name="TServiceContainer"/> from the <paramref name="provider"/> which may reduce the number of required overloads of a method by replacing the dedicated service types with a single <typeparamref name="TServiceContainer"/> parameter.
    /// </summary>
    public static TServiceContainer GetServiceContainer<TServiceContainer>(this IServiceProvider provider)
        where TServiceContainer : ServiceContainer, new()
    {
        var c = new TServiceContainer();
        c.Init(provider);
        return c;
    }

    /// <summary>
    /// retrieves <typeparamref name="TService"/> from the <paramref name="provider"/> once it is actually needed.
    /// </summary>
    public static Lazy<TService> GetRequiredLazyService<TService>(this IServiceProvider provider)
        where TService : notnull
        => new(provider.GetRequiredService<TService>);
    /// <summary>
    /// retrieves <typeparamref name="TService"/> from the <paramref name="provider"/> once it is actually needed.
    /// </summary>
    public static Lazy<TService?> GetLazyService<TService>(this IServiceProvider provider)
        where TService : notnull
        => new(provider.GetService<TService>);
}