using System.Reflection;
using JLib.Exceptions;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.Testing;

public static class ServiceCollectionHelper
{
    public static IServiceCollection AddTypeCache(this IServiceCollection services, Assembly[]? assemblies,
        Type[]? types, out ITypeCache cache, IExceptionManager exceptions)
    {
        cache = new TypeCache(
            assemblies ?? Array.Empty<Assembly>(),
            types ?? Array.Empty<Type>(),
            exceptions
        );
        return services.AddSingleton(cache);
    }
}