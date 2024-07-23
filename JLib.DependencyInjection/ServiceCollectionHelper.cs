using JLib.Configuration;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using JLib.ValueTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ITypeCache = JLib.Reflection.ITypeCache;
using ITypePackage = JLib.Reflection.ITypePackage;
using ITypeValueType = JLib.Reflection.ITypeValueType;
using TypeCache = JLib.Reflection.TypeCache;
using TypePackage = JLib.Reflection.TypePackage;

namespace JLib.DependencyInjection;

public static class ServiceCollectionHelper
{
    
    #region AddAlias

    /// <summary>
    /// injects <typeparamref name="TImpl"/> and provides it as <typeparamref name="TAlias"/> using a factory, therefore using the same instance
    /// </summary>
    public static IServiceCollection AddAlias<TAlias, TImpl>(this IServiceCollection serviceCollection,
        ServiceLifetime lifetime)
        where TImpl : TAlias
        where TAlias : notnull
    {
        serviceCollection.Add(new(typeof(TAlias),
            provider => provider.GetRequiredService<TImpl>(), lifetime));
        return serviceCollection;
    }

    /// <summary>
    /// injects <typeparamref name="TImpl"/> and provides it as <typeparamref name="TAlias"/> using a factory, therefore using the same instance
    /// </summary>
    public static IServiceCollection AddTransientAlias<TAlias, TImpl>(this IServiceCollection serviceCollection)
        where TImpl : TAlias
        where TAlias : notnull
        => serviceCollection.AddAlias<TAlias, TImpl>(ServiceLifetime.Transient);
    /// <summary>
    /// injects <paramref name="existing"/> and provides it as <paramref name="alias"/> using a factory, therefore using the same instance
    /// </summary>
    public static IServiceCollection AddTransientAlias(this IServiceCollection serviceCollection, Type alias, Type existing)
        => serviceCollection.AddAlias(alias, existing, ServiceLifetime.Transient);

    /// <summary>
    /// injects <typeparamref name="TImpl"/> and provides it as <typeparamref name="TAlias"/> using a factory, therefore using the same instance
    /// </summary>
    public static IServiceCollection AddScopedAlias<TAlias, TImpl>(this IServiceCollection serviceCollection)
        where TImpl : TAlias
        where TAlias : notnull
        => serviceCollection.AddAlias<TAlias, TImpl>(ServiceLifetime.Scoped);
    /// <summary>
    /// injects <paramref name="existing"/> and provides it as <paramref name="alias"/> using a factory, therefore using the same instance
    /// </summary>
    public static IServiceCollection AddScopedAlias(this IServiceCollection serviceCollection, Type alias, Type existing)
        => serviceCollection.AddAlias(alias, existing, ServiceLifetime.Scoped);
    /// <summary>
    /// injects <typeparamref name="TImpl"/> and provides it as <typeparamref name="TAlias"/> using a factory, therefore using the same instance
    /// </summary>
    public static IServiceCollection AddSingletonAlias<TAlias, TImpl>(this IServiceCollection serviceCollection)
        where TImpl : TAlias
        where TAlias : notnull
        => serviceCollection.AddAlias<TAlias, TImpl>(ServiceLifetime.Singleton);
    /// <summary>
    /// injects <paramref name="existing"/> and provides it as <paramref name="alias"/> using a factory, therefore using the same instance
    /// </summary>
    public static IServiceCollection AddSingletonAlias(this IServiceCollection serviceCollection, Type alias, Type existing)
        => serviceCollection.AddAlias(alias, existing, ServiceLifetime.Singleton);

