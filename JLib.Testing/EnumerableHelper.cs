using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLib.Exceptions;
using JLib.Helper;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter.Xunit;
using Snapshooter;

namespace JLib.Testing;
public static class EnumerableHelper
{
    /// <summary>
    /// groups the enumerable by namespace and then by typeValueType, using json objects to identify each group
    /// <br/>used to call <see cref="SnapshotExtension.MatchSnapshot(object,Func{MatchOptions,MatchOptions})"/> afterwards (prevents having to provide a <see cref="SnapshotNameExtension"/>)
    /// </summary>
    public static object PrepareSnapshot(this IEnumerable<ITypeValueType> source)
        => source.ToLookup(tvt => tvt.Value.Namespace ?? "")
            .ToDictionary(tvtGroup1 => tvtGroup1.Key, tvt => tvt
                .ToLookup(tvt1 => tvt1.GetType().FullClassName(true))
                .ToDictionary(
                    tvtGroup2 => tvtGroup2.Key,
                    tvtGroup2 => tvtGroup2.Select(tvt2 => tvt2.Value.FullClassName(true))
                )
            );

    /// <summary>
    /// groups all services of the provider by the first generic type argument or under the non-generic group if they are not generic
    /// <br/>then transforms the service provider to a object which contains all relevant information, including the implementation service type but excluding the implementation details.
    /// <br/>used to call <see cref="SnapshotExtension.MatchSnapshot(object,Func{MatchOptions,MatchOptions})"/> afterwards (prevents having to provide a <see cref="SnapshotNameExtension"/>)
    /// <br/>
    /// <br/><b>!!!</b> builds a provider and creates a scope <b>!!!</b>
    /// <br/><b>!!!</b> creates a service instance for each service using a factory as resolver - which is used to implement a alias <b>!!!</b>
    /// </summary>
    public static object PrepareSnapshot(this IServiceCollection services)
    {
        var disposables = new List<IDisposable>();
        var p = new Lazy<IServiceProvider>(() =>
        {
            var p = services.BuildServiceProvider();
            disposables.Add(p);
            return p;
        });
        var sp = new Lazy<IServiceProvider>(() =>
        {
            var s = p.Value.CreateScope();
            disposables.Add(s);
            return s.ServiceProvider;
        });
        var res = services.ToLookup(
                x => x.ServiceType.GenericTypeArguments.LastOrDefault()?.FullClassName(true)
                     ?? "non-generic",
                serviceDescriptor =>
                {
                    var implementation = serviceDescriptor.ImplementationType
                                         ?? serviceDescriptor.ImplementationInstance?.GetType()
                                         ?? serviceDescriptor.ImplementationFactory?.Invoke(sp.Value).GetType();
                    var implementationSrc = serviceDescriptor switch
                    {
                        { ImplementationFactory: not null } => "Factory",
                        { ImplementationInstance: not null } => "Instance",
                        { ImplementationType: not null } => "Type",
                        _ => null
                    };

                    return new
                    {
                        serviceDescriptor.Lifetime,
                        ImplementationSource = implementationSrc,
                        ServiceType = serviceDescriptor.ServiceType.TryGetGenericTypeDefinition().FullClassName(true),
                        ServiceArguments = GetTypeArgs(serviceDescriptor.ServiceType),
                        ImplementationType = implementation?.TryGetGenericTypeDefinition().FullClassName(true),
                        ImplementationTypeArguments = GetTypeArgs(implementation),
                    };

                    static string[] GetTypeArgs(Type? t)
                        => t?.GenericTypeArguments.Select(t => t.FullClassName(true)).ToArray() ??
                           Array.Empty<string>();
                })
            .ToDictionary(x => x.Key, descriptorInfos
                => descriptorInfos
                    .ToLookup(service => service.ServiceType)
                    .ToDictionary(serviceImplementations => serviceImplementations.Key));
        disposables.ForEach(d => d.Dispose());
        return res;
    }

    public static object? PrepareSnapshot(this Exception? exception)
    {
        return exception switch
        {
            null => null,
            JLibAggregateException jLibAggregateException => new
            {
                Type = exception.GetType().FullClassName(true),
                Message = jLibAggregateException.UserMessage.Split(Environment.NewLine),
                InnerExceptions = jLibAggregateException.InnerExceptions.Select(PrepareSnapshot)
            },
            AggregateException aggregateException => new
            {
                Type = exception.GetType().FullClassName(true),
                Message = aggregateException.Message.Split(Environment.NewLine),
                InnerExceptions = aggregateException.InnerExceptions.Select(PrepareSnapshot)
            },
            _ => new
            {
                Type = exception.GetType().FullClassName(true),
                message = exception.Message.Split(Environment.NewLine),
            }
        };
    }
}
