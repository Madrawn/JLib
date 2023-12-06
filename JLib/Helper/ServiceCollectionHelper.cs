using System.Reflection;
using JLib.AutoMapper;
using JLib.Configuration;
using JLib.Data;
using JLib.Exceptions;
using JLib.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace JLib.Helper;

public static class ConfigurationSections
{
    /// <summary>
    /// the key under which environments can be specified.
    /// </summary>
    public const string Environment = "Environment";
}
public static class ServiceCollectionHelper
{
    /// <summary>
    /// adds <see cref="IOptions{TOptions}"/> for each <see cref="ConfigurationSectionType"/> and makes the value directly injectable
    /// supports multiple environments. The environment key is found under <see cref="ConfigurationSections.Environment"/>.
    /// There can be a top level environment and one per section.
    /// + <see cref="IOptions{TOptions}"/> for each <see cref="ConfigurationSectionType"/>
    /// -- a instance of each <see cref="ConfigurationSectionType"/>
    /// <br/>
    /// <example>
    /// Behavior:
    /// <code>
    /// {
    ///     Environment: "Dev1",
    ///     SectionA:{
    ///         Environment: "Dev2",
    ///         Dev1:{
    ///             MyValue:"Ignored"
    ///         },
    ///         Dev2:{
    ///             MyValue:"Used"
    ///         },
    ///         MyValue:"Ignored"
    ///     },
    ///     SectionB:{
    ///         Dev1:{
    ///             MyValue:"Used"
    ///         },
    ///         Dev2:{
    ///             MyValue:"Ignored"
    ///         },
    ///         MyValue:"Ignored"
    ///     },
    ///     SectionC:{
    ///         Environment: "",
    ///         Dev1:{
    ///             MyValue:"Ignored"
    ///         },
    ///         Dev2:{
    ///             MyValue:"Ignored"
    ///         },
    ///         MyValue:"Used"
    ///     }
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public static IServiceCollection AddAllConfigSections(this IServiceCollection services,
        ITypeCache typeCache, IConfiguration config, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {

        var configMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
            .GetMethod(nameof(OptionsConfigurationServiceCollectionExtensions.Configure),
                new[] { typeof(IServiceCollection), typeof(IConfiguration) })
            ?? throw new InvalidSetupException("configure method not found");

        var topLevelEnvironment = config[ConfigurationSections.Environment];
        if (topLevelEnvironment != null)
            Log.Information("Loading config for top level environment {environment}", topLevelEnvironment);
        // code duplicated in ConfigSectionHelper.GetSectionObject
        foreach (var sectionType in typeCache.All<ConfigurationSectionType>())
        {
            var sectionInstance = config.GetSection(sectionType.SectionName.Value);

            var sectionEnvironment = sectionInstance[ConfigurationSections.Environment];

            var environment = sectionEnvironment ?? topLevelEnvironment;
            if (environment is not null)
            {
                var environmentType = sectionEnvironment is not null ? "override" : "topLevel";
                Log.Information("Loading section {environment}.{section} ({sectionType}). Environment is defined in {environmentType}",
                    environment, sectionType.SectionName, sectionType.Value.FullClassName(true), environmentType);
                sectionInstance = sectionInstance.GetSection(environment);
            }
            else
                Log.Information("Loading section {section} ({sectionType})", sectionType.SectionName, sectionType.Value.FullClassName(true));

            var specificConfig = configMethod.MakeGenericMethod(sectionType.Value);

            specificConfig.Invoke(null, new object?[]
            {
                services,sectionInstance
            });

            // extract value from options and make the section directly accessible
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

    #region AddAlias
    /// <summary>
    /// adds <typeparamref name="TAlias"/> as Service of <typeparamref name="TImpl"/>
    /// </summary>
    public static IServiceCollection AddAlias<TAlias, TImpl>(this IServiceCollection serviceCollection, ServiceLifetime lifetime)
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
    public static IServiceCollection AddAlias(this IServiceCollection services, Type alias, Type existing, ServiceLifetime lifetime)
    {
        services.Add(new(alias, provider => provider.GetRequiredService(existing), lifetime));
        return services;
    }
    #endregion

    #region AddTypeCache
    /// <summary>
    /// Adds the <see cref="ITypeCache"/> to your services, executes its Initialization and returns the ready-to-use instance.
    /// </summary>
    public static IServiceCollection AddTypeCache(this IServiceCollection services, out ITypeCache typeCache,
        IExceptionManager exceptions,
        params string[] includedPrefixes)
        => services.AddTypeCache(out typeCache, exceptions, null, SearchOption.TopDirectoryOnly, includedPrefixes);
    public static IServiceCollection AddTypeCache(
        this IServiceCollection services,
        out ITypeCache typeCache,
        IExceptionManager exceptions,
        string? assemblySearchDirectory = null,
        SearchOption searchOption = SearchOption.TopDirectoryOnly,
        params string[] includedPrefixes) 
        => services.AddTypeCache(out typeCache, exceptions, TypePackage.Get(assemblySearchDirectory, includedPrefixes, searchOption));

    public static IServiceCollection AddTypeCache(
        this IServiceCollection services,
        out ITypeCache typeCache,
         IExceptionManager exceptionManager, params ITypePackage[] typePackages)
    {
        typeCache = new TypeCache(TypePackage.Get(typePackages), exceptionManager);
        return services.AddSingleton(typeCache);
    }
    #endregion

    #region AddMapDataprovider

    /// <summary>
    /// adds map data providers for each <see cref="IMappedDataObjectType"/>.
    /// <br/>generates a <see cref="IDataProviderR{TData}"/> if <see cref="IMappedDataObjectType.ReverseMap"/> is false
    /// <br/>generates a <see cref="IDataProviderRw{TData}"/> and a <see cref="IDataProviderR{TData}"/> as alias if it is true
    /// </summary>
    public static IServiceCollection AddMapDataProvider(
        this IServiceCollection services,
        ITypeCache typeCache,
        IExceptionManager exceptions)
    {
        exceptions = exceptions.CreateChild(nameof(AddMapDataProvider));
        foreach (var mappedDataObjectType in typeCache.All<IMappedDataObjectType>())
        {
            foreach (var typeMappingInfoEx in mappedDataObjectType.MappingInfo)
            {
                var (sourceType, destinationType, providerMode) = typeMappingInfoEx;

                var repo = typeCache.TryGet<RepositoryType>(
                    r => r.ProvidedDataObject.Value == destinationType.Value);

                switch (providerMode)
                {
                    case MappingDataProviderMode.Disabled:
                        continue;
                    case MappingDataProviderMode.Read:
                        {
                            if (repo is { CanWrite: true })
                            {
                                exceptions.Add(new InvalidSetupException(
                                    $"Repository {repo.Value.FullClassName()} for Entity {destinationType.Value.FullClassName()} can write data but the mapping is configured to provide a {nameof(IDataProviderR<IDataObject>)}." + Environment.NewLine +
                                    $"To fix this issue either set the mapping to ReadWrite or don't implement {nameof(IDataProviderRw<IEntity>)} on the repository"));
                                continue;
                            }

                            var implementation = typeof(MapDataProviderR<,>).MakeGenericType(sourceType, destinationType);
                            services.AddScoped(implementation);
                            services.AddScoped(typeof(ISourceDataProviderR<>).MakeGenericType(destinationType), implementation);
                            if (repo is null)
                                services.AddScoped(typeof(IDataProviderR<>).MakeGenericType(destinationType), implementation);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        return services;
    }

    /// <summary>
    /// Provides a <see cref="IDataProviderR{TData}"/> for each <typeparamref name="TTvt"/> which takes data from a <see cref="IDataProviderR{TData}"/> of the type retrieved by <see cref="sourcePropertyReader"/> and maps it using AutoMapper Projections
    /// </summary>
    public static IServiceCollection AddMapDataProvider<TTvt>(
    this IServiceCollection services,
    ITypeCache typeCache,
    Func<TTvt, bool>? filter,
    Func<TTvt, ITypeValueType?> sourcePropertyReader,
    Func<TTvt, bool> isReadOnly,
    IExceptionManager exceptions)
    where TTvt : class, IDataObjectType
    {
        filter ??= _ => true;
        exceptions = exceptions.CreateChild(
            $"{nameof(AddMapDataProvider)} failed for valueType {typeof(TTvt).Name}");

        var typeArguments =
            new Func<TTvt, ITypeValueType>[]
            {
                destination => sourcePropertyReader(destination)!,
                destination => destination
            };
        Log.ForContext(typeof(ServiceCollectionHelper)).ForContext<IDataProviderR<IDataObject>>().Verbose("ReadOnly MapDataProvider");
        services.AddDataProvider<TTvt, MapDataProviderR<IEntity, IEntity>, IEntity>(
            typeCache,
            tvt => filter(tvt) && isReadOnly(tvt),
            null,
            typeArguments,
            exceptions
        );
        //Log.ForContext(typeof(ServiceCollectionHelper)).ForContext<IDataProviderR<IDataObject>>().Verbose("WritableMapDataProvider");
        //services.AddDataProvider<TTvt, WritableMapDataProvider<IEntity, IEntity>, IEntity>(
        //    typeCache,
        //    tvt => filter(tvt) && !isReadOnly(tvt),
        //    null,
        //    typeArguments,
        //    exceptions
        //);
        return services;
    }

    #endregion

    #region AddRepositories

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
            services.AddAlias(
                typeof(IDataProviderR<>).MakeGenericType(repo.ProvidedDataObject.Value),
                repo.Value,
                ServiceLifetime.Scoped);

            if (repo.CanWrite)
                services.AddAlias(
                    typeof(IDataProviderRw<>).MakeGenericType(repo.ProvidedDataObject.Value),
                    repo.Value,
                    ServiceLifetime.Scoped);
        }
        return services;
    }

    #endregion

    #region AddDataProvider

    /// <summary>
    /// <inheritdoc cref="AddDataProvider{TTvt,TImplementation,TIgnoredEntitiy}"/>
    /// </summary>
    public static IServiceCollection AddDataProvider<TTvt, TImplementation>(
        this IServiceCollection services,
        ITypeCache typeCache,
        Func<TTvt, bool>? filter,
        Func<TTvt, bool>? forceReadOnly,
        Func<TTvt, ITypeValueType>[]? implementationTypeArgumentResolver,
        IExceptionManager exceptions,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TTvt : class, IDataObjectType
        where TImplementation : IDataProviderR<IDataObject>
        => services.AddDataProvider<TTvt, TImplementation, IDataObject>(typeCache, filter, forceReadOnly,
            implementationTypeArgumentResolver, exceptions, lifetime);
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
    public static IServiceCollection AddDataProvider<TTvt, TImplementation, TIgnoredDataObject>(
    this IServiceCollection services,
    ITypeCache typeCache,
    Func<TTvt, bool>? filter,
    Func<TTvt, bool>? forceReadOnly,
    Func<TTvt, ITypeValueType>[]? implementationTypeArgumentResolver,
    IExceptionManager exceptions,
    ServiceLifetime lifetime = ServiceLifetime.Scoped)
    where TTvt : class, IDataObjectType
    where TImplementation : IDataProviderR<TIgnoredDataObject>
    where TIgnoredDataObject : IDataObject
    {
        filter ??= _ => true;
        forceReadOnly ??= _ => false;
        var implementation = typeof(TImplementation).TryGetGenericTypeDefinition();
        var msg = $"{nameof(AddMapDataProvider)} failed for valueType {typeof(TTvt).Name}";
        exceptions = exceptions.CreateChild(msg);

        services.AddGenericServices(typeCache, implementation, implementation,
            lifetime, exceptions, filter, implementationTypeArgumentResolver,
            implementationTypeArgumentResolver);

        #region read/write mode mismatch check
        var repos = typeCache.All<RepositoryType>(repo => repo.ProvidedDataObject is TTvt).ToArray();
        var implementationCanWrite = implementation.ImplementsAny<IDataProviderRw<IEntity>>();
        foreach (var repo in repos)
        {
            var repoIsReadOnly = repo.Value.ImplementsAny<IDataProviderRw<IEntity>>() is false;
            if (repo.ProvidedDataObject is not TTvt tvt || !filter(tvt))
                continue;
            var readOnlyForced = forceReadOnly(tvt);
            var dataProviderIsReadOnly = !implementationCanWrite || readOnlyForced;
            var args = implementationTypeArgumentResolver?.Select(x => x(tvt)).ToArray() ?? new[] { tvt };
            Type impl;
            try
            {
                impl = implementation.MakeGenericType(args);
            }
            // thrown, when the typeConstraints are violated. Doing this manually is not really a good idea
            catch (Exception e) when (e.InnerException is ArgumentException { HResult: -2147024809 })
            {
                continue;
            }

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
                lifetime, exceptions, FilterIt(implementation, serviceType, repos),
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

    #endregion

    #region AddMockDataProvider

    /// <summary>
    /// adds a <see cref="IDataProviderR{TData}"/> and <see cref="IDataProviderRw{TData}"/> with the <see cref="MockDataProvider{TEntity}"/> as implementation for each <typeparamref name="TTvt"/> as scoped
    /// </summary>
    public static IServiceCollection AddMockDataProvider<TTvt>(
        this IServiceCollection services,
        ITypeCache typeCache,
        IExceptionManager exceptions)
        where TTvt : class, IDataObjectType
    {
        Log.ForContext(typeof(ServiceCollectionHelper)).ForContext(typeof(ServiceCollectionHelper)).Warning("Providing MockDataProvider");
        var msg = $"{nameof(AddMockDataProvider)} failed for valueType {typeof(TTvt).Name}";
        exceptions = exceptions.CreateChild(msg);

        services.AddDataProvider<TTvt, MockDataProvider<IEntity>, IEntity>(typeCache, null, null, null, exceptions, ServiceLifetime.Singleton);

        return services;
    }

    #endregion

    #region AddGenericAlias
    /// <summary>
    /// adds a descriptor which provides <typeparamref name="TAlias"/> as <typeparamref name="TProvided"/> for each instance of <typeparamref name="TTvt"/> which match the <paramref name="filter"/> using the <paramref name="serviceTypeArgumentResolver"/> and <paramref name="implementationTypeArgumentResolver"/> to get the type parameters
    /// </summary>
    public static IServiceCollection AddGenericAlias<TTvt, TAlias, TProvided>(
        this IServiceCollection services,
        ITypeCache typeCache,
        ServiceLifetime lifetime,
        IExceptionManager exceptions,
        Func<TTvt, bool>? filter = null,
        Func<TTvt, ITypeValueType>[]? serviceTypeArgumentResolver = null,
        Func<TTvt, ITypeValueType>[]? implementationTypeArgumentResolver = null)
        where TTvt : class, ITypeValueType
        where TProvided : TAlias
        where TAlias : notnull
        => services.AddGenericAlias(typeCache, typeof(TAlias).TryGetGenericTypeDefinition(), typeof(TProvided), lifetime, exceptions, filter,
            serviceTypeArgumentResolver, implementationTypeArgumentResolver);
    /// <summary>
    /// adds a descriptor which provides <paramref name="aliasType"/> as <paramref name="providedType"/> for each instance of <typeparamref name="TTvt"/> which match the <paramref name="filter"/> using the <paramref name="aliasTypeArgumentResolver"/> and <paramref name="providedTypeArgumentResolver"/> to get the type parameters
    /// </summary>
    public static IServiceCollection AddGenericAlias<TTvt>(
        this IServiceCollection services,
        ITypeCache typeCache,
        Type aliasType,
        Type providedType,
        ServiceLifetime lifetime,
        IExceptionManager exceptions,
        Func<TTvt, bool>? filter = null,
        Func<TTvt, ITypeValueType>[]? aliasTypeArgumentResolver = null,
        Func<TTvt, ITypeValueType>[]? providedTypeArgumentResolver = null)
        where TTvt : class, ITypeValueType
    {
        aliasTypeArgumentResolver ??= new Func<TTvt, ITypeValueType>[] { e => e };
        providedTypeArgumentResolver ??= new Func<TTvt, ITypeValueType>[] { e => e };
        filter ??= _ => true;

        var alias = aliasType.GetGenericTypeDefinition();
        var provided = providedType.GetGenericTypeDefinition();

        var msg =
            $"{nameof(AddGenericAlias)} failed while adding alias {alias.FullClassName(true)} for service {provided.FullClassName(true)} and valueType {typeof(TTvt).FullClassName(true)}";
        exceptions = exceptions.CreateChild(msg);

        Log.ForContext(typeof(ServiceCollectionHelper)).Debug("Linking {provided} to Alias {alias} for each {tvt} with lifetime {lifetime}",
            providedType.Name, aliasType.Name, typeof(TTvt).Name, lifetime);

        foreach (var valueType in typeCache.All(filter))
        {
            try
            {
                var explicitAlias = GetGenericService(alias, valueType, aliasTypeArgumentResolver);
                var explicitService = GetGenericService(provided, valueType, providedTypeArgumentResolver);

                services.Add(new(explicitAlias, provider => provider.GetRequiredService(explicitService), lifetime));

                Log.ForContext(typeof(ServiceCollectionHelper)).Verbose(
                    "    {valueType,-25}: {alias,-65} as {service,-20}",
                    valueType.Name, explicitAlias.FullClassName(), explicitService.FullClassName());
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }
        return services;
    }

    /// <summary>
    /// adds a descriptor of <typeparamref name="TService"/> implemented as <typeparamref name="TImplementation"/> for each instance of <typeparamref name="TTvt"/> which match the <paramref name="filter"/> using the <paramref name="serviceTypeArgumentResolver"/> and <paramref name="implementationTypeArgumentResolver"/> to get the type parameters
    /// </summary>
    public static IServiceCollection AddGenericServices<TTvt, TService, TImplementation>(
        this IServiceCollection services,
        ITypeCache typeCache,
        ServiceLifetime lifetime,
        IExceptionManager exceptions,
        Func<TTvt, bool>? filter = null,
        Func<TTvt, ITypeValueType>[]? serviceTypeArgumentResolver = null,
        Func<TTvt, ITypeValueType>[]? implementationTypeArgumentResolver = null)
        where TTvt : class, ITypeValueType
        where TImplementation : TService
        where TService : notnull
        => services.AddGenericServices<TTvt>(typeCache, typeof(TService), typeof(TImplementation), lifetime,
            exceptions, filter, serviceTypeArgumentResolver, implementationTypeArgumentResolver);

    /// <summary>
    /// adds a descriptor of <paramref name="serviceType"/> implemented as <paramref name="implementationType"/> for each instance of <typeparamref name="TTvt"/> which match the <paramref name="filter"/> using the <paramref name="serviceTypeArgumentResolver"/> and <paramref name="implementationTypeArgumentResolver"/> to get the type parameters
    /// </summary>
    public static IServiceCollection AddGenericServices<TTvt>(
        this IServiceCollection services,
        ITypeCache typeCache,
        Type serviceType,
        Type implementationType,
        ServiceLifetime lifetime,
        IExceptionManager exceptions,
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

        exceptions = exceptions.CreateChild(msg);

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
        return services;

    }
    private static Type GetGenericService<TTvt>(Type genericClass, TTvt currentValueType, Func<TTvt, ITypeValueType>[] typeArgumentResolver)
        where TTvt : class, ITypeValueType
    {
        var typeArguments = typeArgumentResolver.Select(resolver => resolver(currentValueType)).ToArray();
        return genericClass.MakeGenericType(typeArguments);
    }
    #endregion

    /// <summary>
    /// provides scoped a <see cref="IServiceScope"/> which returns the current service provider.
    /// <br/>can be used to detect if the provider is scoped or to enforce a given <see cref="IServiceProvider"/> being scoped
    /// <br/>the injected scope can not be disposed of. doing so will result in nothing being done
    /// </summary>
    public static IServiceCollection AddScopeProvider(this IServiceCollection services)
        => services.AddScoped<IServiceScope, ServiceScopeProxy>();
    private class ServiceScopeProxy : IServiceScope
    {
        public IServiceProvider ServiceProvider { get; }

        public ServiceScopeProxy(IServiceProvider provider)
        {
            ServiceProvider = provider;
        }

        public void Dispose() { }
    }

}