    /// <summary>
    /// injects <paramref name="existing"/> and provides it as <paramref name="alias"/> using a factory, therefore using the same instance
    /// </summary>
    public static IServiceCollection AddAlias(this IServiceCollection services, Type alias, Type existing,
        ServiceLifetime lifetime)
    {
        services.Add(new(alias, provider => provider.GetRequiredService(existing), lifetime));
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
        ExceptionBuilder exceptions,
        ILoggerFactory loggerFactory,
        Func<TTvt, bool>? filter = null,
        Func<TTvt, ITypeValueType>[]? serviceTypeArgumentResolver = null,
        Func<TTvt, ITypeValueType>[]? implementationTypeArgumentResolver = null)
        where TTvt : class, ITypeValueType
        where TProvided : TAlias
        where TAlias : notnull
        => services.AddGenericAlias(typeCache, typeof(TAlias).TryGetGenericTypeDefinition(), typeof(TProvided),
            lifetime, exceptions, loggerFactory, filter,
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
        ExceptionBuilder exceptions,
        ILoggerFactory loggerFactory,
        Func<TTvt, bool>? filter = null,
        Func<TTvt, ITypeValueType>[]? aliasTypeArgumentResolver = null,
        Func<TTvt, ITypeValueType>[]? providedTypeArgumentResolver = null)
        where TTvt : class, ITypeValueType
    {
        var logger = loggerFactory.CreateLogger(typeof(ServiceCollectionHelper).FullName!);
        using var _ = logger.BeginScope(nameof(AddGenericAlias));

        aliasTypeArgumentResolver ??= new Func<TTvt, ITypeValueType>[] { e => e };
        providedTypeArgumentResolver ??= new Func<TTvt, ITypeValueType>[] { e => e };
        filter ??= _ => true;

        var alias = aliasType.GetGenericTypeDefinition();
        var provided = providedType.GetGenericTypeDefinition();

        var msg =
            $"{nameof(AddGenericAlias)} failed while adding alias {alias.FullName(true)} for service {provided.FullName(true)} and valueType {typeof(TTvt).FullName(true)}";
        exceptions = exceptions.CreateChild(msg);

        logger.LogDebug(
            "Linking {provided} to Alias {alias} for each {tvt} with lifetime {lifetime}",
            providedType.Name, aliasType.Name, typeof(TTvt).Name, lifetime);

        foreach (var valueType in typeCache.All(filter))
        {
            try
            {
                var explicitAlias = GetGenericService(alias, valueType, aliasTypeArgumentResolver);
                var explicitService = GetGenericService(provided, valueType, providedTypeArgumentResolver);

                services.Add(new(explicitAlias, provider => provider.GetRequiredService(explicitService), lifetime));

                logger.LogTrace(
                    "    {valueType,-25}: {alias,-65} as {service,-20}",
                    valueType.Name, explicitAlias.FullName(), explicitService.FullName());
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
        ExceptionBuilder exceptions,
        ILoggerFactory loggerFactory,
        Func<TTvt, bool>? filter = null,
        Func<TTvt, ITypeValueType>[]? serviceTypeArgumentResolver = null,
        Func<TTvt, ITypeValueType>[]? implementationTypeArgumentResolver = null)
        where TTvt : class, ITypeValueType
        where TImplementation : TService
        where TService : notnull
        => services.AddGenericServices(typeCache, typeof(TService), typeof(TImplementation), lifetime,
            exceptions, loggerFactory, filter, serviceTypeArgumentResolver, implementationTypeArgumentResolver);

    /// <summary>
    /// adds a descriptor of <paramref name="serviceType"/> implemented as <paramref name="implementationType"/> for each instance of <typeparamref name="TTvt"/> which match the <paramref name="filter"/> using the <paramref name="serviceTypeArgumentResolver"/> and <paramref name="implementationTypeArgumentResolver"/> to get the type parameters
    /// </summary>
    public static IServiceCollection AddGenericServices<TTvt>(
        this IServiceCollection services,
        ITypeCache typeCache,
        Type serviceType,
        Type implementationType,
        ServiceLifetime lifetime,
        ExceptionBuilder exceptions,
        ILoggerFactory loggerFactory,
        Func<TTvt, bool>? filter = null,
        Func<TTvt, ITypeValueType>[]? serviceTypeArgumentResolver = null,
        Func<TTvt, ITypeValueType>[]? implementationTypeArgumentResolver = null)
        where TTvt : class, ITypeValueType
    {
        var logger = loggerFactory.CreateLogger(typeof(ServiceCollectionHelper).FullName!);
        using var _ = logger.BeginScope(nameof(AddGenericServices));

        serviceTypeArgumentResolver ??= new Func<TTvt, ITypeValueType>[] { e => e };
        implementationTypeArgumentResolver ??= new Func<TTvt, ITypeValueType>[] { e => e };

        var serviceDefinition = serviceType.GetGenericTypeDefinition();
        var implementationDefinition = implementationType.GetGenericTypeDefinition();
        filter ??= _ => true;

        var msg =
            $"{nameof(AddGenericServices)} failed while adding service {serviceDefinition.Name} with implementation {implementationDefinition.Name} and valueType {typeof(TTvt).Name}";

        exceptions = exceptions.CreateChild(msg);

        logger.LogDebug(
            "Providing {implementation} as {service} for each {tvt} with lifetime {lifetime}",
            implementationType.Name, serviceType.Name, typeof(TTvt).Name, lifetime);

        foreach (var valueType in typeCache.All(filter))
        {
            try
            {
                var explicitImplementation = GetGenericService(implementationDefinition, valueType,
                    implementationTypeArgumentResolver);
                var explicitService = GetGenericService(serviceDefinition, valueType, serviceTypeArgumentResolver);

                services.Add(new(explicitService, explicitImplementation, lifetime));

                logger.LogTrace(
                    "    {valueType,-25}: {implementation,-65} as {service,-20}",
                    valueType.Name, explicitImplementation.FullName(), explicitService.FullName());
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }

        return services;
    }

    private static Type GetGenericService<TTvt>(Type genericClass, TTvt currentValueType,
        Func<TTvt, ITypeValueType>[] typeArgumentResolver)
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

        public void Dispose()
        {
        }
    }
}