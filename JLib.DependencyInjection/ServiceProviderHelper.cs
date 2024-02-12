using Microsoft.Extensions.DependencyInjection;

namespace JLib.DependencyInjection;

public static class ServiceProviderHelper
{
    public static TSrvContainer GetServiceContainer<TSrvContainer>(this IServiceProvider provider)
        where TSrvContainer : ServiceContainer, new()
    {
        var c = new TSrvContainer();
        c.Init(provider);
        return c;
    }

    public static IServiceProvider GetRequiredServices<T1>(this IServiceProvider provider,
        out T1 s1) where T1 : notnull 
    {
        s1 = provider.GetRequiredService<T1>();
        return provider;
    }
    public static IServiceProvider GetRequiredServices<T1, T2>(this IServiceProvider provider,
        out T1 s1, out T2 s2) where T1 : notnull where T2 : notnull
    {
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        return provider;
    }

    public static IServiceProvider GetRequiredServices<T1, T2, T3>(this IServiceProvider provider,
        out T1 s1, out T2 s2, out T3 s3) where T1 : notnull where T2 : notnull where T3 : notnull
    {
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        return provider;
    }

    public static IServiceProvider GetRequiredServices<T1, T2, T3, T4>(this IServiceProvider provider,
        out T1 s1, out T2 s2, out T3 s3, out T4 s4) where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    {
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        s4 = provider.GetRequiredService<T4>();
        return provider;
    }

    public static IServiceProvider GetRequiredServices<T1, T2, T3, T4, T5>(this IServiceProvider provider,
        out T1 s1, out T2 s2, out T3 s3, out T4 s4, out T5 s5) where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
    {
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        s4 = provider.GetRequiredService<T4>();
        s5 = provider.GetRequiredService<T5>();
        return provider;
    }
}