using System.Reflection;
using System.Reflection.Metadata;
using JLib.Data;
using JLib.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using static JLib.Types;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Linq;

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
    /// <summary>
    /// provides the alias as existing service
    /// </summary>
    /// <param name="services"></param>
    /// <param name="existing"></param>
    /// <param name="alias"></param>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public static IServiceCollection AddAlias(this IServiceCollection services, Type existing, Type alias, ServiceLifetime lifetime)
    {
        services.Add(new(alias, provider => provider.GetRequiredService(existing), lifetime));
        return services;
    }

    public static IServiceCollection AddTypeCache(this IServiceCollection services, Assembly executingAssembly, Func<AssemblyName, bool> assemblyFilter, out ITypeCache typeCache)
    {
        var assemblyNames = executingAssembly.GetReferencedAssemblies();
        var assemblies = assemblyNames
            .Where(assemblyFilter)
            .Select(Assembly.Load).Append(executingAssembly);
        typeCache = new TypeCache(assemblies);

        services.AddSingleton(typeCache);
        return services;
    }

    public static IServiceCollection AddTypeCache(this IServiceCollection services, Assembly executingAssembly, out ITypeCache typeCache, params string[] assemblyPrefix)
        => services.AddTypeCache(executingAssembly, name => assemblyPrefix.Any(prefix => name.Name?.StartsWith(prefix) ?? false), out typeCache);

    public static IServiceCollection AddTypeCache(this IServiceCollection services, Assembly executingAssembly, params string[] assemblyPrefix)
        => services.AddTypeCache(executingAssembly, out _, assemblyPrefix);

    public static IServiceCollection AddDataProvider<TTvt, TImplementation>(this IServiceCollection services,
        ITypeCache typeCache, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TTvt : TypeValueType
        => services.AddDataProvider<TTvt, TImplementation>(typeCache, lifetime, _ => true, a => a);
    public static IServiceCollection AddDataProvider<TTvt, TImplementation>(this IServiceCollection services,
        ITypeCache typeCache,
        Func<TTvt, bool> filter,
        params Func<TTvt, TypeValueType>[] typeArgumentResolver)
        where TTvt : TypeValueType
        => AddDataProvider<TTvt, TImplementation>(services, typeCache, ServiceLifetime.Scoped, filter,
            typeArgumentResolver);

    public static IServiceCollection AddDataProvider<TTvt, TImplementation>(
        this IServiceCollection services,
        ITypeCache typeCache,
        ServiceLifetime lifetime,
        Func<TTvt, bool> filter,
        params Func<TTvt, TypeValueType>[] typeArgumentResolver)
        where TTvt : TypeValueType
    {
        var implementation = typeCache.TryGet<Types.DataProviderImplementation, TImplementation>()
                             ?? throw new InvalidSetupException($"DataProvider Implementation {typeof(TImplementation).Name} is not valid or not registered with the typeCache");

        Console.WriteLine();
        Console.WriteLine($"Adding data provider implementation '{implementation.Name}' for '{typeof(TTvt).Name}'");

        IExceptionManager exceptions = new ExceptionManager(
            $"DataProvider Implementation for Tvt {typeof(TTvt).Name} and Implementation {typeof(TImplementation).Name} failed");

        foreach (var data in typeCache.All(filter))
        {
            var arguments = typeArgumentResolver.Select(r => r(data)).ToArray();
            var explicitImplementation = implementation.Value.MakeGenericType(arguments);

            var readService = typeof(IDataProviderR<>).MakeGenericType(data.Value);

            Console.Write(
                $"    {data.Name}: {explicitImplementation.FullClassName()} as {readService.FullClassName()}");

            if (implementation.WriteSupported)
            {
                var writeService = typeof(IDataProviderRw<>).MakeGenericType(data.Value);

                Console.Write($" and {writeService.FullClassName()}");
                exceptions.TryExecution(() =>
                    services.Add(new ServiceDescriptor(
                        writeService,
                        explicitImplementation, lifetime)));

                // not registering the read service as alias would result in a a second instance, which might cause issues, especially when using singleton mocks
                services.AddAlias(writeService, readService, lifetime);
            }
            else
            {
                exceptions.TryExecution(() =>
                    services.Add(new ServiceDescriptor(readService, explicitImplementation, lifetime))
                );
            }
            Console.WriteLine($" as {lifetime} Service");
        }

        return services;

    }

}
