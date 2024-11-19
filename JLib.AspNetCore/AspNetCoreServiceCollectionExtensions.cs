using JLib.Exceptions;
using JLib.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using Microsoft.Extensions.Logging;
using static JLib.AspNetCore.AspNetCoreServiceCollectionExtensions.AddRequestScopedServiceException;

namespace JLib.AspNetCore;

/// <summary>
/// 
/// </summary>
public static class AspNetCoreServiceCollectionExtensions
{
    public class AddRequestScopedServiceException : JLibException
    {
        public Type ServiceType { get; }
        public Type? ImplementationType { get; }

        public AddRequestScopedServiceException(Type serviceType, Type? implementationType, string message, Exception? innerException) : base(message, innerException)
        {
            ServiceType = serviceType;
            Data[nameof(ServiceType)] = ServiceType;
            ImplementationType = implementationType;
            Data[nameof(ImplementationType)] = ImplementationType;
        }
        public abstract class RuntimeException : AddRequestScopedServiceException
        {
            private protected RuntimeException(Type serviceType, Type? implementationType, string message, Exception? innerException) : base(serviceType, implementationType,
                (implementationType is null
                    ? $"Request scoped service of type {serviceType.FullName(true)} could not be created: "
                    : $"Instance of type {implementationType.FullName(true)} implementing service {serviceType.FullName(true)}could not be created: "
                )
                + message, innerException)
            {
            }
        }
        public abstract class InitializationException : AddRequestScopedServiceException
        {
            private protected InitializationException(Type serviceType, Type? implementationType, string message, Exception? innerException) : base(serviceType, implementationType,
                (implementationType is null
                    ? $"Request scoped service of type {serviceType.FullName(true)} could not be added to the service collection: "
                    : $"Request scoped instance of type {implementationType.FullName(true)} implementing service {serviceType.FullName(true)}could not be added to the service collection: "
                )
                + message, innerException)
            {
            }
        }

        public class MissingRequirementException : RuntimeException
        {
            public Type Requirement { get; }

            internal MissingRequirementException(Type serviceType, Type? implementationType, Type requirement)
                : base(serviceType, implementationType, $"Using Request Scoped Services requires the service {requirement} to be provided.", null)
            {
                Requirement = requirement;
                Data[nameof(Requirement)] = Requirement;
            }
        }

        public class OutsideHttpContextScopeException : RuntimeException
        {
            internal OutsideHttpContextScopeException(Type serviceType, Type? implementationType) : base(serviceType, implementationType, "HttpContext is null", null)
            {
            }
        }
        public class UnsupportedGenericServiceException : InitializationException
        {
            internal UnsupportedGenericServiceException(Type serviceType, Type? implementationType) : base(serviceType, implementationType, "Generic Type Definitions are not supported yet.", null)
            {
            }
        }
    }

    private class ScopeIdGenerator
    {
        public Guid ScopeId { get; } = Guid.NewGuid();
    }


    /// <summary>
    /// provides <typeparamref name="TService"/> once for each http request.
    /// <br/>
    /// <typeparamref name="TService"/> should be thread save, as scopes are often used to create new instances for each thread.
    /// <remarks><br/>
    /// This differs in comparison to a normal scoped service, in that when you create custom scopes for a request,
    /// the service will be instanced multiple times when using a scoped service but only once using this method.
    /// <br/>
    /// this is useful for example when you have to make database requests to fetch a token based authentication info.
    /// </remarks>
    /// </summary>
    /// <typeparam name="TService">the service to be provided</typeparam>
    /// <param name="services">the service collection to add the service to</param>
    /// <returns>the given <see cref="IServiceCollection"/></returns>
    /// <exception cref="InvalidSetupException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddRequestScoped<TService>(this IServiceCollection services)
        where TService : class
        => services.AddRequestScoped<TService, TService>();

    private static readonly MethodInfo GetRequiredServiceMi = typeof(ServiceProviderServiceExtensions)
                                       .GetMethod(
                                           "GetRequiredService",
                                           1,
                                           new[] { typeof(IServiceProvider) })
                                   ?? throw new InvalidSetupException("IServiceProvider.GetRequiredService Method Info not found");
    public static IServiceCollection AddRequestScoped(this IServiceCollection services, Type serviceType)
        => services.AddRequestScoped(serviceType, serviceType);


