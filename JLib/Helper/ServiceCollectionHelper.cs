using Microsoft.Extensions.DependencyInjection;

namespace JLib.Helper;
public static class ServiceCollectionHelper
{
    public static IServiceCollection AddAlias<TImpl, TAlias>(this IServiceCollection serviceCollection, ServiceLifetime lifetime)
        where TImpl : TAlias
        where TAlias : notnull
    {
        serviceCollection.Add(new(typeof(TAlias),
            provider => provider.GetRequiredService<TImpl>(), lifetime));
        return serviceCollection;
    }
}
