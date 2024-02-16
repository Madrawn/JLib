using JLib.Exceptions;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JLib.DataProvider.InMemory;
public static class ServiceCollectionInMemoryDataProviderExtensions
{

    #region AddInMemoryDataProvider


    /// <summary>
    /// adds a <see cref="IDataProviderR{TData}"/> and <see cref="IDataProviderRw{TData}"/> with the <see cref="InMemoryDataProvider{TEntity}"/> as implementation for each <typeparamref name="TTvt"/> as scoped
    /// </summary>
    public static IServiceCollection AddInMemoryDataProvider<TTvt>(
        this IServiceCollection services,
        ITypeCache typeCache,
        ExceptionBuilder exceptions, ILoggerFactory loggerFactory)
        where TTvt : class, IDataObjectType
    {
        var logger = loggerFactory.CreateLogger(typeof(ServiceCollectionInMemoryDataProviderExtensions).FullName!);
        var _ = logger.BeginScope(nameof(AddInMemoryDataProvider));
        logger.LogWarning("Providing InMemoryDataProvider");
        var msg = $"{nameof(AddInMemoryDataProvider)} failed for valueType {typeof(TTvt).Name}";
        exceptions = exceptions.CreateChild(msg);

        services.AddDataProvider<TTvt, InMemoryDataProvider<IEntity>, IEntity>(typeCache, null, null, null, exceptions,
            loggerFactory, ServiceLifetime.Singleton);

        return services;
    }

    #endregion

}
