using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using JLib.Data;
using JLib.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Serilog;
namespace JLib.Helper;
public static class ServiceCollectionHelper
{
    /// <summary>
    /// adds <see cref="IOptions{TOptions}"/> for each <see cref="ConfigurationSectionType"/> and makes the value directly injectable
    /// + <see cref="IOptions{TOptions}"/> for each <see cref="ConfigurationSectionType"/>
    /// -- a instance of each <see cref="ConfigurationSectionType"/>
    /// </summary>
    public static IServiceCollection AddAllConfigSections(this IServiceCollection services,
        ITypeCache typeCache, IConfiguration config, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {

        var configMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
            .GetMethod(nameof(OptionsConfigurationServiceCollectionExtensions.Configure),
                new[] { typeof(IServiceCollection), typeof(IConfiguration) })
            ?? throw new InvalidSetupException("configure method not found");

        foreach (var sectionType in typeCache.All<ConfigurationSectionType>())
        {
            var sectionInstance = config.GetRequiredSection(sectionType.SectionName.Value);
            var specificConfig = configMethod.MakeGenericMethod(sectionType.Value);

            specificConfig.Invoke(null, new object?[]
            {
                services,sectionInstance
            });

            // extract value from options and myke section directly accessible
            var src = typeof(IOptions<>).MakeGenericType(sectionType);
            var prop = src.GetProperty(nameof(IOptions<Ignored>.Value)) ?? throw new InvalidSetupException("Value Prop not found on options");
            services.Add(
                new ServiceDescriptor(sectionType.Value,
                    provider => prop.GetValue(provider.GetRequiredService(src))
                                ?? throw new InvalidSetupException($"options section {sectionType.Name} not found"),
                    lifetime));
        }

        return services;
    }

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
        Log.ForContext(typeof(ServiceCollectionHelper)).Information("TypeCache initializing using {searchDirectory} as path while looking for files in {searchOption} and filtering using {prefixes} as prefix." +
                        " This resulted in {assemblyCount} Assemblies being loaded which are {assemblyList}",
            assemblySearchDirectory, searchOption, includedPrefixes, assemblyNames.Length, assemblyNames);
        return services.AddTypeCache(out typeCache, assemblies);
    }

