using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq.Expressions;
using JLib.Exceptions;
using JLib.Helper;
using Microsoft.Extensions.Logging;

namespace JLib.AspNetCore;

/// <summary>
/// 
/// </summary>
public static class AspNetCoreServiceCollectionExtensions
{
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
    public static IServiceCollection AddRequestScopedService<TService>(this IServiceCollection services)
        where TService : class
    {
        var ctor = typeof(TService).GetConstructors().Single();
        var ctorParams = ctor.GetParameters().Select(p => p.ParameterType).ToArray();
        services.TryAddScoped<ScopeIdGenerator>();


        var getRequiredServiceMi = typeof(ServiceProviderServiceExtensions)
                                       .GetMethod(
                                           "GetRequiredService",
                                           1,
                                           new[] { typeof(IServiceProvider) })
            ?? throw new InvalidSetupException("IServiceProvider.GetRequiredService Method Info not found");

        var param = Expression.Parameter(typeof(IServiceProvider), "provider");
        var args = ctorParams
            .Select(p => Expression.Call(null, getRequiredServiceMi.MakeGenericMethod(p), param))
            .Cast<Expression>()
            .ToArray();
        var body = Expression.New(ctor, args);
        var lambda = Expression.Lambda<Func<IServiceProvider, TService>>(body, param);

        var expression = lambda.Compile();

        services.AddScoped(provider =>
        {
            var context = provider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(AspNetCoreServiceCollectionExtensions));

            if (context is null)
            {
                logger.LogError("tried to retrieve Request scoped service outside of http request (http context is null)");
                throw new InvalidOperationException($"{nameof(HttpContext)} not found");
            }

            var contextScope = context.RequestServices.GetRequiredService<ScopeIdGenerator>().ScopeId;
            var currentScope = provider.GetRequiredService<ScopeIdGenerator>().ScopeId;

            logger.LogTrace("retrieving service {ServiceName} for scope {currentScopeId} with http context scope {contextScopeId}",
                typeof(TService).FullName(true),
                currentScope, contextScope);

            return contextScope != currentScope
                ? context.RequestServices.GetRequiredService<TService>()
                : expression(context.RequestServices);
        });
        return services;
    }
}
