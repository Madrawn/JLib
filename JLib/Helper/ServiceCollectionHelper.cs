using System.Reflection;
using System.Reflection.Metadata;
using JLib.Data;
using JLib.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using static JLib.Types;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Linq;
using System.Runtime;
using JLib.FactoryAttributes;
using System;
using System.Formats.Tar;
using System.Text.Json.Serialization.Metadata;

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

    /// <summary>
    /// Provides a <see cref="IDataProviderR{TData}"/> for each <typeparamref name="TTvt"/> which takes data from a <see cref="IDataProviderR{TData}"/> of the type retrieved by <see cref="sourcePropertyReader"/> and maps it using AutoMapper Projections
    /// <br/>exceptions are added as child to the <see cref="parentExceptionMgr"/> or thrown as a <see cref="JLibAggregateException"/> when it was not provided
    /// </summary>
    public static IServiceCollection AddMapDataProvider<TTvt>(
        this IServiceCollection services,
        ITypeCache typeCache,
        Func<TTvt, TypeValueType?> sourcePropertyReader,
        IExceptionManager? parentExceptionMgr = null)
        where TTvt : TypeValueType
    {
        var msg = $"{nameof(AddMapDataProvider)} failed for valueType {typeof(TTvt).Name}";
        IExceptionManager exceptions = parentExceptionMgr?.CreateChild(msg) ?? new ExceptionManager(msg);

        services.AddGenericServices<TTvt, IDataProviderR<Ignored>, MapDataProvider<IEntity, Ignored>>(typeCache, ServiceLifetime.Scoped, exceptions,
             destination => sourcePropertyReader(destination) is not null,
             null,
             new Func<TTvt, TypeValueType>[]
            {
                destination=>sourcePropertyReader(destination)!,
                destination=>destination
            });

        if (parentExceptionMgr is null)
            exceptions.ThrowIfNotEmpty();
        return services;
    }
    /// <summary>
    /// adds a <see cref="IDataProviderR{TData}"/> and <see cref="IDataProviderRw{TData}"/> with the <see cref="MockDataProvider{TEntity}"/> as implementation for each <typeparamref name="TTvt"/> as scoped
    /// <br/>adds a <see cref="MockDataProviderStore{TEntity}"/> for each entity as singleton
    /// <br/>exceptions are added as child to the <see cref="parentExceptionMgr"/> or thrown as a <see cref="JLibAggregateException"/> when it was not provided
    /// </summary>
    public static IServiceCollection AddMockDataProvider<TTvt>(
        this IServiceCollection services,
        ITypeCache typeCache,
        IExceptionManager? parentExceptionMgr = null)
        where TTvt : TypeValueType
    {
        var msg = $"{nameof(AddMockDataProvider)} failed for valueType {typeof(TTvt).Name}";
        IExceptionManager exceptions = parentExceptionMgr?.CreateChild(msg) ?? new ExceptionManager(msg);

        services.AddScoped<MockStorageActionService>();
        services.AddAlias<MockStorageActionService, IStorageActionService>(ServiceLifetime.Scoped);
        services.AddGenericServices<TTvt, IMockDataProviderStore<IEntity>, MockDataProviderStore<IEntity>>(typeCache, ServiceLifetime.Singleton, exceptions);
        services.AddGenericServices<TTvt, IDataProviderRw<IEntity>, MockDataProvider<IEntity>>(typeCache, ServiceLifetime.Scoped, exceptions);
        services.AddGenericAlias<TTvt, IDataProviderR<IEntity>, IDataProviderRw<IEntity>>(typeCache, ServiceLifetime.Scoped, exceptions);

        if (parentExceptionMgr is null)
            exceptions.ThrowIfNotEmpty();
        return services;
    }

    /// <summary>
    /// adds a descriptor which provides <typeparamref name="TAlias"/> as <typeparamref name="TProvided"/> for each instance of <typeparamref name="TTvt"/> which match the <paramref name="filter"/> using the <paramref name="serviceTypeArgumentResolver"/> and <paramref name="implementationTypeArgumentResolver"/> to get the type parameters
    /// <br/>rethrows parentExceptionMgr after execution as <see cref="JLibAggregateException"/> if no <paramref name="parentExceptionMgr"/> has been provided
    /// <br/>exceptions are added as child to the <see cref="parentExceptionMgr"/> or thrown as a <see cref="JLibAggregateException"/> when it was not provided
    /// </summary>
    public static IServiceCollection AddGenericAlias<TTvt, TAlias, TProvided>(
        this IServiceCollection services,
        ITypeCache typeCache,
        ServiceLifetime lifetime,
        IExceptionManager? parentExceptionMgr = null,
        Func<TTvt, bool>? filter = null,
        Func<TTvt, TypeValueType>[]? serviceTypeArgumentResolver = null,
        Func<TTvt, TypeValueType>[]? implementationTypeArgumentResolver = null)
        where TTvt : TypeValueType
        where TProvided : TAlias
        where TAlias : notnull
    {
        serviceTypeArgumentResolver ??= new Func<TTvt, TypeValueType>[] { e => e };
        implementationTypeArgumentResolver ??= new Func<TTvt, TypeValueType>[] { e => e };
        filter ??= _ => true;

        var alias = typeof(TAlias).GetGenericTypeDefinition();
        var provided = typeof(TProvided).GetGenericTypeDefinition();

        var msg =
            $"{nameof(AddGenericAlias)} failed while adding alias {alias.Name} for service {provided.Name} and valueType {typeof(TTvt).Name}";
        var exceptions = parentExceptionMgr?.CreateChild(msg) ?? new ExceptionManager(msg);

        foreach (var valueType in typeCache.All(filter))
        {
            exceptions.TryExecution(() =>
            {
                var explicitAlias = GetGenericService(alias, valueType, implementationTypeArgumentResolver);
                var explicitService = GetGenericService(provided, valueType, serviceTypeArgumentResolver);

                services.Add(new(explicitAlias, explicitService, lifetime));
            });
        }

        if (parentExceptionMgr is null)
            exceptions.ThrowIfNotEmpty();
        return services;
    }

    /// <summary>
    /// adds a descriptor of <typeparamref name="TService"/> implemented as <typeparamref name="TImplementation"/> for each instance of <typeparamref name="TTvt"/> which match the <paramref name="filter"/> using the <paramref name="serviceTypeArgumentResolver"/> and <paramref name="implementationTypeArgumentResolver"/> to get the type parameters
    /// <br/>rethrows parentExceptionMgr after execution as <see cref="JLibAggregateException"/> if no <paramref name="parentExceptionMgr"/> has been provided
    /// <br/>exceptions are added as child to the <see cref="parentExceptionMgr"/> or thrown as a <see cref="JLibAggregateException"/> when it was not provided
    /// </summary>
    public static IServiceCollection AddGenericServices<TTvt, TService, TImplementation>(
        this IServiceCollection services,
        ITypeCache typeCache,
        ServiceLifetime lifetime,
        IExceptionManager? parentExceptionMgr = null,
        Func<TTvt, bool>? filter = null,
        Func<TTvt, TypeValueType>[]? serviceTypeArgumentResolver = null,
        Func<TTvt, TypeValueType>[]? implementationTypeArgumentResolver = null)
        where TTvt : TypeValueType
        where TImplementation : TService
        where TService : notnull
    {
        serviceTypeArgumentResolver ??= new Func<TTvt, TypeValueType>[] { e => e };
        implementationTypeArgumentResolver ??= new Func<TTvt, TypeValueType>[] { e => e };

        var serviceDefinition = typeof(TService).GetGenericTypeDefinition();
        var implementationDefinition = typeof(TImplementation).GetGenericTypeDefinition();
        filter ??= _ => true;

        var msg =
            $"{nameof(AddGenericServices)} failed while adding service {serviceDefinition.Name} with implementation {implementationDefinition.Name} and valueType {typeof(TTvt).Name}";

        IExceptionManager exceptions = parentExceptionMgr?.CreateChild(msg) ?? new ExceptionManager(msg);

        foreach (var valueType in typeCache.All(filter))
        {
            exceptions.TryExecution(() =>
            {
                var explicitImplementation = GetGenericService(implementationDefinition, valueType, implementationTypeArgumentResolver);
                var explicitService = GetGenericService(serviceDefinition, valueType, serviceTypeArgumentResolver);

                services.Add(new(explicitService, explicitImplementation, lifetime));

                Console.Write(
                    $"    {valueType.Name}: {explicitImplementation.FullClassName()} as {explicitService.FullClassName()}");
            });

        }
        if (parentExceptionMgr is null)
            exceptions.ThrowIfNotEmpty();
        return services;

    }
    private static Type GetGenericService<TTvt>(Type genericClass, TTvt currentValueType, Func<TTvt, TypeValueType>[] typeArgumentResolver)
        where TTvt : TypeValueType
    {
        var typeArguments = typeArgumentResolver.Select(resolver => resolver(currentValueType)).ToArray();
        return genericClass.MakeGenericType(typeArguments);
    }

}