    public static IServiceCollection AddTypeCache(this IServiceCollection services, out ITypeCache typeCache,
        params Assembly[] assemblies)
    {
        Log.ForContext(typeof(ServiceCollectionHelper)).Information("initializing typeCache for assemblies {assemblies}", assemblies);
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
            mdo => !mdo.ReverseMap, parentExceptionMgr);

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
    where TTvt : class, IDataObjectType
    {
        filter ??= _ => true;
        isReadOnly ??= e => !e.Value.Implements<IEntity>() && e.Value.Implements<IDataObject>();
        var msg = $"{nameof(AddMapDataProvider)} failed for valueType {typeof(TTvt).Name}";
        IExceptionManager exceptions = parentExceptionMgr?.CreateChild(msg) ?? new ExceptionManager(msg);

        var typeArguments =
            new Func<TTvt, ITypeValueType>[]
            {
                destination => sourcePropertyReader(destination)!,
                destination => destination
            };
        Log.ForContext(typeof(ServiceCollectionHelper)).ForContext<IDataProviderR<IDataObject>>().Verbose("ReadOnly MapDataProvider");
        services.AddDataProvider<TTvt, MapDataProvider<IEntity, IEntity>>(
            typeCache,
            tvt => filter(tvt) && isReadOnly(tvt),
            null,
            typeArguments,
            exceptions
        );
        Log.ForContext(typeof(ServiceCollectionHelper)).ForContext<IDataProviderR<IDataObject>>().Verbose("WritableMapDataProvider");
        services.AddDataProvider<TTvt, WritableMapDataProvider<IEntity, IEntity>>(
            typeCache,
            tvt => filter(tvt) && !isReadOnly(tvt),
            null,
            typeArguments,
            exceptions
        );
        return services;
    }
    /// <summary>
    /// adds all repositories to the service provider
    /// </summary>
    public static IServiceCollection AddRepositories(
        this IServiceCollection services, ITypeCache typeCache, IExceptionManager exceptions)
    {
        // making sure that no two repos are provided for the same DataObject
        var groupedRepos = typeCache.All<RepositoryType>()
            .GroupBy(x => x.ProvidedDataObject)
            .ToDictionary(x => x.Key, x => x.AsEnumerable());

        var invalidRepos = groupedRepos
            .Where(x => x.Value.Count() > 1)
            .Select(group => new InvalidSetupException(
                $"multiple repos have been provided for data object {group.Key.Value.FullClassName(true)}: {string.Join(", ", group.Value.Select(repo => repo.Value.FullClassName(true)))}"))
            .ToArray();

        if (invalidRepos.Any())
        {
            exceptions.CreateChild(nameof(AddRepositories)).Add(invalidRepos);
            return services;
        }

        foreach (var repo in groupedRepos.Select(x => x.Value.Single()))
        {
            services.AddScoped(repo.Value);
            services.AddAlias(repo.Value, typeof(IDataProviderR<>).MakeGenericType(repo.ProvidedDataObject.Value),
                ServiceLifetime.Scoped);

            if (repo.CanWrite)
                services.AddAlias(repo.Value, typeof(IDataProviderRw<>).MakeGenericType(repo.ProvidedDataObject.Value),
                    ServiceLifetime.Scoped);
        }
        return services;
    }
    /// <summary>
    /// Provides the following services for each <typeparamref name="TTvt"/> under the stated conditions:
    /// <br/>new Services:
    /// <br/>ㅤAlways - <typeparamref name="TImplementation"/> as using <paramref name="implementationTypeArgumentResolver"/> to resolve the type arguments
    /// <br/>Alias for <typeparamref name="TImplementation"/>
    /// <br/>ㅤAlways - <see cref="ISourceDataProviderR{TData}"/> as Alias 
    /// <br/>ㅤ<typeparamref name="TImplementation"/> implements <see cref="ISourceDataProviderRw{TData}"/> - <see cref="ISourceDataProviderRw{TData}"/>
    /// <br/>ㅤno <see cref="RepositoryType"/> for the given <typeparamref name="TTvt"/> - <see cref="IDataProviderR{TData}"/>
    /// <br/>ㅤno <see cref="RepositoryType"/> for the given <typeparamref name="TTvt"/> and <typeparamref name="TImplementation"/> implements <see cref="IDataProviderRw{TData}"/> - <see cref="IDataProviderRw{TData}"/>
    /// </summary>
    public static IServiceCollection AddDataProvider<TTvt, TImplementation>(
    this IServiceCollection services,
    ITypeCache typeCache,
    Func<TTvt, bool>? filter,
    Func<TTvt, bool>? forceReadOnly,
    Func<TTvt, ITypeValueType>[]? implementationTypeArgumentResolver,
    IExceptionManager parentExceptionMgr)
    where TTvt : class, IDataObjectType
    where TImplementation : IDataProviderR<IEntity>
    {
        filter ??= _ => true;
        forceReadOnly ??= _ => false;
        var implementation = typeof(TImplementation).TryGetGenericTypeDefinition();
        var msg = $"{nameof(AddMapDataProvider)} failed for valueType {typeof(TTvt).Name}";
        IExceptionManager exceptions = parentExceptionMgr?.CreateChild(msg) ?? new ExceptionManager(msg);

        services.AddGenericServices(typeCache, implementation, implementation,
            ServiceLifetime.Scoped, exceptions, filter, implementationTypeArgumentResolver,
            implementationTypeArgumentResolver);

        #region read/write mode mismatch check
        var repos = typeCache.All<RepositoryType>(repo => repo.ProvidedDataObject is TTvt).ToArray();
        var implementationCanWrite = implementation.ImplementsAny<IDataProviderRw<IEntity>>();
        foreach (var repo in repos)
        {
            var repoIsReadOnly = repo.Value.ImplementsAny<IDataProviderRw<IEntity>>() is false;
            if (repo.ProvidedDataObject is not TTvt tvt)
                continue;
            var readOnlyForced = forceReadOnly(tvt);
            var dataProviderIsReadOnly = !implementationCanWrite || readOnlyForced;
            var args = implementationTypeArgumentResolver?.Select(x => x(tvt)).ToArray() ?? new[] { tvt };
            var impl = implementation.MakeGenericType(args);

            if (dataProviderIsReadOnly == repoIsReadOnly)
                continue;

            string errorText =
                dataProviderIsReadOnly
                ? readOnlyForced
                    ? $"The data provider Implementation {impl.FullClassName(true)} is forced read only but the Repository {repo.Value.FullClassName(true)} can write data. {Environment.NewLine}" +
                        $"Not forcing the DataProvider to be read only or implementing {nameof(IDataProviderRw<IEntity>)} will solve this issue"
                    : $"The data provider Implementation {impl.FullClassName(true)} is read only but the Repository {repo.Value.FullClassName(true)} can write data. {Environment.NewLine}" +
                        $"You can resolve this issue by not implementing {nameof(IDataProviderRw<IEntity>)} with the Repository or using a data provider which implements {nameof(ISourceDataProviderRw<IEntity>)}"
                : $"The data provider Implementation {impl.FullClassName(true)} can write data but the Repository {repo.Value.FullClassName(true)} can not. {Environment.NewLine}" +
                    $"Force the dataProvider to be ReadOnly or Implement {nameof(IDataProviderRw<IEntity>)} with the repository.";
            exceptions.Add(new InvalidSetupException(errorText));
        }
        #endregion
        foreach (var serviceType in new[] { typeof(IDataProviderR<>), typeof(IDataProviderRw<>), typeof(ISourceDataProviderR<>), typeof(ISourceDataProviderRw<>) })
            services.AddGenericAlias(typeCache, serviceType, implementation,
                ServiceLifetime.Scoped, exceptions, FilterIt(implementation, serviceType, repos),
                null, implementationTypeArgumentResolver);

        return services;
        // Warning: thw following code governs the entire behavior for which service alias is provided when. All Updates which modify the current behavior are breaking changes.
        // run the TypeCacheTests before pushing any changes
        Func<TTvt, bool> FilterIt(Type myImplementation, Type serviceType, RepositoryType[] repositories)
            => tvt =>
            {
                var filterResult = filter!.Invoke(tvt);
                var interfaceImplemented = myImplementation.ImplementsAny(serviceType);
                var excludeWritable = !(serviceType.ImplementsAny<IDataProviderRw<IEntity>>() && forceReadOnly(tvt));
                var excludeRepos = serviceType.ImplementsAny<ISourceDataProviderR<IDataObject>>() || repositories.None(repo => repo.ProvidedDataObject as IDataObjectType == tvt);
                return filterResult && interfaceImplemented && excludeWritable && excludeRepos;
            };
    }