    /// <summary>
    /// provides <typeparamref name="TService"/> once for each http request.
    /// <br/>
    /// <typeparamref name="TService"/> should be thread save, as scopes are often used to create new instances for each thread.
    /// <remarks><br/>
    /// This differs in comparison to a normal scoped service, in that when you create custom scopes for a request,
    /// the service will be instanced multiple times when using a scoped service but only once using this method.
    /// <br/>
    /// this is useful for example when you have to make database requests to fetch a token based authentication info.
    /// </remarks>
    /// </summary>
    /// <typeparam name="TService">the service to be provided</typeparam>
    /// <typeparam name="TImplementation">the implementation to be used for <typeparam name="TService"></typeparam></typeparam>
    /// <param name="services">the service collection to add the service to</param>
    /// <returns>the given <see cref="IServiceCollection"/></returns>
    /// <exception cref="InvalidSetupException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddRequestScoped<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
        => services.AddRequestScoped(typeof(TService), typeof(TImplementation));

    /// <summary>
    /// provides <typeparamref name="TService"/> once for each http request.
    /// <br/>
    /// <typeparamref name="TService"/> should be thread save, as scopes are often used to create new instances for each thread.
    /// <remarks><br/>
    /// This differs in comparison to a normal scoped service, in that when you create custom scopes for a request,
    /// the service will be instanced multiple times when using a scoped service but only once using this method.
    /// <br/>
    /// this is useful for example when you have to make database requests to fetch a token based authentication info.
    /// <br/>
    /// requires the <see cref="IHttpContextAccessor"/> to be registered in the service collection.
    /// </remarks>
    /// </summary>
    /// <typeparam name="TService">the service to be provided</typeparam>
    /// <param name="services">the service collection to add the service to</param>
    /// <param name="factory">the factory to create the service instance</param>
    /// <returns>the given <see cref="IServiceCollection"/></returns>
    /// <exception cref="InvalidSetupException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddRequestScoped<TService>(this IServiceCollection services,
        Func<IServiceProvider, TService> factory)
        where TService : class
    => services.AddRequestScoped(factory, null);


    private static IServiceCollection AddRequestScoped<TService>(this IServiceCollection services,
        Func<IServiceProvider, TService> factory, Type? implementationType)
        where TService : class
        => AddRequestScoped(services, factory, typeof(TService), implementationType);



    public static IServiceCollection AddRequestScoped(this IServiceCollection services, Type serviceType, Type implementationType)
    {
        if (implementationType.IsGenericTypeDefinition)
        {
            throw new UnsupportedGenericServiceException(serviceType, implementationType);
        }

        var factory = ActivatorUtilities.CreateFactory(serviceType, Array.Empty<Type>());

        return services.AddRequestScoped(provider => factory.Invoke(provider, null), serviceType, implementationType);
    }
    private static IServiceCollection AddRequestScoped(this IServiceCollection services, Func<IServiceProvider, object> factory, Type serviceType, Type? implementationType)
    {
        services.TryAddScoped<ScopeIdGenerator>();
        services.AddScoped(serviceType, provider =>
        {
            var context = (provider.GetService<IHttpContextAccessor>()
                           ?? throw new MissingRequirementException(
                               serviceType, implementationType, typeof(IHttpContextAccessor))).HttpContext;
            var logger = provider.GetService<ILoggerFactory>()
                ?.CreateLogger(typeof(AspNetCoreServiceCollectionExtensions));

            if (context is null)
            {
                logger?.LogError("tried to retrieve Request scoped service outside of http request (http context is null)");
                throw new OutsideHttpContextScopeException(serviceType, implementationType);
            }

            var contextScopeId = context.RequestServices.GetRequiredService<ScopeIdGenerator>().ScopeId;
            var currentScopeId = provider.GetRequiredService<ScopeIdGenerator>().ScopeId;

            logger?.LogTrace("retrieving service {ServiceName} for scope {currentScopeId} with http context scope {contextScopeId}",
                serviceType.FullName(true),
                currentScopeId, contextScopeId);

            return contextScopeId != currentScopeId
                ? context.RequestServices.GetRequiredService(serviceType)
                : factory(context.RequestServices);
        });
        return services;
    }
}
