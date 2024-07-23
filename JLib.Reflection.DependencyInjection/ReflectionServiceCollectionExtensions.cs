using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLib.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JLib.Reflection.DependencyInjection;
/// <summary>
/// adds helper methods which add the type cache to a <see cref="IServiceCollection"/>
/// </summary>
public static class ReflectionServiceCollectionExtensions
{
    #region AddTypeCache

    /// <summary>
    /// Adds the <see cref="ITypeCache"/> to your services, executes its Initialization and returns the ready-to-use instance.
    /// </summary>
    public static IServiceCollection AddTypeCache(this IServiceCollection services, out ITypeCache typeCache,
        ExceptionBuilder exceptions, ILoggerFactory loggerFactory,
        params string[] includedPrefixes)
        => services.AddTypeCache(out typeCache, exceptions, loggerFactory, null, SearchOption.TopDirectoryOnly, includedPrefixes);

    public static IServiceCollection AddTypeCache(
        this IServiceCollection services,
        out ITypeCache typeCache,
        ExceptionBuilder exceptions,
        ILoggerFactory loggerFactory,
        string? assemblySearchDirectory = null,
        SearchOption searchOption = SearchOption.TopDirectoryOnly,
        params string[] includedPrefixes)
        => services.AddTypeCache(out typeCache, exceptions, loggerFactory,
            TypePackage.Get(assemblySearchDirectory, includedPrefixes, searchOption));

    public static IServiceCollection AddTypeCache(
        this IServiceCollection services,
        out ITypeCache typeCache,
        ExceptionBuilder exceptionBuilder, ILoggerFactory loggerFactory, params ITypePackage[] typePackages)
    {
        typeCache = new TypeCache(TypePackage.Get(typePackages), exceptionBuilder, loggerFactory);
        return services.AddSingleton(typeCache);
    }

    #endregion
}