    /// <summary>
    /// adds a <see cref="IDataProviderR{TData}"/> and <see cref="IDataProviderRw{TData}"/> with the <see cref="MockDataProvider{TEntity}"/> as implementation for each <typeparamref name="TTvt"/> as scoped
    /// <br/>exceptions are added as child to the <see cref="parentExceptionMgr"/> or thrown as a <see cref="JLibAggregateException"/> when it was not provided
    /// </summary>
    public static IServiceCollection AddMockDataProvider<TTvt>(
        this IServiceCollection services,
        ITypeCache typeCache,
        IExceptionManager parentExceptionMgr)
        where TTvt : class, IDataObjectType
    {
        Log.ForContext(typeof(ServiceCollectionHelper)).ForContext(typeof(ServiceCollectionHelper)).Warning("Providing MockDataProvider");
        var msg = $"{nameof(AddMockDataProvider)} failed for valueType {typeof(TTvt).Name}";
        IExceptionManager exceptions = parentExceptionMgr?.CreateChild(msg) ?? new ExceptionManager(msg);

        services.AddDataProvider<TTvt, MockDataProvider<IEntity>>(typeCache, null, null, null, exceptions);

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
        => services.AddGenericAlias(typeCache, typeof(TAlias), typeof(TProvided), lifetime, parentExceptionMgr, filter);
    /// <summary>
    /// adds a descriptor which provides <paramref name="aliasType"/> as <paramref name="providedType"/> for each instance of <typeparamref name="TTvt"/> which match the <paramref name="filter"/> using the <paramref name="serviceTypeArgumentResolver"/> and <paramref name="implementationTypeArgumentResolver"/> to get the type parameters
    /// <br/>rethrows parentExceptionMgr after execution as <see cref="JLibAggregateException"/> if no <paramref name="parentExceptionMgr"/> has been provided
    /// <br/>exceptions are added as child to the <see cref="parentExceptionMgr"/> or thrown as a <see cref="JLibAggregateException"/> when it was not provided
    /// </summary>
    public static IServiceCollection AddGenericAlias<TTvt>(
        this IServiceCollection services,
        ITypeCache typeCache,
        Type aliasType,
        Type providedType,
        ServiceLifetime lifetime,
        IExceptionManager? parentExceptionMgr = null,
        Func<TTvt, bool>? filter = null,
        Func<TTvt, ITypeValueType>[]? serviceTypeArgumentResolver = null,
        Func<TTvt, ITypeValueType>[]? implementationTypeArgumentResolver = null)
        where TTvt : class, ITypeValueType
    {
        serviceTypeArgumentResolver ??= new Func<TTvt, ITypeValueType>[] { e => e };
        implementationTypeArgumentResolver ??= new Func<TTvt, ITypeValueType>[] { e => e };
        filter ??= _ => true;

        var alias = aliasType.GetGenericTypeDefinition();
        var provided = providedType.GetGenericTypeDefinition();

        var msg =
            $"{nameof(AddGenericAlias)} failed while adding alias {alias.Name} for service {provided.Name} and valueType {typeof(TTvt).Name}";
        var exceptions = parentExceptionMgr?.CreateChild(msg) ?? new ExceptionManager(msg);

        Log.ForContext(typeof(ServiceCollectionHelper)).Debug("Linking {provided} to Alias {alias} for each {tvt} with lifetime {lifetime}",
            providedType.Name, aliasType.Name, typeof(TTvt).Name, lifetime);

        foreach (var valueType in typeCache.All(filter))
        {
            exceptions.TryExecution(() =>
            {
                var explicitAlias = GetGenericService(alias, valueType, implementationTypeArgumentResolver);
                var explicitService = GetGenericService(provided, valueType, serviceTypeArgumentResolver);

                services.Add(new(explicitAlias, provider => provider.GetRequiredService(explicitService), lifetime));

                Log.ForContext(typeof(ServiceCollectionHelper)).Verbose(
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
        => services.AddGenericServices<TTvt>(typeCache, typeof(TService), typeof(TImplementation), lifetime,
            parentExceptionMgr, filter, serviceTypeArgumentResolver, implementationTypeArgumentResolver);

    /// <summary>
    /// adds a descriptor of <paramref name="serviceType"/> implemented as <paramref name="implementationType"/> for each instance of <typeparamref name="TTvt"/> which match the <paramref name="filter"/> using the <paramref name="serviceTypeArgumentResolver"/> and <paramref name="implementationTypeArgumentResolver"/> to get the type parameters
    /// <br/>rethrows parentExceptionMgr after execution as <see cref="JLibAggregateException"/> if no <paramref name="parentExceptionMgr"/> has been provided
    /// <br/>exceptions are added as child to the <see cref="parentExceptionMgr"/> or thrown as a <see cref="JLibAggregateException"/> when it was not provided
    /// </summary>
    public static IServiceCollection AddGenericServices<TTvt>(
        this IServiceCollection services,
        ITypeCache typeCache,
        Type serviceType,
        Type implementationType,
        ServiceLifetime lifetime,
        IExceptionManager? parentExceptionMgr = null,
        Func<TTvt, bool>? filter = null,
        Func<TTvt, ITypeValueType>[]? serviceTypeArgumentResolver = null,
        Func<TTvt, ITypeValueType>[]? implementationTypeArgumentResolver = null)
        where TTvt : class, ITypeValueType
    {
        serviceTypeArgumentResolver ??= new Func<TTvt, ITypeValueType>[] { e => e };
        implementationTypeArgumentResolver ??= new Func<TTvt, ITypeValueType>[] { e => e };

        var serviceDefinition = serviceType.GetGenericTypeDefinition();
        var implementationDefinition = implementationType.GetGenericTypeDefinition();
        filter ??= _ => true;

        var msg =
            $"{nameof(AddGenericServices)} failed while adding service {serviceDefinition.Name} with implementation {implementationDefinition.Name} and valueType {typeof(TTvt).Name}";

        IExceptionManager exceptions = parentExceptionMgr?.CreateChild(msg) ?? new ExceptionManager(msg);

        Log.ForContext(typeof(ServiceCollectionHelper)).Debug("Providing {implementation} as {service} for each {tvt} with lifetime {lifetime}",
            implementationType.Name, serviceType.Name, typeof(TTvt).Name, lifetime);

        foreach (var valueType in typeCache.All(filter))
        {
            exceptions.TryExecution(() =>
            {
                var explicitImplementation = GetGenericService(implementationDefinition, valueType, implementationTypeArgumentResolver);
                var explicitService = GetGenericService(serviceDefinition, valueType, serviceTypeArgumentResolver);

                services.Add(new(explicitService, explicitImplementation, lifetime));

                Log.ForContext(typeof(ServiceCollectionHelper)).Verbose(
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