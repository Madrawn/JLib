using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.DataGeneration;

public static class DataPackageExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="typeCache"></param>
    /// <param name="defaultNamespace">this namespace will be removed from the idGroupName via string replace.</param>
    /// <returns></returns>
    public static IServiceCollection AddDataPackages(this IServiceCollection services, ITypeCache typeCache, string? defaultNamespace = null)
    {
        services.AddSingleton<IIdRegistry, IdRegistry>(provider =>
        {
            return new(provider, PostProcessor);

            DataPackageValues.IdIdentifier PostProcessor(DataPackageValues.IdIdentifier id)
            {
                if (defaultNamespace is null)
                    return id;

                return id with
                {
                    IdGroupName = new(id.IdGroupName.Value.Replace(
                        defaultNamespace.EndsWith(".")
                        ? defaultNamespace
                        : defaultNamespace + ".", ""))
                };
            }
        });
        services.AddSingleton<IDataPackageManager, DataPackageManager>();
        foreach (var package in typeCache.All<DataPackageType>())
            services.AddSingleton(package.Value);
        return services;
    }

    public static IServiceProvider IncludeDataPackages(this IServiceProvider provider,
        params DataPackageType[] packages)
        => provider.IncludeDataPackages(packages.Select(p => p.Value).ToArray());

    #region overloads

    public static IServiceProvider IncludeDataPackages(this IServiceProvider provider, params Type[] packages)
    {
        provider.GetRequiredService<IDataPackageManager>().IncludeDataPackages(packages);
        return provider;
    }

    public static IServiceProvider IncludeDataPackages<T>(this IServiceProvider provider)
        where T : DataPackage
        => provider.IncludeDataPackages(typeof(T));

    public static IServiceProvider IncludeDataPackages<T1, T2>(this IServiceProvider provider)
        where T1 : DataPackage
        where T2 : DataPackage
        => provider.IncludeDataPackages(typeof(T1), typeof(T2));

    public static IServiceProvider IncludeDataPackages<T1, T2, T3>(this IServiceProvider provider)
        where T1 : DataPackage
        where T2 : DataPackage
        where T3 : DataPackage
        => provider.IncludeDataPackages(typeof(T1), typeof(T2), typeof(T3));

    public static IServiceProvider IncludeDataPackages<T1, T2, T3, T4>(this IServiceProvider provider)
        where T1 : DataPackage
        where T2 : DataPackage
        where T3 : DataPackage
        where T4 : DataPackage
        => provider.IncludeDataPackages(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

    public static IServiceProvider IncludeDataPackages<T1, T2, T3, T4, T5>(this IServiceProvider provider)
        where T1 : DataPackage
        where T2 : DataPackage
        where T3 : DataPackage
        where T4 : DataPackage
        where T5 : DataPackage
        => provider.IncludeDataPackages(typeof(T1), typeof(T2), typeof(T3), typeof(T5));

    #endregion
}