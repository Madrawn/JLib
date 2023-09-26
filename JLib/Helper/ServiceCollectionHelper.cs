using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using JLib.Data;
using JLib.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

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

    public static IServiceCollection AddTypeCache(this IServiceCollection services, out ITypeCache typeCache,
        params string[] includedPrefixes)
        => services.AddTypeCache(out typeCache, null, SearchOption.TopDirectoryOnly, includedPrefixes);
    public static IServiceCollection AddTypeCache(this IServiceCollection services, out ITypeCache typeCache,
        string? assemblySearchDirectory = null, SearchOption searchOption = SearchOption.TopDirectoryOnly, params string[] includedPrefixes)
    {
        assemblySearchDirectory ??= AppDomain.CurrentDomain.BaseDirectory;
        var assemblyNames = Directory.EnumerateFiles(assemblySearchDirectory, "*.dll", searchOption).Where(file =>
        {
            var filename = Path.GetFileName(file);
            return includedPrefixes.Any(p => filename.StartsWith(p));
        }).Select(AssemblyName.GetAssemblyName).ToArray();

        var assemblies = assemblyNames.Select(Assembly.Load).ToArray();
        Log.Information("TypeCache initializing using {0} as path while looking for files in {1} and filtering using {2} as prefix." +
                        " This resulted in {3} Assemblies being loaded which are {4}",
            assemblySearchDirectory, searchOption, includedPrefixes, assemblyNames.Length, assemblyNames);
        return services.AddTypeCache(out typeCache, assemblies);
    }

    public static IServiceCollection AddTypeCache(this IServiceCollection services, out ITypeCache typeCache,
        params Assembly[] assemblies)
    {
        Log.Information("initializing typeCache for assemblies {assemblies}", assemblies);
        typeCache = new TypeCache(assemblies);
        return services.AddSingleton(typeCache);
    }

    /// <summary>
    /// adds map data providers for each <see cref="IMappedDataObjectType"/>.
    /// <br/>generates a <see cref="IDataProviderR{TData}"/> if <see cref="IMappedDataObjectType.ReverseMap"/> is false
    /// <br/>generates a <see cref="IDataProviderRw{TData}"/> and a <see cref="IDataProviderR{TData}"/> as alias if it is true
    /// </summary>
    public static IServiceCollection AddMapDataProvider(
        this IServiceCollection services,
        ITypeCache typeCache,
        IExceptionManager? parentExceptionMgr = null)
        => services.AddMapDataProvider<IMappedDataObjectType>(typeCache, null, 
            mdo => mdo.SourceEntity, 
            mdo => !mdo.ReverseMap,parentExceptionMgr);

    /// <summary>
    /// Provides a <see cref="IDataProviderR{TData}"/> for each <typeparamref name="TTvt"/> which takes data from a <see cref="IDataProviderR{TData}"/> of the type retrieved by <see cref="sourcePropertyReader"/> and maps it using AutoMapper Projections
    /// <br/>exceptions are added as child to the <see cref="parentExceptionMgr"/> or thrown as a <see cref="JLibAggregateException"/> when it was not provided
    /// </summary>
    public static IServiceCollection AddMapDataProvider<TTvt>(
    this IServiceCollection services,
    ITypeCache typeCache,
    Func<TTvt, bool>? filter,
    Func<TTvt, ITypeValueType?> sourcePropertyReader,
    Func<TTvt, bool>? isReadOnly = null,
    IExceptionManager? parentExceptionMgr = null)
    where TTvt : class, ITypeValueType
    {
        isReadOnly ??= _ => false;
        var msg = $"{nameof(AddMapDataProvider)} failed for valueType {typeof(TTvt).Name}";
        IExceptionManager exceptions = parentExceptionMgr?.CreateChild(msg) ?? new ExceptionManager(msg);

        var typeArguments =
            new Func<TTvt, ITypeValueType>[]
            {
                destination => sourcePropertyReader(destination)!,
                destination => destination
            };
        //readonly
        services.AddGenericServices<TTvt, IDataProviderR<Ignored>, MapDataProvider<IEntity, Ignored>>(typeCache, ServiceLifetime.Scoped, exceptions,
             tvt => AppliedFilter(tvt) && isReadOnly(tvt),
             null,
             typeArguments);
        // read & write
        services.AddGenericServices<TTvt, IDataProviderRw<IEntity>, WritableMapDataProvider<IEntity, IEntity>>(
            typeCache, ServiceLifetime.Scoped, exceptions,
            tvt => AppliedFilter(tvt) && !isReadOnly(tvt),
            null,
            typeArguments);
        services.AddGenericAlias<TTvt, IDataProviderR<IEntity>, IDataProviderRw<IEntity>>(
            typeCache, ServiceLifetime.Scoped, exceptions,
            tvt => AppliedFilter(tvt) && !isReadOnly(tvt));


        if (parentExceptionMgr is null)
            exceptions.ThrowIfNotEmpty();
        return services;

        bool AppliedFilter(TTvt destination) =>
            sourcePropertyReader(destination) is not null && (filter?.Invoke(destination) ?? true);
    }
    /// <summary>
    /// adds a <see cref="IDataProviderR{TData}"/> and <see cref="IDataProviderRw{TData}"/> with the <see cref="MockDataProvider{TEntity}"/> as implementation for each <typeparamref name="TTvt"/> as scoped
    /// <br/>exceptions are added as child to the <see cref="parentExceptionMgr"/> or thrown as a <see cref="JLibAggregateException"/> when it was not provided
    /// </summary>
    public static IServiceCollection AddMockDataProvider<TTvt>(
        this IServiceCollection services,
        ITypeCache typeCache,
        IExceptionManager? parentExceptionMgr = null)
        where TTvt : TypeValueType
    {
        Log.Warning("Providing MockDataProvider");
        var msg = $"{nameof(AddMockDataProvider)} failed for valueType {typeof(TTvt).Name}";
        IExceptionManager exceptions = parentExceptionMgr?.CreateChild(msg) ?? new ExceptionManager(msg);

        services.AddGenericServices<TTvt, IDataProviderRw<IEntity>, MockDataProvider<IEntity>>(typeCache, ServiceLifetime.Singleton, exceptions);
        services.AddGenericAlias<TTvt, IDataProviderR<IEntity>, IDataProviderRw<IEntity>>(typeCache, ServiceLifetime.Singleton, exceptions);

        if (parentExceptionMgr is null)
            exceptions.ThrowIfNotEmpty();
        return services;
    }

    public static IServiceCollection AddDataProviderRAlias<TTvt>(this IServiceCollection services, ITypeCache typeCache,
        ServiceLifetime lifetime, IExceptionManager exceptionManager, Func<TTvt, bool>? filter = null)
    where TTvt : TypeValueType
        => services.AddGenericAlias<TTvt, IDataProviderR<IEntity>, IDataProviderRw<IEntity>>(typeCache, lifetime,
            exceptionManager, filter);

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
        Func<TTvt, ITypeValueType>[]? serviceTypeArgumentResolver = null,
        Func<TTvt, ITypeValueType>[]? implementationTypeArgumentResolver = null)
        where TTvt : class, ITypeValueType
        where TProvided : TAlias
        where TAlias : notnull
    {
        serviceTypeArgumentResolver ??= new Func<TTvt, ITypeValueType>[] { e => e };
        implementationTypeArgumentResolver ??= new Func<TTvt, ITypeValueType>[] { e => e };
        filter ??= _ => true;

        var alias = typeof(TAlias).GetGenericTypeDefinition();
        var provided = typeof(TProvided).GetGenericTypeDefinition();

        var msg =
            $"{nameof(AddGenericAlias)} failed while adding alias {alias.Name} for service {provided.Name} and valueType {typeof(TTvt).Name}";
        var exceptions = parentExceptionMgr?.CreateChild(msg) ?? new ExceptionManager(msg);

        Log.Debug("Linking {provided} to Alias {alias} for each {tvt} with lifetime {lifetime}", typeof(TProvided).Name, typeof(TAlias).Name, typeof(TTvt).Name, lifetime);

        foreach (var valueType in typeCache.All(filter))
        {
            exceptions.TryExecution(() =>
            {
                var explicitAlias = GetGenericService(alias, valueType, implementationTypeArgumentResolver);
                var explicitService = GetGenericService(provided, valueType, serviceTypeArgumentResolver);

                services.Add(new(explicitAlias, provider => provider.GetRequiredService(explicitService), lifetime));

                Log.Verbose(
                    "    {valueType,-25}: {alias,-65} as {service,-20}",
                    valueType.Name, explicitAlias.FullClassName(), explicitService.FullClassName());
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
        Func<TTvt, ITypeValueType>[]? serviceTypeArgumentResolver = null,
        Func<TTvt, ITypeValueType>[]? implementationTypeArgumentResolver = null)
        where TTvt : class, ITypeValueType
        where TImplementation : TService
        where TService : notnull
    {
        serviceTypeArgumentResolver ??= new Func<TTvt, ITypeValueType>[] { e => e };
        implementationTypeArgumentResolver ??= new Func<TTvt, ITypeValueType>[] { e => e };

        var serviceDefinition = typeof(TService).GetGenericTypeDefinition();
        var implementationDefinition = typeof(TImplementation).GetGenericTypeDefinition();
        filter ??= _ => true;

        var msg =
            $"{nameof(AddGenericServices)} failed while adding service {serviceDefinition.Name} with implementation {implementationDefinition.Name} and valueType {typeof(TTvt).Name}";

        IExceptionManager exceptions = parentExceptionMgr?.CreateChild(msg) ?? new ExceptionManager(msg);

        Log.Debug("Providing {implementation} as {service} for each {tvt} with lifetime {lifetime}",
            typeof(TImplementation).Name, typeof(TService).Name, typeof(TTvt).Name, lifetime);

        foreach (var valueType in typeCache.All(filter))
        {
            exceptions.TryExecution(() =>
            {
                var explicitImplementation = GetGenericService(implementationDefinition, valueType, implementationTypeArgumentResolver);
                var explicitService = GetGenericService(serviceDefinition, valueType, serviceTypeArgumentResolver);

                services.Add(new(explicitService, explicitImplementation, lifetime));

                Log.Verbose(
                    "    {valueType,-25}: {implementation,-65} as {service,-20}",
                    valueType.Name, explicitImplementation.FullClassName(), explicitService.FullClassName());
            });

        }
        if (parentExceptionMgr is null)
            exceptions.ThrowIfNotEmpty();
        return services;

    }
    private static Type GetGenericService<TTvt>(Type genericClass, TTvt currentValueType, Func<TTvt, ITypeValueType>[] typeArgumentResolver)
        where TTvt : class, ITypeValueType
    {
        var typeArguments = typeArgumentResolver.Select(resolver => resolver(currentValueType)).ToArray();
        return genericClass.MakeGenericType(typeArguments);
    }

}