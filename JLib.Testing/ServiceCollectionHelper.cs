using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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